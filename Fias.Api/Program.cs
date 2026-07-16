using Fias.Application.Interfaces;
using Fias.Infrastructure.Options;
using Fias.Infrastructure.Persistence;
using Fias.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddScoped<
    IFiasVersionService,
    FiasVersionService>();

builder.Services.AddHttpClient<
    IFiasDownloadService,
    FiasDownloadService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "FiasService/1.0");
    });

builder.Services.AddScoped<
    IFiasDownloadQueueService,
    FiasDownloadQueueService>();

builder.Services.AddScoped<
    IFiasSearchService,
    FiasSearchService>();

builder.Services.AddScoped<
    IFiasSchemaService,
    FiasSchemaService>();

builder.Services.AddHttpClient<
    IFiasUpdatePlannerService,
    FiasUpdatePlannerService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "FiasService/1.0");
    });

const string CorsPolicy = "FiasWebClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Fias.Api",
        Version = "v1",
        Description =
            "API для загрузки, обновления и поиска данных ФИАС/ГАР."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Fias.Api v1");

    options.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);

app.MapControllers();

app.Run();
