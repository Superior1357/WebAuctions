# Backend Developer Guide

This document describes the design, architecture, and internal workings of the **Backend** server project (`Backend/Backend.csproj`). It is intended for developers who want to understand or extend the codebase.

---

## Project Overview

Backend is an ASP.NET Core Minimal API application. Its responsibility is:

- Persisting auctions and bids in a SQLite database.
- Registering users and issuing JWT tokens via ASP.NET Core Identity.
- Exposing a typed REST API for buyer and seller operations.
- Enforcing auction business rules (bid validation, status evaluation, winner determination).

## Application Startup

`Program.cs` is the single entry point. It performs setup and service registration.

## Layer Architecture

The application is structured in three loosely coupled layers:

```
┌─────────────────────────────────────────┐
│         API Layer (DTOs + Handlers)      │
│  AuthMethods, BuyMethods, SellMethods   │
│  Incoming/Outgoing DTOs                 │
└────────────────┬────────────────────────┘
                 │ uses
┌────────────────▼────────────────────────┐
│         Business Layer                   │
│  Auction (abstract), EnglishAuction,    │
│  DutchAuction, FirstPriceSealedBid      │
│  Bid, User, AuctionEvaluator            │
└────────────────┬────────────────────────┘
                 │ persisted via
┌────────────────▼────────────────────────┐
│         Persistence Layer                │
│  DataModelAuction, DataModelBid,        │
│  DataModelUser, AppRepository,          │
│  AuctionDbContext                        │
└─────────────────────────────────────────┘
```

---

## Domain Model (Business Layer)

### Value Objects (`SharedDataProperties/`)

Strongly typed wrappers around primitive values:

### Auction Hierarchy

`Auction` is an `abstract record`. The three sealed subtypes represent the three supported auction formats:

```
Auction (abstract record)
├── EnglishAuction          (sealed record)
├── DutchAuction            (sealed record)
└── FirstPriceSealedBidAuction (sealed record)
```

Each subtype exposes a `static Restore(...)` factory method used exclusively by the repository layer to reconstruct domain objects from the database without triggering business-rule assertions.

### IAddOnlyCollection

`IAddOnlyCollection<T>` is a custom collection interface that supports `Add` and enumeration but not removal. This makes it impossible for business-layer code to accidentally remove bids from an auction.

`AppRepository` provides a private `ChangesTrackingAddOnlyCollection<T>` implementation that additionally tracks which items were newly added since the collection was loaded.

---

## Data Model (Persistence Layer)

EF Core entity classes live in `Data/DataModel/`. They mirror the business layer hierarchy but include EF navigation properties and a concurrency token (`Version`):

```
DataModelAuction (abstract)
├── EnglishDataModelAuction
├── DutchDataModelAuction
└── FirstPriceSealedBidDataModelAuction
```

### Concurrency Token

`DataModelAuction` (and its subtypes) carry a `Version` integer incremented each time a bid is added (via `IncrementVersion()`). EF Core uses this as an optimistic concurrency token — if two concurrent transactions both read the same `RowVersion` and both attempt to save, only one will succeed; the other will throw a `DbUpdateConcurrencyException`, which `AppRepository.CommitAsync()` wraps as a `ConcurrencyException`.

---

## Repository Pattern

`IRepository<T>` defines the data-access contract:

```csharp
Task<IEnumerable<T>> ListAsync();
Task AddAsync(T entity);
Task UpdateAsync(T entity);
Task CommitAsync();
```

`AppRepository` implements this for `Auction`. Its key responsibilities:

- **`ListAsync`** — queries `AuctionDbContext.Auctions`, eagerly loads `Seller` and `Bids` (with `Bidder`), then converts each `DataModelAuction` to its corresponding `Auction` business record.
- **`AddAsync`** — converts a new `Auction` business object to a `DataModelAuction` and stages it with EF Core.
- **`UpdateAsync`** — retrieves the existing `DataModelAuction` by ID, updates `EndTime`, and applies only the newly added bids (using `ChangesTrackingAddOnlyCollection.AddedItems`). Increments the row version when bids are added.
- **`CommitAsync`** — calls `SaveChangesAsync()` and translates `DbUpdateConcurrencyException` into the application-level `ConcurrencyException`.

