# Bulletin Dells — Paleta Definitiva de Botones

**Versión:** 1.0 | **Fecha:** 2026-05-11 | **Autor:** UI/UX Designer Agent

---

## 1. Inventario de Botones Existentes

### 1.1 Componentes Blazor dedicados (`/Components/Buttons/`)

| Componente | Archivo | Clases Tailwind base | Variantes / Parámetros |
|---|---|---|---|
| `PrimaryButton` | `PrimaryButton.razor` | `bg-accent text-white rounded-full px-5 py-2.5 font-medium` | `ShowTrailingArrow`, `TrailingArrowBubbleBg` |
| `SecondaryButton` | `SecondaryButton.razor` | `text-brand-600 border border-brand-200 rounded-full/rounded-xl px-4 py-2 font-medium` | `Shape`: "pill" \| "rounded" |
| `AccentButton` | `AccentButton.razor` | `w-full bg-accent text-white rounded-full px-6 py-3.5 font-semibold` | Form submit full-width |
| `DangerButton` | `DangerButton.razor` | Outline: `text-red-600 border border-red-200 hover:bg-red-50 rounded-xl`; Solid: `bg-red-600 text-white hover:bg-red-700` | `Variant`: "outline" \| "solid" |
| `GhostButton` | `GhostButton.razor` | `text-brand-500 hover:text-brand-700` (brand); `text-muted hover:text-ink` (muted); `text-white/40 hover:text-white/70` (white) | `Variant`: "brand" \| "muted" \| "white" \| "danger" |
| `LinkButton` | `LinkButton.razor` | `text-brand-500 hover:text-brand-700 underline-offset-2 hover:underline` | `Variant`: "brand" \| "accent" \| "muted" \| "white-dim"; `Href` opcional |
| `IconButton` | `IconButton.razor` | `w-9 h-9 rounded-xl border border-slate-200 text-muted hover:text-ink hover:border-slate-300` | `Variant`: "default" \| "ghost" \| "active" |
| `WhatsAppButton` | `WhatsAppButton.razor` | `bg-green-500 text-white hover:bg-green-600 hover:-translate-y-0.5` | `Size`: "full" \| "compact" \| "pill" |
| `FilterPillButton` | `FilterPillButton.razor` | Activo: `bg-ink text-white`; Inactivo: `bg-white border-slate-200 text-muted hover:text-ink rounded-full` | `IsActive`, `Role` |
| `SocialButton` | `SocialButton.razor` | `bg-white/5 border border-white/10 rounded-full text-white hover:bg-white/10 hover:border-white/20` | Para OAuth Google (panel oscuro) |

### 1.2 Botones Inline detectados (sin usar componente)

