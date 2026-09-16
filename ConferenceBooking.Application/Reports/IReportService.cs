namespace ConferenceBooking.Application.Reports;

public interface IReportService
{
    Task<RevenueReportDto> GetRevenueAsync(ReportPeriodRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<RoomUtilizationDto>> GetUtilizationAsync(ReportPeriodRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<AmenityPopularityDto>> GetAmenityPopularityAsync(ReportPeriodRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<HourlyDistributionDto>> GetHourlyDistributionAsync(ReportPeriodRequest request, CancellationToken ct = default);
}