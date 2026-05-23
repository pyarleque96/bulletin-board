# Bulletin Dells — Icon Set v1

Set de iconos cortos / app-icons / favicons derivados de los logos oficiales de Bulletin Dells. Cada variante se entrega en **dos modos**: `-light` (sobre fondos claros `#FFFFFF` / `#F8FAFC`) y `-dark` (sobre fondos oscuros `#0F172A` / `#111827`).

## Tokens de marca

| Token         | Valor     | Uso                                    |
|---------------|-----------|----------------------------------------|
| `navy`        | `#101A3D` | Color primario ("Bulletin")            |
| `orange`      | `#F39323` | Color de acento ("Dells", pin)         |
| `surface-light` | `#FFFFFF` | Fondo claro principal                |
| `surface-light-alt` | `#F8FAFC` | Fondo claro secundario           |
| `surface-dark` | `#0F172A` | Fondo oscuro principal                |
| `border-light` | `#E2E8F0` | Hairline en versiones light           |

Todos los pares cumplen contraste **WCAG 2.1 AA** (texto navy/blanco sobre su fondo respectivo > 12:1; el naranja sobre azul/blanco está reservado a *accents*, no a texto crítico).

## Decisiones de diseño

1. **Iniciales BD**: la marca se destila a las iniciales del producto **B**ulletin **D**ells, respetando la jerarquía cromática del logo original (navy para "B", naranja para "D").
2. **Pin/dot naranja**: el punto sobre la "i" del logo original se convierte en una **chincheta** (bulletin board metaphor) — refuerza la categoría del producto: marketplace tipo tablón.
3. **Tipografía**: sans-serif ultra-bold geométrico (`Nunito` / `Poppins` / `Mulish` Black) — mantiene el feel del logo (`font-weight: 900`, `letter-spacing` negativo).
4. **Safe area**: ≥ 10% de padding interno en todas las versiones cuadradas (52px en viewBox 512).
5. **SVG vectorial**: todos los archivos son SVG escalables. Si se requieren PNG, exportar desde estos SVG a 16/32/48/64/128/256/512 px.

## Inventario de archivos

| Variante | Archivo light | Archivo dark | Uso recomendado | Tamaño mínimo legible |
|----------|---------------|--------------|------------------|-----------------------|
| **App Icon** | `bulletin-dells-icon-appicon-light.svg` | `bulletin-dells-icon-appicon-dark.svg` | App icon Android, favicon grande, icono general | 32px |
| **Rounded Square** | `bulletin-dells-icon-rounded-light.svg` | `bulletin-dells-icon-rounded-dark.svg` | iOS app icon (squircle), tiles Windows | 40px |
| **Circular** | `bulletin-dells-icon-circular-light.svg` | `bulletin-dells-icon-circular-dark.svg` | Avatar / profile picture, redes sociales | 40px |
| **Monograma** | `bulletin-dells-icon-monogram-light.svg` | `bulletin-dells-icon-monogram-dark.svg` | Slides corporativos, branding interno, watermark grande | 64px |
| **Símbolo (sin texto)** | `bulletin-dells-icon-symbol-light.svg` | `bulletin-dells-icon-symbol-dark.svg` | Loading states, empty states, splash secundario | 48px |
| **Monocromático** | `bulletin-dells-icon-mono-light.svg` | `bulletin-dells-icon-mono-dark.svg` | Nav lateral, header móvil, contextos compactos UI | 24px |
| **Badge / Sello** | `bulletin-dells-icon-badge-light.svg` | `bulletin-dells-icon-badge-dark.svg` | Sello de marca, base reutilizable para "Verified" / "VIP" | 48px |
| **Favicon (16-safe)** | `bulletin-dells-icon-favicon-light.svg` | `bulletin-dells-icon-favicon-dark.svg` | Pestaña navegador, viewBox 32 optimizado para 16px | 16px |
| **Splash PWA** | `bulletin-dells-icon-splash-light.svg` | `bulletin-dells-icon-splash-dark.svg` | Splash screen Progressive Web App (1024x1024) | 512px |
| **Social share** | `bulletin-dells-icon-social-light.svg` | `bulletin-dells-icon-social-dark.svg` | Open Graph / Twitter Card (1200x630) | n/a |
| **Watermark** | `bulletin-dells-icon-watermark-light.svg` | `bulletin-dells-icon-watermark-dark.svg` | Marca de agua sobre imágenes de proveedores | 96px |
| **Notification** | `bulletin-dells-icon-notification-light.svg` | `bulletin-dells-icon-notification-dark.svg` | App icon con dot de unread (notificaciones PWA) | 48px |
| **Pin glyph** | `bulletin-dells-icon-pin-light.svg` | `bulletin-dells-icon-pin-dark.svg` | Tab bar, nav minimalista, viewBox 24 estilo Heroicons | 16px |

