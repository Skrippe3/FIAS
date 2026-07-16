using Fias.Application.Interfaces;
using Fias.Infrastructure.Options;
using Fias.Infrastructure.Persistence;
using Fias.Infrastructure.Services;
using Fias.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("FiasDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не указана строка подключения ConnectionStrings:FiasDatabase.");
}

builder.Services.AddDbContext<FiasDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.Configure<FiasOptions>(
    builder.Configuration.GetSection(FiasOptions.SectionName));

builder.Services.AddHttpClient<
    IFiasArchiveDownloadProcessor,
    FiasArchiveDownloadProcessor>(client =>
    {
        client.Timeout = Timeout.InfiniteTimeSpan;

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "FiasService.Worker/1.0");
    });

builder.Services.AddScoped<
    IFiasArchiveExtractService,
    FiasArchiveExtractService>();

builder.Services.AddScoped<
    IFiasReestrObjectImportService,
    FiasReestrObjectImportService>();

builder.Services.AddScoped<
    IFiasAddressObjectImportService,
    FiasAddressObjectImportService>();

builder.Services.AddScoped<
    IFiasHierarchyImportService,
    FiasHierarchyImportService>();

builder.Services.AddScoped<
    IFiasImportOrchestrator,
    FiasImportOrchestrator>();

builder.Services.AddHttpClient<
    IFiasUpdatePlannerService,
    FiasUpdatePlannerService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "FiasService.Worker/1.0");
    });

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
