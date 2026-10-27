using BITRestAPI.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BITRestAPI.GwacheonAPI
{
	public class GwacheonService : IDisposable
	{
		Timer? gwacheonDataTimer;
		//Timer gwacheonAirQualityTimer;
		//Timer gwacheonWeatherTimer;
		static string serviceKeyPrivate = "08ixNTCwWo%2BfQRSX6bz8EADns5hSePqPJCSag4%2F1zLOtmwgHI%2F6FPP2fCq%2FZq4p6uzBQdj%2BG1wWhLo%2FmYxcvyg%3D%3D";
		static string _serviceKey = "gvJ3930cxcONJYzeeYvlnTcw4mG2dezOE9EPI2NwLqr2mworZ2zC7igmX3Nu1kOUsOtSsNmNojrurmd0KltqFw%3D%3D";

		private readonly IHubContext<BitHub> _hubContext;
		private readonly string _stationId;
		private readonly HttpClient _httpClient;

		private Timer? _gwacheonDataTimer;
		private bool _busyBusData = false;
		private List<BusArrivalList> _lastKnownGoodData = new();

		public GwacheonService(IHubContext<BitHub> hubContext, string stationId, HttpClient httpClient)
		{
			_hubContext = hubContext;
			_stationId = stationId;
			_httpClient = httpClient;
			InitProc();
		}

		private void InitProc()
		{
			gwacheonDataTimer ??= new Timer(async x => { await GetGwachonBusDataAsync(); }, null, 5000, 60 * 1000);// 30000);  15000  //pjhpjh 15초시험중 canceled [change 30sec to 60sec : Insufficient data capacity]
			//gwacheonAirQualityTimer ??= new Timer(async x => { await GetGwachonAirQualityData(); }, null, 3000, 60 * 60 * 1000);   //wait 3sec, each 1hour
			//gwacheonWeatherTimer ??= new Timer(async x => { await GetGwacheonWeatherData(); }, null, 4000, 60 * 60 * 1000);
		}

		public List<BusArrivalList> GetLatestCachedData() => _lastKnownGoodData;

		private async Task GetGwachonBusDataAsync()
		{
			if (_busyBusData) return;
			_busyBusData = true;
			//string stationId = "220000112";
			List<BusArrivalList> data = [];
			try
			{
				//TODO: this is a dummy for test purpose. later need to pass station id from request url. (e.g. /api/gwacheon/busarrival?stationId=220000112)
				 //"119000074";// BITDataManager.BitSystem.STATION_ID;//"220000112";
				string endpoint = $"https://apis.data.go.kr/6410000/busarrivalservice/v2/getBusArrivalListv2?serviceKey={_serviceKey}&stationId={_stationId}&format=json";

				var response = await _httpClient!.GetAsync(endpoint);

				if (response.IsSuccessStatusCode)
				{
					var rdata = await response.Content.ReadAsStringAsync();
					var result = JsonSerializer.Deserialize<GwachonApiResponse>(rdata);

					// msgBody가 null이거나 busArrivalList가 null이면 예외 처리
					if (result?.response?.msgBody?.busArrivalList != null)
					{
						if (result.response.msgBody.busArrivalList.GetType() != typeof(JsonArray))
						{
							var listResult = result.response.msgBody.busArrivalList.Deserialize<BusArrivalList>();
							if (listResult != null)
								data.Add(listResult);
						}
						else
						{
							var listResults = result.response.msgBody.busArrivalList.Deserialize<List<BusArrivalList>>();
							if (listResults != null)
								data.AddRange(listResults);
						}

						_lastKnownGoodData = data;
						//data.ForEach(async a => await GetBusLocationsForRouteId(a.routeId));
					}
					else
					{
						var code = result?.response?.msgHeader?.resultCode;
						var msg = result?.response?.msgHeader?.resultMessage;
						System.Diagnostics.Debug.WriteLine($"[BusArrival] Empty body or no data. Code={code}, Msg={msg}");
						//TraceManager.AddLog($"[BusArrival] No data. resultCode={code}, resultMessage={msg}");
					}
				}
			}
			catch (Exception ee)
			{
				//HttpService.UpdateBusDataStatus(0);
				//TraceManager.AddLog(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
				System.Diagnostics.Debug.WriteLine(string.Format("### {0}\r\n{1} ###", ee.StackTrace, ee.Message));
			}
			finally
			{
				//OnReceivedEvent.Invoke(data);
				_busyBusData = false;
				StationArrivalResponse arrivalData = MapData();
				Console.WriteLine($"Received bus arrival data for station {arrivalData.StationId} at {arrivalData.UpdateTime}. Number of buses: {arrivalData.BusList.Count}");
				await _hubContext.Clients.Group(_stationId).SendAsync("ReceiveBusArrivals", arrivalData);
			}
		}

		private StationArrivalResponse MapData()
		{
			StationArrivalResponse response = new StationArrivalResponse(
				StationId: _stationId,
				StationName: "Gwacheon Station", // You might want to replace this with actual station name if available
				UpdateTime: DateTimeOffset.Now,
				BusList: _lastKnownGoodData.Select(bus => new BusArrivalResponse(
					RouteId: bus.routeId.ToString(),
					RouteNo: bus.routeName?.ToString() ?? "Unknown",
					RouteDirection: bus.routeDestName ?? "Unknown",
					CurrentStationName: bus.stationNm1 ?? "Unknown",
					OperationMode: GetOperationStatus(bus.flag, bus.predictTimeSec1), // Placeholder, replace with actual data if available
					BusType: bus.routeTypeCd,
					PredictType: 0, // Placeholder, replace with actual data if available
					RemainStop: int.TryParse(bus.locationNo1.ToString(), out var remainStop) ? remainStop : 0,
					RemainTime: bus.predictTimeSec1 ?? 0
				)).ToList()
			);
			return response;
		}

		private int GetOperationStatus(string flag, int? predictTimeSec1)
		{
			if (predictTimeSec1 is null)
				return 1;

			return flag switch
			{
				"RUN" => 4,  //운행중
				"PASS" => 3, //운행중
				"STOP" => 2, //운행종료
				"WAIT" => 1, //회차지대기
				_ => 1
			};
		}

		//		public record StationArrivalResponse(
		//	[property: JsonPropertyName("stationId")] string StationId,
		//	[property: JsonPropertyName("stationName")] string StationName,
		//	[property: JsonPropertyName("updateTime")] DateTimeOffset UpdateTime,
		//	[property: JsonPropertyName("busList")] List<BusArrivalResponse> BusList
		//);

		//		public record BusArrivalResponse(
		//			[property: JsonPropertyName("routeId")] string RouteId,
		//			[property: JsonPropertyName("routeNo")] string RouteNo,
		//			[property: JsonPropertyName("routeDirection")] string RouteDirection,
		//			[property: JsonPropertyName("currSttnName")] string CurrentStationName, // Renamed for better readability
		//			[property: JsonPropertyName("operationMode")] int OperationMode,
		//			[property: JsonPropertyName("busType")] int BusType,
		//			[property: JsonPropertyName("predictType")] int PredictType,
		//			[property: JsonPropertyName("remainStop")] int RemainStop,
		//			[property: JsonPropertyName("remainTime")] int RemainTime // Time in seconds		
		//		);

		public void Dispose()
		{
			_gwacheonDataTimer?.Dispose();
			_gwacheonDataTimer = null;
			System.Diagnostics.Debug.WriteLine($"[GwacheonService] Disposed timer worker context loop for Station ID: {_stationId}");
		}
	}
}
