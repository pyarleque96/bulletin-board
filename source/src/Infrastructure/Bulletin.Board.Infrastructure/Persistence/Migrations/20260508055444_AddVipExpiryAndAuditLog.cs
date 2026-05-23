using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVipExpiryAndAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "vip_is_indefinite",
                table: "providers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "vip_until",
                table: "providers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "vip_is_indefinite",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "vip_until",
                table: "listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vip_change_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EntityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousVipUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PreviousIsIndefinite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NewVipUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NewIsIndefinite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vip_change_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vip_change_logs_users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_vip_logs_actor",
                table: "vip_change_logs",
                columns: new[] { "ChangedByUserId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_vip_logs_entity",
                table: "vip_change_logs",
                columns: new[] { "EntityType", "EntityId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vip_change_logs");

            migrationBuilder.DropColumn(
                name: "vip_is_indefinite",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "vip_until",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "vip_is_indefinite",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "vip_until",
                table: "listings");
        }
    }
}
