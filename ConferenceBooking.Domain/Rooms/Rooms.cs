using System.Diagnostics.CodeAnalysis;
using ConferenceBooking.Domain.Exceptions;

namespace ConferenceBooking.Domain.Rooms;

/// <summary>
/// Конференц-зал -агрегат. Послуги не існують поза залом,
/// тому змінюються лише через методи цього класу.
/// </summary>
public class Room
{
    private readonly List<Amenity> _amenities = new();

    public Room(string name, int capacity, decimal basePricePerHour)
    {
        Id = Guid.CreateVersion7();
        Rename(name);
        ChangeCapacity(capacity);
        ChangeBasePrice(basePricePerHour);
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Room() => Name = string.Empty;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int Capacity { get; private set; }
    public decimal BasePricePerHour { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Amenity> Amenities => _amenities.AsReadOnly();

    [MemberNotNull(nameof(Name))]
    public void Rename(string name)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ArgumentException("Ім'я залу не може бути порожнім.", nameof(name));
        
        Name = trimmedName;
    }

    public void ChangeCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Вмістимість залу повинна бути більшою за нуль.", nameof(capacity));
        
        Capacity = capacity;
    }

    public void ChangeBasePrice(decimal basePricePerHour)
    {
        if (basePricePerHour <= 0)
            throw new ArgumentException("Базова вартість оренди має бути більшою за нуль.", nameof(basePricePerHour));

        BasePricePerHour = decimal.Round(basePricePerHour, 2, MidpointRounding.AwayFromZero);
    }

    public bool CanHost(int attendees) => attendees <= Capacity;


    public Amenity AddAmenity(string name, decimal price)
    {
        var amenity = new Amenity(name, price);

        if (_amenities.Any(a => a.IsActive && string.Equals(a.Name, amenity.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Послуга «{amenity.Name}» вже додана до залу.", nameof(name));

        _amenities.Add(amenity);

        return amenity;
    }

    public void UpdateAmenity(Guid amenityId, string name, decimal price)
    {
        var amenity = RequireAmenity(amenityId);

        amenity.Rename(name);
        amenity.ChangePrice(price);
        amenity.Activate();
    }

    public void RemoveAmenity(Guid amenityId)
    {
        var amenity = RequireAmenity(amenityId);
        amenity.Deactivate();
    }

    public void Delete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }
    private Amenity RequireAmenity(Guid amenityId) =>
        _amenities.FirstOrDefault(a => a.Id == amenityId)
        ?? throw new NotFoundException("Послугу", amenityId);

    public Amenity RequireActiveAmenity(Guid amenityId)
    {
        var amenity = RequireAmenity(amenityId);

        if (!amenity.IsActive)
            throw new DomainException("amenity_inactive", $"Послуга «{amenity.Name}» більше не доступна.");

        return amenity;
    }




}