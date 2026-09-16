namespace ConferenceBooking.Domain.Bookings;

/// <summary>
/// Знімок послуги на момент бронювання. Назва й ціна зберігаються копією,
/// щоб подальша зміна прайсу не переписувала заднім числом уже виставлені рахунки.
/// </summary>
public sealed class BookedAmenity
{
    public BookedAmenity(Guid amenityId, string name, decimal price)
    {
        Id = Guid.CreateVersion7();
        AmenityId = amenityId;
        Name = name;
        Price = price;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid AmenityId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
}