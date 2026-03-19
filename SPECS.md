# SPECS.md - Lisere Request Management Application

## VISION & CONTEXT

### Problem to solve
Replace the walkie-talkie system for clothing orders in Lisere stores.

**Current issues:**
- Lack of clarity in voice requests
- Inefficiency (waiting for availability, memorization)
- Partial/interrupted communication
- Fragile and unreliable hardware

### Proposed solution
Mobile application (PWA) allowing sellers to submit clothing requests and stockists to process them in real time.

### What this app is — and is NOT
**Lisere is a request management tool.** Sellers send requests for clothing items, stockists fulfill them. That's it.

- ❌ No article creation, update, or deletion — articles come from Lisere.StockApi
- ❌ No stock modification — stock is managed by Lisere.StockApi (admin UI) and decremented at point of sale (out of scope)
- ❌ No inventory management
- ✅ Sellers search articles, select sizes, submit requests
- ✅ Stockists receive, process, and fulfill requests
- ✅ Real-time communication between seller and stockist

### MVP (Minimum Viable Product)
**Main User Story:**
> As a seller, I can send a request for a clothing item (name, print/color, size(s)) based on store stock, and the stockist receives it instantly.

---

## TECHNICAL ARCHITECTURE

### Services Overview

Two independent services sharing `Lisere.Domain`:

```
Lisere.API          → Main app (sellers, stockists, requests)
Lisere.StockApi     → Autonomous stock service (catalogue, stock levels, admin UI)
```

Both services run simultaneously during development. Lisere.API calls Lisere.StockApi via HTTP.

### Authentication Strategy

Both services share the same JWT secret (configured in appsettings). No dedicated Identity service — proportionate to project scale.

- **Lisere.API** issues JWT tokens (ASP.NET Core Identity)
- **Lisere.StockApi** validates JWT tokens using the shared secret (`AddJwtBearer` only, no Identity dependency)
- When Lisere.API calls StockApi admin endpoints, it forwards the user's JWT in the `Authorization` header
- StockApi checks the token + the `Admin` role claim on admin endpoints

```json
{
  "Jwt": {
    "Secret": "...",
    "Issuer": "lisere-api",
    "Audience": "lisere-services"
  }
}
```

---

### Lisere.API — Backend .NET 10

**Architecture: Clean Architecture (4 layers)**

```
Domain/
├── Entities (Request, Article, User, Stock, RequestLine)
├── Enums (RequestStatus, UserRole, ZoneType, ClothingFamily, Size)
├── Interfaces (IRequestRepository, ILocalArticleRepository, IStockService, IExternalStockApiClient, IArticleSyncService)
└── ValueObjects

Application/
├── DTOs (CreateRequestDto, ArticleDto, StockDto, PagedResult<T>)
├── Services (RequestService, ArticleService, StockService, ArticleSyncService)
├── Validators (DataAnnotations)
└── Interfaces

Infrastructure/
├── Persistence (DbContext EF Core, Repositories)
├── SignalR (Hubs for real-time notifications)
├── Identity (ASP.NET Core Identity + JWT issuance)
├── BackgroundJobs (RequestTimeoutService, ArticleSyncService)
└── ExternalServices (ExternalStockApiClient — HTTP client for Lisere.StockApi)

API/
├── Controllers (RequestsController, ArticlesController — GET only)
├── Middlewares (ExceptionHandlingMiddleware, Auth, Logging)
└── Configuration (Program.cs, appsettings)
```

**Technologies:**
- .NET 10
- Entity Framework Core (Code First)
- SQL Server (database: LisereApp)
- ASP.NET Core Identity (auth + JWT issuance)
- SignalR (real-time WebSocket with automatic retry)
- Redis Cache (stock TTL 30s, shared across store instances)

---

### Lisere.StockApi — Autonomous Stock Service

**Architecture: Clean Architecture (4 layers)**

```
Lisere.StockApi.Domain/
├── Entities (StockEntry, Store)
├── Enums (StoreType: Physical, Online)
└── Interfaces (IStockEntryRepository, IStoreRepository)
— references Lisere.Domain for shared enums + Article entity

Lisere.StockApi.Application/
├── DTOs (StockEntryDto, ArticleStockDto, StoreDto, UpdateStockDto)
├── Services (StockService)
└── Interfaces (IStockService)

Lisere.StockApi.Infrastructure/
├── Persistence (StockApiDbContext, Repositories)
└── Configurations (EF Core)

Lisere.StockApi.API/
├── Controllers (StockController, ArticlesController, AdminStockController)
├── Middlewares (ExceptionHandlingMiddleware — same ProblemDetails standard as Lisere.API)
└── Configuration (Program.cs — port 5200)
```

