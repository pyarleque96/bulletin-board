using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsGmFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_listings_ranked",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "IsGmFeatured",
                table: "listings");

            migrationBuilder.CreateIndex(
                name: "idx_listings_ranked",
                table: "listings",
                columns: new[] { "provider_hierarchy", "AvgRating", "CreatedAt" },
                descending: new bool[0],
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Approved'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_listings_ranked",
                table: "listings");

            migrationBuilder.AddColumn<bool>(
                name: "IsGmFeatured",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_listings_ranked",
                table: "listings",
                columns: new[] { "provider_hierarchy", "IsGmFeatured", "AvgRating", "CreatedAt" },
                descending: new bool[0],
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Approved'");
        }
    }
}
