using AnalisisVentas.Data.Persistence;
using AnalisisInventario.WorkerService.Load;
using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.WKService;
using AnalisisVentas.WKService.Extract;
using AnalisisVentas.WKService.Transform;
using AnalisisVentas.WKService.Staging;

var builder = Host.CreateApplicationBuilder(args);

// =========================
// 🔹 CONFIGURACIÓN
// =========================
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// =========================
// 🔹 LOGGING (YA VIENE POR DEFECTO)
// =========================

// =========================
// 🔹 DAPPER CONTEXT
// =========================
builder.Services.AddSingleton<DapperContext>();

// =========================
// 🔹 ETL SERVICES
// =========================
builder.Services.AddScoped<CsvExtractor>();
builder.Services.AddScoped<ApiExtractor>();
builder.Services.AddScoped<DatabaseExtractor>();

builder.Services.AddScoped<ITransformer, DataTransformer>();
builder.Services.AddScoped<ILoader, DataLoader>();

// =========================
// 🔹 STAGING
// =========================
builder.Services.AddScoped<JsonStagingService>();

// =========================
// 🔹 HTTP CLIENT (API)
// =========================
builder.Services.AddHttpClient();

// =========================
// 🔹 WORKER (🔥 CLAVE)
// =========================
builder.Services.AddHostedService<Worker>();

// =========================
// 🔹 BUILD & RUN
// =========================
var host = builder.Build();

await host.RunAsync();