**Technologies:**
- .NET 10
- Entity Framework Core (Code First)
- SQL Server (database: LisereStockApi — separate from main app)
- JWT validation only (AddJwtBearer, shared secret with Lisere.API)

**Role:**
- Source of truth for article catalogue and stock levels
- Exposes read endpoints consumed by Lisere.API
- Exposes admin write endpoints (JWT required, Admin role) for stock manipulation
- Admin microfrontend integrated in the main React frontend (route `/admin`)

---

### Frontend - React PWA

**Type:** Progressive Web App (installable, partial offline support)

**Stack:**
- React 19 + TypeScript
- Vite (build tool)
- React Router v6 (navigation — classic mode, no loaders/actions)
- Axios (REST API — single `apiClient` instance with interceptors)
- @microsoft/signalr (SignalR client)
- Zustand (state management) + Zustand persist (offline state)
- Tailwind CSS + shadcn/ui (mobile-first styling + accessible components)
- `sonner` (toast notifications)
- @ericblade/quagga2 (EAN-13 barcode scanning)
- `idb-keyval` (IndexedDB for offline data persistence)

**PWA Features:**
- Service Worker
- Manifest.json
- Web Push Notifications

**Routing & Access Control:**
- React Router v6 classique
- `ProtectedRoute` wrapper component — redirects by role (Seller / Stockist / Admin)
- Unauthenticated → redirect `/login`
- Wrong role → redirect `/unauthorized`

**Connection Flow:**
```
Login (email/password) → Store selection → App
```
- Store is selected per session (employees can work across multiple stores)
- `selectedStoreId` + `selectedStoreName` stored in `useAuthStore`
- All stock/request queries scoped to `selectedStoreId`
- Backend must expose `GET /api/stores` (proxy from StockApi)

**Zone Selection:**
- Zone is NOT a login step — it is a persistent selector in `SellerLayout` and `StockistLayout`
- `selectedZone` stored in `useAuthStore`, changeable at any time during the session
- On first app load after store selection: zone selector modal shown if `selectedZone === null`
- Zone is **mandatory to submit a request** — submit button disabled if no zone selected
- Zone determines which stockist receives the request (zone-based assignment)

**Layouts (role-based):**
- `SellerLayout` — bottom nav: Recherche | Demandes | Historique
- `StockistLayout` — bottom nav: File d'attente | En cours | Historique
- `AdminLayout` — top bar + sidebar (desktop-optimised)
- `AuthLayout` — login/store selection screens (no nav)

**Responsive Strategy:**
- Seller/Stockist: mobile-first (iPhone SE → iPad), bottom navigation
- Admin: desktop-optimised (sidebar, data tables)

**Zustand Stores:**

| Store | Contenu | Persisté |
|---|---|---|
| `useAuthStore` | `token`, `user` (id, firstName, lastName, role, assignedZone), `selectedStoreId`, `selectedStoreName` | ✅ localStorage |
| `useRequestStore` | demandes en cours, historique | ✅ IndexedDB |
| `useArticleStore` | résultats de recherche, article sélectionné, dernier état connu (offline) | ✅ IndexedDB |
| `useSignalRStore` | connexion HubConnection, status (disconnected/connecting/connected/reconnecting) | ❌ |
| `useNotificationStore` | notifications non lues, compteur badge | ❌ |

**JWT Strategy:**
- Stored in `localStorage` via `zustand/persist`
- No refresh token for MVP — JWT lifetime set to 8-12h in `appsettings.json`
- Single `apiClient` (Axios) with request interceptor → auto-injects `Authorization: Bearer`
- Response interceptor → 401 triggers logout + redirect `/login`
- ProblemDetails errors surfaced via `sonner` toasts

**SignalR Lifecycle:**
- Connection started after store selection (post-login)
- Connection stopped on logout
- Automatic reconnect with exponential backoff: `[0, 2000, 5000, 10000]`
- Connection status displayed in UI (badge)

