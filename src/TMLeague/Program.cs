using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using TMApplication.Providers;
using TMApplication.Services;
using TMInfrastructure.Http;
using TMInfrastructure.Http.Configuration;
using TMLeague;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .Configure<LocalApiOptions>(builder.Configuration.GetSection("LocalApi"))
    .AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddScoped<IMemoryCache, MemoryCache>()
    .AddScoped<IDataProvider, LocalApi>()
    .AddScoped<HomeService>()
    .AddScoped<LeagueService>()
    .AddScoped<SeasonService>()
    .AddScoped<DivisionService>()
    .AddScoped<GameService>()
    .AddScoped<PlayerService>()
    .AddScoped<DraftService>()
    .AddScoped<PlayerStatsService>()
    .AddScoped<SummaryService>()
    .AddScoped<HallOfFameService>();

await builder.Build().RunAsync();
