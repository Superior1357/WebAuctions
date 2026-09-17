# Frontend Developer Guide

This document describes the design, architecture, and internal workings of the **Frontend** console client project (`Frontend/Frontend.csproj`). It is intended for developers who want to understand or extend the codebase.

---

## Project Overview

The Frontend is a .NET 10 console application that provides an interactive text UI for the WARS auction platform. It communicates exclusively with the Backend over HTTP using typed [Refit](https://github.com/reactiveui/refit) clients, and renders its UI using [Spectre.Console](https://spectreconsole.net/).

The application has no local state beyond an in-memory JWT token. All data is fetched from and written to the backend on demand.

The project also references `Backend.csproj` directly to share domain types (DTOs, value objects, enums) without duplication.

---

## Application Startup and Dependency Injection

`Program.cs` creates a `Host` using `Host.CreateApplicationBuilder(args)` and configures all services before building and running.

### Configuration

`Api:BaseAddress` is read from `appsettings.json` (default: `http://localhost:5205`).

### Refit client registration

Each Refit interface is registered with `AddRefitClient<T>()`. The base address of each client matches its route group on the server:

| Interface | Base address |
|-----------|-------------|
| `IAuthApi` | `{baseAddress}/auth` |
| `IBuyMethodsApi` | `{baseAddress}/buy` |
| `ISellMethodsApi` | `{baseAddress}/sell` |
| `IIdentityApi` | `{baseAddress}` |

`IBuyMethodsApi`, `ISellMethodsApi`, and `IIdentityApi` also add `AuthHandler` as a `DelegatingHandler` so every request automatically carries the `Authorization: Bearer <token>` header. `IAuthApi` does not need authentication.

All Refit clients call `.RemoveAllLoggers()` to suppress the default HTTP request logging to the console (previously caused logging and UI conflicts).

---

## Architecture: ViewController Pattern

The UI is built around a lightweight **ViewController pattern**. All interactive screens implement:

```csharp
public interface IViewController
{
    string ViewName { get; }
    Task RunAsync();
}
```

`ViewName` is the label shown in selection menus. `RunAsync()` contains the screen's logic: rendering, user input, and API calls.

Views are **not reused** across navigations — each navigation resolves a fresh `Transient` instance from DI.

---

## MasterController

`MasterController` (implementing `IMasterController`) is the single point through which views are invoked. Its purpose is **exception handling**:

When a `Refit.ApiException` is caught, the controller tries to deserialize the response body as `ApiErrorBody { string Message }` and prints it. Other exceptions print their `.Message`. In both cases the application continues — the error is displayed and control returns to the caller.

---

## Authentication Flow

`AuthenticationViewController.RunAsync()` implements the following loop:

```
loop:
  clear screen
  prompt: Login | Register
  show chosen view (LoginMode or RegisterMode)
  wait for Enter
until tokenBearer.Token != null
then:
  call GET /identity → display user ID
  wait for Enter
```

---

## Main Menu Loop

`MainMenuLoopViewController.RunAsync()` presents a repeating selection:

```
> Buyer role
  Seller role
```

Each option resolves a fresh `BuyerRole` or `SellerRole` view controller via DI and passes it to `MasterController.ShowViewAsync()`. The loop continues indefinitely — there is no explicit logout or exit option.

---

## Buyer Role Views

`BuyerRole.RunAsync()` presents three options:

```
> List all sellers
  List participated auctions
  List won auctions
```

---

## Seller Role Views

`SellerRole.RunAsync()` presents two options:

```
> Create auction
  List user's auctions
```

---

## API Communication (Refit Clients)

Refit generates HTTP client implementations from C# interfaces at compile time. Each method maps to an HTTP endpoint via attributes:

### `IAuthApi` (`Authentication/AuthAPI.cs`)

### `IBuyMethodsApi` (`BuyerRole/IBuyMethodsAPI.cs`)

### `ISellMethodsApi` (`SellerRole/ISellMethodsAPI.cs`)

### `IIdentityApi` (`Miscellaneous/IIdentityApi.cs`)

---

## JWT Token Management

`ITokenBearer` / `TokenBearer` is a **singleton** that holds the JWT string in memory:

```csharp
public interface ITokenBearer
{
    string? Token { get; set; }
}
```

`AuthHandler` is a `DelegatingHandler` injected into the HTTP pipeline for all authenticated Refit clients. On each request it reads `ITokenBearer.Token` and adds the `Authorization: Bearer <token>` header if a token is present.

Because `TokenBearer` is a singleton and `AuthHandler` is transient, all Refit client instances share the same token. Setting `tokenBearer.Token` in `LoginMode` immediately makes the token available to all subsequent requests in the session.

---

## UI Utilities

### `ScrollingListViewController<T>`

A generic component that presents a scrollable list of items using `Spectre.Console.SelectionPrompt`. It is parameterized by:

- `title` — the prompt title shown above the list.
- `items` — the list of items to display.
- `toPanel` — a `Func<T, Panel>` that renders each item as a `Spectre.Console.Panel`.

`MakeChoiceAsync()` returns the selected `T`, or `null` if the list is empty or the user presses Quit.

### `PresentOptions.PresentAsync`

A helper in `PresentModes.cs` that takes a `params IViewController[]` array, builds a `SelectionPrompt<IViewController>` using each view's `ViewName` as the display label, and returns the selected view controller. This is used throughout to build menus dynamically.

### `Utils.PromptTimeAsync`

Prompts the user for a `DateTimeOffset` value, used when entering auction start and end times.

### `Utils.MakeConfirmationPrompt`

Displays a simple "Press Enter to continue" prompt, used to pause between screens so the user can read the output.

---

## Shared Types with Backend

The Frontend project references `Backend.csproj` and uses its types directly:

- **DTOs**: `CreatedAuction` and subtypes, `BuyerAuctionInfo` and subtypes, `SellerAuctionInfo` and subtypes, `BidRequest`, `AuctionUpdateRequest`, `TokenResponse`, `RegisterRequest`, `LoginRequest`.
- **Value objects**: `AuctionId`, `UserId`, `Price`, `AuctionDescription`.
- **Enums**: `AuctionStatus`.

This means both sides share identical serialization contracts by construction — no separate DTO mapping layer is needed in the frontend.

---

## Extending the Application

### Adding a new buyer action

1. Create a new class implementing `IViewController` in `BuyerRole/`.
2. Register it as `Transient` in `Program.cs`.
3. Add it to the `PresentOptions.PresentAsync(...)` call in `BuyerRole.RunAsync()`.
4. Add the corresponding Refit method to `IBuyMethodsApi` if a new endpoint is needed.

### Adding a new seller action

Same pattern as above but in `SellerRole/` and `SellerRole.RunAsync()`.

### Adding a new auction type

1. The shared DTO types (`CreatedAuction`, `BuyerAuctionInfo`, `SellerAuctionInfo`) must be extended in the backend project — see `docs/Backend.md`.
2. In `CreateMode.RunAsync()`, add a case to the `AuctionType` enum and the corresponding `switch` branch to collect type-specific parameters and build the new DTO subtype.
3. In `ListMode.SellerAuctionInfoToPanel()`, add a case to the `switch` that renders the new auction type's fields in the panel.
4. In `BuyerRole/Utils.cs` (or wherever buyer info is rendered), add rendering for the new `BuyerAuctionInfo` subtype.
5. All existing `switch` expressions in the frontend throw `ArgumentOutOfRangeException` by default, so missing cases will fail loudly during development.