| Ubicación | Archivo:Línea | Propósito | Clases actuales |
|---|---|---|---|
| ListingsQueue — Approve | `ListingsQueue.razor:186–189` | Aprobar listing (GM) | `text-xs font-semibold bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors active:scale-95` |
| ListingsQueue — Needs Changes | `ListingsQueue.razor:191–194` | Solicitar cambios (GM) | `text-xs font-semibold bg-orange-500 text-white px-4 py-2 rounded-lg hover:bg-orange-600 transition-colors active:scale-95` |
| ListingsQueue — Reject | `ListingsQueue.razor:196–199` | Rechazar listing (GM) | `text-xs font-semibold text-red-600 border border-red-200 px-4 py-2 rounded-lg hover:bg-red-50 transition-colors active:scale-95` |
| ListingsQueue Modal — Cancel | `ListingsQueue.razor:273–275` | Cancelar modal | `flex-1 border border-slate-200 text-sm font-medium text-ink py-2.5 rounded-lg hover:bg-slate-50 transition-colors` |
| ListingsQueue Modal — Confirm Reject | `ListingsQueue.razor:277–280` | Confirmar rechazo | `flex-1 bg-red-600 text-white text-sm font-semibold py-2.5 rounded-lg hover:bg-red-700 transition-colors` |
| ListingsQueue Modal — Send Feedback | `ListingsQueue.razor:311–314` | Confirmar Needs Changes | `flex-1 bg-orange-500 text-white text-sm font-semibold py-2.5 rounded-lg hover:bg-orange-600 transition-colors` |
| ListingsQueue — Retry error | `ListingsQueue.razor:88` | Retry tras error | `text-xs text-red-700 border border-red-300 px-4 py-2 rounded-lg hover:bg-red-100 transition-colors` |
| ReviewsQueue — Approve Review | `ReviewsQueue.razor:143–150` | Aprobar reseña | `bg-green-600 text-white text-xs font-semibold px-4 py-2 rounded-lg hover:bg-green-700 active:scale-95` |
| ReviewsQueue — Reject Review | `ReviewsQueue.razor:151–158` | Rechazar reseña (dinámico) | IsSuspicious: `bg-red-600 text-white hover:bg-red-700`; Normal: `border border-red-200 text-red-600 hover:bg-red-50` — ambos `rounded-lg px-4 py-2` |
| ReviewsQueue — Retry error | `ReviewsQueue.razor:53` | Retry tras error | `text-xs text-red-700 border border-red-300 px-4 py-2 rounded-lg hover:bg-red-100` |
| ReviewsQueue — Paginación | `ReviewsQueue.razor:172–192` | Página anterior/siguiente/número | `w-8 h-8 rounded-lg` + page activa: `bg-blue-600 text-white` |
| Providers — Save Hierarchy | `Providers.razor:157–161` | Guardar jerarquía inline | `text-xs text-blue-600 font-medium hover:text-blue-700 opacity-0 group-hover:opacity-100 transition-opacity` |
| Providers — Disable provider | `Providers.razor:197–201` | Deshabilitar proveedor (dropdown) | `text-sm text-rose-600 hover:bg-rose-50` (inline en AdminActionMenu) |
| Categories — New Category | `Categories.razor:89–96` | Crear categoría nueva | `bg-blue-600 text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors` |
| Categories — Edit (icon) | `Categories.razor:225–233` | Editar categoría | `w-8 h-8 rounded-lg text-muted hover:text-ink hover:bg-slate-100` |
| Categories — Toggle Enable | `Categories.razor:236–247` | Habilitar/deshabilitar | `w-8 h-8 rounded-lg text-muted` + hover condicional: rojo/verde |
| Categories — Delete (icon) | `Categories.razor:250–258` | Eliminar categoría | `w-8 h-8 rounded-lg text-muted hover:text-red-600 hover:bg-red-50` |
| Categories Modal — Close (X) | `Categories.razor:293–300` | Cerrar modal | `w-8 h-8 rounded-lg hover:bg-slate-100` |
| Categories Delete — Cancel | `Categories.razor:344–348` | Cancelar delete confirm | `px-4 py-2 rounded-xl text-sm font-medium text-slate-600 hover:bg-slate-100` |
| Categories Delete — Confirm Delete | `Categories.razor:350–365` | Confirmar delete | `px-4 py-2 rounded-xl text-sm font-semibold bg-red-600 text-white hover:bg-red-700 active:scale-95` |
| CategoryForm — Cancel | `CategoryForm.razor:163–170` | Cancelar formulario | `px-4 py-2 rounded-xl text-sm font-medium text-slate-600 hover:text-slate-800 hover:bg-slate-100` |
| CategoryForm — Save/Create | `CategoryForm.razor:173–201` | Guardar categoría | `px-5 py-2.5 rounded-xl text-sm font-semibold bg-brand-500 text-white hover:bg-brand-600 active:scale-[0.98] shadow-sm` |
| VipConfirmModal — Cancel | `VipConfirmModal.razor:108–111` | Cancelar VIP | `text-sm px-4 py-2 rounded-lg text-muted hover:bg-slate-100` |
| VipConfirmModal — Confirm | `VipConfirmModal.razor:114–118` | Confirmar Grant/Revoke VIP | Revoke: `bg-rose-600 hover:bg-rose-700`; Grant: `bg-amber-500 hover:bg-amber-600` — ambos `rounded-lg px-4 py-2` |
| Listings admin — Retry error | `Listings.razor` (vía AdminDataTable) | Retry | Delegado al componente |
| Manage — New Listing (header) | `Manage.razor:82–91` | Crear listing | `px-4 py-2.5 rounded-2xl bg-brand-500 hover:bg-brand-600 active:scale-[0.98] text-white text-sm font-semibold` |
| Manage — Manage (action) | `Manage.razor:282–290` | Ir al detalle del listing | `px-3 py-2 rounded-xl bg-brand-500 hover:bg-brand-600 text-white text-xs font-semibold` |
| Manage — Edit (action) | `Manage.razor:292–300` | Editar listing | `px-3 py-2 rounded-xl bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium border border-slate-200 hover:border-slate-300` |
| Manage — Pause (action) | `Manage.razor:304–313` | Pausar listing | `px-3 py-2 rounded-xl bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium border border-slate-200` |
| Manage — Public view (action) | `Manage.razor:317–330` | Ver versión pública | `px-3 py-2 rounded-xl bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium border border-slate-200` |
| Manage — Delete (action) | `Manage.razor:332–345` | Eliminar listing | `px-3 py-2 rounded-xl bg-white hover:bg-red-50 text-slate-400 hover:text-red-600 border border-slate-200 hover:border-red-200` |
| ManageDetailOverview — WhatsApp contact | `ManageDetailOverview.razor:302–311` | Contactar vía WhatsApp (reserva) | `bg-green-500 hover:bg-green-600 active:scale-[0.98] text-white text-[10px] font-semibold px-2.5 py-1.5 rounded-lg` |
| ListingsQueue — Paginación activa | `ListingsQueue.razor:234–235` | Página activa | `bg-blue-600 text-white font-semibold` — diferente al token `bg-ink` del componente `FilterPillButton` |

