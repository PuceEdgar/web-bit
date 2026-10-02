using Microsoft.AspNetCore.SignalR;

internal class BitHub : Hub
{
    public async Task JoinStation(string stationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, stationId);
    }
}