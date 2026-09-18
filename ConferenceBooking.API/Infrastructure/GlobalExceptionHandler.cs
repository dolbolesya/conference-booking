using ConferenceBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.API.Infrastructure;

/// <summary>
/// Перетворює винятки на ProblemDetails (RFC 9457). Клієнт отримує зрозумілий код
/// помилки, але ніколи -стек викликів чи деталі реалізації.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Необроблена помилка під час обробки запиту");
        else
            _logger.LogInformation("Запит відхилено: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= StatusCodes.Status500InternalServerError
                ? "Внутрішня помилка сервера. Спробуйте пізніше."
                : exception.Message,
            Instance = context.Request.Path
        };

        if (exception is DomainException domainException)
            problem.Extensions["code"] = domainException.Code;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }

    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Ресурс не знайдено"),
        BookingConflictException => (StatusCodes.Status409Conflict, "Конфлікт бронювання"),
        PricingException => (StatusCodes.Status422UnprocessableEntity, "Неприпустимі умови бронювання"),
        DomainException { Code: "access_denied" } => (StatusCodes.Status403Forbidden, "Доступ заборонено"),
        DomainException { Code: "unauthenticated" } => (StatusCodes.Status401Unauthorized, "Потрібна автентифікація"),
        DomainException { Code: "invalid_credentials" } => (StatusCodes.Status401Unauthorized, "Помилка автентифікації"),
        DomainException { Code: "email_taken" } => (StatusCodes.Status409Conflict, "Конфлікт даних"),
        DomainException => (StatusCodes.Status422UnprocessableEntity, "Порушення бізнес-правила"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Некоректні дані запиту"),
        _ => (StatusCodes.Status500InternalServerError, "Внутрішня помилка сервера")
    };
}