---

## 2. Inconsistencias Detectadas

### 2.1 Dos colores "primary" distintos — la inconsistencia más grave

El sistema tiene **dos azules distintos para acción principal**:

- `bg-brand-500` = `#3B82F6` — usado en `CategoryForm`, `Manage.razor`, navegación principal
- `bg-blue-600` = `#2563EB` — usado en `Categories.razor` (botón "New Category"), paginación activa en `ListingsQueue` y `ReviewsQueue`

Visualmente son azules similares pero **semánticamente distintos y hexadecimalmente diferentes**. El `brand-600` ya es `#2563eb` = exactamente el mismo valor que `blue-600` de Tailwind, pero se llega a él por rutas distintas, lo que crea confusión en el código y riesgo de divergencia futura.

### 2.2 Botones admin sin usar el sistema de componentes

Los botones de acción GM críticos (Approve, Reject, Needs Changes, Approve Review, Reject Review) están definidos **inline con clases literales** en lugar de usar `DangerButton`, `PrimaryButton` o un componente admin reutilizable. Esto significa:

- Radios inconsistentes: componentes usan `rounded-xl` o `rounded-full`; admin usa `rounded-lg`
- Padding inconsistente: componentes usan `py-2` o `py-2.5`; admin usa `py-2` directamente
- Animación activa inconsistente: componentes usan `active:scale-[0.98]`; admin usa `active:scale-95`

### 2.3 Botón "Needs Changes" usa naranja sin token semántico

`bg-orange-500` (inline) no existe en el design token del proyecto. El sistema de colores tiene: `brand` (azul), `accent` (naranja `#F97316`). Sin embargo `orange-500` de Tailwind = `#f97316` es idéntico a `accent.DEFAULT`, pero se referencia directamente sin el token `bg-accent`, rompiendo la abstracción.

### 2.4 Hover con cambio de fondo vs. cambio de borde

- `SecondaryButton`: `hover:text-brand-800 hover:border-brand-400` — solo cambia color de texto y borde, **sin fondo**
- Botones inline Manage Edit/Pause/View: `hover:bg-slate-50 hover:border-slate-300` — cambia fondo y borde
- Sin unificación de criterio para el patrón de hover de botones secundarios

### 2.5 Paginación activa: `bg-blue-600` vs. `bg-ink`

- `FilterPillButton` activo: `bg-ink text-white` (`ink` = `#1E293B`, casi negro)
- Paginación admin activa: `bg-blue-600 text-white` (`#2563EB`, azul medio)

Dos elementos de "selección activa" en la misma UI con colores radicalmente distintos.

### 2.6 Animación hover no unificada

- `PrimaryButton`, `AccentButton`, `WhatsAppButton`: `hover:-translate-y-0.5` + sombra glow
- `SecondaryButton`, `GhostButton`, `LinkButton`: solo `transition-colors`, sin lift
- `FilterPillButton`: `hover:-translate-y-px` (lift pequeño)
- Botones inline admin: solo `transition-colors`, sin lift

