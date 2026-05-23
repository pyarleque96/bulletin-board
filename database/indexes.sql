-- ============================================================
-- BULLETIN DELLS — Índices de Performance
-- PostgreSQL 16 + EF Core (.NET 10)
--
-- SECCIÓN 1: Índices que EF Core genera automáticamente
--   via Configurations/*.cs (HasIndex en OnModelCreating).
-- SECCIÓN 2: Partial indexes críticos que EF Core NO genera.
--   Deben agregarse en la migración via MigrationBuilder.Sql().
--
-- Antes de agregar cualquier índice nuevo: EXPLAIN ANALYZE.
-- Nunca índices "por si acaso" — cada uno tiene un query que lo justifica.
-- ============================================================

-- ============================================================
-- SECCIÓN 1: ÍNDICES GENERADOS POR EF CORE
-- (documentados aquí para referencia; la migración los crea)
-- ============================================================

-- ----------------------------------------------------------
-- users (Identity — generados por IdentityDbContext)
-- ----------------------------------------------------------
-- Normalized email: lookup de login
CREATE UNIQUE INDEX IF NOT EXISTS ix_users_normalized_email
  ON users(normalized_email);

-- Normalized username: lookup de login por username
CREATE UNIQUE INDEX IF NOT EXISTS ix_users_normalized_user_name
  ON users(normalized_user_name);

-- ----------------------------------------------------------
-- providers (ProviderConfiguration)
-- ----------------------------------------------------------
-- Lookup de provider por user (1:1 garantizado en app + UNIQUE constraint)
CREATE UNIQUE INDEX IF NOT EXISTS ix_providers_user_id
  ON providers(user_id);

-- Filtrar providers por tier y estado de verificación (admin, ranking)
CREATE INDEX IF NOT EXISTS ix_providers_tier_verification
  ON providers(tier, verification_status);

-- Ranking de providers: hierarchy DESC controla el orden absoluto en todos los listados
-- El GM asigna valores altos a su proveedor; EF Core lo genera via ProviderConfiguration
CREATE INDEX IF NOT EXISTS idx_providers_hierarchy_tier
  ON providers(hierarchy DESC, tier, verification_status);

-- ----------------------------------------------------------
-- listings (ListingConfiguration)
-- ----------------------------------------------------------
-- Búsqueda por categoría + status + precio (query principal del marketplace)
CREATE INDEX IF NOT EXISTS idx_listings_search
  ON listings(category_id, status, price);

-- Ranking: tier DESC (SuperDuperVIP primero), avg_rating DESC, fecha DESC
CREATE INDEX IF NOT EXISTS idx_listings_tier_rating
  ON listings(provider_tier, avg_rating, created_at);

-- Soft-delete filter (WHERE is_deleted = false en la mayoría de queries)
CREATE INDEX IF NOT EXISTS ix_listings_is_deleted
  ON listings(is_deleted);

-- Listings por proveedor (perfil del proveedor)
CREATE INDEX IF NOT EXISTS ix_listings_provider_id
  ON listings(provider_id);

-- ----------------------------------------------------------
-- contacts (ContactConfiguration)
-- ----------------------------------------------------------
-- Verificar si usuario ya contactó un listing (gate para emitir rating)
CREATE INDEX IF NOT EXISTS ix_contacts_listing_initiator
  ON contacts(listing_id, initiator_user_id);

-- Actividad de contactos por proveedor (ranking "more requested")
CREATE INDEX IF NOT EXISTS idx_contacts_provider
  ON contacts(provider_id, created_at DESC);

-- ----------------------------------------------------------
-- ratings (RatingConfiguration)
-- ----------------------------------------------------------
-- 1 rating por contacto (UNIQUE — también constraint de negocio)
CREATE UNIQUE INDEX IF NOT EXISTS ix_ratings_contact_id
  ON ratings(contact_id);

-- Ratings de un listing por status (mostrar aprobados en detalle)
CREATE INDEX IF NOT EXISTS idx_ratings_listing
  ON ratings(listing_id, status);

-- Ratings de un provider por status (calcular avg, moderación)
CREATE INDEX IF NOT EXISTS ix_ratings_provider_status
  ON ratings(provider_id, status);

-- ----------------------------------------------------------
-- audit_logs (AuditLogConfiguration)
-- ----------------------------------------------------------
-- Historial de un recurso específico (ej: todos los eventos de un listing)
CREATE INDEX IF NOT EXISTS ix_audit_resource
  ON audit_logs(resource_type, resource_id);

-- Acciones de un actor específico (ej: todos los cambios de un admin)
CREATE INDEX IF NOT EXISTS ix_audit_actor_user_id
  ON audit_logs(actor_user_id);

-- Búsqueda temporal en auditoría
CREATE INDEX IF NOT EXISTS ix_audit_created_at
  ON audit_logs(created_at);

-- ----------------------------------------------------------
-- refresh_tokens (RefreshTokenConfiguration)
-- ----------------------------------------------------------
-- Lookup de token por hash (autenticación en cada request)
CREATE UNIQUE INDEX IF NOT EXISTS ix_refresh_tokens_token_hash
  ON refresh_tokens(token_hash);

-- Tokens activos por usuario (logout, invalidación masiva)
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_revoked
  ON refresh_tokens(user_id, revoked_at);

-- ----------------------------------------------------------
-- waiver_acceptances (WaiverAcceptanceConfiguration)
-- ----------------------------------------------------------
-- Verificar si usuario ya aceptó el waiver (check antes de primer contacto)
CREATE UNIQUE INDEX IF NOT EXISTS ix_waiver_acceptances_user_id
  ON waiver_acceptances(user_id);

-- ============================================================
-- SECCIÓN 2: PARTIAL INDEXES CRÍTICOS
-- EF Core NO genera partial indexes automáticamente.
-- Agregar en la migración via: migrationBuilder.Sql("CREATE INDEX ...")
-- Justificación: reducen tamaño del índice drásticamente ya que
-- la mayoría del tráfico opera sobre status='Approved' && is_deleted=false.
-- ============================================================

-- ----------------------------------------------------------
-- LISTINGS — queries del frontend público
-- ----------------------------------------------------------

-- Homepage y búsqueda por categoría (95%+ del tráfico)
-- Query: SELECT * FROM listings WHERE category_id=? AND status='Approved' AND is_deleted=false ORDER BY provider_tier, avg_rating DESC
CREATE INDEX IF NOT EXISTS idx_listings_search_approved
  ON listings(category_id, price)
  WHERE status = 'Approved' AND is_deleted = false;

-- Ranking completo: hierarchy DESC -> IsGmFeatured DESC -> Tier (VIP > Verified > Regular) DESC -> avg_rating DESC -> fecha DESC
-- El campo hierarchy en providers controla el orden absoluto; este indice en listings cubre tier y rating
-- Query: SELECT * FROM listings JOIN providers ON ... WHERE status='Approved' AND is_deleted=false ORDER BY providers.hierarchy DESC, provider_tier DESC, avg_rating DESC NULLS LAST, created_at DESC
CREATE INDEX IF NOT EXISTS idx_listings_tier_rating_approved
  ON listings(provider_tier, avg_rating DESC NULLS LAST, created_at DESC)
  WHERE status = 'Approved' AND is_deleted = false;

-- Sección "Nuevos listings" en el homepage
-- Query: SELECT * FROM listings WHERE status='Approved' AND is_deleted=false ORDER BY created_at DESC LIMIT 10
CREATE INDEX IF NOT EXISTS idx_listings_new_approved
  ON listings(created_at DESC)
  WHERE status = 'Approved' AND is_deleted = false;

-- Moderación: queue del GM para listings pendientes
-- Query: SELECT * FROM listings WHERE status='Pending' AND is_deleted=false ORDER BY created_at ASC
CREATE INDEX IF NOT EXISTS idx_listings_pending_moderation
  ON listings(created_at ASC)
  WHERE status = 'Pending' AND is_deleted = false;

-- Perfil del proveedor: sus listings activos
-- Query: SELECT * FROM listings WHERE provider_id=? AND is_deleted=false ORDER BY status, created_at DESC
CREATE INDEX IF NOT EXISTS idx_listings_provider_active
  ON listings(provider_id, status, created_at DESC)
  WHERE is_deleted = false;

-- ----------------------------------------------------------
-- RATINGS — queries del frontend público y moderación
-- ----------------------------------------------------------

-- Ratings aprobados de un listing (mostrar en página de detalle)
-- Query: SELECT * FROM ratings WHERE listing_id=? AND status='Approved' ORDER BY created_at DESC
CREATE INDEX IF NOT EXISTS idx_ratings_approved_by_listing
  ON ratings(listing_id, created_at DESC)
  WHERE status = 'Approved';

-- Queue de moderación del GM: ratings pendientes
-- Query: SELECT * FROM ratings WHERE status='Pending' ORDER BY created_at ASC
CREATE INDEX IF NOT EXISTS idx_ratings_pending_moderation
  ON ratings(created_at ASC)
  WHERE status = 'Pending';

-- ----------------------------------------------------------
-- REPORTS — moderación del GM
-- ----------------------------------------------------------

-- Queue de reportes pendientes para el GM
-- Query: SELECT * FROM reports WHERE status='Pending' ORDER BY created_at ASC
CREATE INDEX IF NOT EXISTS idx_reports_pending_moderation
  ON reports(created_at ASC)
  WHERE status = 'Pending';

-- ----------------------------------------------------------
-- REFRESH_TOKENS — autenticación
-- ----------------------------------------------------------

-- Tokens activos por usuario (revocación masiva en logout, cambio de password)
-- Query: SELECT * FROM refresh_tokens WHERE user_id=? AND revoked_at IS NULL AND expires_at > now()
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_active_by_user
  ON refresh_tokens(user_id, expires_at)
  WHERE revoked_at IS NULL;

-- ----------------------------------------------------------
-- AUDIT_LOGS — queries de compliance
-- ----------------------------------------------------------

-- Historial completo de un recurso ordenado por tiempo (ej: auditoría de un listing)
-- Query: SELECT * FROM audit_logs WHERE resource_type=? AND resource_id=? ORDER BY created_at DESC
CREATE INDEX IF NOT EXISTS idx_audit_resource_timeline
  ON audit_logs(resource_type, resource_id, created_at DESC);

-- Acciones de un actor con timeline (ej: historial del admin en el día)
-- Query: SELECT * FROM audit_logs WHERE actor_user_id=? ORDER BY created_at DESC
CREATE INDEX IF NOT EXISTS idx_audit_actor_timeline
  ON audit_logs(actor_user_id, created_at DESC);
