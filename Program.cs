using CrewRedDemo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json");
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        IConfiguration config = ctx.Configuration;

        services.AddSingleton<ITripSource, CsvTripSource>();
        services.AddTransient<CsvBulkImporter>();
        services.AddSingleton(config);
    })
    .ConfigureLogging((ctx, lb) =>
    {
        lb.AddConsole();
    })
    .Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

IConfigurationSection csvSection = config.GetSection("Csv");
string csvPath = csvSection["Path"];
string destinationTableName = csvSection["DestinationTableName"];
string duplicatesPath = csvSection["DuplicatesPath"];
int batchSize = int.TryParse(config.GetSection("Csv")["BatchSize"], out var b) ? b : 5000;
string connection = config.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(csvPath) || string.IsNullOrWhiteSpace(duplicatesPath))
{
    logger.LogError("Settings are path not provided (check appsettings.json)");
    return;
}

CsvBulkImporter importer = ActivatorUtilities.CreateInstance<CsvBulkImporter>(host.Services, connection, batchSize, destinationTableName, duplicatesPath);

try
{
    logger.LogInformation("Starting import: {Csv}", csvPath);
    await importer.ImportAsync(csvPath);
    logger.LogInformation("Import finished.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Import failed.");
}