### 2.7 Botón Cancel — 3 implementaciones distintas para la misma acción

| Ubicación | Clases |
|---|---|
| ListingsQueue Modal | `border border-slate-200 text-ink py-2.5 rounded-lg hover:bg-slate-50` |
| Categories Delete Modal | `text-slate-600 hover:bg-slate-100 rounded-xl` (sin borde) |
| CategoryForm | `text-slate-600 hover:text-slate-800 hover:bg-slate-100 rounded-xl` (sin borde) |
| VipConfirmModal | `text-muted hover:bg-slate-100 rounded-lg` (sin borde) |

Cuatro patrones distintos para el mismo significado semántico: "cancelar / cerrar".

### 2.8 Tamaños de botones admin críticos insuficientes (accesibilidad)

Los botones Approve/Reject en `ListingsQueue.razor` y `ReviewsQueue.razor` usan `py-2` que da aproximadamente 36px de altura total. El mínimo WCAG AA es 44x44px. Fuera de contexto touch resultan en targets pequeños.

---

## 3. Paleta Definitiva de Botones

### Tokens de referencia del design system

```
brand-500  = #3B82F6  (azul primario)
brand-600  = #2563EB
brand-700  = #1D4ED8
accent     = #F97316  (naranja CTA)
accent-600 = #EA580C
ink        = #1E293B  (casi negro)
muted      = #64748B  (gris medio)
surface    = #F8FAFC  (fondo)
```

---

### `btn-primary` — Acción principal (Save, Approve, Submit, New)

**Contexto:** CTA de máxima jerarquía. Guardar formulario, Crear listing, Aprobar (GM positivo). El azul `brand` transmite confianza institucional — fundamental para acciones GM que deben sentirse **autoritarias y definitivas**.

```
Estado base:
  bg-brand-500 text-white font-semibold
  px-5 py-2.5 rounded-xl
  transition-[transform,box-shadow,background-color] duration-200
  [transition-timing-function:cubic-bezier(0.32,0.72,0,1)]

Estado hover:
  hover:bg-brand-600
  hover:-translate-y-0.5
  hover:shadow-[0_8px_20px_-4px_rgba(59,130,246,0.40)]

Estado active:
  active:translate-y-0 active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed
  disabled:transform-none disabled:shadow-none

Estado loading:
  [cursor-wait] + spinner SVG animate-spin w-4 h-4 + texto
```

**HEX definitivos:**
- Base: `#3B82F6`
- Hover: `#2563EB`
- Glow hover: `rgba(59,130,246,0.40)`
- Ring focus: `#3B82F6` con offset blanco

**Clases completas Tailwind:**
```
bg-brand-500 text-white font-semibold px-5 py-2.5 rounded-xl
transition-[transform,box-shadow,background-color] duration-200 [transition-timing-function:cubic-bezier(0.32,0.72,0,1)]
hover:bg-brand-600 hover:-translate-y-0.5 hover:shadow-[0_8px_20px_-4px_rgba(59,130,246,0.40)]
active:translate-y-0 active:scale-[0.98]
focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2
disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none disabled:shadow-none
cursor-pointer flex items-center justify-center gap-2
```

**Nota para GM:** En contexto admin, los botones Approve deben usar este token. El azul institucional `brand-500` comunica "acción segura y definitiva" — diferenciándose claramente del verde que podría confundirse con un estado de sistema.

---

### `btn-cta` — Llamada a la acción principal al usuario (Contact, WhatsApp, Sign in)

**Contexto:** El naranja `accent` es el CTA de conversión: contactar proveedor via WhatsApp, iniciar sesión, crear cuenta. Solo uno por pantalla. Comunica urgencia y calidez local.

```
Estado base:
  bg-accent text-white font-semibold
  px-5 py-2.5 rounded-full
  transition-[transform,box-shadow,background-color] duration-200
  [transition-timing-function:cubic-bezier(0.32,0.72,0,1)]

Estado hover:
  hover:bg-accent-600
  hover:-translate-y-0.5
  hover:shadow-[0_12px_32px_-4px_rgba(249,115,22,0.40)]

Estado active:
  active:translate-y-0 active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-accent focus-visible:ring-offset-2

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed
  disabled:transform-none disabled:shadow-none
```

