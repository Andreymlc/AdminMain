using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prometheus;

const int ApiPort = 6001;
const string DataServiceUrl = "http://data-service:6002";

var requestCounter = Metrics.CreateCounter(
    "adminmediator_requests_total",
    "Total HTTP requests handled by AdminMediatorService",
    new CounterConfiguration
    {
        LabelNames = ["path", "method", "status_code"]
    });

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(op => op.ListenAnyIP(ApiPort, opt => opt.Protocols = HttpProtocols.Http1));

builder.Services.AddHealthChecks();
builder.Services.AddHttpClient("data", client =>
{
    client.BaseAddress = new Uri(DataServiceUrl);
});

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

app.MapGet("/internal-data", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("data");

    var response = await client.GetAsync("/data");
    response.EnsureSuccessStatusCode();

    var data = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>() 
               ?? new Dictionary<string, object>();

    data["processedBy"] = "AdminMediatorService";
    data["processedAtUtc"] = DateTime.UtcNow;

    return Results.Ok(data);
})
.WithName("GetInternalData")
.Produces<Dictionary<string, object>>(StatusCodes.Status200OK);

app.Run();
