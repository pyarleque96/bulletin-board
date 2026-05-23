using Bulletin.Board.Domain.Exceptions;

namespace Bulletin.Board.Domain.Entities;

/// <summary>
/// A photo for a Vehicle. <see cref="IsPublic"/> defaults to false — the GM must
/// approve visibility before it appears in the public detail layouts.
/// </summary>
public class VehiclePhoto
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid VehicleId { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public bool IsPublic { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Vehicle Vehicle { get; private set; } = null!;

    private VehiclePhoto() { }

    public static VehiclePhoto Create(Guid vehicleId, string filePath, int displayOrder, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new DomainException("Vehicle photo file path is required.");

        return new VehiclePhoto
        {
            VehicleId = vehicleId,
            FilePath = filePath,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary,
            IsPublic = false  // GM approves visibility separately.
        };
    }

    public void MakePublic()  => IsPublic = true;
    public void MakePrivate() => IsPublic = false;
    public void MarkPrimary()    => IsPrimary = true;
    public void UnmarkPrimary()  => IsPrimary = false;
}