## Cómo elegir la variante correcta

- **Tab del navegador / bookmark** → `favicon`
- **iOS Home Screen** → `rounded`
- **Android Home Screen / Play Store** → `appicon`
- **PWA `manifest.json` (any)** → `appicon`
- **PWA `manifest.json` (maskable)** → `rounded` (tiene safe area)
- **Avatar del GM / usuario sistema** → `circular`
- **Open Graph (Facebook, LinkedIn) / Twitter Card** → `social`
- **Sidebar del panel admin contraído** → `mono` o `pin`
- **Bottom tab bar móvil** → `pin`
- **Sello "Verified" o "VIP" en listing cards** → `badge` (re-coloreable según tier)
- **Splash inicial al abrir PWA** → `splash`
- **Watermark sobre fotos de proveedores** → `watermark`

## Integración en Blazor

```razor
@* Favicon en App.razor / _Host.cshtml *@
<link rel="icon" type="image/svg+xml" href="/images/brand/icons/bulletin-dells-icon-favicon-light.svg" media="(prefers-color-scheme: light)" />
<link rel="icon" type="image/svg+xml" href="/images/brand/icons/bulletin-dells-icon-favicon-dark.svg" media="(prefers-color-scheme: dark)" />
<link rel="apple-touch-icon" href="/images/brand/icons/bulletin-dells-icon-rounded-light.svg" />
```

```razor
@* Componente reutilizable *@
<img src="/images/brand/icons/bulletin-dells-icon-mono-@(IsDark ? "dark" : "light").svg"
     alt="Bulletin Dells"
     class="h-8 w-8" />
```

## Generar PNG desde estos SVG

Si necesitas PNG (ej. `apple-touch-icon-180.png`, `android-chrome-512.png`):

```bash
# usando ImageMagick
magick -background none -density 1024 bulletin-dells-icon-rounded-light.svg -resize 180x180 apple-touch-icon-180.png
magick -background none -density 1024 bulletin-dells-icon-appicon-light.svg -resize 512x512 android-chrome-512.png
magick -background none -density 1024 bulletin-dells-icon-favicon-light.svg -resize 32x32 favicon-32.png
```

O usando `rsvg-convert`:

```bash
rsvg-convert -w 512 -h 512 bulletin-dells-icon-appicon-light.svg -o appicon-512.png
```

## Próximos pasos sugeridos

1. Sustituir la fuente system-fallback (`Nunito/Poppins/Mulish`) por la fuente exacta del logo (pendiente de confirmar con branding) y exportar los textos a `<path>` para evitar dependencia de tipografía instalada en cliente.
2. Generar PNG en lote (16/32/48/180/192/512/1024) y `favicon.ico` multi-tamaño.
3. Producir variantes monocromáticas en **naranja sólido** y **navy sólido** sobre transparente para watermarks contextuales.
4. Crear una versión "maskable" estricta (safe area 20%) para PWA Android adaptive icons.
