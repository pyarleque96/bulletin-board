# Propuesta de armonía visual — Home ↔ HowItWorks (v2 — scope reducido)

**Fecha:** 2026-05-23
**Autor:** UI/UX Designer Agent
**Estado:** Scope corregido tras feedback del usuario.

---

## 0. Scope (corregido)

> El usuario aclaró: **NO** se está pidiendo cambiar tipografía / familias / pesos / tamaños / spacing / layout de HowItWorks. Esos elementos quedan **intactos**, idénticos al archivo `Pages/HowItWorks.razor` original.
>
> La armonización aplica **solo** a tres dimensiones:
>
> 1. **Paleta de color** — adoptar el Dark OLED (`#050B14`) de Home.
> 2. **Gradientes y efectos** — especialmente el gradiente VIP naranja del `btn-primary` de Home, además de glass, double-bezel, glow auras y blurs.
> 3. **Animaciones** — shimmer sweep en botones, `btn-arrow` con nudge horizontal, `hover-lift`, `fade-up` con `IntersectionObserver`, glow palpitante en tier VIP.

Nada más. La fuente DM Sans, los pesos `font-black`, los tamaños `text-3xl sm:text-4xl` para H2 de sección y `text-xl` para títulos de tier — todo se conserva exactamente igual.

---

## 1. La firma visual que el usuario destacó: gradiente VIP en botones

El usuario llamó explícitamente la atención al **gradiente naranja del `btn-primary`** en Home. Es el elemento más reconocible del lenguaje visual de Bulletin Dells y debe aparecer idéntico en HowItWorks.

### 1.1 `btn-primary` (CTA estándar) — el ámbar→naranja

Definición canónica en `wwwroot/app.css` línea 1872-1880:

```css
.dell-landing .btn-primary {
    background: linear-gradient(135deg, #F59E0B, #F97316);
    box-shadow:
        0 0 0 1px rgba(249,115,22,0.30),
        0 10px 28px -8px rgba(249,115,22,0.45);
    color: #fff;
}
.dell-landing .btn-primary:hover {
    box-shadow:
        0 0 0 1px rgba(249,115,22,0.55),
        0 2px 44px -10px rgba(249,115,22,0.70),
        0 0 18px rgba(249,115,22,0.40);
}
```

Notas clave:
- **Ámbar `#F59E0B` → naranja `#F97316` a 135°**. El ámbar es la nota cálida que distingue a Bulletin Dells de cualquier paleta genérica "saas-orange".
- Triple sombra acumulada: hairline (`0 0 0 1px`) + diffuse drop (`0 10px 28px`) + en hover se añade glow halo (`0 0 18px`).

### 1.2 `btn-vip` (tier VIP) — el rainbow multistop

Definición en `wwwroot/app.css` línea 1882-1890:

```css
.dell-landing .btn-vip {
    background: linear-gradient(135deg,
        #60a5fa 0%,    /* sky blue */
        #c4b5fd 25%,   /* lavender */
        #fb923c 60%,   /* orange */
        #F97316 100%   /* deep orange */
    );
    color: #fff;
    box-shadow:
        0 0 0 1px rgba(249,115,22,0.40),
        0 10px 28px -8px rgba(249,115,22,0.50),
        0 0 22px -4px rgba(96,165,250,0.30);
}
```

Reservado al CTA "Apply for VIP" en el tier card premium. Comunica trascendencia y movimiento sin necesidad de animar el fill.

### 1.3 Sweep shimmer (animación que envuelve el gradiente)

Ambos botones llevan un pseudo-elemento `::after` que barre un highlight diagonal sobre el gradiente al pasar el cursor (700ms con `cubic-bezier(0.16,1,0.3,1)`). Es lo que hace que el botón "se vea vivo" en lugar de plano.

---

## 2. Lo que SÍ se trae de Home a HowItWorks

