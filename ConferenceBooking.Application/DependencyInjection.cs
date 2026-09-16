using ConferenceBooking.Application.Bookings;
using ConferenceBooking.Application.Rooms;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}