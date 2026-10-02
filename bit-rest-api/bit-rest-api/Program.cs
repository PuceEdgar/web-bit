using BITRestAPI.DTOs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapHub<BitHub>("/bitHub");

app.MapPost("/api/bus-arrivals", async (StationArrivalResponse arrivalData, IHubContext<BitHub> hubContext) =>
{
    if (arrivalData == null || arrivalData.BusList.Count == 0)
    {
        return Results.BadRequest("No arrival data provided.");
    }

    Console.WriteLine($"Received bus arrival data for station {arrivalData.StationId} at {arrivalData.UpdateTime}. Number of buses: {arrivalData.BusList.Count}");

    await hubContext.Clients.Group($"{arrivalData.StationId}")
        .SendAsync("ReceiveBusArrivals", arrivalData);


    return Results.Ok(new { ProcessedCount = arrivalData.BusList.Count });
});

app.Run();