**HEX definitivos:**
- Base: `#F97316`
- Hover: `#EA580C`
- Glow hover: `rgba(249,115,22,0.40)`

**Nota:** Este es el botón `PrimaryButton` actual. Se renombra conceptualmente a `btn-cta` para clarificar que es exclusivo para acciones de conversión usuario-proveedor, NO para acciones admin.

---

### `btn-secondary` — Acción secundaria (Cancel, Back, Edit, View)

**Contexto:** Acción complementaria. No destructiva. Sin peso visual. Usa borde `brand` para mantener coherencia con la acción primaria adyacente.

```
Estado base:
  text-brand-600 border border-brand-200 font-medium bg-transparent
  px-5 py-2.5 rounded-xl
  transition-[transform,background-color,border-color,color] duration-200
  [transition-timing-function:cubic-bezier(0.32,0.72,0,1)]

Estado hover:
  hover:bg-brand-50 hover:border-brand-400 hover:text-brand-700
  hover:-translate-y-px

Estado active:
  active:translate-y-0 active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-brand-400 focus-visible:ring-offset-2

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed
```

**HEX definitivos:**
- Texto base: `#2563EB` (brand-600)
- Borde base: `#BFDBFE` (brand-200)
- Fondo hover: `#EFF6FF` (brand-50)
- Borde hover: `#60A5FA` (brand-400)
- Texto hover: `#1D4ED8` (brand-700)

**Clases completas Tailwind:**
```
text-brand-600 border border-brand-200 font-medium bg-transparent px-5 py-2.5 rounded-xl
transition-[transform,background-color,border-color,color] duration-200 [transition-timing-function:cubic-bezier(0.32,0.72,0,1)]
hover:bg-brand-50 hover:border-brand-400 hover:text-brand-700 hover:-translate-y-px
active:translate-y-0 active:scale-[0.98]
focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-400 focus-visible:ring-offset-2
disabled:opacity-50 disabled:cursor-not-allowed
cursor-pointer flex items-center justify-center gap-2
```

---

### `btn-default` — Acción neutra / terciaria (Pause, View public, Retry, Sort)

**Contexto:** Acción sin jerarquía alta. Neutral. No compite visualmente con primary ni secondary. Usado en barras de acciones múltiples.

```
Estado base:
  text-muted border border-slate-200 font-medium bg-white
  px-4 py-2 rounded-xl text-sm
  transition-[background-color,border-color,color] duration-200

Estado hover:
  hover:bg-slate-50 hover:border-slate-300 hover:text-ink

Estado active:
  active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-slate-400 focus-visible:ring-offset-1

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed
```

**HEX definitivos:**
- Texto base: `#64748B` (muted)
- Borde base: `#E2E8F0` (slate-200)
- Fondo hover: `#F8FAFC` (slate-50)
- Texto hover: `#1E293B` (ink)

**Clases completas Tailwind:**
```
text-muted border border-slate-200 font-medium bg-white px-4 py-2 rounded-xl text-sm
transition-[background-color,border-color,color] duration-200
hover:bg-slate-50 hover:border-slate-300 hover:text-ink
active:scale-[0.98]
focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400 focus-visible:ring-offset-1
disabled:opacity-50 disabled:cursor-not-allowed
cursor-pointer flex items-center justify-center gap-2
```

---

### `btn-danger` — Acción destructiva (Delete, Reject, Revoke)

**Dos variantes obligatorias:**

#### Variante `solid` — Destrucción inmediata o irreversible (Delete definitivo, Reject en modal)

```
Estado base:
  bg-red-600 text-white font-semibold
  px-5 py-2.5 rounded-xl
  transition-[transform,box-shadow,background-color] duration-200
  [transition-timing-function:cubic-bezier(0.32,0.72,0,1)]

Estado hover:
  hover:bg-red-700
  hover:shadow-[0_8px_20px_-4px_rgba(220,38,38,0.35)]
  hover:-translate-y-0.5

Estado active:
  active:translate-y-0 active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-red-500 focus-visible:ring-offset-2

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none disabled:shadow-none
```

**HEX definitivos:**
- Base: `#DC2626` (red-600)
- Hover: `#B91C1C` (red-700)
- Glow: `rgba(220,38,38,0.35)`

