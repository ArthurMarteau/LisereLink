# Lisere Request Management App

PWA for managing clothing requests between sellers and stockists in retail stores. Replaces walkie-talkie system.

## What this app is — and is NOT
- ✅ Sellers search articles, select sizes, submit requests
- ✅ Stockists receive, process, and fulfill requests
- ❌ No article creation/update/delete — articles are managed by Lisere.StockApi only
- ❌ No stock modification — stock is read-only from Lisere.StockApi
- ❌ ArticlesController in Lisere.API exposes GET only (no POST, PUT, DELETE)

---

## Stack

### Lisere.API (main app)
- **Backend:** .NET 10, EF Core (Code First), SQL Server, SignalR, ASP.NET Identity + JWT, Redis
- **Frontend:** React 18 + TypeScript, Vite, Zustand, Tailwind CSS, @ericblade/quagga2 (EAN-13), @microsoft/signalr
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)

### Lisere.StockApi (autonomous stock service — already implemented)
- **Backend:** .NET 10, EF Core (Code First), SQL Server (separate DB), port 5200
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)
- **Shared:** Lisere.Domain (enums only — Article is NOT a shared entity)

---

## Project Structure

```
src/
├── Lisere.Domain/                        # Shared entities, Enums, Interfaces (zero dependencies)
├── Lisere.Application/                   # DTOs, Services, Validators, Interfaces
├── Lisere.Infrastructure/                # EF Core, Repositories, SignalR, Identity, BackgroundJobs
├── Lisere.API/                           # Controllers, Middlewares, Program.cs
├── Lisere.Tests/                         # xUnit, unit + integration tests
└── Lisere.StockApi/
    ├── Lisere.StockApi.Domain/
    ├── Lisere.StockApi.Application/
    ├── Lisere.StockApi.Infrastructure/
    ├── Lisere.StockApi.API/              # port 5200
    └── Lisere.StockApi.Tests/

frontend/
└── src/
    ├── components/
    ├── pages/
    │   └── admin/                        # Admin microfrontend (stock management)
    ├── stores/                           # Zustand stores
    ├── services/                         # API + SignalR clients
    ├── types/
    └── hooks/
```

---

## Commands

```bash
# Lisere.API (main app)
dotnet build
dotnet test
dotnet run --project src/Lisere.API
dotnet ef migrations add <Name> --project src/Lisere.Infrastructure --startup-project src/Lisere.API
dotnet ef database update --project src/Lisere.Infrastructure --startup-project src/Lisere.API

# Lisere.StockApi (run in parallel during dev)
dotnet run --project src/Lisere.StockApi/Lisere.StockApi.API
dotnet ef migrations add <Name> --project src/Lisere.StockApi/Lisere.StockApi.Infrastructure --startup-project src/Lisere.StockApi/Lisere.StockApi.API

# Frontend
cd frontend && npm install
npm run dev
npm run build
npm run lint
npm run test
```

---

## Code Conventions

### C# / Backend
- PascalCase: classes, methods, properties
- camelCase: local variables, parameters
- All entities inherit `BaseEntity` which has: `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`, `IsDeleted` (soft delete)
- User does NOT inherit BaseEntity (inherits IdentityUser<Guid>) — audit fields duplicated manually
- StockApi entities use `LastUpdatedAt` only (no soft delete, no full audit trail — intentional)
- Repository Pattern for data access — never access DbContext directly from services
- DataAnnotations on all DTOs for validation
- ProblemDetails (RFC 7807) for all API errors — both services
- Async/await everywhere (no `.Result` or `.Wait()`)
- No business logic in controllers — controllers call Application services only
- ILogger<T> for logging throughout (Serilog is a nice-to-have, to be added post-MVP)

### TypeScript / Frontend
- Strict mode, zero `any`
- Functional components + hooks only
- Zustand for state management (no Context API for shared state)
- Axios for REST, @microsoft/signalr for real-time
- Tailwind CSS only (no inline styles, no CSS modules)

### General
- **Code in English, UI/messages in French**
- Timezone: Europe/Paris
- Date format: dd/MM/yyyy HH:mm
- All API responses paginated (max 50 per page) — both services
- No sensitive data in logs

---

## Key Entities (Lisere.Domain — shared)

