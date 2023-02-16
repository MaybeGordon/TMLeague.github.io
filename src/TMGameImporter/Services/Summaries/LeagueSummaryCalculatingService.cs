﻿using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Summaries;

internal class LeagueSummaryCalculatingService
{
    private readonly SeasonSummaryCalculatingService _seasonSummaryCalculatingService;
    private readonly FileLoader _fileLoader;
    private readonly ILogger<LeagueSummaryCalculatingService> _logger;

    public LeagueSummaryCalculatingService(
        SeasonSummaryCalculatingService seasonSummaryCalculatingService,
        FileLoader fileLoader, ILogger<LeagueSummaryCalculatingService> logger)
    {
        _seasonSummaryCalculatingService = seasonSummaryCalculatingService;
        _fileLoader = fileLoader;
        _logger = logger;
    }

    public async Task<Summary?> Calculate(string leagueId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            " League {leagueId} summary calculation started...",
            leagueId.ToUpper());

        var league = await _fileLoader.LoadLeague(leagueId, cancellationToken);
        if (league == null)
        {
            _logger.LogError(
                " League {leagueId} cannot be deserialized correctly.",
                leagueId.ToUpper());
            return null;
        }

        var summary = new Summary(leagueId, league.Name, Array.Empty<SummaryDivision>());

        foreach (var seasonId in league.Seasons)
        {
            var seasonSummary = await _seasonSummaryCalculatingService.Calculate(
                leagueId, league.Name, seasonId, league.MainDivisions, cancellationToken);
            if (seasonSummary != null)
                summary += seasonSummary;
        }

        _logger.LogInformation(
            " League {leagueId} summary calculated.",
            leagueId.ToUpper());

        summary.Sort(league.Scoring?.Tiebreakers ?? Tiebreakers.Default);

        return summary;
    }
}