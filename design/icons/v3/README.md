# Bulletin Board — Icon Set v3

Cuarta iteración del set de iconos para **Bulletin Board**, el marketplace bilingüe de Wisconsin Dells. Esta versión nace de un feedback explícito sobre v2: *"se parece mucho al de Booking"*. v3 reescribe la dirección visual con el objetivo central de **diferenciarse de Booking.com** mientras mantiene la paleta del logo oficial (navy `#101A3D` + naranja `#F39323`).

> **v1** está en `design/icons/v1/` (marca anterior "Bulletin Dells").
> **v2** está en `design/icons/v2/` (primera iteración "Bulletin Board" — diagonal stacking + system font).
> **v3** (este folder) — lookbook de cuatro propuestas distintas.

## NUEVA convención: light vs. dark (¡leer antes de usar!)

A partir de v3 invertimos la nomenclatura habitual. La etiqueta `-light` / `-dark` describe el **color de la tinta**, no del fondo:

| Variante  | Color de la tinta                        | Fondo recomendado                |
|-----------|------------------------------------------|----------------------------------|
| `-dark`   | navy `#101A3D` + naranja `#F39323`       | claro (`#FFFFFF` / `#F8FAFC`)   |
| `-light`  | blanco `#FFFFFF` + naranja `#F39323`     | oscuro (`#0F172A` / `#111827`)  |

**Mnemónico**: piensa en "ink" en una imprenta — la tinta *dark* (navy) se aplica sobre papel blanco; la tinta *light* (blanca) se aplica sobre papel negro. Es decir: el sufijo indica qué color irá impreso, no el de la página.

Si vienes de v1/v2, donde `-dark` significaba "para fondos oscuros", la nomenclatura está **invertida** aquí.

## Cambios estructurales respecto a v2

| Aspecto                | v2                                   | v3                                                          |
|------------------------|--------------------------------------|-------------------------------------------------------------|
| Tipografía de las Bs  | `<text>` con `Nunito`/`Poppins` 900  | **Paths SVG custom**, no dependen de fuente del cliente    |
| Espacio entre Bs       | `letter-spacing: -6` (separadas)     | **Ligadura compartida** (asta vertical única), o overlap   |
| Convención light/dark  | "Para fondos claros / oscuros"        | **Color de la tinta** (navy/blanco respectivamente)        |
| Cantidad de direcciones| 1                                     | **4** propuestas distintas en paralelo                     |
| Diferenciación Booking | Insuficiente (mismo tile navy+naranja)| Squircle estricto + sticker + corkboard + stencil          |

## Cómo nos alejamos de Booking.com

Booking.com tiene una identidad muy específica: rectángulo azul `#003580` casi cuadrado, esquinas medio redondeadas, wordmark sans-serif blanca neutra. v2 cayó en esa silueta por usar el mismo combo (block navy + texto blanco sobre `<text>` en sans bold). v3 corta el parecido con cuatro estrategias distintas:

1. **Glyph custom (no typeset)**. Las Bs de v3 son paths vectoriales construidos a mano (Propuestas A, B, C, D). No son una fuente, no se leen como un wordmark, y por tanto no se confunden con "Booking".
2. **Ligadura compartida (Propuesta A)**. Las dos Bs comparten una sola asta vertical. Booking jamás haría esto (su brand es un wordmark legible). Es la maniobra más fuerte de diferenciación tipográfica.
3. **Push-pin físico (Propuesta B)**. El símbolo predominante deja de ser texto y se vuelve un objeto físico (chincheta sobre nota sobre corkboard). Booking nunca usa skeumorfismo.
4. **Sharp corners + editorial accents (Propuesta C)**. Esquinas casi sharp (rx=20px en lugar de rx=64-112 que es lo habitual en tile apps), tab vertical naranja en el borde izquierdo, eyebrow micro-label en ALL-CAPS. Reads as magazine masthead, no app icon.
5. **Sticker die-cut + offset shadow (Propuesta D)**. Forma irregular con outline blanco, sombra dura desplazada, sparkle accents — un lenguaje skate-sticker / zine. Lo opuesto a "flat enterprise tile".

Adicionalmente:
- Mantenemos el naranja `#F39323` original del logo (no lo desaturamos) pero lo usamos en proporciones distintas: a veces como acento mínimo (tab vertical, accent en monograma), a veces como mitad cromática (la 2ª B), a veces como objeto 3D (la cabeza del pin).
- Ningún icono v3 usa el "stacking diagonal" que tenía v2, que era una de las pistas más fuertes hacia la silueta Booking-tile.

