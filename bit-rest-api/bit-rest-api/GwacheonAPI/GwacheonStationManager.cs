using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BITRestAPI.GwacheonAPI
{
	public class GwacheonStationManager : IGwacheonStationManager
	{
		private readonly ConcurrentDictionary<string, GwacheonService> _activeStations = new();
		private readonly ConcurrentDictionary<string, int> _stationClientCounts = new();
		private readonly ConcurrentDictionary<string, string> _clientToStationMap = new();

		private readonly IHubContext<BitHub> _hubContext;
		private readonly IHttpClientFactory _httpClientFactory;

		public GwacheonStationManager(IHubContext<BitHub> hubContext,IHttpClientFactory httpClientFactory)
		{
			_hubContext = hubContext;
			_httpClientFactory = httpClientFactory;
		}
		public async Task RegisterClientAsync(string connectionId, string stationId)
		{
			// 1. Keep track of what station this specific client connection is looking at
			_clientToStationMap[connectionId] = stationId;

			// 2. Safely increment the active reference counter for this station
			_stationClientCounts.AddOrUpdate(stationId, 1, (_, count) => count + 1);

			// 3. If it's the very first client for this station, spin up the background data loop
			if (_stationClientCounts[stationId] == 1)
			{
				var serviceClient = _httpClientFactory.CreateClient("GwacheonClient");				

				var service = new GwacheonService(_hubContext, stationId, serviceClient);
				_activeStations[stationId] = service;
			}
			else
			{
				// If the service already exists, instantly push the latest data back to this new client
				if (_activeStations.TryGetValue(stationId, out var existingService))
				{
					var cachedData = existingService.GetLatestCachedData();
					if (cachedData != null && cachedData.Count > 0)
					{
						await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveBusArrivals", cachedData);
					}
				}
			}
		}

		public async Task UnregisterClientAsync(string connectionId)
		{
			if (_clientToStationMap.TryRemove(connectionId, out var stationId))
			{
				if (_stationClientCounts.TryGetValue(stationId, out int count))
				{
					if (count <= 1)
					{
						_stationClientCounts.TryRemove(stationId, out _);

						// Kill the timers and dispose of the service since zero kiosks are watching it
						if (_activeStations.TryRemove(stationId, out var service))
						{
							service.Dispose();
						}
					}
					else
					{
						_stationClientCounts.TryUpdate(stationId, count - 1, count);
					}
				}
			}
		}
	}
}