| Dimensión | Token / efecto | Origen en app.css |
|---|---|---|
| **Paleta base** | Fondo `#050B14` OLED + ambient orbs naranja+sky | `.dell-landing`, `.page-ambient` |
| **Glass surface** | `.glass` (white/5 + blur 14px saturate 140%) | línea ~1763 |
| **Double bezel** | `.bezel-outer` + `.bezel-inner` (radii concéntricos) | línea ~1786 |
| **Glow VIP card** | `.glass-vip` (border + 24px 64px halo naranja) | línea ~1773 |
| **Gradiente primary** | `#F59E0B → #F97316` con triple sombra | línea 1872 |
| **Gradiente VIP** | Multistop blue→lavender→orange | línea 1882 |
| **Shimmer sweep** | `::after translateX(-120% → 120%)` 700ms | línea 1845 |
| **btn-arrow nudge** | `translateX(3px)` en hover | línea 1907 |
| **fade-up entrada** | `opacity + translateY + blur` resuelto por IntersectionObserver | línea ~1735 |
| **hover-lift** | `translateY(-4px)` 350ms spring | línea ~1800 |
| **trust-card** | hover border rojo→naranja transition | usado en cards de trust |
| **brand-text** | Color `#F97316` + underline SVG manuscrito | usado en headings |

---

## 3. Lo que NO se toca (vs el HowItWorks original)

- Fuente `DM Sans` — global, sin override.
- Pesos `font-black` / `font-bold` / `font-semibold` — exactos.
- Tamaños tipográficos:
  - Hero H1: `text-5xl md:text-6xl`
  - Section H2: `text-3xl sm:text-4xl`
  - Tier H3: `text-xl tracking-tight`
  - Final CTA H2: `text-4xl sm:text-5xl`
  - Body: `text-base` / `text-sm` / `text-xs`
- Tracking: `tracking-tighter` para displays, `tracking-tight` para títulos, `[0.18em]` para eyebrows.
- Spacing macro: `py-24 px-4`, `max-w-6xl mx-auto`, `gap-6`, `mb-14` etc.
- Layout / estructura: 6 secciones idénticas (Hero, For customers, For providers, Trust, Tiers, FAQ, Final CTA).
- Componentes únicos de HowItWorks que se preservan: `vstep` carousel vertical (números stroked con glow naranja en activo) y FAQ accordion.

---

## 4. Implementación entregada

**Archivo:** `how-it-works-harmonized.html`

Estructura:
1. `<style>` interno mirrorea los tokens `.dell-landing` necesarios (color, glass, bezel, eyebrow, btn-primary, btn-vip, btn-ghost, btn-arrow, shimmer, hover-lift, trust-card, fade-up, brand-text) — esto permite revisar el mockup en standalone sin levantar el Blazor.
2. Markup que mantiene **exactamente** las clases tipográficas de `Pages/HowItWorks.razor`.
3. Cambios respecto al original:
   - Eliminado `<style>` local `hiw-*` y `path-card` (sustituido por sus equivalentes Dell-landing).
   - `bg-white` / `bg-surface` / `text-ink` / `text-muted` reemplazados por `text-white/55,/65,/45` + fondo `#050B14`.
   - El CTA del tier card VIP ahora usa `class="btn-vip"` (rainbow multistop) en lugar de `btn-primary` — para que el premium tier se distinga visualmente del CTA estándar.

**Para portar a `Pages/HowItWorks.razor`:**
1. Eliminar el bloque `<style>` local con `hiw-bezel-*`, `hiw-btn-*`, `hiw-section-eyebrow`, `card-hover`, `path-card`, `tier-vip`.
2. Envolver todo el contenido del page en `<div class="dell-landing">…</div>`.
3. Sustituir clases:
   - `bg-white`, `bg-surface` → eliminar (heredan de `.dell-landing`).
   - `text-ink` → `text-white`.
   - `text-muted` → `text-white/55` o `/65`.
   - `hiw-section-eyebrow` (azul/verde/naranja) → `eyebrow` (glass neutro).
   - `hiw-bezel-outer/inner` → `bezel-outer` / `bezel-inner`.
   - `hiw-btn-primary` → `btn-primary`.
   - `hiw-btn-secondary` → `btn-ghost glass`.
   - `card-hover` / `path-card` → `hover-lift` o `trust-card`.
   - El CTA "Apply for VIP" del tier card → `btn-vip`.
4. Cero CSS nuevo. Todo lo necesario ya existe en `app.css`.

---

## 5. Mockup adicional

`components-comparison.html` muestra side-by-side **únicamente** los componentes que cambian de aspecto:
- `btn-primary` (antes vs después — con el gradiente ámbar→naranja).
- `btn-vip` (CTA del tier VIP con su rainbow gradient).
- Glow palpitante del tier VIP card.
- Shimmer sweep y `btn-arrow` rotation en hover.

No se compara tipografía porque, por scope, no cambia.
