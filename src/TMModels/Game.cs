﻿using System.Text.Json.Serialization;

namespace TMModels;

public record Game(
    int Id,
    string Name,
    bool IsFinished,
    bool IsStalling,
    int Turn,
    Map Map,
    HouseScore[] Houses,
    DateTimeOffset GeneratedTime,
    bool IsCreatedManually = false)
{
    public double Progress => IsFinished ? 
        100 : IsStalling ?
            97 : 100 * (double)Turn / 11;
}

public record HouseScore(
    House House,
    string Player,
    int Throne,
    int Fiefdoms,
    int KingsCourt,
    int Supplies,
    int PowerTokens,
    int Strongholds,
    int Castles,
    int Cla,
    double MinutesPerMove,
    int Moves,
    int[] BattlesInTurn,
    Stats? Stats) : IComparable<HouseScore>
{
    public Stats Stats { get; } = Stats ?? new Stats();

    public int CompareTo(HouseScore? otherHouse)
    {
        if (otherHouse == null)
            return 1;

        if (Castles + Strongholds != otherHouse.Castles + otherHouse.Strongholds)
            return Castles + Strongholds - (otherHouse.Castles + otherHouse.Strongholds);

        if (Cla != otherHouse.Cla)
            return Cla - otherHouse.Cla;

        if (Supplies != otherHouse.Supplies)
            return Supplies - otherHouse.Supplies;

        return otherHouse.Throne - Throne;
    }
}

public record Map(
    Land[] Lands,
    Sea[] Seas,
    Port[] Ports);

public record Land(
    bool IsEnabled,
    int Id,
    string Name,
    House House,
    int Footmen,
    int Knights,
    int SiegeEngines,
    int Tokens,
    int MobilizationPoints,
    int Supplies,
    int Crowns);

public record Sea(
    bool IsEnabled,
    int Id,
    string Name,
    House House,
    int Ships);

public record Port(
    bool IsEnabled,
    int Id,
    string Name,
    int Ships);

public record Stats(
    BattleStats Battles,
    UnitStats Kills,
    UnitStats Casualties,
    PowerTokenStats PowerTokens,
    BidStats Bids)
{
    public Stats() :
        this(new BattleStats(), new UnitStats(), new UnitStats(), new PowerTokenStats(), new BidStats())
    { }

    public static Stats Max(Stats stats1, Stats stats2) => new(
        BattleStats.Max(stats1.Battles, stats2.Battles),
        UnitStats.Max(stats1.Kills, stats2.Kills),
        UnitStats.Min(stats1.Casualties, stats2.Casualties),
        PowerTokenStats.Max(stats1.PowerTokens, stats2.PowerTokens),
        BidStats.Max(stats1.Bids, stats2.Bids));

    public static Stats operator +(Stats stats1, Stats stats2) => new(
         stats1.Battles + stats2.Battles,
         stats1.Kills + stats2.Kills,
         stats1.Casualties + stats2.Casualties,
         stats1.PowerTokens + stats2.PowerTokens,
         stats1.Bids + stats2.Bids);

    public static Stats operator /(Stats stats, double divisor) => new(
         stats.Battles / divisor,
         stats.Kills / divisor,
         stats.Casualties / divisor,
         stats.PowerTokens / divisor,
         stats.Bids / divisor);
}

public record BattleStats
{
    public double Won { get; set; }
    public double Lost { get; set; }
    [JsonIgnore]
    public double Total => Won + Lost;

    public BattleStats() { }
    public BattleStats(double won, double lost)
    {
        Won = won;
        Lost = lost;
    }

    public static BattleStats Max(BattleStats stats1, BattleStats stats2) => new(
        Math.Max(stats1.Won, stats2.Won),
        Math.Min(stats1.Lost, stats2.Lost));

    public static BattleStats operator +(BattleStats stats1, BattleStats stats2) => new(
        stats1.Won + stats2.Won,
        stats1.Lost + stats2.Lost);

    public static BattleStats operator /(BattleStats stats, double divisor) => new(
        stats.Won / divisor,
        stats.Lost / divisor);
}

public record UnitStats
{
    public UnitStats() { }
    public UnitStats(double footmen, double knights, double siegeEngines, double ships)
    {
        Footmen = footmen;
        Knights = knights;
        SiegeEngines = siegeEngines;
        Ships = ships;
    }

    public double Footmen { get; set; }
    public double Knights { get; set; }
    public double SiegeEngines { get; set; }
    public double Ships { get; set; }
    [JsonIgnore]
    public double Total => Footmen + Knights + SiegeEngines + Ships;
    [JsonIgnore]
    public double MobilizationPoints => Footmen + 2 * Knights + 2 * SiegeEngines + Ships;