**Error Handling:**
- Form validation errors → inline (under field)
- Business errors (stock unavailable, request not editable...) → `sonner` toast
- Network / 500 errors → `sonner` toast (generic message)
- All centralized in Axios response interceptor

**Barcode Scan:**
- Dedicated full-screen page `/scan` (better camera framing)
- On success → auto-navigate back to search with article pre-filled
- Uses `@ericblade/quagga2`

**Offline Strategy:**
- `useAuthStore` → `localStorage` (token survives page refresh)
- `useRequestStore` + `useArticleStore` → IndexedDB via `idb-keyval` (current requests + last known articles)
- Seller sees requests with "hors ligne" badge when disconnected
- No offline request creation (stock check impossible)
- SignalR auto-reconnect on network restore

**Tests:**
- Reportés en Phase 5 (Vitest + React Testing Library)
- E2E Playwright couvre les workflows critiques (Phase 5)

**Main Screens:**
1. Login
2. Store Selection
3. Article Search (search bar + barcode scan button → `/scan`)
5. Current Requests List
6. History
7. Admin — Stock Management (route `/admin`, Admin role only)

---

## STANDARDS & BEST PRACTICES

### Code Quality

**Frontend:**
- ESLint + Prettier (auto formatting, strict rules)
- TypeScript strict mode
- No `any`, complete typing
- Functional components + hooks only

**Backend (both services):**
- SonarAnalyzer (bug/vulnerability detection)
- Code analysis enabled (warnings as errors in prod)
- Conventions: PascalCase classes/methods, camelCase variables

### Security

- **HTTPS mandatory** in production
- **CORS** strictly configured (authorized origins only)
- **Rate Limiting**: 100 req/min per user
- **Input validation** server-side (DataAnnotations on all DTOs)
- **JWT** for authentication (issued by Lisere.API, validated by both services)
- **No sensitive data** in logs

### Database

**Lisere.API — Entity Framework Core:**
- Code First with migrations
- Soft Delete (`IsDeleted` field)
- Full audit trail on all entities:
  ```csharp
  public DateTime CreatedAt { get; set; }
  public string CreatedBy { get; set; }
  public DateTime? ModifiedAt { get; set; }
  public string? ModifiedBy { get; set; }
  public bool IsDeleted { get; set; }
  ```

**Lisere.StockApi — Entity Framework Core:**
- Code First with migrations
- No soft delete — stock entries are operational data, physical deletion is acceptable
- No full audit trail — `StockEntry` uses `LastUpdatedAt` only (lightweight, updated on every stock change)
- `Store` has no audit trail (static reference data)

**Conventions (both services):**
- Table and entity names in English
- Relations with navigation properties
- Indexes on frequently filtered columns

### Error Handling

Both services follow the same standard: **ProblemDetails (RFC 7807)** via a global `ExceptionHandlingMiddleware`.

```json
{
  "type": "https://api.lisere.app/errors/stock-unavailable",
  "title": "Stock insuffisant",
  "status": 400,
  "detail": "Robe Rouge taille M non disponible",
  "instance": "/api/requests/123"
}
```

- BusinessException / StockException → 400
- Not found → 404
- Unhandled → 500 (stack trace in dev only, generic message in prod)
- Logs: Info/Warning/Error via ILogger (structured JSON)

### Logging

Both services use `ILogger<T>` (interface standard ASP.NET Core) throughout the codebase.

**Nice-to-have — Serilog:**
Serilog can be added as a logging provider behind `ILogger` without modifying existing code.
It is recommended once the deployment target is known, as the choice of sinks (Console, File, Seq, Datadog, etc.)
depends on the infrastructure. Adding Serilog does not require any code changes — only `Program.cs` configuration
and NuGet packages.

