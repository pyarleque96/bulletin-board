# Backend Audit — /manage v3 (Fase A → backend para Fases B/C/D)

> **Audiencia:** sr-net-developer (.NET 10 / Clean Architecture).
> **Frontend status:** Fase A completa (ManageDetailHeader con matriz canónica de tabs, infinity scroll en /manage, componentes shared de Manage). Cualquier dependencia listada aquí es lo que el frontend invocará en las Fases B/C/D al implementar los 27 mockups con datos reales.
> **Reglas obligatorias del producto:** #1, #4, #5, #7, #8, #9 (ver `CLAUDE.md`). Si un endpoint no las respeta, no se acepta.

---

## 0. Cambios de contrato global

### 0.1 Cursor pagination en `GET /api/v1/manage/listings`

El index `/manage` ya consume **page+pageSize** pero se le va a montar **infinity scroll**. Para evitar pulls duplicados / saltos al insertar listings nuevos, migrar a **cursor-based pagination**.

- **Cursor opaco base64** del par `{ListedAt}_{Id}` (`ListedAt` = `Provider.CreatedAt` si el listing aún no se aprueba, si no `Listing.ApprovedAt`).
- Ordenamiento canónico (el mismo que se usa en `/browse`, sin excepciones — el frontend cuenta con que listings recién aprobados con `Hierarchy` alto suban al top):

```
Hierarchy DESC, IsGmFeatured DESC, ProviderTier DESC, AvgRating DESC, CreatedAt DESC
```

- **Request:**
  ```
  GET /api/v1/manage/listings
    ?cursor={opaque}          // omitido = primera página
    &pageSize=20              // máx. 50
    &status=Pending|Approved|Rejected|NeedsChanges|
    &category={slug}|
    &search={q}
  ```
- **Response:**
  ```json
  {
    "items": [ /* MyListingDto[] */ ],
    "nextCursor": "eyJsaXN0ZWRBdC...",   // null si no hay más
    "hasMore": true,
    "totalItems": 47                       // opcional pero recomendado para "Showing X of Y"
  }
  ```
- **Compatibilidad:** mantener `page`+`pageSize` como deprecated durante 1 sprint para no romper el index actual.
- **Fallback:** si `cursor` es inválido o expira (>24h), responder 400 con `MessageKey="Api_Error_InvalidCursor"`; el frontend reseteará a primera página.

### 0.2 Recordatorio de reglas que afectan TODOS los endpoints

| Regla | Impacto en API |
|---|---|
| #4 — GM decide qué fotos son públicas | Cualquier endpoint que devuelva fotos al público filtra por `Photo.Status == Public`. En `/manage/{id}/photos` el provider ve `Public/Pending/Hidden/Rejected`. |
| #5 — Edición vuelve a Pending | Todo `PUT/PATCH` sobre listing/inventario/servicio del provider debe poner `Listing.Status = Pending` y registrar `ListingAuditLog`. |
| #7 — Reviews + ProviderReply moderadas por GM | `POST /reviews` y `POST /reviews/{id}/reply` quedan en `Status=PendingGm`. Solo el GM puede `PATCH /admin/reviews/{id}/approve|reject`. |
| #8 — Inquiries via WhatsApp + email al GM | El backend debe loguear cada interacción cliente↔provider en `ContactLog` y disparar email al GM (background job). El frontend ya pasa por `WhatsAppCtaButton` solo después de waiver. |

---

## 1. Endpoints por categoría → tab

### 1.1 Common (todas las categorías)

#### Tab Overview
- `GET /api/v1/manage/listings/{id}/overview` → `ListingOverviewDto`
  - Campos: `ListingId, Title{En,Es}, CategorySlug, Status, Tier, Hierarchy, IsGmFeatured, ApprovedAt, CreatedAt, ProviderName, ProviderPhone, ProviderEmail, AvgRating, TotalReviews, PendingReviewsCount, TotalContactsLast30d, IsActive, IsPausedByOwner, GmFeedback?, NeedsChangesFeedback?`.
  - KPIs por categoría agregados aquí (el mockup `*-overview-*` muestra distinto número de KPIs por categoría/tier — ver §1.2–§1.7).

