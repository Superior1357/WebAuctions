# WARS Application — User Guide

WARS (Web Auction Realtime Service) is a console-based auction platform consisting of two components:

- **Server** (`Backend/`) — an ASP.NET Core Web API that manages auctions, bids, and user authentication.
- **Client** (`Frontend/`) — a .NET console application that communicates with the server through its REST API.

Both components target **.NET 10** and must be built and run separately.

---

## Backend (Server) User Guide

### Backend Initialization (from scratch)

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- `dotnet-ef` global tool (Entity Framework Core CLI):

  ```bash
  dotnet tool install --global dotnet-ef
  ```

- `openssl` command-line utility **or** `/dev/urandom`

#### Steps

1. **Navigate to the server project directory:**

   ```bash
   cd Backend
   ```

2. **Run the initialization script:**

   ```bash
   ./initialize.sh
   ```

   This script performs the following actions in order:

   - Creates the `Databases/` directory inside `Backend/` (if it does not exist).
   - Deletes any previously existing databases (`Auctions.db`, `Auth.db`) and JWT secret file — all data will be wiped.
   - Generates a new cryptographically random JWT secret (48 bytes, base64-encoded) using `openssl rand -base64 48`, or falls back to `/dev/urandom`, and saves it to `Databases/jwt_secret` with owner-only read permissions (`umask 077`).
   - Restores NuGet packages for the `Backend.csproj`.
   - Applies all EF Core migrations to create `Databases/Auctions.db` (auction and bid data).
   - Applies all EF Core migrations to create `Databases/Auth.db` (user identity and authentication data).

   On success, the script prints the paths of both databases and the JWT secret file.

   > **Note:** Re-running `initialize.sh` deletes all existing data and starts fresh. Do not run it on a production instance unless you intend to wipe all data.

3. **Verify the databases were created:**

   ```bash
   ls Databases/
   # Auctions.db  Auth.db  jwt_secret
   ```

#### Configuration

The server reads its configuration from `Backend/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Auctions": "Data Source=Databases/Auctions.db",
    "Auth": "Data Source=Databases/Auth.db"
  }
}
```

If either connection string is absent, the server falls back to the paths relative to its content root (`Databases/Auctions.db` and `Databases/Auth.db`).

The JWT secret is read at startup from `Databases/jwt_secret`. If that file does not exist, the server throws an exception and refuses to start — run `initialize.sh` first.

---

### Backend Usage

#### Starting the server

From the `Backend/` directory:

```bash
dotnet run
```

The default `http` profile serves on **`http://localhost:5205`**.

The server starts in Development mode, which enables:

- The developer exception page (detailed error responses).
- The OpenAPI document at `GET /openapi/v1.json`.
- The Swagger UI at **`http://localhost:5205/swagger`** — useful for exploring and testing endpoints interactively.

#### Authentication

All buyer and seller endpoints require a valid JWT Bearer token. Tokens are obtained by logging in and expire after **8 hours**.

Password rules are intentionally relaxed (no digit, non-alphanumeric, or uppercase requirement; minimum length is 1 character).

#### API Endpoints

##### Authentication (`/auth`) — no token required

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/auth/register` | Create a new user account. Body: `{ "userName": "...", "password": "..." }`. Returns `200 OK` or `400 Bad Request` (e.g. username already taken). |
| `POST` | `/auth/login` | Log in and receive a JWT token. Body: `{ "userName": "...", "password": "..." }`. Returns `200 OK` with `{ "token": "..." }` or `401 Unauthorized`. |

##### Identity — token required

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/identity` | Returns the current user's ID (`{ "id": "..." }`). |