Relevant packages when needed:
- `Serilog.AspNetCore`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`
- Additional sinks depending on target (Seq, Datadog, Application Insights...)

### Performance

- **Redis Cache** for stock (TTL 30s) — Lisere.API only, shared across store instances
- **Mandatory pagination**: max 50 results per page — applies to both services
- **Lazy Loading** images (React Suspense/Intersection Observer)
- **SignalR reconnection** automatic (exponential backoff)

### Tests

**Minimum coverage: 80% — applies to both services**

**Lisere.API:**
1. **Unit** (xUnit): RequestService, ArticleService, StockService, ArticleSyncService, Domain logic
2. **Integration** (WebApplicationFactory): Repositories, API endpoints
3. **E2E** (Playwright): Full seller/stockist workflows
4. **Mock** Lisere.StockApi via NSubstitute on IExternalStockApiClient

**Lisere.StockApi:**
1. **Unit** (xUnit): StockService — UpdateStock (negative quantity rejected), GetStock (per store), article catalogue
2. **Integration** (WebApplicationFactory): StockController, ArticlesController, AdminStockController (JWT Admin required)
3. No E2E — StockApi has no UI of its own

**Shared tooling:** xUnit, NSubstitute (or Moq), WebApplicationFactory

---

## BUSINESS DOMAIN

### Main Entities — Lisere.Domain (shared)

**1. Request (Aggregate Root)**
```csharp
public class Request
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid? StockistId { get; set; }
    public ZoneType Zone { get; set; }
    public RequestStatus Status { get; set; }
    public List<RequestLine> Lines { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public User Seller { get; set; }
    public User? Stockist { get; set; }
}
```

**2. RequestLine (pivot entity)**
```csharp
public class RequestLine
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ArticleId { get; set; }  // FK to StockApi article — no navigation property
    public string ColorOrPrint { get; set; }
    public List<Size> RequestedSizes { get; set; }
    public int Quantity { get; set; }
    public RequestLineStatus Status { get; set; } // Pending, Found, NotFound, AlternativeProposed

    // Alternative fields (MVP — Option A: flat fields on RequestLine)
    public Guid? AlternativeArticleId { get; set; }
    public string? AlternativeColorOrPrint { get; set; }
    public List<Size>? AlternativeSizes { get; set; }
    public bool AlternativeStockOverride { get; set; } // true = stockist forced out-of-stock item

    // Navigation
    public Request Request { get; set; }
    // No Article navigation — Article is not an entity in Lisere.API
}
```

**3. Article — DTO only in Lisere.API (no entity, no DB table)**

`Article` is NOT a domain entity in `Lisere.Domain`. It is represented solely as `ArticleDto` in `Lisere.Application.DTOs`.
Articles are fetched live from `Lisere.StockApi` via `IExternalStockApiClient`. No local storage, no sync, no `DbSet<Article>` in `LisereDbContext`.

```csharp
// Lisere.Application.DTOs.ArticleDto
public class ArticleDto
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } // EAN-13
    public string Family { get; set; }
    public string Name { get; set; }
    public string ColorOrPrint { get; set; }
    public List<string> AvailableSizes { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
}
```

**4. User**
```csharp
public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserRole Role { get; set; } // Seller, Stockist, Admin
    public ZoneType? AssignedZone { get; set; }
}
```

**5. Stock (value object — read from Lisere.StockApi, cached in Redis)**
```csharp
public class Stock
{
    public Guid ArticleId { get; set; }
    public Size Size { get; set; }
    public int AvailableQuantity { get; set; }
}
```

---

### Main Entities — Lisere.StockApi.Domain

**1. StockEntry**
```csharp
public class StockEntry
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public Size Size { get; set; }
    public int AvailableQuantity { get; set; }
    public StoreType StoreType { get; set; }
    public string StoreId { get; set; }
    public DateTime LastUpdatedAt { get; set; } // lightweight audit: last write only
}
```

**2. Store**
```csharp
public class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public StoreType Type { get; set; } // Physical, Online
}
```

---

### Enums

**RequestStatus:** Pending, InProgress, AwaitingSellerResponse, Delivered, Unavailable, Cancelled

**RequestLineStatus:** Pending, Found, NotFound, AlternativeProposed

**ZoneType:** RTW, FittingRooms, Checkout, Reception, Custom

**ClothingFamily:**
- COA (Coats), JAC (Jackets), TSH (T-shirts), SWE (Sweaters), VES (Vests)
- JEA (Jeans), PAN (Pants), SHO (Shorts), SKI (Skirts), DRE (Dresses)
- SHI (Shirts), BLO (Blouses)
- SHE (Shoes), BEL (Belts), BAG (Bags), JEW (Jewelry)

**Size:** XXS, XS, S, M, L, XL, XXL, OneSize

**StoreType:** Physical, Online

---

## WORKFLOWS

### Seller Workflow

1. **Login** → Email/Password (JWT)
2. **Zone Selection** → via zone selector in layout (mandatory before submitting, changeable anytime)
3. **Article Search:**
   - Search bar (name, family) — results from local DB (synced from StockApi)
   - Stock availability checked via Lisere.StockApi (Redis TTL 30s) at search time
   - OR Barcode scan (EAN-13 via smartphone camera, @ericblade/quagga2)
4. **Color/Print + Size(s) Selection**
   - Multiple sizes allowed (customer hesitates S/M)
5. **Add to cart OR Submit**
   - Add → back to search
   - Submit → stock re-checked → Send request + Notify stockist
6. **Notifications received:**
   - Request accepted
   - Alternative proposed (other color/size)
   - Article delivered / Unavailable
7. **Request management:**
   - View current requests
   - Modify / Cancel (only while Pending)
   - Accept/Decline alternatives
8. **History** (current day)

### Stockist Workflow

1. **Login** → Email/Password
2. **Zone Selection** → via zone selector in layout (mandatory to receive requests, changeable anytime)
3. **Receive Notification** (push + sound + vibration)
4. **View request** → Article, color, size(s), seller
5. **Processing:**
   - Mark "In Progress"
   - Search article in stock
   - Mark "Found" OR "Not Found"
   - Propose alternative if needed (see Alternative Flow below)
6. **Delivery** → Bring to floor
7. **Visibility:**
   - List all pending requests
   - Day history
8. **Strict FIFO** (no manual prioritization)
9. **No stock reservation** — first stockist to find the article wins. Second stockist declares "Not Found"

### Alternative Proposal Flow (Stockist → Seller)

1. Stockist clicks "Proposer une alternative"
2. **Scan barcode first** (full-screen `/scan`) — primary path
3. If no barcode label → button "Rechercher manuellement" → article search screen
4. Select alternative article + size(s)
5. If size is out-of-stock (greyed out): stockist can **override** with confirmation warning
   - "Article non en stock — confirmer quand même ?"
   - Sets `AlternativeStockOverride = true` on RequestLine
6. Confirm → `RequestLineStatus` → `AlternativeProposed`, `RequestStatus` → `AwaitingSellerResponse`
7. Seller receives push notification: "Alternative proposée : [Article] · [Coloris] · [Taille(s)]"
8. Seller views request → sees alternative details
9. **ACCEPTER** → request updated with alternative article, status → `InProgress`
10. **REFUSER** → status → `Unavailable`

> ⚠️ Stock override (step 5) is allowed only for alternatives, never for initial requests.
> 📝 Nice-to-have (post-MVP): auto-update stock when `AlternativeStockOverride = true`

---

### Admin Workflow

- Create user accounts (sellers/stockists)
- View all requests (all stores)
- Manage custom zones
- **Stock management via `/admin` route** (microfrontend integrated in React app):
  - View stock by article / store / size
  - Manually update stock quantities (for testing and operations)
  - View article catalogue
- Statistics (post-MVP)

### Nice-to-have (post-MVP)

- **i18n frontend** : internationalisation des labels via `react-i18next`. Actuellement les traductions FR sont hardcodées dans `formatters.ts` — migration triviale si besoin multi-langue (refacto `formatters.ts` uniquement, zéro impact backend).
- **Stock override auto-update** : mettre à jour le stock automatiquement quand `AlternativeStockOverride = true` sur une `RequestLine` livrée.
- **NSwag / Swagger codegen** : génération automatique des types TypeScript (`enums.ts`, `types/index.ts`) depuis le Swagger .NET. Élimine la synchronisation manuelle — un `npx nswag run` regénère tous les types depuis le backend. À mettre en place si les enums/DTOs évoluent fréquemment.
- **Temporal API** : migration de `formatDate` (et toute future manipulation de dates) vers l'API native `Temporal` (ES2026, Stage 4 atteint le 11 mars 2026). Actuellement Chrome 144+, Firefox 139+ et Edge 144+ la supportent nativement — Safari encore en preview. Remplace `new Date()` + `Intl.DateTimeFormat` par `Temporal.Instant.from(date).toZonedDateTimeISO('Europe/Paris')`. Polyfill disponible : `@js-temporal/polyfill` pour la compatibilité Safari.

---

## BUSINESS RULES

### Validations

1. **A request without stock = impossible**
   - Stock checked via Lisere.StockApi before creation
   - If stock = 0 → "Unavailable" message, request not created

2. **Stock check at search time + re-check at submission**
   - Search: Redis cache (TTL 30s) — acceptable 30s lag
   - Submission: fresh check to avoid race conditions

3. **Multiple sizes same article = allowed**
   - Customer hesitates between S and M
   - Stockist brings both

4. **No stock decrement on delivery**
   - Stock is decremented at point of sale by the cashier system (out of scope)
   - Lisere only tracks request status, not physical stock movement

5. **Auto-cancellation after 30 min** if not processed
   - Handled by `RequestTimeoutService` (BackgroundService)
   - Runs every minute, checks `Pending` requests older than 30 min
   - Status → `Cancelled`, SignalR notification sent to seller

6. **Request editable** while Status = `Pending`
   - If `InProgress` → must cancel then recreate

### Priorities & Assignment

- **Strict FIFO** (First In First Out)
- **Zone-based assignment:** request sent to zone stockist only
- **No manual reassignment**
- **No stock reservation:** no locking mechanism, first come first served

### Stock per Store

- Each store (Physical or Online) has its own independent stock
- Lisere.API always queries stock for the seller's current store context
- Online stock is tracked separately and is generally higher than physical

### History & Stats

- **Retention: current day**
- **Post-MVP:**
  - Average processing time
  - Most requested articles
  - Automated end-of-day export (CSV via BackgroundService)

---

## TECHNICAL CONSTRAINTS

### Performance

- **Max simultaneous users:** ~100 across all stores (~20 stores)
- **Per store:** ~5 sellers + ~3 stockists
- **Peak requests: 100/hour** (Saturday afternoon)
- **SignalR latency: < 500ms**

### Real-time Notifications

**Mandatory SignalR with:**
- Automatic reconnection (exponential backoff retry)
- Network disconnection handling
- Message queue if temporarily offline

**Notification triggers:**
- New request → Stockist
- Request accepted → Seller
- Article found/not found → Seller
- Alternative proposed → Seller
- Request cancelled → Stockist

### Article Strategy

- Articles are fetched **live** from Lisere.StockApi via `IExternalStockApiClient` — no local DB table, no sync service
- `ArticleService` in Lisere.Application delegates directly to `IExternalStockApiClient`
- Stock levels fetched live from Lisere.StockApi via Redis cache (TTL 30s)
- If Lisere.StockApi is down → log warning, return empty result, do NOT propagate exception, block new request creation

### Mobile First

- Installable PWA (add to home screen)
- Responsive design (iPhone SE → iPad)
- EAN-13 barcode scan via native camera (@ericblade/quagga2)
- Web push notifications (requires HTTPS)

### Webhook — Invalidation cache stock temps réel

Lisere.StockApi notifie Lisere.API en temps réel à chaque modification de stock,
permettant une invalidation immédiate du cache Redis. Le TTL 30s reste en place
comme filet de sécurité si un webhook est raté.

**Côté Lisere.StockApi :**
- `IWebhookNotifier` dans Application/Interfaces
- `WebhookNotifier` dans Infrastructure (HttpClient)
- URL cible configurable : `Webhooks:LisereApiUrl` dans appsettings.json
- Retry 3 tentatives avec backoff exponentiel
- Signature HMAC-SHA256 dans le header `X-Webhook-Signature` (secret partagé en config)
- Appelé dans `StockService.UpdateStockAsync` après l'upsert

**Côté Lisere.API :**
- `POST /webhooks/stock` dans un `WebhooksController`
- Vérification signature HMAC avant traitement
- Invalidation clé Redis `stock:{articleId}:{storeId}`
- Retourne 200 OK immédiatement

**Tests :**
- Unit : `WebhookNotifier` (mock HttpClient), signature HMAC
- Integration : endpoint webhook avec signature valide/invalide,
  vérification invalidation cache

---

## Lisere.StockApi — Autonomous Stock Service

### Role

Autonomous HTTP service exposing the article catalogue and stock levels per store.
It is the **source of truth** for articles and stock. Lisere.API consumes this service in read mode.

The base URL is configurable in Lisere.API's `appsettings.json`:

```json
{
  "ExternalStockApi": {
    "BaseUrl": "https://localhost:5200"
  }
}
```

### Endpoints — Read (consumed by Lisere.API)

#### `GET /api/articles?page=1&pageSize=50`
Returns all catalogue articles (paginated, max 50).

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "barcode": "1234567890123",
      "family": "DRE",
      "name": "Robe Emma",
      "colorOrPrint": "Rouge",
      "availableSizes": ["XS", "S", "M", "L"]
    }
  ],
  "totalCount": 20,
  "page": 1,
  "pageSize": 50
}
```