#### Variante `outline` — Alerta antes de acción destructiva (Reject en lista, botón secundario de peligro)

```
Estado base:
  text-red-600 border border-red-200 font-medium bg-transparent
  px-4 py-2 rounded-xl text-sm
  transition-[background-color,border-color,color] duration-200

Estado hover:
  hover:bg-red-50 hover:border-red-400 hover:text-red-700

Estado active:
  active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-red-400 focus-visible:ring-offset-1
```

**HEX definitivos:**
- Texto: `#DC2626` (red-600)
- Borde: `#FECACA` (red-200)
- Fondo hover: `#FEF2F2` (red-50)
- Borde hover: `#F87171` (red-400)

---

### `btn-warning` — Necesita cambios / Estado de alerta (Needs Changes)

**Contexto NUEVO — formaliza el patrón naranja/amber para moderación.** Distinto del `accent` (naranja de conversión): este es un naranja más oscuro/amber para señalar estados de advertencia que requieren acción del GM.

```
Estado base:
  bg-amber-500 text-white font-semibold
  px-5 py-2.5 rounded-xl
  transition-[transform,background-color] duration-200

Estado hover:
  hover:bg-amber-600 hover:-translate-y-0.5

Estado active:
  active:translate-y-0 active:scale-[0.98]

Estado focus:
  focus-visible:outline-none
  focus-visible:ring-2 focus-visible:ring-amber-500 focus-visible:ring-offset-2

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none
```

**HEX definitivos:**
- Base: `#F59E0B` (amber-500)
- Hover: `#D97706` (amber-600)

