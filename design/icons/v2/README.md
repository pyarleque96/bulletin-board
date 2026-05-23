# Bulletin Board — Icon Set v2

Set de iconos cortos / app-icons / favicons derivados de los logos oficiales de **Bulletin Board** (el wordmark "Bulletin Board" — `Bulletin` en navy, `Board` en naranja). Cada variante se entrega en **dos modos**: `-light` (sobre fondos claros `#FFFFFF` / `#F8FAFC`) y `-dark` (sobre fondos oscuros `#0F172A` / `#111827`).

Esta es la **v2** del set. La v1 (que usaba la marca anterior "Bulletin Dells") permanece en `design/icons/v1/` como referencia histórica.

## Tokens de marca

| Token         | Valor     | Uso                                    |
|---------------|-----------|----------------------------------------|
| `navy`        | `#101A3D` | Color primario ("Bulletin")            |
| `orange`      | `#F39323` | Color de acento ("Board", pin)         |
| `surface-light` | `#FFFFFF` | Fondo claro principal                |
| `surface-light-alt` | `#F8FAFC` | Fondo claro secundario           |
| `surface-dark` | `#0F172A` | Fondo oscuro principal                |
| `border-light` | `#E2E8F0` | Hairline en versiones light           |

Todos los pares cumplen contraste **WCAG 2.1 AA** (texto navy/blanco sobre su fondo respectivo > 12:1; el naranja sobre azul/blanco está reservado a *accents*, no a texto crítico).

## Decisiones de diseño

1. **Iniciales BB**: la marca se destila a las iniciales del producto **B**ulletin **B**oard. Como ambas palabras comienzan con B, se respeta la jerarquía cromática del logo original (navy para la primera B = "Bulletin", naranja para la segunda B = "Board").
2. **Doble-B sin colisión visual**: para que las dos Bs no se lean como una sola letra:
   - **Color contrastante**: navy vs. naranja crea separación cromática inmediata.
   - **Stacking diagonal**: la primera B se posiciona arriba-izquierda, la segunda abajo-derecha (offset diagonal). Nunca alineadas en la misma línea de base (excepto en el monograma, donde se añade un separador explícito).
   - **Kerning positivo**: `letter-spacing: -6` en lugar del `-8` de v1 — un poco más de aire para que cada B mantenga su silueta.
   - **Monograma**: como ambas Bs van en línea, se introduce una hairline divisoria (4px, opacity 0.18/0.22) entre ellas + un dot naranja anclado sobre la divisoria, que refuerza la lectura "dos letras separadas" y conserva el guiño al pin/dot del logo original.
3. **Pin/dot naranja**: el punto sobre la "i" del logo original ("Bulletin") se convierte en una **chincheta** (bulletin board metaphor) — refuerza la categoría del producto: marketplace tipo tablón.
4. **Tipografía**: sans-serif ultra-bold geométrico (`Nunito` / `Poppins` / `Mulish` Black) — mantiene el feel del logo (`font-weight: 900`, `letter-spacing` negativo).
5. **Safe area**: ≥ 10% de padding interno en todas las versiones cuadradas (52px en viewBox 512).
6. **SVG vectorial**: todos los archivos son SVG escalables. Si se requieren PNG, exportar desde estos SVG a 16/32/48/64/128/256/512 px.
7. **Favicon**: a 16px solo es legible una sola "B" + dot. Como ambas palabras comienzan con B, el favicon es idéntico en concepto a v1 (una B blanca/navy + dot naranja).

## Inventario de archivos