#### `GET /api/articles/{barcode}`
Returns an article by EAN-13 barcode. Returns `404` if not found.

#### `GET /api/stock/{articleId}?storeId=xxx`
Returns stock levels for all sizes of an article, for a given store.

```json
[
  {
    "articleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "size": "S",
    "availableQuantity": 3,
    "storeType": "Physical",
    "storeId": "paris-opera"
  },
  {
    "articleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "size": "M",
    "availableQuantity": 0,
    "storeType": "Physical",
    "storeId": "paris-opera"
  }
]
```

#### `GET /api/stock/articles?storeId=xxx&page=1&pageSize=50`
Returns all articles with their stock for a given store (paginated, max 50).

### Endpoints — Admin (JWT required, Admin role)

#### `PUT /api/admin/stock`
Updates the quantity for an article/size/store. Requires valid JWT with Admin role claim.

```json
{
  "articleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "size": "M",
  "storeId": "paris-opera",
  "newQuantity": 5
}
```
Returns `204 No Content`. Returns `400` if quantity is negative.

### Dev Seed Data

| StoreId | Name | Type |
|---|---|---|
| paris-2 | Paris 2 | Physical |
| paris-4 | Paris 4 | Physical |
| lyon-bellecour | Lyon Bellecour | Physical |

20 articles covering all ClothingFamily values, with varied stock per store and size (some at 0 to test unavailability). Note: Online store (warehouse) is excluded from the app — sellers only work in physical stores.