**Justificación:** Amber es visualmente distinguible de `accent` naranja (#F97316) y de `red` peligro. En el contexto del GM, verde = Approve, amber = Needs Changes, rojo = Reject forma una triada semántica clara.

---

### `btn-cancel` — Cancelar / Cerrar modal (patrón unificado)

Reemplaza las 4 implementaciones diferentes detectadas.

```
Estado base:
  text-slate-600 font-medium bg-transparent
  px-4 py-2 rounded-xl text-sm
  transition-[background-color,color] duration-200

Estado hover:
  hover:bg-slate-100 hover:text-ink

Estado active:
  active:scale-[0.98]

Estado focus:
  focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-300 focus-visible:ring-offset-1

Estado disabled:
  disabled:opacity-50 disabled:cursor-not-allowed
```

**Nota:** Sin borde. La ausencia de borde lo distingue visualmente del `btn-secondary` (que sí tiene borde). Cancel no compite, no enfatiza — simplemente permite salir.

---

### `btn-icon` — Botón solo icono (Edit, Delete, Toggle, Close modal)

```
Estado base:
  w-9 h-9 rounded-xl flex items-center justify-center
  text-muted border border-slate-200 bg-transparent
  transition-[background-color,border-color,color] duration-200

Variante hover-ink (neutral):
  hover:text-ink hover:bg-slate-100 hover:border-slate-300

Variante hover-danger (delete):
  hover:text-red-600 hover:bg-red-50 hover:border-red-200

Variante hover-success (enable):
  hover:text-green-600 hover:bg-green-50 hover:border-green-200

Estado active:
  active:scale-[0.97]

Estado focus:
  focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-300 focus-visible:ring-offset-1

Estado disabled:
  disabled:opacity-40 disabled:cursor-not-allowed
```

---

### `btn-ghost` — Texto puro sin fondo ni borde (links textuales, navegación, anclas)

```
Estado base:
  text-brand-500 font-medium text-sm
  transition-colors duration-200

Estado hover:
  hover:text-brand-700

Variante muted: text-muted hover:text-ink
Variante white: text-white/40 hover:text-white/70
Variante danger: text-red-500 hover:text-red-700
```

---

## 4. Especificación de Animación Hover Unificada

### Principio rector

Todos los botones con peso visual (primary, cta, secondary, danger solid, warning) usan **lift + glow**. Los botones sin peso (default, cancel, icon, ghost) usan solo **transition-colors**. Esto crea jerarquía de movimiento que refuerza jerarquía visual.

### Curva de easing unificada

```
[transition-timing-function:cubic-bezier(0.32,0.72,0,1)]
```
Ya existe como token `spring` en `tailwind.config.js`. Esta curva da sensación de masa con frenado natural, diferente del aburrido `ease-in-out`.

### Duración

```
duration-200   →  botones con texto (primary, secondary, cta, danger)
duration-150   →  botones de icono y botones pequeños
```

### Tabla de animaciones por variante

| Variante | Hover lift | Hover glow | Active scale |
|---|---|---|---|
| `btn-primary` | `-translate-y-0.5` | `shadow-[0_8px_20px_-4px_rgba(59,130,246,0.40)]` | `scale-[0.98]` |
| `btn-cta` | `-translate-y-0.5` | `shadow-[0_12px_32px_-4px_rgba(249,115,22,0.40)]` | `scale-[0.98]` |
| `btn-secondary` | `-translate-y-px` | ninguno | `scale-[0.98]` |
| `btn-default` | ninguno | ninguno | `scale-[0.98]` |
| `btn-danger solid` | `-translate-y-0.5` | `shadow-[0_8px_20px_-4px_rgba(220,38,38,0.35)]` | `scale-[0.98]` |
| `btn-danger outline` | ninguno | ninguno | `scale-[0.98]` |
| `btn-warning` | `-translate-y-0.5` | ninguno | `scale-[0.98]` |
| `btn-cancel` | ninguno | ninguno | `scale-[0.98]` |
| `btn-icon` | ninguno | ninguno | `scale-[0.97]` |
| `btn-ghost` | ninguno | ninguno | ninguno |

### Política de `prefers-reduced-motion`

Agregar en el CSS global o en la capa `@layer utilities`:

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

Ya existe en `ManageDetailOverview.razor` — debe moverse al layout global.

---

## 5. Justificación de la Paleta

### Por qué azul `brand` = primary y naranja `accent` = cta

En un marketplace local bilingüe la jerarquía de confianza es el activo más valioso:

- **Azul brand (#3B82F6):** Confianza institucional. Los botones del GM (Approve, Save Category, Create, Save Hierarchy) son decisiones que afectan a terceros. El azul comunica autoridad y estabilidad — apropiado para el panel de administración y formularios del proveedor.
- **Naranja accent (#F97316):** Calor local, urgencia de conversión. Los CTAs de contacto (WhatsApp, Sign in, Create listing) son momentos de conversión entre personas. El naranja local, cálido y directo, invita a la acción sin ser agresivo.

Esta separación es deliberada: el GM nunca usa un botón naranja; el usuario nunca topa con un botón azul autoritario en el front público.

### Por qué `btn-warning` usa amber, no el mismo naranja que `accent`

El naranja `accent` (#F97316) es señal de conversión positiva. Si el botón "Needs Changes" usara el mismo color, enviaría una señal contradictoria: "esto es bueno" en vez de "esto requiere atención". Amber (#F59E0B) es visualmente próximo pero semánticamente distinto — en el campo de semáforos semánticos (verde/amber/rojo) es universalmente reconocido como "precaución/atención".

### Por qué la paginación activa debe ser `bg-ink` y no `bg-blue-600`

La paginación es navegación estructural, no una acción. El token `ink` (#1E293B) para estado activo/seleccionado es apropiado porque:

1. No compite con los CTAs azules adyacentes
2. El contraste 15:1 sobre fondo blanco supera WCAG AAA
3. Comunica "estás aquí" de forma neutral, sin implicar que la acción es "blue/brand"

### Accesibilidad WCAG 2.1 AA

| Combinación | Ratio de contraste | Estado WCAG |
|---|---|---|
| `brand-500` (#3B82F6) sobre blanco | 3.1:1 | Falla AA para texto normal |
| `brand-600` (#2563EB) sobre blanco | 4.5:1 | Pasa AA |
| `brand-500` texto blanco (#FFF) | 3.1:1 | Solo válido en tamaño 18px+ o bold 14px+ |
| `accent` (#F97316) sobre blanco | 2.98:1 | **Falla AA** — solo válido en botón con texto blanco grande |
| `red-600` (#DC2626) sobre blanco | 4.7:1 | Pasa AA |
| `amber-500` (#F59E0B) sobre blanco | 2.44:1 | Falla — usar texto oscuro |
| `ink` (#1E293B) sobre blanco | 15.1:1 | Pasa AAA |

**Correcciones requeridas:**

1. `btn-primary` usa `bg-brand-500 text-white` — el azul es válido para texto blanco **solo si el botón tiene font-semibold y px >= 14px** (texto grande/bold = 3:1 mínimo OK). En tamaños xs se debe subir a `brand-600`.
2. `btn-cta` naranja con texto blanco: idéntico caso. Válido si el texto es semibold y >= 14px.
3. `btn-warning` amber: **NUNCA texto blanco**. Debe usar `text-white font-bold` o cambiar a `text-amber-900` con fondo claro. Recomendación: amber sólido `bg-amber-500 text-white font-bold` (3:1 cumple con texto bold grande) para botones GM donde el texto es siempre >=12px semibold.

**Alternativa segura para btn-warning en contextos pequeños:**
```
border border-amber-400 text-amber-700 bg-amber-50 hover:bg-amber-100
```
Ratio amber-700 (#B45309) sobre amber-50 (#FFFBEB) = 4.6:1 — pasa AA.

---

## 6. Tabla Resumen para Implementación

| Token semántico | Fondo base | Texto | Hover fondo | Radio | Uso |
|---|---|---|---|---|---|
| `btn-primary` | `brand-500` `#3B82F6` | `white` | `brand-600` | `rounded-xl` | Save, Approve, Create (admin) |
| `btn-cta` | `accent` `#F97316` | `white` | `accent-600` | `rounded-full` | WhatsApp, Sign in, CTA público |
| `btn-secondary` | `transparent` | `brand-600` | `brand-50` | `rounded-xl` | Back, Cancel-with-border, Edit |
| `btn-default` | `white` | `muted` | `slate-50` | `rounded-xl` | Pause, Sort, View public |
| `btn-danger solid` | `red-600` `#DC2626` | `white` | `red-700` | `rounded-xl` | Delete, Confirm Reject |
| `btn-danger outline` | `transparent` | `red-600` | `red-50` | `rounded-xl` | Reject en lista, acción secundaria destructiva |
| `btn-warning` | `amber-500` `#F59E0B` | `white font-bold` | `amber-600` | `rounded-xl` | Needs Changes (GM) |
| `btn-cancel` | `transparent` | `slate-600` | `slate-100` | `rounded-xl` | Cancelar modales, formularios |
| `btn-icon` | `transparent` | `muted` | conditional | `rounded-xl` | Edit/Delete/Toggle iconos |
| `btn-ghost` | `transparent` | `brand-500` | text only | none | Links textuales, navegación |

---

## 7. Checklist de Implementación para Frontend Developer

- [ ] Actualizar `PrimaryButton.razor` — cambiar `bg-accent` a `bg-brand-500` y actualizar glow a azul (actualmente usa naranja incorrectamente para acciones admin)
- [ ] Mantener `AccentButton.razor` como `btn-cta` — solo para auth y conversión pública
- [ ] Agregar variante `warning` a `DangerButton.razor` o crear `WarningButton.razor` para Needs Changes
- [ ] Reemplazar botones inline en `ListingsQueue.razor` con componentes: Approve → `PrimaryButton`, Needs Changes → `WarningButton`, Reject → `DangerButton variant=outline`
- [ ] Reemplazar botones inline en `ReviewsQueue.razor` equivalente
- [ ] Unificar `btn-cancel` en todos los modales (ListingsQueue, Categories, VipConfirmModal, CategoryForm)
- [ ] Corregir paginación activa: cambiar `bg-blue-600` a `bg-ink` en `ListingsQueue.razor:235` y `ReviewsQueue.razor:182`
- [ ] Corregir `Categories.razor:91` — cambiar `bg-blue-600` a `bg-brand-500` (o mejor: usar `PrimaryButton`)
- [ ] Revisar tamaños en admin: todos los botones de acción deben tener mínimo `py-2.5` para ≥36px, o `py-3` para ≥44px en mobile
- [ ] Agregar `prefers-reduced-motion` al layout global (no inline por componente)
- [ ] Unificar `active:scale-95` → `active:scale-[0.98]` en todos los botones inline

---

*Documento generado por UI/UX Designer Agent para uso exclusivo del Frontend Developer. No modificar código Blazor hasta revisar este documento.*
