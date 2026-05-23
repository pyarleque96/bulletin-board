using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulletin.Board.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDbSchemaImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // /postgres-best-practices: pg_trgm requerido para índices GIN trigram sobre texto
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // /postgres-best-practices: unaccent() es STABLE; PG no la acepta en índices. Wrapper IMMUTABLE es práctica estándar.
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION immutable_unaccent(text) RETURNS text
                  LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT AS
                  $$ SELECT public.unaccent('public.unaccent', $1) $$;
            ");

            migrationBuilder.DropIndex(
                name: "idx_listings_featured_tier_rating",
                table: "listings");

            migrationBuilder.DropIndex(
                name: "idx_listings_search",
                table: "listings");

            migrationBuilder.DropIndex(
                name: "IX_listings_IsDeleted",
                table: "listings");

            migrationBuilder.AddColumn<int>(
                name: "provider_hierarchy",
                table: "listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: sincroniza provider_hierarchy desde providers.hierarchy antes del índice ranked
            migrationBuilder.Sql(@"
                UPDATE listings l
                SET provider_hierarchy = p.hierarchy
                FROM providers p
                WHERE p.""Id"" = l.""ProviderId"";
            ");

            // /postgres-best-practices: cast text -> jsonb requiere USING; EF no lo emite.
            // Sql() reemplaza el AlterColumn autogenerado, validando JSON existente al pasar.
            migrationBuilder.Sql(@"
                ALTER TABLE audit_logs
                ALTER COLUMN ""ChangesJson"" TYPE jsonb
                USING ""ChangesJson""::jsonb;
            ");

            migrationBuilder.AddCheckConstraint(
                name: "chk_user_phone_format",
                table: "users",
                sql: "\"PhoneNumber\" IS NULL OR length(\"PhoneNumber\") <= 30");

            migrationBuilder.CreateIndex(
                name: "idx_ratings_approved_by",
                table: "ratings",
                column: "ApprovedBy",
                filter: "\"ApprovedBy\" IS NOT NULL");

            // /database-design: filas VIP existentes (seed previo) no tenían vip_until ni vip_is_indefinite
            // declarados — antes de imponer el CHECK, normalizamos a "VIP indefinido" como interpretación
            // más permisiva. Si un provider tenía vip_until válido lo respetamos.
            migrationBuilder.Sql(@"
                UPDATE providers
                SET vip_is_indefinite = true
                WHERE ""Tier"" = 'VIP'
                  AND vip_until IS NULL
                  AND vip_is_indefinite = false;
            ");

            migrationBuilder.AddCheckConstraint(
                name: "chk_provider_vip_consistency",
                table: "providers",
                sql: "\"Tier\" != 'VIP' OR (vip_is_indefinite = true AND vip_until IS NULL) OR (vip_is_indefinite = false AND vip_until IS NOT NULL)");

            // Triggers: mantienen provider_hierarchy y ProviderTier en listings sincronizados con providers
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION sync_listing_provider_hierarchy()
                RETURNS TRIGGER LANGUAGE plpgsql AS $$
                BEGIN
                  IF NEW.hierarchy IS DISTINCT FROM OLD.hierarchy THEN
                    UPDATE listings SET provider_hierarchy = NEW.hierarchy
                    WHERE ""ProviderId"" = NEW.""Id"";
                  END IF;
                  RETURN NEW;
                END;
                $$;

                CREATE TRIGGER trg_provider_hierarchy_changed
                  AFTER UPDATE OF hierarchy ON providers
                  FOR EACH ROW EXECUTE FUNCTION sync_listing_provider_hierarchy();
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION sync_listing_provider_tier()
                RETURNS TRIGGER LANGUAGE plpgsql AS $$
                BEGIN
                  IF NEW.""Tier"" IS DISTINCT FROM OLD.""Tier"" THEN
                    UPDATE listings SET ""ProviderTier"" = NEW.""Tier""
                    WHERE ""ProviderId"" = NEW.""Id"";
                  END IF;
                  RETURN NEW;
                END;
                $$;

                CREATE TRIGGER trg_provider_tier_changed
                  AFTER UPDATE OF ""Tier"" ON providers
                  FOR EACH ROW EXECUTE FUNCTION sync_listing_provider_tier();
            ");

            migrationBuilder.CreateIndex(
                name: "idx_listings_pending",
                table: "listings",
                column: "CreatedAt",
                filter: "\"Status\" = 'Pending' AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "idx_listings_ranked",
                table: "listings",
                columns: new[] { "provider_hierarchy", "IsGmFeatured", "AvgRating", "CreatedAt" },
                descending: new bool[0],
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "idx_listings_search",
                table: "listings",
                columns: new[] { "CategoryId", "Price" },
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Approved'");

            // /database-design: misma normalización que providers — listings VIP heredan indefinido.
            migrationBuilder.Sql(@"
                UPDATE listings
                SET vip_is_indefinite = true
                WHERE ""ProviderTier"" = 'VIP'
                  AND vip_until IS NULL
                  AND vip_is_indefinite = false;
            ");

            migrationBuilder.AddCheckConstraint(
                name: "chk_listing_vip_consistency",
                table: "listings",
                sql: "\"ProviderTier\" != 'VIP' OR (vip_is_indefinite = true AND vip_until IS NULL) OR (vip_is_indefinite = false AND vip_until IS NOT NULL)");

            // Índices GIN trigram: EF no puede expresar índices con función; se agregan manualmente
            migrationBuilder.Sql(@"
                CREATE INDEX idx_listings_title_en_trgm ON listings
                  USING GIN (immutable_unaccent(""TitleEn"") gin_trgm_ops)
                  WHERE ""IsDeleted"" = false AND ""Status"" = 'Approved';
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX idx_listings_title_es_trgm ON listings
                  USING GIN (immutable_unaccent(""TitleEs"") gin_trgm_ops)
                  WHERE ""IsDeleted"" = false AND ""Status"" = 'Approved';
            ");

            // Índice GIN sobre SocialProfilesJson: EF no lo scaffoldó, se agrega manual
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_verifications_social_profiles
                  ON verifications USING GIN (""SocialProfilesJson"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Orden inverso: primero objetos que dependen de la columna/función, luego la columna, luego la función
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_verifications_social_profiles;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_listings_title_es_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_listings_title_en_trgm;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_provider_tier_changed ON providers;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS sync_listing_provider_tier();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_provider_hierarchy_changed ON providers;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS sync_listing_provider_hierarchy();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS immutable_unaccent(text);");
            // pg_trgm se deja instalada — otras migraciones futuras pueden depender de ella

            migrationBuilder.DropCheckConstraint(
                name: "chk_user_phone_format",
                table: "users");

            migrationBuilder.DropIndex(
                name: "idx_ratings_approved_by",
                table: "ratings");

            migrationBuilder.DropCheckConstraint(
                name: "chk_provider_vip_consistency",
                table: "providers");

            migrationBuilder.DropIndex(
                name: "idx_listings_pending",
                table: "listings");

            migrationBuilder.DropIndex(
                name: "idx_listings_ranked",
                table: "listings");

            migrationBuilder.DropIndex(
                name: "idx_listings_search",
                table: "listings");

            migrationBuilder.DropCheckConstraint(
                name: "chk_listing_vip_consistency",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "provider_hierarchy",
                table: "listings");

            migrationBuilder.Sql(@"
                ALTER TABLE audit_logs
                ALTER COLUMN ""ChangesJson"" TYPE text
                USING ""ChangesJson""::text;
            ");

            migrationBuilder.CreateIndex(
                name: "idx_listings_featured_tier_rating",
                table: "listings",
                columns: new[] { "IsGmFeatured", "ProviderTier", "AvgRating" });

            migrationBuilder.CreateIndex(
                name: "idx_listings_search",
                table: "listings",
                columns: new[] { "CategoryId", "Status", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_listings_IsDeleted",
                table: "listings",
                column: "IsDeleted");
        }
    }
}
