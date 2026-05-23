namespace Bulletin.Board.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string NameEn { get; private set; } = string.Empty;
    public string NameEs { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? IconUrl { get; private set; }

    public ICollection<Listing> Listings { get; private set; } = [];

    private Category() { }

    public static Category Create(string nameEn, string nameEs, string slug, int displayOrder = 0, string? iconUrl = null)
        => new() { NameEn = nameEn, NameEs = nameEs, Slug = slug, DisplayOrder = displayOrder, IconUrl = iconUrl };

    public void Update(string nameEn, string nameEs, string slug, int displayOrder, string? iconUrl)
    {
        NameEn = nameEn;
        NameEs = nameEs;
        Slug = slug;
        DisplayOrder = displayOrder;
        IconUrl = iconUrl;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
