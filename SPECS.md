# CLAUDE.md - Lisere Request Management Application

## ðŸŽ¯ VISION & CONTEXT

### Problem to solve
Replace the walkie-talkie system for clothing orders in Lisere stores.

**Current issues:**
- Lack of clarity in voice requests
- Inefficiency (waiting for availability, memorization)
- Partial/interrupted communication
- Fragile and unreliable hardware

### Proposed solution
Mobile application (PWA) allowing sellers to submit clothing requests and stockists to process them in real time.

### MVP (Minimum Viable Product)
**Main User Story:**
> As a seller, I can send a request for a clothing item (name, print/color, size(s)) based on store stock, and the stockist receives it instantly.

---

## ðŸ—ï¸ TECHNICAL ARCHITECTURE

### Backend - .NET 10 / C#

**Architecture: Clean Architecture (4 layers)**

```
â”œâ”€â”€ Domain/
â”‚   â”œâ”€â”€ Entities (Request, Article, User, Stock, RequestLine)
â”‚   â”œâ”€â”€ Enums (RequestStatus, UserRole, ZoneType)
â”‚   â”œâ”€â”€ Interfaces (IRequestRepository, IStockService)
â”‚   â””â”€â”€ ValueObjects
â”‚
â”œâ”€â”€ Application/
â”‚   â”œâ”€â”€ DTOs (CreateRequestDto, ArticleDto)
â”‚   â”œâ”€â”€ Services (RequestService, NotificationService)
â”‚   â”œâ”€â”€ Validators (FluentValidation or DataAnnotations)
â”‚   â””â”€â”€ Interfaces
â”‚
â”œâ”€â”€ Infrastructure/
â”‚   â”œâ”€â”€ Persistence (DbContext EF Core, Repositories)
â”‚   â”œâ”€â”€ SignalR (Hubs for real-time notifications)
â”‚   â”œâ”€â”€ Identity (ASP.NET Core Identity)
â”‚   â”œâ”€â”€ BackgroundJobs (RequestTimeoutService)
â”‚   â””â”€â”€ ExternalServices (External Stock API)
â”‚
â””â”€â”€ API/
    â”œâ”€â”€ Controllers (REST endpoints)
    â”œâ”€â”€ Middlewares (Errors, Auth, Logging)
    â””â”€â”€ Configuration (Program.cs, appsettings)
```

**Technologies:**
- .NET 10
- Entity Framework Core (Code First)
- SQL Server
- ASP.NET Core Identity (auth/JWT)
- SignalR (real-time WebSocket with automatic retry)
- Serilog (structured logging)
- Redis Cache (shared cache across store instances)

**Patterns:**
- Repository Pattern
- Dependency Injection (native .NET)

---

### Frontend - React PWA

**Type:** Progressive Web App (installable, partial offline support)

**Stack:**
- React 18 + TypeScript
- Vite (build tool)
- React Router (navigation)
- Axios (REST API)
- @microsoft/signalr (SignalR client)
- Zustand (state management) + Zustand persist (offline state)
- Tailwind CSS (mobile-first styling)
- @ericblade/quagga2 (EAN-13 barcode scanning)

**PWA Features:**
- Service Worker
- Manifest.json
- Web Push Notifications

**Offline Strategy:**
- Current requests cached locally (IndexedDB via Zustand persist)
- Seller sees their requests with an "offline" badge
- New requests queued locally, auto-sent on reconnection
- No offline creation (stock check impossible), but last known state displayed
- SignalR auto-reconnect with exponential backoff

**Main Screens:**
1. Login
2. Zone Selection (RTW/FittingRooms/Checkout/Reception/Custom)
3. Article Search (search bar + barcode scan)
4. Current Requests List
5. History

---

## ðŸ“‹ STANDARDS & BEST PRACTICES

### Code Quality

**Frontend:**
- ESLint + Prettier (auto formatting, strict rules)
- TypeScript strict mode
- No `any`, complete typing
- Functional components + hooks only

**Backend:**
- SonarAnalyzer (bug/vulnerability detection)
- Code analysis enabled (warnings as errors in prod)
- Conventions: PascalCase classes/methods, camelCase variables

### Security

- **HTTPS mandatory** in production
- **CORS** strictly configured (authorized origins only)
- **Rate Limiting**: 100 req/min per user
- **Input validation** server-side (DataAnnotations on all DTOs)
- **JWT** for API authentication
- **No sensitive data** in logs

### Database

**Entity Framework Core:**
- Code First with migrations
- Soft Delete (`IsDeleted` field)
- Audit Trail on all entities:
  ```csharp
  public DateTime CreatedAt { get; set; }
  public string CreatedBy { get; set; }
  public DateTime? ModifiedAt { get; set; }
  public string? ModifiedBy { get; set; }
  ```

