using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reservation_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    requester_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    requester_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    response_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_requests", x => x.id);
                    table.CheckConstraint("chk_reservation_dates", "end_date >= start_date");
                    table.ForeignKey(
                        name: "FK_reservation_requests_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservation_requests_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_reservations_listing_status",
                table: "reservation_requests",
                columns: new[] { "listing_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_reservations_requester",
                table: "reservation_requests",
                columns: new[] { "requester_user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_reservations_vehicle_status",
                table: "reservation_requests",
                columns: new[] { "vehicle_id", "status" });

            // /database-design: el FK desde vehicle_unavailability.reservation_id no pudo crearse
            // en la migración AddVehicleTables porque reservation_requests aún no existía. Lo
            // agregamos ahora con ON DELETE SET NULL: si una reserva se elimina por hard delete,
            // el blackout queda como "manual" en vez de orfanarse.
            migrationBuilder.Sql(@"
                ALTER TABLE vehicle_unavailability
                ADD CONSTRAINT fk_vehicle_unavailability_reservation
                FOREIGN KEY (reservation_id)
                REFERENCES reservation_requests (id)
                ON DELETE SET NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE vehicle_unavailability DROP CONSTRAINT IF EXISTS fk_vehicle_unavailability_reservation;");

            migrationBuilder.DropTable(
                name: "reservation_requests");
        }
    }
}
