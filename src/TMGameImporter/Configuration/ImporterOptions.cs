﻿namespace TMGameImporter.Configuration;

internal class ImporterOptions
{
    public string BaseLocation { get; set; } = "../TMLeague/wwwroot/data";
    public bool FetchFinishedDivisions { get; set; } = false;
}