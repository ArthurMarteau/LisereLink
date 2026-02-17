# Lisere Request Management App

PWA for managing clothing requests between sellers and stockists in retail stores. Replaces walkie-talkie system.

Full specifications: see `SPECS.md` at project root.

## Stack

- **Backend:** .NET 10, EF Core (Code First), SQL Server, SignalR, ASP.NET Identity + JWT, Serilog, Redis
- **Frontend:** React 18 + TypeScript, Vite, Zustand, Tailwind CSS, @zxing/library (EAN-13), @microsoft/signalr
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)

## Project Structure

```
src/
├── Lisere.Domain/           # Entities, Enums, Interfaces, ValueObjects (zero dependencies)
├── Lisere.Application/      # DTOs, Services, Validators, Interfaces
├── Lisere.Infrastructure/   # EF Core, Repositories, SignalR, Identity, BackgroundJobs
├── Lisere.API/              # Controllers, Middlewares, Program.cs
└── Lisere.Tests/            # xUnit, unit + integration tests
frontend/
├── src/
│   ├── components/
│   ├── pages/
│   ├── stores/              # Zustand stores
│   ├── services/            # API + SignalR clients
│   ├── types/
│   └── hooks/
```

## Commands

```bash
# Backend
dotnet build
dotnet test
dotnet run --project src/Lisere.API
dotnet ef migrations add <Name> --project src/Lisere.Infrastructure --startup-project src/Lisere.API
dotnet ef database update --project src/Lisere.Infrastructure --startup-project src/Lisere.API

# Frontend
cd frontend && npm install
npm run dev
npm run build
npm run lint
npm run test
```

## Code Conventions

### C# / Backend
- PascalCase: classes, methods, properties
- camelCase: local variables, parameters
- All entities have: `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`, `IsDeleted` (soft delete)
- Repository Pattern for data access
- DataAnnotations on all DTOs for validation
- ProblemDetails (RFC 7807) for all API errors
- Async/await everywhere (no `.Result` or `.Wait()`)
- No business logic in controllers — controllers call Application services only

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
- All API responses paginated (max 50 per page)
- No sensitive data in logs

## Key Entities

Defined in `SPECS.md`. Summary:
- `Request` (aggregate root) → has many `RequestLine`
- `RequestLine` → links Request to Article + size + quantity + status
- `Article` → clothing item with barcode (EAN-13), family, color
- `User` (extends IdentityUser) → roles: Seller, Stockist, Admin
- `Stock` → read-only from external API, cached in Redis (TTL 30s)

## Key Enums

- `RequestStatus`: Pending, InProgress, Delivered, Unavailable, Cancelled
- `RequestLineStatus`: Pending, Found, NotFound
- `UserRole`: Seller, Stockist, Admin
- `ZoneType`: RTW, FittingRooms, Checkout, Reception, Custom
- `ClothingFamily`: COA, JAC, TSH, SWE, VES, JEA, PAN, SHO, SKI, DRE, SHI, BLO, SHE, BEL, BAG, JEW
- `Size`: XXS, XS, S, M, L, XL, XXL, OneSize

## Testing

- xUnit for unit + integration tests
- WebApplicationFactory for API integration tests
- Moq or NSubstitute for mocking
- Target: 80% coverage minimum
- Run single test: `dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"`

## Don't

- Don't put business logic in controllers
- Don't use `any` in TypeScript
- Don't skip input validation on DTOs
- Don't log sensitive user data
- Don't use synchronous EF Core calls
- Don't create migrations without reviewing the generated SQL
- Don't bypass the Repository Pattern to access DbContext directly from services
