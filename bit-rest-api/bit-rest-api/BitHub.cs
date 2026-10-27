using BITRestAPI.GwacheonAPI;
using Microsoft.AspNetCore.SignalR;

public class BitHub : Hub
{
	private readonly IGwacheonStationManager _stationManager;

	public BitHub(IGwacheonStationManager stationManager)
	{
		_stationManager = stationManager;
	}

	public async Task JoinStation(string stationId)
	{
		// 1. Drop them into the SignalR Group target space
		await Groups.AddToGroupAsync(Context.ConnectionId, stationId);

		// 2. Track references via the Manager orchestration layer
		await _stationManager.RegisterClientAsync(Context.ConnectionId, stationId);
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		// Automatically purge reference tracking counts when the socket terminates
		await _stationManager.UnregisterClientAsync(Context.ConnectionId);
		await base.OnDisconnectedAsync(exception);
	}
}