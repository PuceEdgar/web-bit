namespace BITRestAPI.GwacheonAPI
{
	public interface IGwacheonStationManager
	{
		Task RegisterClientAsync(string connectionId, string stationId);
		Task UnregisterClientAsync(string connectionId);
	}
}
