using BITRestAPI.DTOs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<BitHub>("/bitHub");

app.MapPost("/api/bus-arrivals", async (BusArrivalData[] arrivalData, IHubContext<BitHub> hubContext) =>
{
    if (arrivalData == null || arrivalData.Length == 0)
    {
        return Results.BadRequest("No arrival data provided.");
    }

    var groupedByStation = arrivalData.GroupBy(d => d.StationId);
    foreach (var group in groupedByStation)
    {
        await hubContext.Clients.Group($"{group.Key}")
            .SendAsync("ReceiveBusArrivals", group.ToArray());
    }

    return Results.Ok(new { ProcessedCount = arrivalData.Length });
});

app.Run();


