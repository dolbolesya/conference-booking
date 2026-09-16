namespace ConferenceBooking.Application.Abstractions;

/// <summary>
/// Час бізнесу, а не час сервера. Абстракція потрібна, щоб тести на «не можна бронювати
/// в минулому» не залежали від реального годинника машини.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Поточний локальний час компанії (Kind = Unspecified).</summary>
    DateTime Now { get; }

    /// <summary>Поточний UTC - для службових міток (створення, скасування).</summary>
    DateTime UtcNow { get; }
}