---

## API Data Transfer Objects

### Incoming DTOs (`Data/APIData/Incoming/`)

`CreatedAuction` is an abstract record with a JSON polymorphic discriminator `$type`. Its three subtypes are:

- `EnglishCreatedAuction` (`$type: "English"`)
- `DutchCreatedAuction` (`$type: "Dutch"`)
- `FirstPriceSealedBidCreatedAuction` (`$type: "FirstPriceSealedBid"`)

### Outgoing DTOs (`Data/APIData/Outgoing/`)

`BuyerAuctionInfo` and `SellerAuctionInfo` are also abstract polymorphic records with subtypes per auction type.
Both types are tailored to the user's role usage.

---

## Data Converters

`IDataConverter<TIn, TOut>` is a single-method interface:

```csharp
TOut Convert(TIn input);
```

Two implementations are registered as scoped services:

- `AuctionToSellerAuctionInfo` — maps a business `Auction` to a `SellerAuctionInfo` DTO
- `BusinessToBuyerAuctionInfo` — maps a business `Auction` to a `BuyerAuctionInfo` DTO

---

## Auction Evaluation

`IAuctionEvaluator` provides three evaluation operations:

### `EvaluateStatus(Auction) → AuctionStatus`

Computes status from the current time and the auction's time window:

- `Pending` — if `now <= startTime`.
- `Active` — if `startTime < now < endTime` (with the exception that a Dutch auction becomes `Finished` as soon as `Bid` is set).
- `Finished` — if `now >= endTime`.

### `EvaluateWinnerInfo(Auction) → WinnerInfo?`

Returns `null` unless the auction is `Finished`. For finished auctions:

- **English / First Price Sealed Bid** — the winner is the bidder with the highest bid value.
- **Dutch** — the winner is the single bidder (if any).

Returns `null` (no winner) if no bid was placed.

### `EvaluateCurrentMinimumBidValue(Auction) → Price?`

- Returns `null` for finished auctions or First Price Sealed Bid auctions (minimum not revealed).
- **English** — starting price if no bids, otherwise highest bid + 1.
- **Dutch (pending)** — the starting price. **Dutch (active)** — `startingPrice - (elapsed / frequency) * decrementRate`, where elapsed time is measured from `StartTime`.

---

## Authentication and Authorization

The application uses two databases:

- **`Auth.db`** (`AuthDbContext`) — ASP.NET Core Identity tables: `AspNetUsers`, etc.
- **`Auctions.db`** (`AuctionDbContext`) — application tables: `Auctions`, `Bids`, `Users`.

Registration creates an `IdentityUser` in `Auth.db` and a corresponding `DataModelUser` (keyed by `IdentityId`) in `Auctions.db`.

Login verifies the password via `UserManager<IdentityUser>`, then issues a JWT signed with the symmetric key from `Databases/jwt_secret`. The token carries a single claim: `ClaimTypes.NameIdentifier = user.Id`. It expires after 8 hours.

Protected endpoints receive the token via the `Authorization: Bearer <token>` header. ASP.NET Core's JWT middleware validates the signature and extracts the claim. Endpoint handlers that need the current user declare a `User user` parameter; the framework resolves this by looking up the `DataModelUser` by `IdentityId` from `AuctionDbContext`.

---

## API Endpoints

Endpoint logic lives in two static classes, `BuyMethods` and `SellMethods`, both in `BusinessMethods/`. Each public method corresponds to one route and is wired up in `Program.cs` via `ApiMappingExtensions`.

Parameters are injected by ASP.NET Core's Minimal API binding: body parameters are deserialized from JSON, route parameters are taken from the URL, and service parameters are resolved from DI.

