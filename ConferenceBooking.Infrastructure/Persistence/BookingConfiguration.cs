using ConferenceBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceBooking.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.CustomerEmail).IsRequired().HasMaxLength(320);

        builder.Property(b => b.RoomCharge).HasPrecision(18, 2);
        builder.Property(b => b.AmenitiesCharge).HasPrecision(18, 2);
        builder.Property(b => b.TotalPrice).HasPrecision(18, 2);

        builder.Property(b => b.Status).HasConversion<int>();

        // Обчислювана властивість — у БД її немає.
        builder.Ignore(b => b.Period);
        builder.Ignore(b => b.IsActive);

        // Головний індекс: пошук вільних залів і перевірка перетинів
        // завжди йдуть по залу й інтервалу.
        builder.HasIndex(b => new { b.RoomId, b.StartsAt, b.EndsAt });

        builder.Metadata
            .FindNavigation(nameof(Booking.Amenities))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(b => b.Amenities, amenity =>
        {
            amenity.ToTable("BookedAmenities");
            amenity.WithOwner().HasForeignKey(a => a.BookingId);
            amenity.HasKey(a => a.Id);

            amenity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            amenity.Property(a => a.Price).HasPrecision(18, 2);
        });
    }
}