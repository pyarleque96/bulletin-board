using System.ComponentModel.DataAnnotations;

namespace Bulletin.Board.Web.Client.Models;

// ============================================================
// Admin-only models for category management.
// Used exclusively in /admin/categories — never exposed to
// public-facing pages.  Field order mirrors the API contract.
// ============================================================

/// <summary>
/// Payload for POST /api/v1/categories (Admin only).
/// </summary>
public sealed class CategoryCreateModel
{
    [Required(ErrorMessage = "Name (EN) is required.")]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name (ES) is required.")]
    [MaxLength(100)]
    public string NameEs { get; set; } = string.Empty;

    /// <summary>
    /// URL-safe slug, auto-generated from NameEn but fully editable.
    /// Example: "car-rental"
    /// </summary>
    [Required(ErrorMessage = "Slug is required.")]
    [MaxLength(100)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase letters, numbers and hyphens only.")]
    public string Slug { get; set; } = string.Empty;

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999.")]
    public int DisplayOrder { get; set; }

    [MaxLength(500)]
    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Payload for PUT /api/v1/categories/{id} (Admin only).
/// Identical fields to CategoryCreateModel — kept as a separate
/// type so the contract can diverge independently in the future.
/// </summary>
public sealed class CategoryUpdateModel
{
    [Required(ErrorMessage = "Name (EN) is required.")]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name (ES) is required.")]
    [MaxLength(100)]
    public string NameEs { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug is required.")]
    [MaxLength(100)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase letters, numbers and hyphens only.")]
    public string Slug { get; set; } = string.Empty;

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999.")]
    public int DisplayOrder { get; set; }

    [MaxLength(500)]
    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Extends the public CategoryDto returned by the API so that admin
/// pages can also access <see cref="DisplayOrder"/> and <see cref="IsActive"/>
/// without duplicating all fields.
/// </summary>
public record CategoryAdminDto(
    Guid    Id,
    string  NameEn,
    string  NameEs,
    string  Slug,
    string? Icon,
    int     DisplayOrder,
    bool    IsActive
);
