using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// A rentable vehicle owned by a car-rental listing. Multiple vehicles per listing
/// are allowed and optional — the basic layout ignores them, the VIP layout shows
/// the fleet. Rates are stored in USD cents (int) to avoid decimal rounding drift.
/// </summary>
public class Vehicle
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ListingId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public int PassengerMax { get; private set; }
    public Transmission Transmission { get; private set; }
    public bool HasAirConditioning { get; private set; }

    public int DailyRateCents { get; private set; }
    public int? WeeklyRateCents { get; private set; }
    public int? MonthlyRateCents { get; private set; }

    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Listing Listing { get; private set; } = null!;
    public ICollection<VehiclePhoto> Photos { get; private set; } = [];
    public ICollection<VehicleUnavailability> Unavailability { get; private set; } = [];

    private Vehicle() { }

    public static Vehicle Create(
        Guid listingId,
        string name,
        int passengerMax,
        Transmission transmission,
        bool hasAirConditioning,
        int dailyRateCents,
        int? weeklyRateCents = null,
        int? monthlyRateCents = null,
        int displayOrder = 0)
    {
        ValidateInvariants(name, passengerMax, dailyRateCents, weeklyRateCents, monthlyRateCents);

        return new Vehicle
        {
            ListingId = listingId,
            Name = name.Trim(),
            PassengerMax = passengerMax,
            Transmission = transmission,
            HasAirConditioning = hasAirConditioning,
            DailyRateCents = dailyRateCents,
            WeeklyRateCents = weeklyRateCents,
            MonthlyRateCents = monthlyRateCents,
            DisplayOrder = displayOrder
        };
    }

    public void Edit(
        string name,
        int passengerMax,
        Transmission transmission,
        bool hasAirConditioning,
        int dailyRateCents,
        int? weeklyRateCents,
        int? monthlyRateCents,
        int displayOrder)
    {
        ValidateInvariants(name, passengerMax, dailyRateCents, weeklyRateCents, monthlyRateCents);

        Name = name.Trim();
        PassengerMax = passengerMax;
        Transmission = transmission;
        HasAirConditioning = hasAirConditioning;
        DailyRateCents = dailyRateCents;
        WeeklyRateCents = weeklyRateCents;
        MonthlyRateCents = monthlyRateCents;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateInvariants(
        string name,
        int passengerMax,
        int dailyRateCents,
        int? weeklyRateCents,
        int? monthlyRateCents)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Vehicle name is required.");
        if (name.Length > 80)
            throw new DomainException("Vehicle name must be 80 characters or less.");

        if (passengerMax < 1)
            throw new DomainException("Passenger maximum must be at least 1.");
        if (passengerMax > 50)
            throw new DomainException("Passenger maximum is unreasonably high.");

        if (dailyRateCents <= 0)
            throw new DomainException("Daily rate must be greater than zero.");
        if (weeklyRateCents.HasValue && weeklyRateCents.Value <= 0)
            throw new DomainException("Weekly rate must be greater than zero when provided.");
        if (monthlyRateCents.HasValue && monthlyRateCents.Value <= 0)
            throw new DomainException("Monthly rate must be greater than zero when provided.");
    }
}
