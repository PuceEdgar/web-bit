using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

internal class BitHub : Hub
{
    public async Task JoinStation(string stationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, stationId);
        Debug.WriteLine($"Connection {Context.ConnectionId} joined station {stationId}");
    }
}