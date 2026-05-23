-- ============================================================
-- BULLETIN DELLS — Constraints y Triggers de Negocio
-- PostgreSQL 16 + EF Core (.NET 10)
--
-- Estos constraints son la SEGUNDA LÍNEA DE DEFENSA.
-- La primera es el dominio C# (Entities/*.cs).
-- La tercera son los tests de integración.
--
-- Los CHECK constraints de esta sección van en las migraciones EF Core
-- via: table.HasCheckConstraint("name", "expression")
-- Los triggers van en: migrationBuilder.Sql("CREATE TRIGGER ...")
-- ============================================================

-- ============================================================
-- CHECK CONSTRAINTS DE NEGOCIO
-- (Algunos ya están en schema.sql; aquí están como ALTER TABLE
--  para aplicar sobre una BD existente si es necesario)
-- ============================================================

-- ----------------------------------------------------------
-- PROVIDERS
-- Invariante más crítica: nadie es VIP sin estar Verified.
-- EF Core: ProviderConfiguration.HasCheckConstraint("chk_vip_requires_verified", ...)
-- ----------------------------------------------------------
ALTER TABLE providers
  ADD CONSTRAINT IF NOT EXISTS chk_provider_tier
    CHECK (tier IN ('Regular', 'Verified', 'VIP'));

ALTER TABLE providers
  ADD CONSTRAINT IF NOT EXISTS chk_provider_verification_status
    CHECK (verification_status IN ('Pending', 'Approved', 'Rejected'));

-- Segunda línea de defensa: la BD rechaza VIP sin verificación aunque el código falle.
-- Primera línea: Provider.ApproveTier() lanza InvalidOperationException.
ALTER TABLE providers
  ADD CONSTRAINT IF NOT EXISTS chk_vip_requires_verified
    CHECK (
      tier IN ('Regular', 'Verified')
      OR (tier = 'VIP' AND verification_status = 'Approved')
    );

-- ----------------------------------------------------------
-- LISTINGS
-- EF Core: ListingConfiguration tiene chk_listing_price_positive
--          y chk_listing_published_at_approved
-- ----------------------------------------------------------
ALTER TABLE listings
  ADD CONSTRAINT IF NOT EXISTS chk_listing_status
    CHECK (status IN ('Pending', 'Approved', 'Rejected', 'NeedsChanges'));

ALTER TABLE listings
  ADD CONSTRAINT IF NOT EXISTS chk_listing_price_positive
    CHECK (price IS NULL OR price > 0);

-- published_at solo puede existir si status = 'Approved'.
-- Nota: la versión en EF Core es "published_at IS NULL OR status = 'Approved'"
-- que es ligeramente más permisiva (permite Approved sin published_at durante la transición).
-- El dominio Listing.Approve() siempre fija published_at = UtcNow.
ALTER TABLE listings
  ADD CONSTRAINT IF NOT EXISTS chk_listing_published_at_approved
    CHECK (published_at IS NULL OR status = 'Approved');

ALTER TABLE listings
  ADD CONSTRAINT IF NOT EXISTS chk_listing_provider_tier
    CHECK (provider_tier IN ('Regular', 'Verified', 'VIP'));

-- ----------------------------------------------------------
-- RATINGS
-- EF Core: RatingConfiguration tiene chk_rating_stars y chk_rating_approved_has_approver
-- ----------------------------------------------------------
ALTER TABLE ratings
  ADD CONSTRAINT IF NOT EXISTS chk_rating_stars
    CHECK (stars BETWEEN 1 AND 5);

ALTER TABLE ratings
  ADD CONSTRAINT IF NOT EXISTS chk_rating_status
    CHECK (status IN ('Pending', 'Approved', 'Rejected'));

-- Si el rating es Approved, debe tener aprobador registrado.
-- Nota: la versión en EF Core es "status != 'Approved' OR approved_by IS NOT NULL"
-- El dominio Rating.Approve() siempre fija approved_at y approved_by.
ALTER TABLE ratings
  ADD CONSTRAINT IF NOT EXISTS chk_rating_approved_has_approver
    CHECK (status != 'Approved' OR approved_by IS NOT NULL);