- `Request` (aggregate root) → has many `RequestLine`
- `RequestLine` → links Request to ArticleId (Guid) + size + quantity + status — NO navigation to Article entity
- `User` (extends IdentityUser<Guid>) → roles: Seller, Stockist, Admin
- `Stock` → value object, read-only from Lisere.StockApi, cached in Redis (TTL 30s)

> ⚠️ `Article` is NOT an entity in Lisere.Domain. It is represented solely by `ArticleDto` in Lisere.Application.DTOs.
> Articles are fetched live from Lisere.StockApi via `IExternalStockApiClient`. No DbSet<Article> in LisereDbContext.

## Key Entities (Lisere.StockApi.Domain)

- `Article` → source of truth for articles (Barcode, Family, Name, ColorOrPrint, AvailableSizes, Price?, ImageUrl?, LastUpdatedAt) — NO BaseEntity, physical delete only
- `StockEntry` → article + size + quantity + store (LastUpdatedAt only)
- `Store` → physical store or online

---

## Key Enums

### Shared (Lisere.Domain)
- `RequestStatus`: Pending, InProgress, Delivered, Unavailable, Cancelled
- `RequestLineStatus`: Pending, Found, NotFound
- `UserRole`: Seller, Stockist, Admin
- `ZoneType`: RTW, FittingRooms, Checkout, Reception, Custom
- `ClothingFamily`: COA, JAC, TSH, SWE, VES, JEA, PAN, SHO, SKI, DRE, SHI, BLO, SHE, BEL, BAG, JEW
- `Size`: XXS, XS, S, M, L, XL, XXL, OneSize

### Lisere.StockApi.Domain
- `StoreType`: Physical, Online

---

## Authentication

- **Lisere.API** issues JWT tokens (ASP.NET Core Identity)
- **Lisere.StockApi** validates JWT tokens only (AddJwtBearer, shared secret — no Identity)
- Shared JWT config: Secret, Issuer="lisere-api", Audience="lisere-services"

---

## Business Rules (critical)

1. **Stock check mandatory before request creation** — if stock = 0 → BusinessException
2. **Stock check at search time (Redis TTL 30s) + re-check at submission** (fresh call)
3. **Request editable only while Status = Pending** — otherwise BusinessException
4. **Auto-cancellation after 30 min** — RequestTimeoutService (BackgroundService)
5. **Strict FIFO** — no manual prioritization
6. **No stock decrement on delivery** — stock managed at point of sale (out of scope)
7. **ArticlesController = GET only** — never add POST/PUT/DELETE

---

## Redis Cache

- Key format: `"stock:{articleId}:{storeId}:{size}"`
- TTL: 30 seconds
- Used in Lisere.API only (StockApi is the source of truth)

---

## ExternalStockApi

- BaseUrl configured in appsettings: `ExternalStockApi:BaseUrl` (default: https://localhost:5200)
- If StockApi is down → log warning, return empty list, do NOT propagate exception
- JWT forwarded in Authorization header for admin calls

---

## Error Handling

Both services use ExceptionHandlingMiddleware with ProblemDetails (RFC 7807):
- `BusinessException` → 400
- `KeyNotFoundException` → 404
- Unhandled → 500 (stack trace in dev only, generic message in prod)

```json
{
  "type": "https://api.lisere.app/errors/...",
  "title": "...",
  "status": 400,
  "detail": "...",
  "instance": "/api/..."
}
```

---

## Testing

- xUnit for unit + integration tests — both services, 80% coverage minimum
- WebApplicationFactory for API integration tests
- NSubstitute for mocking
- E2E with Playwright for Lisere.API workflows only
- Run single test: `dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"`

---

## Don't

- Don't put business logic in controllers
- Don't use `any` in TypeScript
- Don't skip input validation on DTOs
- Don't log sensitive user data
- Don't use synchronous EF Core calls
- Don't create migrations without reviewing the generated SQL
- Don't bypass the Repository Pattern to access DbContext directly from services
- Don't add Create/Update/Delete to ArticlesController in Lisere.API
- Don't modify stock from Lisere.API — all stock writes go through Lisere.StockApi.API
- Don't add ASP.NET Identity to Lisere.StockApi — JWT validation only
