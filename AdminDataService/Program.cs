using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prometheus;

const int ApiPort = 6002;

var requestCounter = Metrics.CreateCounter(
    "admindata_requests_total",
    "Total HTTP requests handled by AdminDataService",
    new CounterConfiguration
    {
        LabelNames = ["path", "method", "status_code"]
    });

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(op => op.ListenAnyIP(ApiPort, opt => opt.Protocols = HttpProtocols.Http1));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMetricServer("/metrics");
app.UseHttpMetrics();

app.Use(async (context, next) =>
{
    await next();
    requestCounter
        .WithLabels(context.Request.Path, context.Request.Method, context.Response.StatusCode.ToString())
        .Inc();
});

app.MapHealthChecks("/check");

app.MapGet("/data", () =>
{
    var result = new
    {
        id = Guid.NewGuid(),
        value = "sample data",
        createdAtUtc = DateTime.UtcNow
    };

    return Results.Ok(result);
})
.WithName("GetData");

app.Run();
