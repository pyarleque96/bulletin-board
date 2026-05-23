using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // /postgres-best-practices: btree_gist habilita el operador GiST sobre tipos como uuid,
            // requerido por el EXCLUDE constraint de vehicle_unavailability más abajo.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    passenger_min = table.Column<int>(type: "integer", nullable: false),
                    passenger_max = table.Column<int>(type: "integer", nullable: false),
                    transmission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    has_air_conditioning = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    daily_rate_cents = table.Column<int>(type: "integer", nullable: false),
                    weekly_rate_cents = table.Column<int>(type: "integer", nullable: true),
                    monthly_rate_cents = table.Column<int>(type: "integer", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                    table.CheckConstraint("chk_vehicle_daily_rate_positive", "daily_rate_cents > 0");
                    table.CheckConstraint("chk_vehicle_monthly_rate_positive", "monthly_rate_cents IS NULL OR monthly_rate_cents > 0");
                    table.CheckConstraint("chk_vehicle_passenger_range", "passenger_min >= 1 AND passenger_max >= passenger_min AND passenger_max <= 50");
                    table.CheckConstraint("chk_vehicle_weekly_rate_positive", "weekly_rate_cents IS NULL OR weekly_rate_cents > 0");
                    table.ForeignKey(
                        name: "FK_vehicles_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicle_photos_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_unavailability",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_unavailability", x => x.id);
                    table.CheckConstraint("chk_vehicle_unavailability_dates", "end_date >= start_date");
                    table.ForeignKey(
                        name: "FK_vehicle_unavailability_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_photos_public",
                table: "vehicle_photos",
                columns: new[] { "vehicle_id", "is_public", "display_order" });

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_unavailability_range",
                table: "vehicle_unavailability",
                columns: new[] { "vehicle_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "idx_vehicle_unavailability_reservation",
                table: "vehicle_unavailability",
                column: "reservation_id",
                filter: "reservation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_listing_active",
                table: "vehicles",
                columns: new[] { "listing_id", "is_active", "display_order" });

            // /database-design: EXCLUDE constraint impide solapamientos de blackouts por vehicle
            // a nivel storage — dos requests concurrentes nunca pueden crear rangos pisándose.
            // Usa daterange con bounds inclusivos '[]' para alinear con la semántica del dominio
            // (StartDate y EndDate inclusivos, validado por VehicleUnavailability.Covers).
            migrationBuilder.Sql(@"
                ALTER TABLE vehicle_unavailability
                ADD CONSTRAINT excl_vehicle_unavailability_no_overlap
                EXCLUDE USING gist (
                    vehicle_id WITH =,
                    daterange(start_date, end_date, '[]') WITH &&
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE vehicle_unavailability DROP CONSTRAINT IF EXISTS excl_vehicle_unavailability_no_overlap;");

            migrationBuilder.DropTable(
                name: "vehicle_photos");

            migrationBuilder.DropTable(
                name: "vehicle_unavailability");

            migrationBuilder.DropTable(
                name: "vehicles");

            // btree_gist se deja instalada — otras migraciones futuras pueden depender.
        }
    }
}
