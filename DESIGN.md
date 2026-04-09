# DESIGN.md — Lisere UI Design System

Inspired by Sézane's visual identity, adapted for a professional in-store mobile app.

---

## Design Principles

- **Épuré** — no gradients, no shadows, flat surfaces
- **Contrasté** — dark header on light content, clear status colors
- **Typographique** — serif for content, sans-serif condensed for labels/actions
- **Touch-first** — minimum 44px touch targets, bottom navigation, generous spacing

---

## Color Tokens

```css
/* globals.css — shadcn/ui config */
:root {
  /* Backgrounds */
  --background: 0 0% 100%;              /* #fff — card surfaces */
  --background-cream: 30 43% 96%;       /* #f9f4ef — page background (seashell) */
  --background-dark: 0 0% 7%;           /* #121212 — header / status bar */

  /* Text */
  --foreground: 0 0% 7%;                /* #121212 — primary text */
  --muted-foreground: 0 0% 59%;         /* #969696 — secondary labels */
  --subtle: 0 0% 35%;                   /* #555 — tertiary text */

  /* Borders & dividers */
  --border: 0 0% 88%;                   /* #e1e1e1 — default border */
  --border-strong: 0 0% 7%;             /* #121212 — active/selected border */

  /* Brand accent */
  --accent-gold: 38 61% 49%;            /* #b28a2c — in progress, highlights */

  /* Status */
  --status-success: 103 100% 32%;       /* #43a200 — found / delivered */
  --status-error: 349 100% 57%;         /* #e51940 — not found / error */
  --status-pending: 0 0% 7%;            /* #121212 — pending badge */
  --status-inprogress: 38 61% 49%;      /* #b28a2c — in progress badge */

  /* shadcn/ui base */
  --card: 0 0% 100%;
  --card-foreground: 0 0% 7%;
  --primary: 0 0% 7%;
  --primary-foreground: 0 0% 100%;
  --secondary: 30 43% 96%;
  --secondary-foreground: 0 0% 7%;
  --muted: 0 0% 88%;
  --radius: 0rem;                       /* Sezane: zero border-radius on buttons */
}
```

---

## Typography

| Usage | Font | Weight | Size | Letter-spacing |
|---|---|---|---|---|
| Titres, noms articles | `Libre Baskerville` (serif) | 400 | 18–28px | normal |
| Labels, boutons, statuts | `Oswald` (condensed sans) | 300–400 | 10–13px | 1.5–2.5px |
| Corps de texte | `Libre Baskerville` | 400 | 14–16px | 0.5px |
| Méta (zones, catégories) | `Oswald` | 300 | 10–11px | 2px |

```css
/* Import dans index.css */
@import url('https://fonts.googleapis.com/css2?family=Libre+Baskerville:ital,wght@0,400;0,700;1,400&family=Oswald:wght@300;400;500&display=swap');

body {
  font-family: 'Libre Baskerville', serif;
  letter-spacing: 0.5px;
  background-color: #f9f4ef;
  color: #121212;
}

.label {
  font-family: 'Oswald', sans-serif;
  font-size: 11px;
  letter-spacing: 2px;
  text-transform: uppercase;
  color: #969696;
}

.btn-label {
  font-family: 'Oswald', sans-serif;
  font-size: 13px;
  letter-spacing: 2.5px;
  text-transform: uppercase;
}
```

---

## Spacing & Layout

| Token | Value | Usage |
|---|---|---|
| Screen padding | 20–24px | Horizontal padding on all screens |
| Card padding | 14–16px | Internal card padding |
| Section gap | 10–12px | Gap between cards in a list |
| Touch target | min 44px | All interactive elements |
| Bottom nav height | 64px | Fixed bottom navigation |
| Header height | ~80px | Dark top header |

---

## Components

### Button (primary)
```css
background: #121212;
color: #fff;
padding: 16px;
border-radius: 0;           /* No radius — Sezane style */
font-family: 'Oswald';
font-size: 13px;
letter-spacing: 2.5px;
text-transform: uppercase;
```

### Button (outline)
```css
background: transparent;
border: 1px solid #121212;
color: #121212;
/* Same font as primary */
```

### Status Badge

| Status | Background | Text |
|---|---|---|
| Pending | `#121212` | `#fff` |
| InProgress | `#b28a2c` | `#fff` |
| Found | transparent | `#43a200` (border) |
| NotFound | transparent | `#e51940` (border) |
| Delivered | `#43a200` | `#fff` |
| Cancelled | `#e1e1e1` | `#555` |

### Size Chip
```css
/* Available */
border: 1px solid #121212;
color: #121212;
padding: 3px 8px;
font-family: 'Oswald';
font-size: 10px;

/* Unavailable */
border: 1px solid #e1e1e1;
color: #bcbcbc;
text-decoration: line-through;
```

### Card (article / demande)
```css
background: #fff;
padding: 14px;
border-radius: 0;
/* Active state: border-left: 3px solid #121212 */
/* InProgress:   border-left: 3px solid #b28a2c */
```

### Zone Selector (in header)
```css
border: 1px solid #555;
padding: 6px 12px;
font-family: 'Oswald';
font-size: 11px;
color: #fff;
letter-spacing: 1px;
```

### Bottom Navigation
```css
background: #fff;
border-top: 1px solid #e1e1e1;
height: 64px;
/* Active tab: 2px solid #121212 indicator above label */
/* Inactive: color #969696 */
```

### SignalR Status Badge (in header)
```css
/* Connected    */ background: #43a200; width: 8px; height: 8px; border-radius: 50%;
/* Reconnecting */ background: #b28a2c;
/* Disconnected */ background: #e51940;
```

---

## Screen Inventory

| # | Écran | Rôle | Statut |
|---|---|---|---|
| 1 | Login | Auth | ✅ Maquetté |
| 2 | Sélection magasin | Auth | ✅ Maquetté |
| 3 | Recherche article | Seller | ✅ Maquetté |
| 4 | File d'attente | Stockist | ✅ Maquetté |
| 5 | Détail article + sélection taille | Seller | ✅ Maquetté |
| 6 | Mes demandes en cours | Seller | ✅ Maquetté |
| 7 | Traitement d'une demande | Stockist | ✅ Maquetté |
| 8 | Scan barcode `/scan` | Seller | ✅ Maquetté |
| 9 | Admin — Gestion stock | Admin | ✅ Maquetté |

---

## Key UI Decisions

- **Zone selector** : toujours visible dans le header, modal au premier accès si zone non définie
- **Header** : fond `#121212`, titre serif blanc, zone selector à droite
- **Page background** : crème `#f9f4ef`, cartes blanches `#fff`
- **Navigation** : bottom bar pour Seller/Stockist, sidebar desktop pour Admin
- **Tailles épuisées** : barrées + grises (jamais cachées — info utile pour le stockiste)
- **FIFO visuel** : ordre chronologique strict, pas de réorganisation possible
- **SignalR status** : badge coloré discret dans le header (vert/orange/rouge)
- **Border-radius** : 0 partout sauf bottom nav (légèrement arrondi pour les chips de taille)

---

**Version:** 1.0
**Last updated:** March 2026
