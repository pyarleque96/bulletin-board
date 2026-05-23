-- ============================================================
-- BULLETIN DELLS — DDL de Referencia (PostgreSQL 16)
-- Generado desde el código fuente real:
--   - Domain/Entities/*.cs
--   - Infrastructure/Persistence/Configurations/*.cs
--   - ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
--
-- NOTA: Este archivo es documentación de referencia.
-- La fuente de verdad son las migraciones EF Core (dotnet ef migrations).
-- Ejecutar migrations/update, nunca este DDL directamente en prod.
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto"; -- gen_random_uuid()

-- ============================================================
-- TABLAS DE ASP.NET IDENTITY
-- IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
-- ApplicationUser extiende IdentityUser<Guid> con:
--   locale, is_active, created_at, updated_at
-- ============================================================

-- ----------------------------------------------------------
-- USERS (ApplicationUser : IdentityUser<Guid>)
-- El rol del usuario se gestiona via user_roles (N:M con roles).
-- No hay columna "role" directa: la autorización usa Claims/Roles de Identity.
-- ----------------------------------------------------------
CREATE TABLE users (
  -- Campos de IdentityUser<Guid>
  id                    UUID          NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  user_name             VARCHAR(256),
  normalized_user_name  VARCHAR(256),
  email                 VARCHAR(256),
  normalized_email      VARCHAR(256),
  email_confirmed       BOOLEAN       NOT NULL DEFAULT FALSE,
  password_hash         TEXT,
  security_stamp        TEXT,
  concurrency_stamp     TEXT,
  phone_number          VARCHAR(256),
  phone_number_confirmed BOOLEAN      NOT NULL DEFAULT FALSE,
  two_factor_enabled    BOOLEAN       NOT NULL DEFAULT FALSE,
  lockout_end           TIMESTAMPTZ,
  lockout_enabled       BOOLEAN       NOT NULL DEFAULT FALSE,
  access_failed_count   INT           NOT NULL DEFAULT 0,
  -- Campos adicionales de ApplicationUser
  locale                VARCHAR(5)    NOT NULL DEFAULT 'en',
  is_active             BOOLEAN       NOT NULL DEFAULT TRUE,
  created_at            TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  updated_at            TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

  CONSTRAINT uq_users_normalized_email    UNIQUE (normalized_email),
  CONSTRAINT uq_users_normalized_username UNIQUE (normalized_user_name)
);

-- Índice funcional de Identity
CREATE INDEX idx_users_normalized_email    ON users(normalized_email);
CREATE INDEX idx_users_normalized_username ON users(normalized_user_name);

-- ----------------------------------------------------------
-- ROLES (ApplicationRole : IdentityRole<Guid>)
-- Roles del sistema: User, Provider, Admin
-- ----------------------------------------------------------
CREATE TABLE roles (
  id                  UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  name                VARCHAR(256),
  normalized_name     VARCHAR(256),
  concurrency_stamp   TEXT,

  CONSTRAINT uq_roles_normalized_name UNIQUE (normalized_name)
);

-- ----------------------------------------------------------
-- USER_ROLES — join table N:M entre users y roles
-- ----------------------------------------------------------
CREATE TABLE user_roles (
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,

  CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id)
);

-- ----------------------------------------------------------
-- USER_CLAIMS — claims adicionales por usuario
-- ----------------------------------------------------------
CREATE TABLE user_claims (
  id          SERIAL  PRIMARY KEY,
  user_id     UUID    NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  claim_type  TEXT,
  claim_value TEXT
);

CREATE INDEX idx_user_claims_user_id ON user_claims(user_id);

-- ----------------------------------------------------------
-- USER_LOGINS — proveedores OAuth externos (Google, etc.)
-- ----------------------------------------------------------
CREATE TABLE user_logins (
  login_provider        VARCHAR(128) NOT NULL,
  provider_key          VARCHAR(128) NOT NULL,
  provider_display_name TEXT,
  user_id               UUID         NOT NULL REFERENCES users(id) ON DELETE CASCADE,

  CONSTRAINT pk_user_logins PRIMARY KEY (login_provider, provider_key)
);

CREATE INDEX idx_user_logins_user_id ON user_logins(user_id);

-- ----------------------------------------------------------
-- USER_TOKENS — tokens de Identity (email confirmation, 2FA, etc.)
-- ----------------------------------------------------------
CREATE TABLE user_tokens (
  user_id        UUID         NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  login_provider VARCHAR(128) NOT NULL,
  name           VARCHAR(128) NOT NULL,
  value          TEXT,

  CONSTRAINT pk_user_tokens PRIMARY KEY (user_id, login_provider, name)
);

-- ----------------------------------------------------------
-- ROLE_CLAIMS — claims adicionales por rol
-- ----------------------------------------------------------
CREATE TABLE role_claims (
  id          SERIAL  PRIMARY KEY,
  role_id     UUID    NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
  claim_type  TEXT,
  claim_value TEXT
);

CREATE INDEX idx_role_claims_role_id ON role_claims(role_id);

-- ============================================================
-- TABLAS DE NEGOCIO (Bulletin Dells Domain)
-- ============================================================

-- ----------------------------------------------------------
-- PROVIDERS — extiende users 1:1
-- Invariante crítica: VIP requiere verification_status = 'Approved'
-- hierarchy: campo int que controla el orden de ranking (mayor = aparece primero).
--   El GM asigna valores altos (ej: 9999) a su propio proveedor.
-- ON DELETE CASCADE: si se elimina el user, se elimina el provider.
-- ----------------------------------------------------------
CREATE TABLE providers (
  id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id             UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  tier                VARCHAR(20) NOT NULL DEFAULT 'Regular',
  verification_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
  verified_at         TIMESTAMPTZ,
  bio                 TEXT,
  whatsapp_number     VARCHAR(30) NOT NULL,
  hierarchy           INTEGER     NOT NULL DEFAULT 0,
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),

  CONSTRAINT uq_providers_user_id UNIQUE (user_id),
  CONSTRAINT chk_provider_tier
    CHECK (tier IN ('Regular', 'Verified', 'VIP')),
  CONSTRAINT chk_provider_verification_status
    CHECK (verification_status IN ('Pending', 'Approved', 'Rejected')),
  -- Segunda línea de defensa: la BD rechaza VIP sin verificación aunque el código falle
  CONSTRAINT chk_vip_requires_verified
    CHECK (
      tier IN ('Regular', 'Verified')
      OR (tier = 'VIP' AND verification_status = 'Approved')
    )
);

CREATE INDEX idx_providers_tier_status ON providers(tier, verification_status);

-- ----------------------------------------------------------
-- CATEGORIES — predefinidas por la plataforma (nunca texto libre del proveedor)
-- Solo el Admin puede crear/editar categorías.
-- ----------------------------------------------------------
CREATE TABLE categories (
  id            UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  name_en       VARCHAR(120) NOT NULL,
  name_es       VARCHAR(120) NOT NULL,
  slug          VARCHAR(120) NOT NULL,
  display_order INT          NOT NULL DEFAULT 0,
  is_active     BOOLEAN      NOT NULL DEFAULT TRUE,
  icon_url      TEXT,

  CONSTRAINT uq_categories_slug UNIQUE (slug)
);

-- ----------------------------------------------------------
-- LISTINGS — corazón del marketplace
-- Toda edición de proveedor vuelve el status a Pending (ver trigger).
-- provider_tier está desnormalizado para performance de ranking (ver trigger sync).
-- ----------------------------------------------------------
CREATE TABLE listings (
  id               UUID           NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  provider_id      UUID           NOT NULL REFERENCES providers(id) ON DELETE RESTRICT,
  category_id      UUID           NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
  title_en         VARCHAR(255)   NOT NULL,
  title_es         VARCHAR(255)   NOT NULL,
  description_en   TEXT,
  description_es   TEXT,
  price            NUMERIC(10, 2),
  price_label_en   VARCHAR(80),
  price_label_es   VARCHAR(80),
  location         VARCHAR(255),
  whatsapp_number  VARCHAR(30),
  status           VARCHAR(20)    NOT NULL DEFAULT 'Pending',
  rejection_reason TEXT,
  provider_tier    VARCHAR(20)    NOT NULL,
  avg_rating       NUMERIC(3, 2),
  published_at     TIMESTAMPTZ,
  is_deleted       BOOLEAN        NOT NULL DEFAULT FALSE,
  created_at       TIMESTAMPTZ    NOT NULL DEFAULT NOW(),
  updated_at       TIMESTAMPTZ    NOT NULL DEFAULT NOW(),

  CONSTRAINT chk_listing_status
    CHECK (status IN ('Pending', 'Approved', 'Rejected', 'NeedsChanges')),
  CONSTRAINT chk_listing_price_positive
    CHECK (price IS NULL OR price > 0),
  -- Según la configuración real de EF Core: published_at solo puede existir si status=Approved
  CONSTRAINT chk_listing_published_at_approved
    CHECK (published_at IS NULL OR status = 'Approved'),
  CONSTRAINT chk_listing_provider_tier
    CHECK (provider_tier IN ('Regular', 'Verified', 'VIP'))
);

-- Índices de negocio — ver también indexes.sql para partial indexes
CREATE INDEX idx_listings_search
  ON listings(category_id, status, price);          -- EF Core genera este
CREATE INDEX idx_listings_tier_rating
  ON listings(provider_tier, avg_rating, created_at); -- EF Core genera este
CREATE INDEX idx_listings_is_deleted ON listings(is_deleted);
CREATE INDEX idx_listings_provider_id ON listings(provider_id);

-- ----------------------------------------------------------
-- LISTING_IMAGES — el GM decide cuáles son públicas (is_public)
-- ON DELETE CASCADE: si se borra el listing, se borran sus imágenes.
-- ----------------------------------------------------------
CREATE TABLE listing_images (
  id            UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  listing_id    UUID        NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  file_path     TEXT        NOT NULL,
  is_public     BOOLEAN     NOT NULL DEFAULT FALSE,
  display_order INT         NOT NULL DEFAULT 0,
  uploaded_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),

  CONSTRAINT uq_listing_image_order UNIQUE (listing_id, display_order)
);

CREATE INDEX idx_listing_images_public ON listing_images(listing_id, is_public);

-- ----------------------------------------------------------
-- CONTACTS — única base válida para poder emitir un rating
-- Se crea cuando un usuario inicia contacto vía WhatsApp.
-- El GM recibe email de notificación por cada contact creado.
-- ----------------------------------------------------------
CREATE TABLE contacts (
  id                UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  listing_id        UUID        NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  initiator_user_id UUID        NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
  provider_id       UUID        NOT NULL REFERENCES providers(id) ON DELETE CASCADE,
  channel           VARCHAR(20) NOT NULL DEFAULT 'WhatsApp',
  created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- EF Core genera índice compuesto (listing_id, initiator_user_id)
CREATE INDEX idx_contacts_listing_user ON contacts(listing_id, initiator_user_id);
-- EF Core genera idx_contacts_provider (proveedor_id, created_at)
CREATE INDEX idx_contacts_provider ON contacts(provider_id, created_at DESC);

-- ----------------------------------------------------------
-- RATINGS — requiere contact previo; aprobados por el GM antes de publicarse
-- contact_id UNIQUE garantiza exactamente 1 rating por interacción real.
-- ----------------------------------------------------------
CREATE TABLE ratings (
  id             UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  contact_id     UUID        NOT NULL REFERENCES contacts(id) ON DELETE CASCADE,
  listing_id     UUID        NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  provider_id    UUID        NOT NULL REFERENCES providers(id) ON DELETE CASCADE,
  author_user_id UUID        NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
  stars          SMALLINT    NOT NULL,
  comment        TEXT,
  status         VARCHAR(20) NOT NULL DEFAULT 'Pending',
  approved_at    TIMESTAMPTZ,
  approved_by    UUID        REFERENCES users(id) ON DELETE SET NULL,
  created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),

  -- 1 rating por contacto (garantizado por FK + UNIQUE)
  CONSTRAINT uq_ratings_contact UNIQUE (contact_id),
  CONSTRAINT chk_rating_stars
    CHECK (stars BETWEEN 1 AND 5),
  CONSTRAINT chk_rating_status
    CHECK (status IN ('Pending', 'Approved', 'Rejected')),
  -- Según configuración EF Core real: Approved requiere aprobador
  CONSTRAINT chk_rating_approved_has_approver
    CHECK (status != 'Approved' OR approved_by IS NOT NULL)
);

CREATE INDEX idx_ratings_listing ON ratings(listing_id, status);      -- EF Core nombrado
CREATE INDEX idx_ratings_provider_status ON ratings(provider_id, status);

-- ----------------------------------------------------------
-- VERIFICATIONS — flujo multi-step de verificación de identidad
-- overall_status se recalcula en aplicación (no es GENERATED en BD).
-- identity_document_encrypted: cifrado AES-256 a nivel de aplicación.
-- social_profiles_json: JSONB con {"instagram":"url","facebook":"url",...}
-- ----------------------------------------------------------
CREATE TABLE verifications (
  id                          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  provider_id                 UUID        NOT NULL REFERENCES providers(id) ON DELETE CASCADE,
  phone_number                VARCHAR(30),  -- cifrado AES-256 en aplicación
  phone_verified_at           TIMESTAMPTZ,
  phone_verification_token    TEXT,
  identity_document_encrypted TEXT,         -- cifrado AES-256, NUNCA plain-text
  identity_status             VARCHAR(20) NOT NULL DEFAULT 'Pending',
  social_profiles_json        TEXT,         -- JSON almacenado como text desde C# (SocialProfilesJson)
  overall_status              VARCHAR(20) NOT NULL DEFAULT 'Pending',
  created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),

  CONSTRAINT uq_verifications_provider UNIQUE (provider_id),
  CONSTRAINT chk_verification_identity_status
    CHECK (identity_status IN ('Pending', 'Approved', 'Rejected')),
  CONSTRAINT chk_verification_overall_status
    CHECK (overall_status IN ('Pending', 'Approved', 'Rejected'))
);

-- ----------------------------------------------------------
-- REPORTS — denuncia de provider o listing
-- Un report debe referenciar al menos un provider o un listing.
-- ----------------------------------------------------------
CREATE TABLE reports (
  id            UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  provider_id   UUID        REFERENCES providers(id) ON DELETE SET NULL,
  listing_id    UUID        REFERENCES listings(id) ON DELETE SET NULL,
  reported_by_id UUID       NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
  reason        TEXT        NOT NULL,
  status        VARCHAR(20) NOT NULL DEFAULT 'Pending',
  reviewed_by_id UUID       REFERENCES users(id) ON DELETE SET NULL,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),

  CONSTRAINT chk_report_status
    CHECK (status IN ('Pending', 'Reviewed', 'Dismissed')),
  CONSTRAINT chk_report_has_target
    CHECK (provider_id IS NOT NULL OR listing_id IS NOT NULL)
);

CREATE INDEX idx_reports_listing   ON reports(listing_id);
CREATE INDEX idx_reports_provider  ON reports(provider_id);
CREATE INDEX idx_reports_status    ON reports(status);

-- ----------------------------------------------------------
-- AUDIT_LOGS — append-only, retención mínima 1 año
-- actor_user_id SET NULL si el usuario se elimina (preservar historial).
-- changes_json: TEXT con JSON {"before":{...},"after":{...}}
-- ip_address: VARCHAR(45) para soportar IPv4 e IPv6
-- ----------------------------------------------------------
CREATE TABLE audit_logs (
  id            UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  actor_user_id UUID         REFERENCES users(id) ON DELETE SET NULL,
  action        VARCHAR(80)  NOT NULL,
  resource_type VARCHAR(50)  NOT NULL,
  resource_id   UUID,
  changes_json  TEXT,
  ip_address    VARCHAR(45),
  created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Nunca UPDATE ni DELETE sobre audit_logs (append-only).
CREATE INDEX idx_audit_resource ON audit_logs(resource_type, resource_id, created_at DESC);
CREATE INDEX idx_audit_actor    ON audit_logs(actor_user_id, created_at DESC);
CREATE INDEX idx_audit_created  ON audit_logs(created_at DESC);

-- ----------------------------------------------------------
-- WAIVER_ACCEPTANCES — dispara la primera vez que un usuario hace contacto
-- UNIQUE en user_id: un solo waiver por usuario.
-- ip_address: VARCHAR(45) para IPv4/IPv6
-- ----------------------------------------------------------
CREATE TABLE waiver_acceptances (
  id          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  accepted_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  ip_address  VARCHAR(45),
  user_agent  TEXT,

  CONSTRAINT uq_waiver_user UNIQUE (user_id)
);

-- ----------------------------------------------------------
-- REFRESH_TOKENS — JWT rotativo (Access ~15min / Refresh ~7d)
-- Solo se almacena el hash del token, nunca el token en plain-text.
-- ON DELETE CASCADE: si se elimina el user, se eliminan sus tokens.
-- ----------------------------------------------------------
CREATE TABLE refresh_tokens (
  id         UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id    UUID         NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash VARCHAR(255) NOT NULL,
  expires_at TIMESTAMPTZ  NOT NULL,
  revoked_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

  CONSTRAINT uq_refresh_tokens_hash UNIQUE (token_hash),
  CONSTRAINT chk_refresh_token_expires CHECK (expires_at > created_at)
);

CREATE INDEX idx_refresh_tokens_user_revoked ON refresh_tokens(user_id, revoked_at);