##### Buyer endpoints (`/buy`) — token required

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/buy/list/sellers` | Returns an array of user IDs of all users who have created at least one auction. |
| `POST` | `/buy/list/not_finished` | Body: a `UserId` object. Returns auction IDs of all non-finished auctions belonging to the given seller. Returns `404` if the seller ID is unknown. |
| `POST` | `/buy/info` | Body: an array of `AuctionId` values. Returns buyer-relevant auction info for each requested auction ID, in the same order as requested. Returns `404` if any ID is not found. |
| `POST` | `/buy/bid` | Body: `{ "auctionId": ..., "amount": ... }`. Places a bid on an active auction. Returns `200 OK`, `404` (auction not found), `422` (bid violates rules), or `409` (concurrency conflict). |
| `GET` | `/buy/list/won` | Returns IDs of all finished auctions that the current user has won. |
| `GET` | `/buy/list/participated` | Returns IDs of all auctions in which the current user has placed at least one bid. |

##### Seller endpoints (`/sell`) — token required

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/sell/create` | Create a new auction. Body is a polymorphic `CreatedAuction` object (see auction types below). Returns `200 OK` or `422` (invalid parameters). |
| `PATCH` | `/sell/update` | Update the end time of a non-finished auction. Body: `{ "id": ..., "newEnd": "..." }`. Returns `200 OK`, `404` (auction not found or belongs to a different seller), or `422` (new end time is before the current end time, or auction is already finished). |
| `GET` | `/sell/list/{status}` | List IDs of the current user's auctions filtered by status. `{status}` is one of `Pending`, `Active`, or `Finished`. |
| `POST` | `/sell/info` | Body: an array of `AuctionId` values. Returns seller-relevant info (including winner details) for each requested auction. Returns `404` if any ID is not found or belongs to a different seller. |

#### Auction Types

The system supports three auction types, each passed as a discriminated JSON object with a `$type` discriminator field.

**English Auction** — highest bidder wins; each bid must exceed the previous one; the minimum bid starts at the starting price and increments by 1 after each bid.

```json
{
  "$type": "English",
  "description": "Vintage lamp",
  "startTime": "2026-10-01T10:00:00Z",
  "endTime":   "2026-10-07T10:00:00Z",
  "startingPrice": 50.00
}
```

**Dutch Auction** — the price starts high and decreases over time at a fixed rate and frequency; the first bidder wins and the auction closes immediately. Only one bid is possible.

```json
{
  "$type": "Dutch",
  "description": "Fresh tulips",
  "startTime": "2026-10-01T10:00:00Z",
  "endTime":   "2026-10-01T18:00:00Z",
  "startingPrice": 200.00,
  "priceDecrementRate": 10.00,
  "priceDecrementFrequency": "01:00:00"
}
```

**First Price Sealed Bid Auction** — each bidder may bid only once; the minimum bid is not revealed; the highest bidder wins when the auction closes.

```json
{
  "$type": "FirstPriceSealedBid",
  "description": "Rare coin",
  "startTime": "2026-10-01T10:00:00Z",
  "endTime":   "2026-10-14T10:00:00Z"
}
```

#### Auction Statuses

| Status | Meaning |
|--------|---------|
| `Pending` | Before the auction's `startTime`. |
| `Active` | Between `startTime` and `endTime` (or until the Dutch auction receives its first bid). |
| `Finished` | After `endTime`, or for a Dutch auction, after the first bid. |

#### Concurrency

Bidding endpoints apply optimistic concurrency control. If two bids are submitted simultaneously for the same auction, one will succeed and the other will receive `409 Conflict`. The client should retry when this occurs.

#### Running Tests

```bash
# All tests (from the solution root)
dotnet test

# Unit tests only
dotnet test UnitTests/UnitTests.csproj

# Integration tests only
dotnet test IntegrationTests/IntegrationTests.csproj
```

---

## Frontend (Client) User Guide

