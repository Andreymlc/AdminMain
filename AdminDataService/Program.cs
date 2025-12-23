using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prometheus;

const int ApiPort = 6002;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(op => op.ListenAnyIP(ApiPort, opt => opt.Protocols = HttpProtocols.Http1));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMetricServer("/metrics");
app.UseHttpMetrics();

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
