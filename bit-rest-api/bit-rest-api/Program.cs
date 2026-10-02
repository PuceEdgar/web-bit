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

app.MapPost("/api/bus-arrivals", async (BusArrivalData[] data, IHubContext<BitHub> hubContext) =>
{
    await hubContext.Clients.Group($"{data[0].StationId}")
        .SendAsync("ReceiveBusArrivals", data);

    return Results.Ok();
});

app.Run();