#### Tab Photos (regla #4)
- `GET /api/v1/manage/listings/{id}/photos` → `PhotoDto[]`
  - Campos: `Id, Url, ThumbnailUrl, AltEn, AltEs, Status (Public|Pending|Hidden|Rejected), Position, GmFeedback?, UploadedAt`.
- `POST /api/v1/manage/listings/{id}/photos` (multipart)
  - Body: archivo + `AltEn?, AltEs?`. Devuelve photo con `Status=Pending` por defecto.
- `PATCH /api/v1/manage/listings/{id}/photos/{photoId}` — provider edita `Alt*, Position`. Reordenar mantiene status.
- `DELETE /api/v1/manage/listings/{id}/photos/{photoId}` — el provider puede borrar **solo sus propias** fotos.
- `PATCH /api/v1/admin/photos/{photoId}/moderate` (Admin) — body `{ Status: Public|Hidden|Rejected, GmFeedback?: string }`.
- **Migración pendiente:** tabla `photos` necesita columnas `status (varchar)`, `gm_feedback (text)`, `moderated_at`, `moderated_by_admin_id`. (No usar enum en EF aún — frontend usa strings.)

#### Tab Reviews (regla #7) — todas salvo `jobs`
- `GET /api/v1/manage/listings/{id}/reviews?status=Published|PendingGm|Rejected|all` → `ReviewDto[]`
  - Campos: `Id, ClientDisplayName, ClientAvatarUrl?, Rating (1-5), Comment, CreatedAt, Status, GmFeedback?, ProviderReply { Text, Status, CreatedAt }?`.
- `POST /api/v1/manage/listings/{id}/reviews/{reviewId}/reply` — provider envía respuesta. Backend la pone en `Status=PendingGm`.
- `GET /api/v1/manage/listings/{id}/reviews/summary` → `{ Average, Total, Distribution: { "5": n, "4": n, ... } }`. Usado por `RatingSummaryCard`.
- `PATCH /api/v1/admin/reviews/{id}/approve|reject` (Admin) — cambia status, opcional `GmFeedback`.

#### Tab Settings
- `PUT /api/v1/manage/listings/{id}` — editar listing principal. Regla #5: respuesta `200` con `Status=Pending`.
- `POST /api/v1/manage/listings/{id}/pause` y `/resume`.
- `DELETE /api/v1/manage/listings/{id}`.

#### Tab Analytics (solo VIP) — todas las categorías
- `GET /api/v1/manage/listings/{id}/analytics?range=7d|30d|90d` → `AnalyticsDto`
  - Series: `views`, `contacts`, `bookings|appointments|applications` (según categoría), `conversionRate`.
  - **Autorización:** rechazar con `403 Api_Error_VipRequired` si `Provider.Tier != VIP`.

---

### 1.2 car-rental