### Frontend Initialization (from scratch)

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A running instance of the WARS backend server (see [Backend Initialization](#backend-initialization-from-scratch) above).

#### Steps

1. **Start the backend server first** (the client cannot connect without it).

2. **Navigate to the client project directory:**

   ```bash
   cd Frontend
   ```

3. **Configure the server URL** (only if the server is not running on the default address):

   Open `Frontend/appsettings.json` and set `Api:BaseAddress` to match the server's address:

   ```json
   {
     "Api": {
       "BaseAddress": "http://localhost:5205"
     }
   }
   ```

   The default value `http://localhost:5205` matches the server's default HTTP profile.

4. **Restore dependencies** (optional — `dotnet run` does this automatically):

   ```bash
   dotnet restore
   ```

---

### Frontend Usage

#### Starting the client

From the `Frontend/` directory:

```bash
dotnet run
```

The client is a full-screen interactive console application rendered with Spectre.Console. It communicates with the backend over HTTP using typed Refit clients.

#### Authentication screen

On startup, the client clears the terminal and presents a selection menu:

```
> Login
  Register
```

Use the arrow keys to select and press **Enter** to confirm. You must successfully register or log in before proceeding. The client loops on the authentication screen until a valid token is obtained.

**Register** — prompts for a username and password. The username must be unique. Spaces are not supported. Password requirements are minimal (minimum length: 1 character).

**Login** — prompts for username and password. On success, a JWT token is stored in memory for the duration of the session (tokens are valid for 8 hours). Your user ID is then displayed — note it if you intend to share it with other users acting as buyers.

#### Main menu

After authentication, the main menu presents two roles:

```
> Buyer role
  Seller role
```

The menu loops — after each action completes you are returned here.

---

#### Buyer role

From the buyer role menu you can:

```
> List all sellers
  List participated auctions
  List won auctions
```

**List all sellers**

Displays a scrollable list of all user IDs that have created at least one auction. Select a seller's ID with the arrow keys and press Enter to view their active and pending auctions.

From the seller's auction list, select an auction to see its details. You are then prompted to enter a bid amount. Entering nothing (leaving the prompt blank) cancels without bidding. If you confirm the bid, the result is displayed:

- Success: the bid is recorded.
- `422 Unprocessable Entity`: the bid violated auction rules (e.g. bid too low, auction not active, already bid in a First Price Sealed Bid auction).
- `409 Conflict`: a concurrent bid was placed — retry.

**Minimum bid rules by auction type:**

| Auction type | Visible minimum bid | Rule |
|---|---|---|
| English | Starting price (or last bid + 1) | Each new bid must exceed the current highest bid. |
| Dutch | Current decremented price | Any bid at or above the current price is accepted. The auction closes immediately. |
| First Price Sealed Bid | Not shown | Each user may bid at most once. |

**List participated auctions**

Displays a scrollable list of all auction IDs in which you have placed at least one bid. Selection doesn't do anything.

**List won auctions**

Displays IDs of all finished auctions you have won.

---

#### Seller role

From the seller role menu you can:

```
> Create auction
  List user's auctions
```

**Create auction**

Prompts for:

1. **Description** — free-text description of the item being auctioned.
2. **Start time** — when the auction opens for bidding.
3. **End time** — when the auction closes. Must be after the start time and in the future.
4. **Auction type** — choose from `English`, `Dutch`, or `FirstPriceSealedBid`.
5. Type-specific fields:
   - **English**: starting price.
   - **Dutch**: starting price, price decrement rate, price decrement frequency (default: 1 day).
   - **First Price Sealed Bid**: no additional fields.

**List user's auctions**

Prompts you to choose a status (`Pending`, `Active`, or `Finished`), then displays a scrollable, panelled list of all your auctions with that status. Each panel shows:

- Auction ID
- Auction type and type-specific parameters
- Description
- Start and end times
- Current status, and for finished auctions, the winner's user ID and winning bid (if any)

Selecting a `Pending` or `Active` auction opens the **update screen**, where you can extend the auction's end time. The new end time must be later than the current end time. Selecting a `Finished` auction returns to the list.

---

#### Navigation

All list screens support scrolling with the **arrow keys** or with the dedicated buttons. Selecting the Quit option in any menu returns to the menu. After each completed action, the client waits for you to press **Enter** before returning to the menu.

#### Error handling

If the backend returns an error response, the client displays the HTTP error message and any error body returned by the server. Network errors or unexpected exceptions are also caught and displayed without crashing the application.