### Stock Business Rules

- Quantity cannot be negative
- Only Admin can modify stock via the admin endpoint (JWT + Admin role)
- Online stock is independent from physical stock
- No stock reservation — first come first served

---

## DEVELOPMENT DIRECTIVES

### General Principles

1. **Professional solutions ALWAYS** — no hacky shortcuts
2. **Concise and iterative** — one step at a time, wait for validation
3. **Security by default** — input validation everywhere, HTTPS + CORS + Rate limiting
4. **Performance from the start** — pagination, cache, lazy loading

### Suggested Development Order

**Phase 0 — Lisere.StockApi (first, enables realistic testing of everything)**
1. Clean Architecture setup
2. Domain entities + EF Core + migrations
3. Repositories + Services
4. Controllers + Seed data (20 articles, 3 stores, varied stock)

**Phase 1 — Lisere.API Backend Foundations**
1. Domain corrections (ILocalArticleRepository read-only, IExternalStockApiClient, IArticleSyncService)
2. DTOs + Mapping
3. Application Services (RequestService, ArticleService, StockService, ArticleSyncService)
4. Program.cs configuration

**Phase 2 — Auth & Security**
1. ASP.NET Identity + JWT (Lisere.API issues tokens)
2. JWT validation in Lisere.StockApi (shared secret)
3. CORS + Rate Limiting
4. ProblemDetails middleware (both services)

**Phase 3 — Real-time & Cache**
1. SignalR Hub
2. Real-time notifications
3. RequestTimeoutService (BackgroundService)
4. Webhook StockApi → Lisere.API (invalidation cache Redis temps réel)

**Phase 4 — Frontend**
1. React PWA + TypeScript setup
2. Auth flow
3. Main screens (seller + stockist)
4. SignalR client
5. Zustand + offline persistence
6. PWA manifest + Service Worker
7. Admin microfrontend (`/admin` route — stock management)

**Phase 5 — Tests & Polish**
1. Unit + integration tests (both services, 80% coverage)
2. E2E tests Playwright (Lisere.API workflows only)
3. Security audit
4. Performance optimizations

---

## IMPORTANT NOTES

- **Language:** Code in English, UI/messages in French
- **Timezone:** Europe/Paris
- **Date format:** dd/MM/yyyy HH:mm
- **Environments:** Dev → Staging → Prod (HTTPS mandatory Staging+)

---

**Version:** 3.4
**Last updated:** March 2026
