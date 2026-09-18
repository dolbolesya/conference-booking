using ConferenceBooking.Application.Abstractions;
using ConferenceBooking.Domain.Rooms;
using ConferenceBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync(ct);

        await SeedAdminAsync(context, hasher, configuration, ct);
        await SeedRoomsAsync(context, ct);
    }

    private static async Task SeedAdminAsync(
        AppDbContext context, IPasswordHasher hasher, IConfiguration configuration, CancellationToken ct)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin, ct))
            return;

        // Пароль береться з конфігурації: у продакшені задається змінною оточення,
        // а не залишається значенням за замовчуванням.
        var email = configuration["Seed:AdminEmail"] ?? "admin@local.com";
        var password = configuration["Seed:AdminPassword"] ?? "Admin12345";

        context.Users.Add(new User(email, hasher.Hash(password), "Адміністратор", UserRole.Admin));

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedRoomsAsync(AppDbContext context, CancellationToken ct)
    {
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