# Bulletin Dells — Landing Page Proposals (v5/loadings/v5)

Cinco propuestas de landing page completa para Bulletin Dells, entregadas como HTML/Tailwind auto-contenido (Tailwind via CDN). Todas mantienen el hero actual (al usuario le gusta) y reinventan radicalmente todo lo que va por debajo.

Cada archivo cubre las secciones requeridas:

1. **Hero** (similar al actual)
2. **Categorías** (9 categorías reales del producto)
3. **Top anuncios / Featured listings** (con jerarquía Hierarchy → GM Featured → VIP → Verified → Regular)
4. **Comparativa de tiers** (Regular / Verified / VIP)
5. **Trust signals** (las 9 reglas no-negociables del CLAUDE.md)
6. **Cómo convertirte en proveedor** (5 pasos del blueprint)
7. **CTA final + footer bilingüe**

Todos los archivos cumplen:
- DM Sans (display) + opcionalmente DM Serif Display (editorial)
- Tipografía consistente con el resto del producto
- Paleta del proyecto: brand `#3B82F6`, accent `#F97316`, ink `#1E293B`
- WCAG 2.1 AA: skip link, focus visible, alt text, `aria-labels`, `prefers-reduced-motion`
- Mobile-first, `min-h-[100dvh]`, `IntersectionObserver` (no `window.scroll`)
- Textos reales en inglés y español (sin lorem ipsum)
- WhatsApp como único CTA de contacto (verde `#25D366` con copy "GM is notified by email")
- Avisos explícitos del waiver legal en cada tarjeta de contacto principal
- `picsum.photos` con seeds estables para mantener las imágenes consistentes

---

## Resumen de cada propuesta

### 01 — Amazon-like Marketplace
`landing-01-amazon-marketplace.html`

Densidad visual al estilo Amazon Departments. Categorías como bento grid de tiles fotográficos (uno hero col-span-2 + 7 más), Top GM Featured como hero card horizontal, fila densa de 4 cards VIP, y dos sub-filas compactas Verified + Regular de 6 cards cada una (icon grid Yellow Pages-like). Trust signals en grid 3×2 con iconos coloridos. Mucha información above the fold, sensación de catálogo e-commerce.

**Cómo resuelve los puntos débiles:**
- El usuario no veía suficiente "producto" debajo del hero — esta versión muestra 1 GM Featured + 4 VIP + 12 más en menos de dos scrolls.
- La jerarquía VIP/Verified/Regular queda visualmente clara con strips de color (gold/orange/green) en el top de cada card.
- La grid bento de categorías ocupa el ancho completo y permite escanear las 9 categorías en una mirada.

---

### 02 — Yellow Pages Reimagined
`landing-02-yellow-pages-editorial.html`

Editorial Luxury: cream paper bg con noise grain overlay (opacity 0.04), DM Serif Display italic en headlines, drop-cap en el opening editorial, índice de categorías como rows tipo directorio con numeración romana, GM Featured como masthead horizontal con quote del GM, comparativa de tiers como pricing editorial con stamps tipo sello postal. Sensación de revista trimestral curada.

**Cómo resuelve los puntos débiles:**
- Convierte la home en un "número editorial" — entrega la sensación de curaduría que el modelo de negocio (cada listing aprobado a mano) realmente comunica.
- El índice de categorías como rows numeradas vence al patrón aburrido de 3-columnas-de-cards y comunica "directorio premium" directamente.
- La sección de testimonios/featured con quotes del GM convierte el GM Featured en una marca personal — refuerza confianza.

---

### 03 — Trust-first / Verified-first
`landing-03-trust-verified-first.html`

Soft Structuralism al estilo Airbnb. Stats band justo bajo el hero (100% / 3× / 2+ / 0), categorías como tiles 4:5 con foto large + count "X · Y verified", GM Featured con avatar circular del host (ring verde verified), cards de listing con face ring verde y badge "✓ Verified Mechanics" siempre arriba del nombre, 3 medallas grandes "Phone / ID / Social verified". En la comparativa de tiers, **Verified es la tarjeta destacada** (no VIP) — propuesta del "camino realista".

**Cómo resuelve los puntos débiles:**
- Convierte el "GM-approved + verified" de fine-print a protagonista absoluto. Cada card lleva avatar + ring verde + badge "Verified".
- Stats band con números grandes da feedback inmediato del valor que la plataforma entrega.
- La narrativa "verification unlocks visibility" alinea el incentivo del proveedor (verificarse → más visibilidad → más leads) con el valor del cliente (encontrar a alguien real).

---

### 04 — Bilingual & Community-driven
`landing-04-bilingual-community.html`

Cálido (cream warm bg `#FFFAF0`), DM Serif Display italic en color accent naranja como segunda voz bilingüe sistemática (cada heading va en EN bold + ES italic justo debajo). Switch EN/ES grande en el hero. Sección "Who we serve" con J1 Students / Seasonal Workers / Local Families. GM Featured con quote real del provider Marisol R. Cards de provider incluyen mini-avatar + quote. Footer con switch EN/ES.

**Cómo resuelve los puntos débiles:**
- Bilingüismo deja de ser un toggle escondido en el navbar — vive en CADA heading como par EN/ES.
- La sección "Who we serve" identifica explícitamente a J1 students y seasonal workers, audiencia clave del producto que ninguna otra versión menciona por nombre.
- Quotes de providers personalizan el directorio y convierten "Tony" / "Marisol" / "Dells Express" en cara visible del producto.

---

### 05 — Premium VIP-forward
`landing-05-premium-vip-forward.html`

