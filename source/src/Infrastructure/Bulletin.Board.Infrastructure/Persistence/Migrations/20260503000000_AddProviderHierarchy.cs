using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "hierarchy",
                table: "providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_providers_hierarchy_tier",
                table: "providers",
                columns: new[] { "hierarchy", "Tier", "VerificationStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_providers_hierarchy_tier",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "hierarchy",
                table: "providers");
        }
    }
}
