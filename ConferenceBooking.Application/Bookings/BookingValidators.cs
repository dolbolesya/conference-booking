using FluentValidation;

namespace ConferenceBooking.Application.Bookings;

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();

        RuleFor(x => x.StartsAt)
            .NotEmpty()
            .WithMessage("Дата й час початку обов'язкові.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Тривалість має бути додатною.")
            .LessThanOrEqualTo(17 * 60).WithMessage("Тривалість не може перевищувати робочий день.");

        RuleFor(x => x.Attendees)
            .GreaterThan(0).WithMessage("Кількість учасників має бути більшою за нуль.");
    }
}

public sealed class QuoteRequestValidator : AbstractValidator<QuoteRequest>
{
    public QuoteRequestValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
    }
}

public sealed class AvailabilityRequestValidator : AbstractValidator<AvailabilityRequest>
{
    public AvailabilityRequestValidator()
    {
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}