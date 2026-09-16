using ConferenceBooking.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.API.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    /// <summary>Виручка за період із розбивкою по залах.</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDto>> Revenue(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(await _reports.GetRevenueAsync(new ReportPeriodRequest(from, to), ct));

    /// <summary>Завантаженість залів у відсотках від доступного фонду годин.</summary>
    [HttpGet("utilization")]
    public async Task<ActionResult<IReadOnlyList<RoomUtilizationDto>>> Utilization(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(await _reports.GetUtilizationAsync(new ReportPeriodRequest(from, to), ct));

    /// <summary>Популярність додаткових послуг: скільки разів замовляли та скільки принесли.</summary>
    [HttpGet("amenities")]
    public async Task<ActionResult<IReadOnlyList<AmenityPopularityDto>>> Amenities(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(await _reports.GetAmenityPopularityAsync(new ReportPeriodRequest(from, to), ct));

    /// <summary>Розподіл бронювань за годинами доби — перевірка коректності пікових годин.</summary>
    [HttpGet("hourly")]
    public async Task<ActionResult<IReadOnlyList<HourlyDistributionDto>>> Hourly(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(await _reports.GetHourlyDistributionAsync(new ReportPeriodRequest(from, to), ct));
}