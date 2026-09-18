using FluentValidation;

namespace ConferenceBooking.Application.Rooms;

public sealed class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(10_000);
        RuleFor(x => x.BasePricePerHour).GreaterThan(0);

        RuleForEach(x => x.Amenities).ChildRules(amenity =>
        {
            amenity.RuleFor(a => a.Name).NotEmpty().MaximumLength(200);
            amenity.RuleFor(a => a.Price).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(10_000);
        RuleFor(x => x.BasePricePerHour).GreaterThan(0);

        RuleForEach(x => x.Amenities).ChildRules(amenity =>
        {
            amenity.RuleFor(a => a.Name).NotEmpty().MaximumLength(200);
            amenity.RuleFor(a => a.Price).GreaterThanOrEqualTo(0);
        });
    }
}