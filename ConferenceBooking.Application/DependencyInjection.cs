using ConferenceBooking.Application.Rooms;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();

        return services;
    }
}