using ConferenceBooking.Domain.Bookings;
using ConferenceBooking.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Application.Abstractions;

/// <summary>
/// Контракт сховища для прикладного шару — Application не знає про конкретний провайдер БД.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Booking> Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Транзакція з рівнем ізоляції Serializable — захист від «подвійного бронювання»
    /// при паралельних запитах на один зал.
    /// </summary>
    Task<IApplicationTransaction> BeginSerializableTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IApplicationTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}