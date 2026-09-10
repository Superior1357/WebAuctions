# WARSApplication

WARSApplication is an auction system with two parts:

- an ASP.NET Core web API that stores auctions, bids, and authentication data in SQLite
- a console frontend that talks to the API over HTTP

This repository also contains unit and integration tests.

## How to run it

The code is split into two runnable applications:

- `WARSApplication` — the API server
- `Frontend` — the console client

The frontend is configured to call the API at `http://localhost:5205`, so the API needs to be running first.

Typical commands from the solution root are:

- `dotnet run --project src/WARSApplication/WARSApplication`
- `dotnet run --project src/WARSApplication/Frontend`
- `dotnet test`

## Configuration and storage

The API reads these configuration values:

- connection string `Auctions`, defaulting to `Data Source=Auctions.db`
- connection string `Auth`, defaulting to `Data Source=Auth.db`
- `JwtSecret` for JWT signing and validation

The application stores auction data in the auction SQLite database and authentication data in the auth SQLite database.

## User guide

### First-time setup

1. Start the API.
2. Start the frontend.
3. Register a user.
4. Log in with that user.

After login, the frontend stores the JWT token and sends it as a Bearer token on authenticated requests.

### Buyer flow

The buyer path is:

1. list all sellers
2. choose a seller
3. list that seller’s auctions that are not finished
4. inspect one auction
5. place a bid if desired

Buyer features are exposed under `/buy`:

- `GET /buy/list/sellers`
- `POST /buy/list/not_finished`
- `POST /buy/info`
- `POST /buy/bid`
- `GET /buy/list/won`
- `GET /buy/list/participated`

### Seller flow

The seller path is:

1. create an auction
2. list your auctions by status
3. inspect one of your auctions
4. update the end time if needed

Seller features are exposed under `/sell`:

- `POST /sell/create`
- `PATCH /sell/update`
- `GET /sell/list/{status}`
- `POST /sell/info`

### Authentication

Authentication features are exposed under `/auth`:

- `POST /auth/register`
- `POST /auth/login`

## Auction types

The code supports three auction types:

- English auction
- Dutch auction
- First-price sealed-bid auction

### English auction

- Has a starting price.
- Bids are stored as a list.
- When active, the current minimum bid is the highest bid plus 1.
- When pending, the minimum bid is the starting price.

### Dutch auction

- Has a starting price.
- Has a price decrement rate.
- Has a price decrement frequency.
- Can end early when a bid is placed.

### First-price sealed-bid auction

- Has no visible current minimum bid in the API response.
- Each user can bid only once.

## Request and response shapes

### Authentication

- register request: `UserName`, `Password`
- login request: `UserName`, `Password`
- login response: `Token`

### Bid request

- `AuctionId`
- `Amount`

### Update auction request

- `Id`
- `NewEnd`

### Create auction request

The create request is polymorphic and uses a `$type` discriminator:

- `"english"`
- `"dutch"`
- `"sealed"`

Common fields:

- `StartTime`
- `EndTime`
- `Description`

English fields:

- `StartingPrice`

Dutch fields:

- `StartingPrice`
- `PriceDecrementRate`
- `PriceDecrementFrequency`

Sealed-bid fields:

- no extra fields

### Buyer auction info response

Buyer info is polymorphic and includes:

- `AuctionId`
- `Description`
- `SellerId`
- `StartTime`
- `EndTime`
- `Status`

English buyer info includes `CurrentMinimumBid`.

Dutch buyer info includes `CurrentPrice`.

### Seller auction info response

Seller info is polymorphic and includes:

- `Id`
- `Description`
- `StartTime`
- `EndTime`
- `Info`

`Info` depends on status:

- pending
- active
- finished

Finished seller info includes winner information when available.

## Developer guide

The developer guide is in [docs/Developer.md](docs/Developer.md).