Return types use the `Results<T1, T2, ...>` union type so that Minimal API can generate accurate OpenAPI response schemas.

---

## Error Handling

Each endpoint returns a union of typed result types. Error response bodies are plain records:

| Type | HTTP status | Scenario |
|------|-------------|---------|
| `AuctionIdNotFound` | 404 | Auction ID not in the database, or belongs to a different seller |
| `UserIdNotFound` | 404 | Seller user ID not found |
| `BidAuctionRulesViolationResponse` | 422 | Bid on non-active auction, bid too low, already bid |
| `InvalidAuctionParameters` | 422 | Invalid `startTime`/`endTime`, or auction already finished on update |
| `ConcurrencyConflictResponse` | 409 | Optimistic concurrency failure on bid commit |

All error records carry a `Message` string field.

---

## Concurrency Control

Bidding is subject to concurrent access. The design uses **optimistic concurrency**:

1. The endpoint reads the auction and its current `Version` from the database.
2. The bid is staged in memory.
3. `CommitAsync()` calls `SaveChangesAsync()`. EF Core includes a `WHERE RowVersion = <original>` clause on the `UPDATE` statement.
4. If a concurrent transaction has already incremented `Version`, the update affects zero rows, causing `DbUpdateConcurrencyException`.
5. `AppRepository.CommitAsync()` catches this and rethrows as `ConcurrencyException`.
6. The endpoint handler catches `ConcurrencyException` and returns `409 Conflict`.

The `Version` is incremented explicitly by `IncrementVersion()` only when a bid is actually added.

---

## Tests

### Unit Tests (`UnitTests/`)

Tests cover individual components in isolation using mocks:

| Test file | Covers |
|-----------|--------|
| `AuctionEvaluatorTest.cs` | Status evaluation, winner determination, minimum bid |
| `AppRepositoryTests.cs` | Repository mapping between data and business models |
| `BuyMethodsUnitTests.cs` | Bid validation, listing logic |
| `SellMethodsUnitTests.cs` | Auction creation validation, update validation |
| `AuctionToSellerAuctionInfoTest.cs` | Seller DTO converter |
| `BusinessToBuyerAuctionInfoTest.cs` | Buyer DTO converter |
| `CurrentTimeTest.cs` | `CurrentTime` implementation |

### Integration Tests (`IntegrationTests/`)

`WARSAppFactory` extends `WebApplicationFactory<Program>` and configures in-memory SQLite databases so tests run without touching the filesystem. `BusinessIntegrationTests` sends HTTP requests against the in-process server and verifies full request/response cycles.

---

## Extending the Application

### Adding a new auction type

1. **Business model** — add a new `sealed record MyNewAuction(...) : Auction(...)` in `Auction.cs` with a `static Restore(...)` factory.
2. **Data model** — add a `MyNewDataModelAuction : DataModelAuction` in `DataModelAuction.cs`.
3. **Repository** — update the `switch` expressions in `AppRepository.DataToBusinessAuction`, `NewBusinessToNewDataAuction`, and `UpdateAsync` to handle the new type.
4. **EF Core** — add a migration (`dotnet ef migrations add <Name>`).
5. **Auction evaluator** — update all three `switch` statements in `AuctionEvaluator` (`EvaluateAuctionTypeSpecificActiveTimeWindowStatus`, `EvaluateWinnerInfo`, `EvaluateCurrentMinimumBidValue`).
6. **Bid handler** — update the `switch` in `BuyMethods.BidAsync`.
7. **Participated-auction filter** — update the `switch` in `BuyMethods.ListParticipatedAuctionsAsync`.
8. **DTOs** — add corresponding incoming and outgoing DTO subtypes.
9. **Converters** — update both `IDataConverter` implementations.
10. **Frontend** — see `docs/Frontend.md`.

All existing `switch` expressions that dispatch on auction type throw `ArgumentOutOfRangeException` by default, so missing cases will be caught at runtime during development.
The application supposes the future use of union types which will be supported in .NET 11.