| Tab | Endpoint | DTO / campos clave |
|---|---|---|
| Overview | (común §1.1) + KPIs `ActiveVehicles, ReservationsLast30d, OccupancyPct, RevenueLast30d (VIP)` | `CarRentalOverviewDto` |
| Vehicles | `GET /api/v1/manage/listings/{id}/vehicles` → `VehicleDto[]` | `Id, MakeModel, Year, LicensePlate, SeatCount, TransmissionEn, DailyRate, Status (Available|Rented|Maintenance), PhotoUrls[], FeaturesEn/Es[]` |
|  | `POST /vehicles`, `PUT /vehicles/{vehicleId}`, `DELETE /vehicles/{vehicleId}` | Edición → listing a Pending (#5) |
| Reservations | `GET /api/v1/manage/listings/{id}/reservations` → `ReservationDto[]` | `Id, ClientName, ClientPhone, VehicleId, VehicleLabel, StartDate, EndDate, TotalPrice, Status (Pending|Confirmed|InProgress|Completed|Cancelled), ClientWhatsAppPhone, ClientLanguage` |
|  | `PATCH /reservations/{rId}` → `{Status, ProviderNote?}` | Disparo email al GM en cada cambio de status |
| Photos | (común §1.1) — todas opcionales aquí | — |
| Reviews | (común §1.1) | — |
| Analytics (VIP) | (común §1.1) + breakdown por vehículo | `VehicleUtilization[] { VehicleId, Label, DaysRented, OccupancyPct }` |
| Settings | (común §1.1) | — |

---

### 1.3 housing

| Tab | Endpoint | DTO / campos clave |
|---|---|---|
| Overview | KPIs `ActiveProperties, OccupancyPct, AvgNightlyRate, MonthlyRevenue (VIP)` | `HousingOverviewDto` |
| Properties | `GET /listings/{id}/properties` → `PropertyDto[]` | `Id, TitleEn/Es, AddressLine, BedroomCount, BathroomCount, MaxGuests, NightlyRate, MonthlyRate?, Amenities[], CalendarRulesId?, IsActive` |
|  | CRUD `POST/PUT/DELETE /properties[/{pId}]` | #5 |
| Reservations | `GET /listings/{id}/reservations?propertyId=` | Igual estructura que car-rental, swap `VehicleId`→`PropertyId` |
| Inquiries | `GET /listings/{id}/inquiries` → `InquiryDto[]` | `Id, ClientName, ClientWhatsAppPhone, ClientLanguage, Subject, MessagePreview, CreatedAt, IsRead, ContactLogged (bool — regla #8), RelatedPropertyId?` |
|  | `POST /inquiries/{iId}/mark-contacted` → registra `ContactLog`, dispara email al GM | — |
| Photos | (común) | — |
| Reviews | (común) | — |
| Analytics (VIP) | (común) | — |
| Settings | (común) | — |

---

### 1.4 beauty

| Tab | Endpoint | DTO / campos clave |
|---|---|---|
| Overview | KPIs `ActiveServices, AppointmentsLast30d, Rebookings, RevenueLast30d (VIP)` | `BeautyOverviewDto` |
| Services | `GET /listings/{id}/services` → `ServiceDto[]` | `Id, NameEn/Es, DurationMinutes, Price, CategoryTag (Hair/Nails/Makeup/...), DescriptionEn/Es, IsActive` |
|  | CRUD | #5 |
| Appointments | `GET /listings/{id}/appointments?from=&to=` → `AppointmentDto[]` | `Id, ClientName, ClientWhatsAppPhone, ServiceId, ServiceLabel, ScheduledAt, DurationMinutes, Status (Pending|Confirmed|Completed|NoShow|Cancelled), ProviderNote?` |
|  | `PATCH /appointments/{aId}` → `{Status}` | Email GM (#8) |
| Inquiries | (igual a housing — `InquiryDto`) | — |
| Photos | (común — el mockup lo llama "Portfolio" pero el endpoint es el mismo; el frontend solo cambia el label) | — |
| Reviews | (común) | — |
| Analytics (VIP) | (común) + `ServicePopularity[] { ServiceId, Bookings30d }` | — |
| Settings | (común) | — |

---

### 1.5 mechanic (slug normalizado — backend acepta `mechanic` o `mechanics`)

| Tab | Endpoint | DTO / campos clave |
|---|---|---|
| Overview | KPIs `ActiveServices, OpenJobs, AvgResponseHours` | `MechanicOverviewDto` |
| Services | `GET /listings/{id}/services` → `ServiceDto[]` (igual a beauty, pero `CategoryTag` cambia: Oil/Tires/Brakes/...) | — |
| Inquiries | (común) | — |
| Photos (opcional) | (común — frontend lo activa con `ShowPhotosOptional=true`) | — |
| Reviews | (común) | — |
| Analytics (VIP) | (común) | — |
| Settings | (común) | — |

---

### 1.6 transport (slug normalizado — backend acepta `transport` o `transportation`)

| Tab | Endpoint | DTO / campos clave |
|---|---|---|
| Overview | KPIs `ActiveRoutes, RidesLast30d, OccupancyPct, AvgRevenue (VIP)` | `TransportOverviewDto` |
| Routes | `GET /listings/{id}/routes` → `RouteDto[]` | `Id, OriginEn/Es, DestinationEn/Es, DurationMinutes, PassengerCapacity, BaseFare, DynamicPricingEnabled (VIP only), Schedule [{ DayOfWeek, DepartureTime }], IsActive` |
|  | CRUD `POST/PUT/DELETE /routes` | #5; rechazar `DynamicPricingEnabled=true` si `Tier != VIP` con `403 Api_Error_VipRequired` |
| Reservations | (igual a car-rental — swap `VehicleId`→`RouteId` + `SeatCount`) | — |
| Inquiries | (común) | — |
| Photos (opcional) | (común) | — |
| Reviews | (común) | — |
| Analytics (VIP) | (común) + `RouteOccupancy[]` | — |
| Settings | (común) | — |

---

### 1.7 jobs

| Tab | Endpoint | DTO / campos clave |
|---|---|---|
| Overview | KPIs `ActivePostings, ApplicationsLast30d, HiredLast30d` | `JobsOverviewDto` |
| Applications | `GET /listings/{id}/applications` → `ApplicationDto[]` | `Id, ApplicantName, ApplicantWhatsAppPhone, ApplicantEmail, AppliedAt, Status (New|InReview|Contacted|Hired|Rejected), CvUrl?, CoverNote?, ProviderNote?, LanguagePreference` |
|  | `PATCH /applications/{aId}` → `{Status, ProviderNote?}` | Email GM en `Status=Contacted` (regla #8) |
| Analytics (VIP) | (común) — `FunnelByStage` | — |
| Settings | (común — el mockup de jobs **no incluye reviews**) | — |

---

## 2. DTOs nuevos (orientativo — el sr-net-developer ajusta a Application layer)

```csharp
public record MyListingDto(
    Guid Id, string TitleEn, string TitleEs, string CategorySlug,
    string Status, string Tier, string? Slug,
    DateTimeOffset CreatedAt, bool IsPausedByOwner);

public record ListingOverviewDto(
    Guid ListingId, string TitleEn, string TitleEs, string CategorySlug,
    string Status, string Tier, int Hierarchy, bool IsGmFeatured,
    DateTimeOffset? ApprovedAt, DateTimeOffset CreatedAt,
    string ProviderName, string ProviderPhone, string ProviderEmail,
    double AvgRating, int TotalReviews, int PendingReviewsCount,
    int TotalContactsLast30d, bool IsActive, bool IsPausedByOwner,
    string? GmFeedback, string? NeedsChangesFeedback,
    object Kpis); // payload categoría-específico — sub-DTOs por categoría

public record PhotoDto(
    Guid Id, string Url, string ThumbnailUrl,
    string? AltEn, string? AltEs,
    string Status,            // "Public" | "Pending" | "Hidden" | "Rejected"
    int Position, string? GmFeedback, DateTimeOffset UploadedAt);

public record ReviewDto(
    Guid Id, string ClientDisplayName, string? ClientAvatarUrl,
    int Rating, string Comment, DateTimeOffset CreatedAt,
    string Status,            // "Published" | "PendingGm" | "Rejected"
    string? GmFeedback,
    ReviewReplyDto? ProviderReply);

public record ReviewReplyDto(string Text, string Status, DateTimeOffset CreatedAt);

public record RatingSummaryDto(double Average, int Total, IDictionary<int,int> Distribution);

public record InquiryDto(
    Guid Id, string ClientName, string ClientWhatsAppPhone, string ClientLanguage,
    string Subject, string MessagePreview, DateTimeOffset CreatedAt,
    bool IsRead, bool ContactLogged, Guid? RelatedPropertyId);

public record CursorPagedResultDto<T>(
    IReadOnlyList<T> Items, string? NextCursor, bool HasMore, int? TotalItems);
```

---

## 3. Migraciones EF Core sugeridas

| Migración | Tabla | Cambios |
|---|---|---|
| `AddPhotoModeration` | `photos` | `status varchar(16) NOT NULL DEFAULT 'Pending'`, `gm_feedback text NULL`, `moderated_at timestamptz NULL`, `moderated_by_admin_id uuid NULL FK admins(id)`. Índice `idx_photos_listing_status (listing_id, status)`. |
| `AddReviewModeration` | `reviews` | `status varchar(16) NOT NULL DEFAULT 'PendingGm'`, `gm_feedback text NULL`. Tabla nueva `review_replies (id, review_id, text, status, created_at)`. |
| `AddInquiryContactLog` | `inquiries`, `contact_logs` | `inquiries.is_read bool DEFAULT false`. Tabla nueva `contact_logs (id, inquiry_id, listing_id, contacted_at, contact_method varchar(16), gm_notified_at)`. |
| `AddVehiclesPropertiesServicesRoutes` | (4 nuevas) | Esqueletos por categoría. `vehicles`, `properties`, `services`, `routes` apuntan a `listing_id`. |
| `AddApplicationsAndAppointments` | (2 nuevas) | `applications` para jobs, `appointments` para beauty. |
| `AddListingAuditLog` | `listing_audit_logs` | Para registrar transiciones a Pending por edición (#5). |

---

## 4. Reglas que el backend DEBE enforce-ar (no solo respetar en happy path)

1. **#1** — `POST /manage/listings` siempre crea con `Status=Pending`. `PATCH /admin/listings/{id}/approve` es el único path para `Status=Approved`.
2. **#4** — `Photo.Status` defecto `Pending`. Endpoints públicos (`/api/v1/listings/{slug}`) filtran `Status=Public`. Solo Admin puede mover a `Public/Hidden/Rejected`.
3. **#5** — Cualquier `PUT/PATCH` del provider sobre listing/vehicle/property/service/route → poner `Listing.Status=Pending`, registrar `ListingAuditLog`. (Excepción: `pause`/`resume`, que no edita contenido.)
4. **#7** — `POST /reviews` y `POST /reviews/{id}/reply` siempre crean con `Status=PendingGm`. Solo Admin puede aprobar.
5. **#8** — Cualquier creación/cambio de status en `ContactLog`, `Inquiry`, `Reservation`, `Appointment`, `Application` dispara background job que envía email al GM con detalle. Si el envío falla, dejar en `pending_emails` para reintentar (ya hay precedente con `EmailSenderService`).
6. **#9** — `POST/PATCH /listings` valida que `CategoryId` ∈ catálogo predefinido (tabla `categories`). Rechazar texto libre con `400 Api_Error_InvalidCategory`.

---

## 5. Quick wins de orden de implementación (sugerencia para Fase B/C/D)

1. **Bloque 1 (alto valor, bajo riesgo):** photo moderation (PhotoStatusBadge ya existe en frontend); reviews moderation + RatingSummaryCard.
2. **Bloque 2:** cursor pagination en `/manage/listings` (frontend lo necesita para escalar).
3. **Bloque 3:** inquiries + ContactLog + email al GM (regla #8 — bloqueador para cualquier flujo de contacto público).
4. **Bloque 4:** entidades por categoría (vehicles, properties, services, routes, applications, appointments) en orden de prioridad de negocio (qué categoría sale primero a producción).
5. **Bloque 5:** analytics VIP (puede salir último — gated por `Tier=VIP`).

---

## 6. Contacto

Frontend dueño de esta auditoría: agente **front-end-developer** (Senior Frontend Blazor).
Si un endpoint propuesto choca con la arquitectura actual, pingear antes de implementar — preferimos ajustar el contrato a romper el frontend que tener una segunda iteración.
