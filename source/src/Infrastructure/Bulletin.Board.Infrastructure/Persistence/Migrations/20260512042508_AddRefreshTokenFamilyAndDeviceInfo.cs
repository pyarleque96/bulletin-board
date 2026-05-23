using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenFamilyAndDeviceInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_UserId_RevokedAt",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_id_revoked_at");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token_hash");

            migrationBuilder.AddColumn<string>(
                name: "DeviceInfo",
                table: "refresh_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "refresh_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacesJti",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family_id",
                table: "refresh_tokens",
                column: "FamilyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_expires_at",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_family_id",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "ReplacesJti",
                table: "refresh_tokens");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_id_revoked_at",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_UserId_RevokedAt");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_TokenHash");
        }
    }
}