## Tokens de marca (sin cambios respecto a v2)

| Token               | Valor     | Uso                                    |
|---------------------|-----------|----------------------------------------|
| `navy`              | `#101A3D` | Color primario (B de "Bulletin")       |
| `orange`            | `#F39323` | Color de acento (B de "Board", pin)    |
| `surface-light`     | `#FFFFFF` | Fondo claro principal                  |
| `surface-light-alt` | `#F8FAFC` | Fondo claro secundario                 |
| `surface-dark`      | `#0F172A` | Fondo oscuro principal                 |

Contraste validado WCAG 2.1 AA: navy/blanco sobre su superficie respectiva > 12:1.

## Las cuatro propuestas

### A — Ligature monogram (RECOMMENDED DIRECTION)

Folder: [`proposal-a-ligature/`](./proposal-a-ligature/)

**Concepto**: las dos Bs comparten una única asta vertical en el centro (la columna navy). Los bowls de la B izquierda abren hacia la izquierda (en navy), los de la B derecha abren hacia la derecha (en naranja). La chincheta dot reemplaza el punto sobre la "i" de Bulletin y se ancla en lo alto de la asta compartida.

**Por qué la recomiendo**:
- Es la dirección más **diferenciada de Booking**: ningún brand del rubro tiene una ligadura tipográfica BB con asta compartida; es una solución única, propietaria y registrable.
- Es **escalable**: la misma silueta funciona desde 16px hasta 1024px sin cambiar la lógica del glyph (solo se simplifican proporciones en favicon).
- **Independiente de tipografía**: como es un path vectorial custom, no depende de fuente instalada. Confiable cross-platform.
- **Versátil de aplicación**: el monogram funciona como avatar circular, app icon, splash, social card, watermark, badge — todas las variantes derivadas reutilizan el mismo glyph.

Esta propuesta entrega el **set completo** (15 variantes):
- `monogram`, `appicon`, `rounded`, `circular`, `symbol`, `favicon` — los esenciales
- `splash`, `social`, `watermark`, `notification`, `mono`, `pin`, `badge` — las familias auxiliares

### B — Pushpin / Corkboard

Folder: [`proposal-b-pushpin/`](./proposal-b-pushpin/)

**Concepto**: el icono es una escena física en miniatura. Una nota adhesiva (sticky note) navy con la esquina inferior derecha doblada, pinchada por una chincheta naranja brillante sobre una superficie de corcho. La nota lleva el monogram BB (heredado de la propuesta A en escala reducida).

**Cuándo elegirla**:
- Si quieres reforzar **fuertemente la metáfora "bulletin board"** (que es literalmente el nombre del producto).
- Si te interesa una identidad más cálida y narrativa, menos abstracta.

**Trade-off**: el corkboard pattern + el pushpin glossy meten más complejidad visual y peor legibilidad en tamaños pequeños. Por eso el favicon de esta propuesta simplifica drásticamente.

Entrega 6 variantes esenciales: `appicon`, `rounded`, `circular`, `monogram`, `symbol`, `favicon`.

### C — Editorial Stencil

Folder: [`proposal-c-stencil/`](./proposal-c-stencil/)

**Concepto**: tratamiento editorial / magazine masthead. Las Bs viven sobre fondo blanco/navy con un **tab vertical naranja** que cuelga del borde izquierdo, doble regla horizontal, eyebrow micro-label en ALL CAPS con tracking extra-letras (`WISCONSIN · DELLS`). Las esquinas del contenedor son casi sharp (rx=20-64px) en lugar de la suavidad típica de app icons.

**Cuándo elegirla**:
- Si la marca quiere proyectar **autoridad/serío/editorial** (más cerca de un diario local que de un app cool).
- Si el producto se va a posicionar como "directorio confiable" en lugar de "marketplace divertido".

Entrega 6 variantes esenciales.

### D — Die-cut Sticker

Folder: [`proposal-d-sticker/`](./proposal-d-sticker/)

**Concepto**: skater-sticker / zine die-cut. Sticker con forma irregular redondeada, **outline blanco grueso** alrededor del navy fill, **sombra dura offset** que da feel físico de pegatina, sparkle accents naranja como decoración.