    public static UnitStats Max(UnitStats stats1, UnitStats stats2) => new(
        Math.Max(stats1.Footmen, stats2.Footmen),
        Math.Max(stats1.Knights, stats2.Knights),
        Math.Max(stats1.SiegeEngines, stats2.SiegeEngines),
        Math.Max(stats1.Ships, stats2.Ships));

    public static UnitStats Min(UnitStats stats1, UnitStats stats2) => new(
        Math.Min(stats1.Footmen, stats2.Footmen),
        Math.Min(stats1.Knights, stats2.Knights),
        Math.Min(stats1.SiegeEngines, stats2.SiegeEngines),
        Math.Min(stats1.Ships, stats2.Ships));

    public static UnitStats operator +(UnitStats stats1, UnitStats stats2) => new(
        stats1.Footmen + stats2.Footmen,
        stats1.Knights + stats2.Knights,
        stats1.SiegeEngines + stats2.SiegeEngines,
        stats1.Ships + stats2.Ships);

    public static UnitStats operator /(UnitStats stats, double divisor) => new(
        stats.Footmen / divisor,
        stats.Knights / divisor,
        stats.SiegeEngines / divisor,
        stats.Ships / divisor);
}

public record PowerTokenStats
{
    public double ConsolidatePower { get; set; }
    public double Raids { get; set; }
    public double GameOfThrones { get; set; }
    public double Wildlings { get; set; }
    public double Tywin { get; set; }
    [JsonIgnore]
    public double Total => ConsolidatePower + Raids + GameOfThrones + Wildlings + Tywin;

    public PowerTokenStats() { }
    public PowerTokenStats(double consolidatePower, double raids, double gameOfThrones, double wildlings, double tywin)
    {
        ConsolidatePower = consolidatePower;
        Raids = raids;
        GameOfThrones = gameOfThrones;
        Wildlings = wildlings;
        Tywin = tywin;
    }

    public static PowerTokenStats Max(PowerTokenStats stats1, PowerTokenStats stats2) => new(
        Math.Max(stats1.ConsolidatePower, stats2.ConsolidatePower),
        Math.Max(stats1.Raids, stats2.Raids),
        Math.Max(stats1.GameOfThrones, stats2.GameOfThrones),
        Math.Max(stats1.Wildlings, stats2.Wildlings),
        Math.Max(stats1.Tywin, stats2.Tywin));

    public static PowerTokenStats operator +(PowerTokenStats stats1, PowerTokenStats stats2) => new(
        stats1.ConsolidatePower + stats2.ConsolidatePower,
        stats1.Raids + stats2.Raids,
        stats1.GameOfThrones + stats2.GameOfThrones,
        stats1.Wildlings + stats2.Wildlings,
        stats1.Tywin + stats2.Tywin);

    public static PowerTokenStats operator /(PowerTokenStats stats, double divisor) => new(
        stats.ConsolidatePower / divisor,
        stats.Raids / divisor,
        stats.GameOfThrones / divisor,
        stats.Wildlings / divisor,
        stats.Tywin / divisor);
}

public record BidStats
{
    public double IronThrone { get; set; }
    public double Fiefdoms { get; set; }
    public double KingsCourt { get; set; }
    public double Wildlings { get; set; }
    public double Aeron { get; set; }
    [JsonIgnore]
    public double Total => IronThrone + Fiefdoms + KingsCourt + Wildlings + Aeron;

    public BidStats() { }
    public BidStats(double ironThrone, double fiefdoms, double kingsCourt, double wildlings, double aeron)
    {
        IronThrone = ironThrone;
        Fiefdoms = fiefdoms;
        KingsCourt = kingsCourt;
        Wildlings = wildlings;
        Aeron = aeron;
    }

    public static BidStats Max(BidStats stats1, BidStats stats2) => new(
        Math.Max(stats1.IronThrone, stats2.IronThrone),
        Math.Max(stats1.Fiefdoms, stats2.Fiefdoms),
        Math.Max(stats1.KingsCourt, stats2.KingsCourt),
        Math.Max(stats1.Wildlings, stats2.Wildlings),
        Math.Max(stats1.Aeron, stats2.Aeron));

    public static BidStats operator +(BidStats stats1, BidStats stats2) => new(
        stats1.IronThrone + stats2.IronThrone,
        stats1.Fiefdoms + stats2.Fiefdoms,
        stats1.KingsCourt + stats2.KingsCourt,
        stats1.Wildlings + stats2.Wildlings,
        stats1.Aeron + stats2.Aeron);

    public static BidStats operator /(BidStats stats, double divisor) => new(
        stats.IronThrone / divisor,
        stats.Fiefdoms / divisor,
        stats.KingsCourt / divisor,
        stats.Wildlings / divisor,
        stats.Aeron / divisor);
}