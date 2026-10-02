using System.Text.Json.Serialization;

namespace BITRestAPI.DTOs;

public record StationArrivalResponse(
    [property: JsonPropertyName("stationId")] string StationId,
    [property: JsonPropertyName("stationName")] string StationName,
    [property: JsonPropertyName("updateTime")] DateTimeOffset UpdateTime,
    [property: JsonPropertyName("busList")] List<BusArrivalResponse> BusList
);

public record BusArrivalResponse(
    [property: JsonPropertyName("routeId")] string RouteId,
    [property: JsonPropertyName("routeNo")] string RouteNo,
    [property: JsonPropertyName("routeDirection")] string RouteDirection,
    [property: JsonPropertyName("currSttnName")] string CurrentStationName, // Renamed for better readability
    [property: JsonPropertyName("operationMode")] int OperationMode,
    [property: JsonPropertyName("busType")] int BusType,
    [property: JsonPropertyName("predictType")] int PredictType,
    [property: JsonPropertyName("remainStop")] int RemainStop,
    [property: JsonPropertyName("remainTime")] int RemainTime // Time in seconds		
);
