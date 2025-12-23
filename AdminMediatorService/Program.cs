using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prometheus;
using System.Net.Http.Json;

const int ApiPort = 6001;
const string DataServiceUrl = "http://data-service:6002"; // docker-compose service name

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