| Variante | Archivo light | Archivo dark | Uso recomendado | Tamaño mínimo legible |
|----------|---------------|--------------|------------------|-----------------------|
| **App Icon** | `bulletin-board-icon-appicon-light.svg` | `bulletin-board-icon-appicon-dark.svg` | App icon Android, favicon grande, icono general | 32px |
| **Rounded Square** | `bulletin-board-icon-rounded-light.svg` | `bulletin-board-icon-rounded-dark.svg` | iOS app icon (squircle), tiles Windows | 40px |
| **Circular** | `bulletin-board-icon-circular-light.svg` | `bulletin-board-icon-circular-dark.svg` | Avatar / profile picture, redes sociales | 40px |
| **Monograma** | `bulletin-board-icon-monogram-light.svg` | `bulletin-board-icon-monogram-dark.svg` | Slides corporativos, branding interno, watermark grande | 64px |
| **Símbolo (sin texto)** | `bulletin-board-icon-symbol-light.svg` | `bulletin-board-icon-symbol-dark.svg` | Loading states, empty states, splash secundario | 48px |
| **Monocromático** | `bulletin-board-icon-mono-light.svg` | `bulletin-board-icon-mono-dark.svg` | Nav lateral, header móvil, contextos compactos UI | 24px |
| **Badge / Sello** | `bulletin-board-icon-badge-light.svg` | `bulletin-board-icon-badge-dark.svg` | Sello de marca, base reutilizable para "Verified" / "VIP" | 48px |
| **Favicon (16-safe)** | `bulletin-board-icon-favicon-light.svg` | `bulletin-board-icon-favicon-dark.svg` | Pestaña navegador, viewBox 32 optimizado para 16px | 16px |
| **Splash PWA** | `bulletin-board-icon-splash-light.svg` | `bulletin-board-icon-splash-dark.svg` | Splash screen Progressive Web App (1024x1024) | 512px |
| **Social share** | `bulletin-board-icon-social-light.svg` | `bulletin-board-icon-social-dark.svg` | Open Graph / Twitter Card (1200x630) | n/a |
| **Watermark** | `bulletin-board-icon-watermark-light.svg` | `bulletin-board-icon-watermark-dark.svg` | Marca de agua sobre imágenes de proveedores | 96px |
| **Notification** | `bulletin-board-icon-notification-light.svg` | `bulletin-board-icon-notification-dark.svg` | App icon con dot de unread (notificaciones PWA) | 48px |
| **Pin glyph** | `bulletin-board-icon-pin-light.svg` | `bulletin-board-icon-pin-dark.svg` | Tab bar, nav minimalista, viewBox 24 estilo Heroicons | 16px |

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
<link rel="icon" type="image/svg+xml" href="/images/brand/icons/bulletin-board-icon-favicon-light.svg" media="(prefers-color-scheme: light)" />
<link rel="icon" type="image/svg+xml" href="/images/brand/icons/bulletin-board-icon-favicon-dark.svg" media="(prefers-color-scheme: dark)" />
<link rel="apple-touch-icon" href="/images/brand/icons/bulletin-board-icon-rounded-light.svg" />
```

```razor
@* Componente reutilizable *@
<img src="/images/brand/icons/bulletin-board-icon-mono-@(IsDark ? "dark" : "light").svg"
     alt="Bulletin Board"
     class="h-8 w-8" />
```

## Generar PNG desde estos SVG

Si necesitas PNG (ej. `apple-touch-icon-180.png`, `android-chrome-512.png`):

```bash
# usando ImageMagick
magick -background none -density 1024 bulletin-board-icon-rounded-light.svg -resize 180x180 apple-touch-icon-180.png
magick -background none -density 1024 bulletin-board-icon-appicon-light.svg -resize 512x512 android-chrome-512.png
magick -background none -density 1024 bulletin-board-icon-favicon-light.svg -resize 32x32 favicon-32.png
```

O usando `rsvg-convert`:

```bash
rsvg-convert -w 512 -h 512 bulletin-board-icon-appicon-light.svg -o appicon-512.png
```

## Diferencias respecto a v1

- **Marca**: "Bulletin Dells" → "Bulletin Board" en todo wordmark/tagline (splash, social).
- **Monograma**: `BD` (B navy + D naranja) → `BB` (B navy + B naranja). Resuelto con stacking diagonal + contraste cromático en todos los iconos; en el monograma horizontal específicamente se añade una hairline divisoria + dot naranja para evitar lectura como una sola B.
- **Kerning**: `letter-spacing` ajustado de `-8` a `-6` en variantes BB para preservar la silueta individual de cada B.
- **Favicon**: visualmente idéntico (era ya una sola B + dot, y ambas marcas comienzan por B).
- **Pin glyph / Símbolo**: idénticos a v1 (no dependen del wordmark).

## Próximos pasos sugeridos

1. Sustituir la fuente system-fallback (`Nunito/Poppins/Mulish`) por la fuente exacta del logo (pendiente de confirmar con branding) y exportar los textos a `<path>` para evitar dependencia de tipografía instalada en cliente.
2. Generar PNG en lote (16/32/48/180/192/512/1024) y `favicon.ico` multi-tamaño.
3. Producir variantes monocromáticas en **naranja sólido** y **navy sólido** sobre transparente para watermarks contextuales.
4. Crear una versión "maskable" estricta (safe area 20%) para PWA Android adaptive icons.
5. Validar con A/B test interno que el monograma `BB` se lee como dos letras (no como una `B` con sombra) en tamaños pequeños — si falla, aumentar el offset horizontal del segundo glyph o engrosar la divisoria.
