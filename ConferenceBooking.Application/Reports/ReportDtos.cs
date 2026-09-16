namespace ConferenceBooking.Application.Reports;

/// <summary>Період звіту. Обидві межі включно за датою.</summary>
public sealed record ReportPeriodRequest(DateTime From, DateTime To);

public sealed record RevenueByRoomDto(
    Guid RoomId,
    string RoomName,
    int BookingsCount,
    decimal RoomCharge,
    decimal AmenitiesCharge,
    decimal Total);

public sealed record RevenueReportDto(
    DateTime From,
    DateTime To,
    decimal TotalRevenue,
    decimal RoomCharge,
    decimal AmenitiesCharge,
    int BookingsCount,
    int CancelledCount,
    IReadOnlyList<RevenueByRoomDto> ByRoom);

public sealed record RoomUtilizationDto(
    Guid RoomId,
    string RoomName,
    decimal BookedHours,
    decimal AvailableHours,
    decimal UtilizationPercent);

public sealed record AmenityPopularityDto(
    string Name,
    int TimesBooked,
    decimal Revenue);

public sealed record HourlyDistributionDto(
    int Hour,
    int BookingsStarted,
    decimal Revenue);