# Bulletin Dells

Marketplace local bilingüe (inglés/español) para Wisconsin Dells. Inspirado en la lógica de descubrimiento de *Yellow Pages* y la experiencia de navegación visual de *Amazon*.

> *A bilingual, moderated, trust-first local marketplace for Wisconsin Dells with Amazon-style browsing and Yellow Pages-style discoverability.*

---

## Propósito

Conectar clientes con proveedores reales de servicios locales (housing, transporte, mecánicos, trabajos, belleza, rentas de autos, rides al aeropuerto, etc.) en un ecosistema **moderado**, con listings aprobados manualmente, proveedores verificables, visibilidad VIP monetizada y sistema de ratings anti-manipulación.

**Audiencia:** residentes locales, estudiantes J1, trabajadores de temporada, visitantes y pequeños negocios.

## Reglas de producto no-negociables

1. Ningún listing se publica sin aprobación del GM.
2. La primera interacción significativa dispara un waiver/disclaimer legal.
3. Nadie es VIP sin estar verificado (teléfono + ID/pasaporte + redes sociales).
4. Proveedores suben fotos; el GM decide cuáles son públicas.
5. Cualquier edición del proveedor regresa el listing a `Pending`.
6. Los listings no expiran automáticamente.
7. Ratings solo de usuarios que realmente contactaron al proveedor vía la plataforma, revisados por el GM.
8. Contacto cliente↔proveedor por WhatsApp; el GM recibe email con la interacción.
9. Categorías predefinidas; nunca texto libre del proveedor.

## Jerarquía de ranking

Orden canónico en todos los listados:

1. `Hierarchy DESC` — boost numérico asignado por el GM.
2. `ProviderTier DESC` — VIP (2) > Verified (1) > Regular (0).
3. `AvgRating DESC`.
4. `CreatedAt DESC`.
5. `Id DESC` (desempate estable para keyset pagination).

## Stack técnico

| Capa | Tecnología |
|------|-----------|
| Backend / API | .NET 10, Clean Architecture, SOLID, DDD |
| Frontend | Blazor |
| Base de datos | PostgreSQL + EF Core (migraciones) |
| Styling | Tailwind CSS |
| i18n | Inglés y español (first-class desde día uno) |
| Integraciones | WhatsApp (wa.me deep links), email transaccional |

## Estructura del repositorio

```
Bulletin Board/
├── source/        # código .NET 10 / Blazor / EF Core (Clean Architecture)
├── database/      # modelo de datos, ER, scripts SQL de referencia
├── design/        # mockups, wireframes, identidad visual
├── docs/          # documentación canónica (blueprint del producto)
├── assets/        # imágenes y logos
└── research/      # benchmarking y notas de UX
```

## Tiers de proveedor

`Regular` → `Verified` → `VIP`

El campo `hierarchy` (int) es el mecanismo de ranking absoluto. Valores más altos aparecen primero, independientemente del tier. Administrado vía `PUT /api/v1/providers/{id}/hierarchy` (Admin only).

## Estados del listing

`Pending` → `Approved` | `Rejected` | `Needs Changes`

Toda edición vuelve a `Pending`. Rechazos y *Needs Changes* requieren feedback al proveedor.

## Endpoints destacados (estado actual)

- `GET /api/v1/listings/top?count=N` — Top listings ranqueado por `trending_score` (recalc cada 1h por background job).
- `POST /api/v1/listings/{id}/view` — registra vista pública (throttle 5min por sesión).
- `POST /api/v1/admin/trending/recompute` — fuerza recálculo on-demand (Admin only).
- `/api/v1/manage/listings` — cursor pagination para infinity scroll del panel admin.

## Roles del equipo

- **UI/UX Designer** — entrega HTML/CSS (Tailwind)/JS listos para Blazor.
- **Frontend (Blazor)** — componentes, estado, integración con APIs.
- **Backend (.NET 10)** — APIs, lógica de negocio, Clean Architecture.
- **Database Owner** — modelo de datos, queries, migraciones EF Core.
- **Security Engineer** — authn/authz, permisos, hardening.
- **DevOps** — CI/CD, infraestructura, monitoreo.

## Idioma

Documentación interna en español; UI bilingüe EN/ES desde el inicio (no traducción posterior).
