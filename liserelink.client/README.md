# Lisere — Frontend PWA

Application de gestion de demandes de liseré avec scan de codes-barres.

## Stack Technique
- React 19 + TypeScript
- Vite (build tool)
- Tailwind CSS v4 + shadcn/ui
- Zustand (state management)
- SignalR (WebSocket temps réel)
- Quagga2 (scanner barcode)

## Installation
```bash
npm install
```

## Développement
```bash
npm run dev          # Serveur dev sur http://localhost:5173
npm run lint         # Vérifier le code
npm run format       # Formater le code
npm run type-check   # Vérifier les types TypeScript
npm test             # Tests unitaires
```

## Build Production
```bash
npm run build        # Compile TypeScript + build Vite
npm run preview      # Prévisualiser le build
```

## Variables d'environnement
Créer `.env.local` (non versionné) :
```env
VITE_API_URL=https://localhost:7000
VITE_SIGNALR_HUB_URL=https://localhost:7000/hubs/requests
```

## Architecture
```
src/
├── components/     # Composants UI réutilisables
├── pages/          # Pages de l'application
├── layouts/        # Layouts (header, footer...)
├── hooks/          # Hooks React custom
├── stores/         # Stores Zustand
├── services/       # Services API
├── utils/          # Fonctions utilitaires
└── types/          # Types TypeScript
```

## Documentation
- [Design System](../../DESIGN.md)
- [Spécifications](../../SPECS.md)
