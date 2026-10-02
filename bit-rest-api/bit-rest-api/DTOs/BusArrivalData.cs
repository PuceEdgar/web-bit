namespace BITRestAPI.DTOs;

public record BusArrivalData(
    int StationId,
    string? StopName,
    string? RouteNumber,
    string? Destination,
    int RemainingTime,
    int RemainingStops
);