-- ----------------------------------------------------------
-- VERIFICATIONS
-- (No hay configuración EF Core separada — convención de nombres)
-- ----------------------------------------------------------
ALTER TABLE verifications
  ADD CONSTRAINT IF NOT EXISTS chk_verification_identity_status
    CHECK (identity_status IN ('Pending', 'Approved', 'Rejected'));

ALTER TABLE verifications
  ADD CONSTRAINT IF NOT EXISTS chk_verification_overall_status
    CHECK (overall_status IN ('Pending', 'Approved', 'Rejected'));

-- ----------------------------------------------------------
-- REPORTS
-- ----------------------------------------------------------
ALTER TABLE reports
  ADD CONSTRAINT IF NOT EXISTS chk_report_status
    CHECK (status IN ('Pending', 'Reviewed', 'Dismissed'));

-- Un report debe referenciar al menos un provider o un listing.
-- Primera línea: Report.Create() lanza ArgumentException si ambos son null.
ALTER TABLE reports
  ADD CONSTRAINT IF NOT EXISTS chk_report_has_target
    CHECK (provider_id IS NOT NULL OR listing_id IS NOT NULL);

-- ----------------------------------------------------------
-- REFRESH_TOKENS
-- EF Core: RefreshTokenConfiguration tiene chk_refresh_token_expires
-- ----------------------------------------------------------
ALTER TABLE refresh_tokens
  ADD CONSTRAINT IF NOT EXISTS chk_refresh_token_expires
    CHECK (expires_at > created_at);

-- ============================================================
-- TRIGGERS DE NEGOCIO
-- Agregar en migración via migrationBuilder.Sql(...)
-- ============================================================

-- ----------------------------------------------------------
-- FUNCIÓN: set_updated_at
-- Actualiza automáticamente la columna updated_at en cualquier UPDATE.
-- Usada por múltiples triggers.
-- ----------------------------------------------------------
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at := now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger en users
CREATE OR REPLACE TRIGGER trg_users_updated_at
  BEFORE UPDATE ON users
  FOR EACH ROW
  EXECUTE FUNCTION set_updated_at();

-- Trigger en listings
CREATE OR REPLACE TRIGGER trg_listings_updated_at
  BEFORE UPDATE ON listings
  FOR EACH ROW
  EXECUTE FUNCTION set_updated_at();

-- Trigger en verifications
CREATE OR REPLACE TRIGGER trg_verifications_updated_at
  BEFORE UPDATE ON verifications
  FOR EACH ROW
  EXECUTE FUNCTION set_updated_at();

-- ----------------------------------------------------------
-- FUNCIÓN: sync_listing_provider_tier
-- Mantiene listings.provider_tier sincronizado con providers.tier.
-- Requerido porque provider_tier está desnormalizado en listings para
-- performance de ranking. Sin este trigger el campo se desincronizaría.
-- Impacto: UPDATE en providers donde tier cambia → UPDATE en listings.
-- ----------------------------------------------------------
CREATE OR REPLACE FUNCTION sync_listing_provider_tier()
RETURNS TRIGGER AS $$
BEGIN
  IF NEW.tier IS DISTINCT FROM OLD.tier THEN
    UPDATE listings
    SET    provider_tier = NEW.tier,
           updated_at    = now()
    WHERE  provider_id = NEW.id
      AND  is_deleted  = false;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_sync_listing_provider_tier
  AFTER UPDATE ON providers
  FOR EACH ROW
  EXECUTE FUNCTION sync_listing_provider_tier();

-- ----------------------------------------------------------
-- FUNCIÓN: update_listing_avg_rating
-- Recalcula listings.avg_rating al insertar o actualizar un rating.
-- avg_rating está desnormalizado para evitar AVG() en cada request.
-- Solo considera ratings con status = 'Approved'.
-- Impacto: INSERT o UPDATE en ratings → UPDATE en listings.
-- ----------------------------------------------------------
CREATE OR REPLACE FUNCTION update_listing_avg_rating()
RETURNS TRIGGER AS $$
DECLARE
  v_listing_id UUID;
