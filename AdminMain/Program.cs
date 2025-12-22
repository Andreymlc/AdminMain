using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prometheus;

const int ApiPort = 6000;
const string MediatorServiceUrl = "http://mediator-service:6001"; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient("mediator", client =>
{
    client.BaseAddress = new Uri(MediatorServiceUrl);
});

builder.WebHost.UseKestrel(op => op.ListenAnyIP(ApiPort, opt => opt.Protocols = HttpProtocols.Http1));

var app = builder.Build();

app.UseMetricServer("/metrics");
app.UseHttpMetrics();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdminMain API V1");
    c.RoutePrefix = "swagger";
});

app.MapHealthChecks("/check");

app.MapGet("/public-data", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("mediator");
    var response = await client.GetAsync("/internal-data");
    response.EnsureSuccessStatusCode();

    var data = await response.Content.ReadFromJsonAsync<object>();
    return Results.Ok(data);
})
.WithName("GetPublicData")
.Produces<object>(StatusCodes.Status200OK);

app.Run();
