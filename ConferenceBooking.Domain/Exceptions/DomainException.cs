namespace ConferenceBooking.Domain.Exceptions;

/// <summary>
/// Базова помилка порушення бізнес-правила.
/// Code дозволяє однозначно зіставити помилку з HTTP-статусом,
/// не розбираючи текст повідомлення.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message) => Code = code;
}

/// <summary>Ресурс не існує або видалений. → HTTP 404</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entity, Guid id)
        : base("not_found", $"{entity} з ідентифікатором {id} не знайдено.") { }
}

/// <summary>Зал уже зайнятий на цей інтервал. → HTTP 409</summary>
public sealed class BookingConflictException : DomainException
{
    public BookingConflictException(Guid roomId, DateTime startsAt, DateTime endsAt)
        : base("booking_conflict",
            $"Зал {roomId} вже заброньовано на {startsAt:dd.MM.yyyy HH:mm}–{endsAt:HH:mm}.")
    { }
}

/// <summary>Порушення правил тарифікації або робочого часу. → HTTP 422</summary>
public sealed class PricingException : DomainException
{
    public PricingException(string message) : base("pricing_error", message) { }
}