**Conventions:**
- Table and entity names in English
- Relations with navigation properties
- Indexes on frequently filtered columns

### Error Handling

**Backend:**
- Serilog (structured JSON logs)
- ProblemDetails (RFC 7807) for API errors
- Global exception handler middleware
- Logs: Info/Warning/Error by severity

**Error example:**
```json
{
  "type": "https://api.lisere.app/errors/stock-unavailable",
  "title": "Stock insuffisant",
  "status": 400,
  "detail": "Robe Rouge taille M non disponible",
  "instance": "/api/requests/123"
}
```

### Performance

- **Redis Cache** for stock (TTL 30s) â€” shared across ~20 store instances
- **Mandatory pagination**: max 50 results per page
- **Lazy Loading** images (React Suspense/Intersection Observer)
- **SignalR reconnection** automatic (exponential backoff)

### Tests

**Minimum coverage: 80%**

**Test types:**
1. **Unit** (xUnit): Services, Validators, Domain logic
2. **Integration** (WebApplicationFactory): Repositories, API endpoints
3. **E2E** (Playwright): Full seller/stockist workflows
4. **Mock** external Stock API (Moq/NSubstitute)

---

## ðŸŽ¨ BUSINESS DOMAIN

### Main Entities

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
    public User Stockist { get; set; }
}
```

**2. RequestLine (pivot entity)**
```csharp
public class RequestLine
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ArticleId { get; set; }
    public string ColorOrPrint { get; set; }
    public List<Size> RequestedSizes { get; set; }
    public int Quantity { get; set; }
    public RequestLineStatus Status { get; set; } // Found, NotFound, Pending

    // Navigation
    public Request Request { get; set; }
    public Article Article { get; set; }
}
```

**3. Article**
```csharp
public class Article
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } // EAN-13
    public ClothingFamily Family { get; set; }
    public string Name { get; set; }
    public string ColorOrPrint { get; set; }
    public List<Size> AvailableSizes { get; set; }
}
```

**4. User**
```csharp
public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserRole Role { get; set; } // Seller, Stockist, Admin
    public ZoneType? AssignedZone { get; set; }
}
```

**5. Stock (read-only from external API)**
```csharp
public class Stock
{
    public Guid ArticleId { get; set; }
    public Size Size { get; set; }
    public int AvailableQuantity { get; set; }
}
```

### Enums

**RequestStatus:**
- `Pending` (created, not yet picked up)
- `InProgress` (stockist accepted)
- `Delivered` (article brought to seller)
- `Unavailable` (article not found)
- `Cancelled` (seller cancelled or auto-timeout)

**RequestLineStatus:**
- `Pending`
- `Found`
- `NotFound`

**ZoneType:**
- `RTW` (Ready-To-Wear)
- `FittingRooms`
- `Checkout`
- `Reception`
- `Custom` (customizable zones)

**ClothingFamily:**
- `COA` (Coats), `JAC` (Jackets), `TSH` (T-shirts), `SWE` (Sweaters), `VES` (Vests)
- `JEA` (Jeans), `PAN` (Pants), `SHO` (Shorts), `SKI` (Skirts), `DRE` (Dresses)
- `SHI` (Shirts), `BLO` (Blouses)
- `SHE` (Shoes), `BEL` (Belts), `BAG` (Bags), `JEW` (Jewelry)

**Size:**
- `XXS`, `XS`, `S`, `M`, `L`, `XL`, `XXL`, `OneSize`

---

## ðŸ”„ WORKFLOWS

### Seller Workflow

1. **Login** â†’ Email/Password (JWT)
2. **Zone Selection** â†’ RTW / FittingRooms / Checkout / Reception / Custom
3. **Article Search:**
   - Search bar (name, family)
   - OR Barcode scan (EAN-13 via smartphone camera, @ericblade/quagga2)
4. **Color/Print + Size(s) Selection**
   - Multiple sizes allowed (customer hesitates S/M)
5. **Add to cart OR Submit**
   - Add â†’ back to search
   - Submit â†’ Send request + Notify stockist
6. **Notifications received:**
   - Request accepted
   - Alternative proposed (other color/size)
   - Article delivered / Unavailable
7. **Request management:**
   - View current requests
   - Modify / Cancel
   - Accept/Decline alternatives
8. **History** (current day)

### Stockist Workflow

1. **Login** â†’ Email/Password
2. **Zone Selection** â†’ RTW / FittingRooms / Checkout / Reception / Custom
3. **Receive Notification** (push + sound + vibration)
4. **View request** â†’ Article, color, size(s), seller
5. **Processing:**
   - Mark "In Progress"
   - Search article in stock
   - Mark "Found" OR "Not Found"
   - Propose alternative if needed
6. **Delivery** â†’ Bring to floor
7. **Visibility:**
   - List all pending requests
   - Day history
8. **Strict FIFO** (no manual prioritization)

### Admin Workflow

- Create user accounts (sellers/stockists)
- View all requests (all stores)
- Statistics (post-MVP)
- Manage custom zones

---

## âš–ï¸ BUSINESS RULES

### Validations

1. **A request without stock = impossible**
   - Check availability before creation
   - If stock = 0 â†’ "Unavailable" message

2. **Multiple sizes same article = allowed**
   - Customer hesitates between S and M
   - Stockist brings both

3. **Auto-cancellation after 30 min** if not processed
   - Handled by `RequestTimeoutService` (IHostedService / BackgroundService)
   - Runs every minute, checks `Pending` requests older than 30 min
   - Status changes to `Cancelled`
   - SignalR notification sent to seller

4. **Request editable** while Status = `Pending`
   - If `InProgress` â†’ must cancel then recreate

### Priorities & Assignment

- **Strict FIFO** (First In First Out)
- **Zone-based assignment:**
  - N sellers + N stockists per zone
  - Request sent to zone stockist only
- **No manual reassignment**

### History & Stats

- **Retention: current day**
- **Post-MVP:**
  - Average processing time
  - Most requested articles
  - Automated end-of-day export (CSV via BackgroundService, scheduled at closing time)

---

## ðŸ”§ TECHNICAL CONSTRAINTS

### Performance

- **Max simultaneous users:**
  - ~100 across all stores (~20 stores)
  - Per store: ~5 sellers + ~3 stockists
- **Peak requests: 100/hour** (Saturday afternoon)
- **SignalR latency: < 500ms**

### Real-time Notifications

**Mandatory SignalR with:**
- Automatic reconnection (exponential backoff retry)
- Network disconnection handling
- Message queue if temporarily offline

**Notification triggers:**
- New request â†’ Stockist
- Request accepted â†’ Seller
- Article found/not found â†’ Seller
- Alternative proposed â†’ Seller
- Request cancelled â†’ Stockist

### External Stock API

- **Read-only** (no update from the app)
- Redis cache 30s to limit calls
- Fallback if API down â†’ user message

### Mobile First

- Installable PWA (add to home screen)
- Responsive design (iPhone SE â†’ iPad)
- EAN-13 barcode scan via native camera (@ericblade/quagga2)
- Web push notifications (requires HTTPS)

---

## ðŸš€ DEVELOPMENT DIRECTIVES

### General Principles

1. **Professional solutions ALWAYS**
   - No hacky shortcuts
   - Maintainable and scalable code
   - Systematic testing

2. **Concise and iterative**
   - One step at a time
   - Wait for validation before next step
   - No long code dumps

3. **Security by default**
   - Input validation everywhere
   - Logs without sensitive data
   - HTTPS + CORS + Rate limiting

4. **Performance from the start**
   - Pagination
   - Cache
   - Lazy loading
   - No unnecessary premature optimization

### Suggested Development Order

**Phase 1 - Backend Foundations:**
1. Clean Architecture project setup
2. Domain entities + EF Core
3. Repositories + DbContext
4. Basic CRUD API endpoints
5. Domain/Application unit tests

**Phase 2 - Auth & Security:**
1. ASP.NET Identity + JWT
2. CORS + Rate Limiting
3. ProblemDetails middleware
4. Serilog configuration

**Phase 3 - Real-time:**
1. SignalR Hub configuration
2. Real-time notifications
3. Reconnection handling
4. RequestTimeoutService (BackgroundService)

**Phase 4 - Frontend:**
1. React PWA + TypeScript setup
2. Auth flow (login/JWT)
3. Main screens
4. SignalR client integration
5. Zustand state + offline persistence
6. PWA manifest + Service Worker

**Phase 5 - Integrations:**
1. External Stock API (mock then real)
2. Redis cache
3. EAN-13 barcode scan (@ericblade/quagga2)

**Phase 6 - Tests & Polish:**
1. E2E tests Playwright
2. Integration tests
3. Security audit
4. Performance optimizations

---

## ðŸ“ IMPORTANT NOTES

- **Language:** Code in English, UI/messages in French
- **Timezone:** Europe/Paris
- **Date format:** dd/MM/yyyy HH:mm
- **Currency:** Not applicable (no pricing)
- **Environments:** Dev â†’ Staging â†’ Prod (HTTPS mandatory Staging+)

---

**Version:** 2.0  
**Last updated:** February 2026  
**Project contact:** [To fill]
