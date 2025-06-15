using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

SentrySdk.Init(options =>
{
    options.Dsn = "https://5ae497d2c7ce327ed3d7c3487cf64595@o4509326192148480.ingest.de.sentry.io/4509394240471120";
    options.TracesSampleRate = 1.0;
    options.SendDefaultPii = true;
    // Optional: environment, release, etc.
});

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // DI as needed
    })
    .Build();

builder.Build().Run();
