using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropVehiclePassengerMin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El check anterior referenciaba passenger_min, así que se reemplaza por completo.
            migrationBuilder.Sql(
                "ALTER TABLE vehicles DROP CONSTRAINT IF EXISTS chk_vehicle_passenger_range;");

            migrationBuilder.DropColumn(
                name: "passenger_min",
                table: "vehicles");

            migrationBuilder.AddCheckConstraint(
                name: "chk_vehicle_passenger_range",
                table: "vehicles",
                sql: "passenger_max >= 1 AND passenger_max <= 50");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE vehicles DROP CONSTRAINT IF EXISTS chk_vehicle_passenger_range;");

            migrationBuilder.AddColumn<int>(
                name: "passenger_min",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "chk_vehicle_passenger_range",
                table: "vehicles",
                sql: "passenger_min >= 1 AND passenger_max >= passenger_min AND passenger_max <= 50");
        }
    }
}