**Cuándo elegirla**:
- Si queremos ir con una **identidad joven, playful, memorable** (mercado J1 / estudiantes / trabajadores de temporada — el feel "Wisconsin Dells es un water park con personalidad").
- Si Marketing va a hacer mucho contenido para Instagram/TikTok y necesitamos algo que destaque en feed.

Entrega 6 variantes esenciales.

## Vista rápida

Abre [`preview.html`](./preview.html) en el navegador para ver las cuatro propuestas lado a lado, con cada variante etiquetada y aplicada sobre su fondo recomendado (claro u oscuro según la convención).

## Inventario completo de archivos

### Propuesta A — Ligature (15 archivos × 2 modos = 30 SVG)

```
proposal-a-ligature/
├── bulletin-board-icon-appicon-{dark,light}.svg
├── bulletin-board-icon-rounded-{dark,light}.svg
├── bulletin-board-icon-circular-{dark,light}.svg
├── bulletin-board-icon-monogram-{dark,light}.svg
├── bulletin-board-icon-symbol-{dark,light}.svg
├── bulletin-board-icon-favicon-{dark,light}.svg
├── bulletin-board-icon-splash-{dark,light}.svg
├── bulletin-board-icon-social-{dark,light}.svg
├── bulletin-board-icon-mono-{dark,light}.svg
├── bulletin-board-icon-pin-{dark,light}.svg
├── bulletin-board-icon-badge-{dark,light}.svg
├── bulletin-board-icon-watermark-{dark,light}.svg
└── bulletin-board-icon-notification-{dark,light}.svg
```

### Propuestas B, C, D (6 archivos × 2 modos = 12 SVG cada una)

```
proposal-{b-pushpin,c-stencil,d-sticker}/
├── bulletin-board-icon-appicon-{dark,light}.svg
├── bulletin-board-icon-rounded-{dark,light}.svg
├── bulletin-board-icon-circular-{dark,light}.svg
├── bulletin-board-icon-monogram-{dark,light}.svg
├── bulletin-board-icon-symbol-{dark,light}.svg
└── bulletin-board-icon-favicon-{dark,light}.svg
```

## Integración en Blazor

```razor
@* Light surface (default Bulletin Board palette) — use the "dark ink" variant *@
<link rel="icon" type="image/svg+xml"
      href="/images/brand/icons/v3/proposal-a-ligature/bulletin-board-icon-favicon-dark.svg"
      media="(prefers-color-scheme: light)" />

@* Dark surface — use the "light ink" variant *@
<link rel="icon" type="image/svg+xml"
      href="/images/brand/icons/v3/proposal-a-ligature/bulletin-board-icon-favicon-light.svg"
      media="(prefers-color-scheme: dark)" />

@* iOS apple-touch — recommended direction is Proposal A rounded *@
<link rel="apple-touch-icon"
      href="/images/brand/icons/v3/proposal-a-ligature/bulletin-board-icon-rounded-dark.svg" />
```

```razor
@* Componente Blazor reutilizable — variant prop selects proposal direction *@
<img src="/images/brand/icons/v3/proposal-@(Proposal)/bulletin-board-icon-mono-@(IsDarkSurface ? "light" : "dark").svg"
     alt="Bulletin Board"
     class="h-8 w-8" />

@code {
    [Parameter] public string Proposal { get; set; } = "a-ligature";
    [Parameter] public bool IsDarkSurface { get; set; }  // true if rendered on a navy/black background
}
```

## Próximos pasos sugeridos

1. **Decisión de dirección**: el cliente/branding lead elige una de las cuatro propuestas como dirección oficial. La A es la sugerida; B/C/D son backups con personalidad distinta.
2. **Completar la dirección elegida**: si la elegida es B, C o D, expandir su set para incluir las familias auxiliares (splash, social, watermark, notification, mono, pin, badge).
3. **Generar PNG en lote** (16/32/48/180/192/512/1024) y `favicon.ico` multi-tamaño desde los SVG de la dirección elegida (ver comandos `magick` y `rsvg-convert` en `v2/README.md` — siguen aplicando).
4. **A/B test ligero** del favicon en 16px: ¿se lee como BB o como una mancha? Si Propuesta A falla a 16px, usar Propuesta D favicon (que es texto compactado) para tamaños pequeños y A para el resto.
5. **Maskable PWA**: producir una variante con safe-area 20% para Android adaptive icons.
6. **Validar con stakeholders** que la nueva convención light/dark (color de la tinta, no del fondo) está clara antes de propagarla al sistema de design tokens.
