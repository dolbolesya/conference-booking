using ConferenceBooking.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceBooking.Infrastructure.Persistence;

/// <summary>
/// Застосовує міграції та наповнює БД початковими даними з ТЗ,
/// щоб проєкт піднімався однією командою без ручних кроків.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync(ct);

        if (await context.Rooms.AnyAsync(ct))
            return;

        var roomA = new Room("Зал А", 50, 2000m);
        var roomB = new Room("Зал B", 100, 3500m);
        var roomC = new Room("Зал C", 30, 1500m);

        foreach (var room in new[] { roomA, roomB, roomC })
        {
            room.AddAmenity("Проєктор", 500m);
            room.AddAmenity("Wi-Fi", 300m);
            room.AddAmenity("Звук", 700m);
        }

        context.Rooms.AddRange(roomA, roomB, roomC);
        await context.SaveChangesAsync(ct);
    }
}