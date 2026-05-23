# Database — Bulletin Dells

## Archivos

| Archivo | Propósito |
|---------|-----------|
| `schema.dbml` | **Fuente de verdad.** Esquema en formato DBML, versionable en git. |
| `schema.sql` | DDL PostgreSQL de referencia (generado conceptualmente desde el DBML). |
| `constraints.sql` | Constraints de negocio y triggers — van en las migraciones EF Core. |
| `indexes.sql` | Partial indexes críticos — se agregan manualmente en `OnModelCreating`. |
| `schema.png` | Diagrama ER exportado de dbdiagram.io. |

---

## Cómo visualizar el diagrama ER

1. Abrir [https://dbdiagram.io](https://dbdiagram.io)
2. Crear nuevo diagrama → pegar el contenido de `schema.dbml`
3. El diagrama se renderiza automáticamente con todas las relaciones
4. Exportar como PNG → guardar en este directorio como `schema.png`

---

## Flujo: diagrama → EF Core

```
schema.dbml  (este directorio, fuente de verdad)
     ↓
dbdiagram.io (visualización + export SQL de referencia)
     ↓
Entidades C# en source/Domain/Entities/
     ↓
DbContext con Fluent API en source/Infrastructure/Persistence/
     ↓
dotnet ef migrations add InitialSchema
     ↓ (editar migración para agregar partial indexes y triggers)
dotnet ef database update
```

---

## Decisiones de diseño

### UUIDs como PKs
- Compatibles con generación en el cliente (sin round-trip a BD)
- `gen_random_uuid()` nativo en PostgreSQL 13+
- EF Core: `ValueGeneratedOnAdd()` con `HasDefaultValueSql("gen_random_uuid()")`

### provider_tier desnormalizado en listings
- La query de ranking (SuperDuperVIP → VIP → rest) se ejecuta en cada búsqueda
- Un JOIN a providers en cada query sería costoso a escala
- El trigger `trg_sync_listing_provider_tier` mantiene el valor sincronizado
- Usar `EXPLAIN ANALYZE` para verificar que `idx_listings_tier_rating` se usa

### contact_id UNIQUE en ratings
- Garantiza exactamente 1 rating por interacción real
- La FK a contacts impide ratings fantasma
- Es la segunda línea de defensa después del dominio C#

### avg_rating desnormalizado en listings
- Evita `SELECT AVG(stars)` en cada request de detalle
- El trigger `trg_update_avg_rating` lo actualiza al aprobar/rechazar un rating
- Aceptable: ratings no se aprueban en tiempo real (pasa por el GM)

### Cifrado a nivel aplicación
- `phone` y `identity_document_encrypted` se cifran en EF Core con Value Converters
- La BD nunca almacena estos valores en plain-text
- PostgreSQL `pgcrypto` disponible como alternativa para cifrado en BD

### Partial indexes
- Todos los índices de búsqueda tienen `WHERE status = 'Approved'`
- Reduce el tamaño del índice drásticamente (la mayoría del tráfico es sobre Approved)
- EF Core no los genera automáticamente — agregar en `MigrationBuilder.Sql()`

---

## Gap activo: entidades sin configuración EF Core explícita

Las siguientes entidades **no tienen** un archivo `IEntityTypeConfiguration<T>` en `Infrastructure/Persistence/Configurations/`:

| Entidad | DbSet en ApplicationDbContext | Tabla esperada | Tabla real sin config |
|---------|-------------------------------|----------------|-----------------------|
| `Category` | `Categories` | `categories` | `"Categories"` (quoted) |
| `ListingImage` | `ListingImages` | `listing_images` | `"ListingImages"` (quoted) |
| `Verification` | `Verifications` | `verifications` | `"Verifications"` (quoted) |
| `Report` | `Reports` | `reports` | `"Reports"` (quoted) |

Sin `builder.ToTable("snake_case")` explícito ni `UseSnakeCaseNamingConvention()` en `DependencyInjection.cs`, Npgsql genera tablas con nombre entrecomillado y PascalCase. Esto rompe todas las queries documentadas en este directorio.

**Solución requerida (elegir una):**

**Opción A — Crear configuraciones explícitas** (consistente con el resto del proyecto):
Crear `CategoryConfiguration.cs`, `ListingImageConfiguration.cs`, `VerificationConfiguration.cs`, `ReportConfiguration.cs` con al menos `builder.ToTable("nombre_snake_case")`.

**Opción B — Naming convention global** (más simple):
En `DependencyInjection.cs`, agregar `UseSnakeCaseNamingConvention()`:
```csharp
options.UseNpgsql(connectionString, ...)
       .UseSnakeCaseNamingConvention(); // requiere NuGet: EFCore.NamingConventions
```
Esto aplica snake_case a TODAS las tablas y columnas automáticamente.

> **Impacto**: hasta que se resuelva, las migraciones EF Core generarán nombres de tabla incorrectos para estas 4 entidades. Los scripts SQL en este directorio son la especificación; el código debe converger a ellos.

---

## Invariantes de negocio en la BD

| Regla | Mecanismo |
|-------|-----------|
| VIP requiere Verified | `chk_vip_requires_verified` en providers |
| published_at solo si Approved | `chk_listing_published_at_approved` en listings |
| Rating solo con contact previo | FK + UNIQUE en `ratings.contact_id` |
| Foto no pública por defecto | DEFAULT false en `listing_images.is_public` |
| Edición vuelve listing a Pending | Trigger `trg_listing_edit_pending` |
| 1 waiver por usuario | UNIQUE en `waiver_acceptances.user_id` |