BEGIN
  -- Determinar el listing_id afectado
  v_listing_id := COALESCE(NEW.listing_id, OLD.listing_id);

  UPDATE listings
  SET avg_rating = (
    SELECT ROUND(AVG(stars)::numeric, 2)
    FROM   ratings
    WHERE  listing_id = v_listing_id
      AND  status     = 'Approved'
  ),
  updated_at = now()
  WHERE id = v_listing_id;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_update_avg_rating
  AFTER INSERT OR UPDATE OF status ON ratings
  FOR EACH ROW
  EXECUTE FUNCTION update_listing_avg_rating();

-- ----------------------------------------------------------
-- FUNCIÓN: listing_edit_resets_to_pending
-- Invariante: toda edición de contenido por el proveedor vuelve el
-- listing a status = 'Pending' y limpia published_at.
-- Solo se dispara cuando cambian campos de contenido editables
-- (no cuando el GM cambia status, rejection_reason, provider_tier, etc.).
-- Condición: campos de contenido cambiaron Y el status no fue modificado en
-- este mismo UPDATE (para no interferir con aprobaciones/rechazos del GM).
-- ----------------------------------------------------------
CREATE OR REPLACE FUNCTION listing_edit_resets_to_pending()
RETURNS TRIGGER AS $$
BEGIN
  IF (
    NEW.title_en         IS DISTINCT FROM OLD.title_en         OR
    NEW.title_es         IS DISTINCT FROM OLD.title_es         OR
    NEW.description_en   IS DISTINCT FROM OLD.description_en   OR
    NEW.description_es   IS DISTINCT FROM OLD.description_es   OR
    NEW.price            IS DISTINCT FROM OLD.price            OR
    NEW.price_label_en   IS DISTINCT FROM OLD.price_label_en   OR
    NEW.price_label_es   IS DISTINCT FROM OLD.price_label_es   OR
    NEW.location         IS DISTINCT FROM OLD.location         OR
    NEW.whatsapp_number  IS DISTINCT FROM OLD.whatsapp_number
  )
  -- Solo resetear si el GM no está cambiando el status explícitamente
  AND NEW.status = OLD.status
  THEN
    NEW.status       := 'Pending';
    NEW.published_at := NULL;
    NEW.updated_at   := now();
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_listing_edit_pending
  BEFORE UPDATE ON listings
  FOR EACH ROW
  EXECUTE FUNCTION listing_edit_resets_to_pending();

-- ----------------------------------------------------------
-- FUNCIÓN: audit_sensitive_access
-- Registra automáticamente en audit_logs cada lectura de datos sensibles.
-- Nota: PostgreSQL no tiene triggers en SELECT directamente; esta función
-- debe llamarse explícitamente desde la capa de aplicación (EncryptionService)
-- o desde una función wrapper que encapsule el acceso a identity_document_encrypted.
-- Se incluye aquí como documentación de intención — la implementación
-- está en Infrastructure/Services/EncryptionService.cs.
-- ----------------------------------------------------------

-- Placeholder: ver EncryptionService.cs para la implementación.
-- La auditoría de acceso a datos sensibles ocurre en:
--   Infrastructure/Services/EncryptionService.cs → cada llamada a Decrypt()
--   registra un AuditLog via IAuditLogRepository.

-- ============================================================
-- POLÍTICAS DE RETENCIÓN (para ejecutar en jobs de mantenimiento)
-- ============================================================

-- audit_logs: retención mínima 1 año
-- Ejecutar con un job periódico (pg_cron o aplicación):
-- DELETE FROM audit_logs WHERE created_at < now() - INTERVAL '1 year';

-- refresh_tokens: limpiar tokens expirados y revocados
-- DELETE FROM refresh_tokens
-- WHERE (expires_at < now() OR revoked_at IS NOT NULL)
--   AND created_at < now() - INTERVAL '30 days';
