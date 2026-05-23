using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListingTrendingScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "score_updated_at",
                table: "listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "trending_score",
                table: "listings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "idx_listings_trending",
                table: "listings",
                column: "trending_score",
                descending: new bool[0],
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Approved' AND is_paused_by_owner = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_listings_trending",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "score_updated_at",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "trending_score",
                table: "listings");
        }
    }
}