Ethereal Glass / OLED Dark (`#050B14` base). Page-ambient orbs naranja + brand blue. Vip-text gradient (gold→orange) en wordmark. Comparativa de tiers como **hero secundario justo bajo el hero principal** — VIP card con ring naranja + glow + strip gold → naranja en top. Marquee horizontal bilingüe `★ VIP MEMBERSHIP · Curated by humans · Wisconsin Dells`. Bento de categorías sobre fondo OLED con overlay de gradiente al `bg`. Cards VIP con `vip-ring` glow.

**Cómo resuelve los puntos débiles:**
- Vende VIP desde el primer segundo. Comparativa de tiers no esperando al final — aparece a un scroll del hero.
- Comunica "Premium" como valor del producto sin gritarlo — el lujo está en el material (glass, ambient orbs) no en el copy.
- Ideal para los listings VIP destacados que pagan; los proveedores VIP se sentirán representados por la estética.

---

## Recomendación

**Para llevar a Blazor primero:** **Propuesta 03 (Trust-first / Verified-first)**.

Razones:
1. Encaja exactamente con el corazón del modelo de negocio: cada regla no-negociable (CLAUDE.md sección 2) trata de moderación/verificación/trust. Esta propuesta amplifica eso visualmente sin inventar nada.
2. La paleta es la que ya tienes (blanco + brand-blue + emerald-trust + accent-orange) — no requiere nueva identidad.
3. Es la más fácil de poblar con datos reales (no necesita quotes humanos curados como la 02 y 04, ni una marquee VIP como la 05, ni layout bento que el backend tiene que respetar como la 01).
4. La sección "100% / 3× / 2+ / 0" es contenido estático que ya está implícito en el producto — entrega valor sin endpoint nuevo.

**Combinaciones que recomiendo:**

- **Tomar de 03 (Trust-first):** band de stats, medallas de verificación (Phone/ID/Social), face-ring verde en cards, bilingual italic en heading principal, recomendación de tier Verified destacado en pricing.
- **Tomar de 01 (Amazon):** el bento grid de categorías como sección "Browse all 9 categories" — más escaneable que la versión scroll horizontal actual.
- **Tomar de 04 (Community):** la sección "Who we serve" como bloque opcional para reforzar el segmento J1.
- **Reservar 02 (Editorial) y 05 (Premium VIP):** como direcciones alternativas para campañas de marketing o landings específicas (ej. landing para captar proveedores VIP usaría la 05 directamente).

**Hero:** mantener el actual exactamente como está. Las 5 propuestas comparten esa misma sección con variaciones leves (cambio del search button CTA color/texto). La 04 añade un switch EN/ES grande dentro del hero que recomiendo adoptar si el bilingüismo se sigue posicionando como valor central.

---

## Inconsistencias detectadas entre código actual y blueprint/CLAUDE.md

Durante la revisión del código encontré estos puntos que conviene anotar (sin urgencia de resolver para los mockups, pero relevantes):

1. **Categoría "Restaurants" no está en el seed canónico actual.** Aparece en los mockups porque el blueprint la cita implícitamente como tipo de proveedor local, pero no he visto su slug confirmado en seeds. **Acción:** confirmar con DB owner si `restaurants` está sembrado; si no, omitir en la versión Blazor o agregar al seed antes de mergear.

2. **"Hierarchy" es ranking puro, no un tier visual.** El CLAUDE.md (sección 5) es claro: `Regular → Verified → VIP` son los únicos tiers; `hierarchy` es un INT que el GM administra. He respetado esto: en los mockups, cuando hablo del "ranking jerárquico" lo cuento como "Hierarchy DESC → GM Featured → Tier → Rating" en el orden documentado, sin pretender que sea otro tier visual. Las cards GM-Featured llevan strip amber claro (que NO es un tier, es marca de GM Featured).

3. **VIP pricing no está fijado.** El blueprint cita "monetización VIP" pero no precio concreto. Todos los mockups dicen "Contact us for pricing · Consultar precio" para no inventar montos.

4. **Counts (`142 listings`, `38 listings`, etc.)** son placeholders en los mockups. El endpoint `/api/v1/categories?includeCount=true` ya existe en backend — el FE debe consumir y reemplazar.

5. **Avatares de hosts y nombres ("Marisol R.", "Tony Mazzini")** son placeholders aspiracionales para mostrar el patrón "face-ring + name + tier badge" en cards. Cuando se porte a Blazor, vienen de `Provider.DisplayName` + `Provider.AvatarUrl`.

6. **Quote del GM en propuesta 02 (editorial)** es un dispositivo narrativo del mockup; el backend NO tiene campo "GM note" actualmente. Si se adopta, requiere extender el modelo de `Listing` con un campo opcional `GmNote` (o `GmFeaturedReason`).

---

## Archivos en esta carpeta

```
v5/loadings/v5/
├── landing-01-amazon-marketplace.html      # Dense e-commerce grid
├── landing-02-yellow-pages-editorial.html  # Cream paper + serif + dropcap
├── landing-03-trust-verified-first.html    # Airbnb-style, verification-led [RECOMENDADO]
├── landing-04-bilingual-community.html     # Warm + EN/ES side-by-side
├── landing-05-premium-vip-forward.html     # OLED dark + glass + VIP hero
└── README.md                                # this file
```

Cada archivo se abre directamente en el navegador (Tailwind CDN incluido). Para ver el grain overlay y los efectos de blur correctamente, abrir con servidor local (`live-server` o `dotnet run`) si CORS bloquea las fuentes de Google.
