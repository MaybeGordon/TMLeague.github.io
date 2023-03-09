﻿using Microsoft.Extensions.Logging;
using System.Text.Json;
using TMGameImporter.Http.Converters.Models;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal class GameConverter
{
    private readonly LogConverter _logConverter;
    private readonly StateConverter _stateConverter;
    private readonly ILogger<GameConverter> _logger;

    public GameConverter(LogConverter logConverter, StateConverter stateConverter, ILogger<GameConverter> logger)
    {
        _logConverter = logConverter;
        _stateConverter = stateConverter;
        _logger = logger;
    }

    public Game? Convert(int gameId, string dataString, string chatString, string logHtmlString)
    {
        var stateRaw = JsonSerializer.Deserialize<StateRaw>(dataString);
        var chatRaw = JsonSerializer.Deserialize<StateRaw>(chatString);
        if (stateRaw == null)
            return null;

        var state = _stateConverter.Convert(stateRaw, chatRaw);
        if (state?.GameId != gameId)
        {
            _logger.LogError("Fetched game ID {fetchedGameId} is different than expected: {expectedGameId}.", state?.GameId, gameId);
            return null;
        }

        var isStalling = state.Chat?.Contains("delay!", StringComparison.InvariantCultureIgnoreCase) ?? false;

        var log = _logConverter.Convert(gameId, logHtmlString);

        var houses = GetHouses(state, log);

        return new Game(gameId, state.Name, state.IsFinished, isStalling, state.Turn, state.Map, houses, DateTimeOffset.UtcNow);
    }

    private HouseScore[] GetHouses(State state, Log? log)
    {
        const int houseSize = 6;

        var logsPerTurn = log?.Logs.GroupBy(item => item.Turn);

        var houses = state.HousesOrder.Select((house, i) =>
                GetHouseScore(i, house, state, state.HousesDataRaw[(i * houseSize)..((i + 1) * houseSize)], logsPerTurn))
            .ToArray();

        Array.Sort(houses);
        Array.Reverse(houses);

        if (log == null) 
            return houses;

        try
        {
            houses = houses.CalculateStats(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Calculating stats for game {gameId} failed.", state.GameId);
        }
        return houses;
    }

    private static HouseScore GetHouseScore(int idx, House house, State state, string houseDataRaw, IEnumerable<IGrouping<int, LogItem>>? logsPerTurn)
    {
        var player = state.Players[idx];
        var throne = int.Parse(houseDataRaw[..1]);
        var fiefdoms = int.Parse(houseDataRaw[1..2]);
        var kingsCourt = int.Parse(houseDataRaw[2..3]);
        var supplies = int.Parse(houseDataRaw[3..4]);
        var powerTokens = int.Parse(houseDataRaw[4..6]);
        var strongholds = 0;
        var castles = 0;
        var cla = 0;
        foreach (var land in state.Map.Lands)
        {
            if (land.House != house)
                continue;

            cla++;
            if (land.MobilizationPoints == 1)
                castles++;
            else if (land.MobilizationPoints == 2)
                strongholds++;
        }

        var houseSpeed = state.Stats.FirstOrDefault(speed => speed.House == house);

        var battlesInTurn = logsPerTurn?
            .Select(items => items
                .Count(item => IsPlayerBattleLogItem(house, item)))
            .ToArray() ?? Array.Empty<int>();

        return new HouseScore(house, player, throne,
            fiefdoms, kingsCourt, supplies, powerTokens, strongholds,
            castles, cla, houseSpeed?.MinutesPerMove ?? 0, houseSpeed?.MovesCount ?? 0, battlesInTurn, new HouseStats());
    }

    private static bool IsPlayerBattleLogItem(House house, LogItem item) =>
        item.Phase == Phase.March &&
        item.Message.Contains("Battle!") &&
        item.Message.Contains(house.ToString(), StringComparison.InvariantCultureIgnoreCase);
}