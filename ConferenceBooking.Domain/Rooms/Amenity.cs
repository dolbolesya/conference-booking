using System.Diagnostics.CodeAnalysis;

namespace ConferenceBooking.Domain.Rooms;

/// <summary>
/// Додаткова послуга залу (проєктор, Wi-Fi, звук).
/// Оплачується разово за бронювання.
/// </summary>
public class Amenity
{
    public Amenity(string name, decimal price)
    {
        Id = Guid.CreateVersion7();
        Rename(name);
        ChangePrice(price);
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    [MemberNotNull(nameof(Name))]
    public void Rename(string name)
    {
        var trimmedName = name?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ArgumentException("Ім'я послуги не може бути порожнім.", nameof(name));

        Name = trimmedName;
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Ціна послуги не може бути від'ємною.", nameof(price));

        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}