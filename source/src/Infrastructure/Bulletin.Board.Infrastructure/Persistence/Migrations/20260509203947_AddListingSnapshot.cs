using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListingSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "published_snapshot",
                table: "listings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "published_snapshot_version",
                table: "listings",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "published_snapshot",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "published_snapshot_version",
                table: "listings");
        }
    }
}
