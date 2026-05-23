using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManageV3BackendAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Data backfill: preservar moderación previa de fotos ---
            // Antes de soltar IsPublic, agregar status, copiar valores existentes
            // (IsPublic=true → 'Public', IsPublic=false → 'Pending') y luego dropear la columna.

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "listing_images",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql(
                "UPDATE listing_images SET status = CASE WHEN \"IsPublic\" = true THEN 'Public' ELSE 'Pending' END;");

            migrationBuilder.DropIndex(
                name: "idx_listing_images_public",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "listing_images");

            migrationBuilder.AddColumn<string>(
                name: "GmFeedback",
                table: "ratings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AltEn",
                table: "listing_images",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AltEs",
                table: "listing_images",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "listing_images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gm_feedback",
                table: "listing_images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "moderated_at",
                table: "listing_images",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "moderated_by_admin_id",
                table: "listing_images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "inquiries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ClientWhatsAppPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ClientLanguage = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MessagePreview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "New"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ContactedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inquiries_listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inquiries_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "listing_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Event = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_listing_audit_logs_listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_replies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "PendingGm"),
                    GmFeedback = table.Column<string>(type: "text", nullable: true),
                    ModeratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModeratedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_replies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_review_replies_ratings_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ratings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contact_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    InquiryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ContactedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    GmNotifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contact_logs_inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalTable: "inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_listing_images_display_order",
                table: "listing_images",
                columns: new[] { "ListingId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "idx_photos_listing_status",
                table: "listing_images",
                columns: new[] { "ListingId", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_contact_logs_inquiry",
                table: "contact_logs",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "idx_contact_logs_listing",
                table: "contact_logs",
                columns: new[] { "ListingId", "ContactedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_contact_logs_pending_notify",
                table: "contact_logs",
                column: "ContactedAt",
                filter: "\"GmNotifiedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_inquiries_listing_created",
                table: "inquiries",
                columns: new[] { "ListingId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_inquiries_provider_status",
                table: "inquiries",
                columns: new[] { "ProviderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_listing_audit_listing_occurred",
                table: "listing_audit_logs",
                columns: new[] { "ListingId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_review_replies_review_unique",
                table: "review_replies",
                column: "ReviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_review_replies_status",
                table: "review_replies",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_logs");

            migrationBuilder.DropTable(
                name: "listing_audit_logs");

            migrationBuilder.DropTable(
                name: "review_replies");

            migrationBuilder.DropTable(
                name: "inquiries");

            migrationBuilder.DropIndex(
                name: "idx_listing_images_display_order",
                table: "listing_images");

            migrationBuilder.DropIndex(
                name: "idx_photos_listing_status",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "GmFeedback",
                table: "ratings");

            migrationBuilder.DropColumn(
                name: "AltEn",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "AltEs",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "gm_feedback",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "moderated_at",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "moderated_by_admin_id",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "status",
                table: "listing_images");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "listing_images",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_listing_images_public",
                table: "listing_images",
                columns: new[] { "ListingId", "IsPublic", "DisplayOrder" });
        }
    }
}
