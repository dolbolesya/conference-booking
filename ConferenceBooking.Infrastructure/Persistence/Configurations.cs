using ConferenceBooking.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceBooking.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.BasePricePerHour)
            .HasPrecision(18, 2);

        // Зали з IsDeleted = true зникають з усіх запитів автоматично,
        // але залишаються в БД разом з історією бронювань.
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasIndex(r => r.Name);

        // Колекція закрита (_amenities), тому EF має писати у поле, а не у властивість.
        builder.Metadata
            .FindNavigation(nameof(Room.Amenities))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(r => r.Amenities, amenity =>
        {
            amenity.ToTable("Amenities");
            amenity.WithOwner().HasForeignKey("RoomId");
            amenity.HasKey(a => a.Id);

            amenity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);

            amenity.Property(a => a.Price).HasPrecision(18, 2);
        });
    }
}