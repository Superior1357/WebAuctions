# WARSApplication developer guide

## Solution layout

The solution has four projects:

- `WARSApplication`: ASP.NET Core web API
- `Frontend`: console client
- `UnitTests`: unit tests for business logic
- `IntegrationTests`: HTTP-level tests against the API

## Runtime architecture

The API is a minimal hosting app with these main parts:

- SQLite persistence for auctions and authentication
- JWT authentication
- OpenAPI in development
- a repository layer that maps between EF Core entities and business records
- an evaluation service for auction status, winner, and minimum bid calculations

The frontend uses Refit clients and a shared token bearer. Requests to the buy and sell APIs attach the JWT as a Bearer token when one has been stored.

## Configuration

The API reads these configuration values:

- connection string `Auctions`, defaulting to `Data Source=Auctions.db`
- connection string `Auth`, defaulting to `Data Source=Auth.db`
- `JwtSecret` for JWT signing and validation

The frontend is hardcoded to call `http://localhost:5205`.

## Data model

### Authentication database

`AuthDbContext` is an `IdentityDbContext` used for ASP.NET Identity users.

### Auction database

`AuctionDbContext` stores:

- auctions
- users
- bids

It also configures:

- a discriminator column for auction type
- price conversion between `Price` and `decimal`
- ID conversions for `AuctionId` and `UserId`
- a concurrency token on auction version

## Business model

The business model uses immutable/mostly immutable records:

- `User`
- `Bid`
- `Auction` with concrete types:
  - `EnglishAuction`
  - `DutchAuction`
  - `FirstPriceSealedBidAuction`

Restoration helpers are used when data is loaded from persistence.

## Services

- `ICurrentTime` / `CurrentTime`: request-scoped time snapshot
- `IAuctionEvaluator` / `AuctionEvaluator`: status, winner, and current minimum bid logic
- `IRepository<Auction>` / `AppRepository`: persistence bridge
- `IDataConverter<Auction, SellerAuctionInfo>` / `AuctionToSellerAuctionInfo`: seller response mapping

## Endpoint behavior

### Authentication

- `POST /auth/register`
- `POST /auth/login`

Registration creates an Identity user and a matching auction-domain user row.
Login verifies credentials and returns a JWT valid for 8 hours.

### Buy endpoints

- `GET /buy/list/sellers`
- `POST /buy/list/not_finished`
- `GET /buy/list/won`
- `GET /buy/list/participated`
- `POST /buy/bid`
- `POST /buy/info`

All buy endpoints require authorization.

`BidAsync` currently:

- rejects missing auctions
- rejects bids on non-active auctions
- rejects a second bid from the same user on first-price sealed-bid auctions
- persists the bid and commits the update

`GetInfoAsync` returns auction info in the same order as the requested IDs.

### Sell endpoints

- `POST /sell/create`
- `PATCH /sell/update`
- `GET /sell/list/{status}`
- `POST /sell/info`

All sell endpoints require authorization.

`CreateAuctionAsync` validates:

- start time must not be in the past
- end time must be after start time

`UpdateAuctionAsync` validates:

- the auction must exist
- the new end time must not be earlier than the current end time
- the authenticated user must match the seller

## Testing

Run both test projects:

- `UnitTests`
- `IntegrationTests`

The integration tests use:

- in-memory SQLite
- a fake time provider
- a dummy authentication handler

## Notes and concrete issues in the current code

These are implementation mismatches visible in the source:

- `BuyMethods.ListNotFinishedAuctionsOfASellerAsync` returns an empty list for an unknown seller id, even though its signature and XML comment suggest a `NotFound<UserIdNotFound>` path.
- `AppRepository.CommitAsync` catches `DbUpdateConcurrencyException`, but the caught exception is not propagated, so the caller does not observe a failure.
- `BuyMethods.BidAsync` does not enforce minimum bid rules in code, even though the evaluator exposes current minimum bid logic and the unit tests exercise low-bid rejection.
- `currentTime` is accepted by `UpdateAuctionAsync` but is not used.
