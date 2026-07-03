# Specification of the final project for relevant C# courses

## C# Courses selection
(Change `[ ]` to `[x]` for the courses you plan to use this final project for.)

- [x] NPRG035 (Programming in C# language | Programování v jazyce C#)
- [x] NPRG038 (Advanced C# Programming | Pokročilé programování v jazyce C#)
- [ ] NPRG057 (Advanced .NET Programming II | Pokročilé programování pro .NET II)
- [ ] NPRG064 (Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET)

## Specification

### Project Specification: Web Auction Realtime Service (WARS)

WARS is a client-server application functioning as an online auction house. The project aims to create a platform
where users can set up auctions and bid on items in real time. 

The server acts as an independent auctioneer: it manages the state of the auctions, enforces the rules of individual
auction models, maintains persistent data, and ensures client synchronization. 

The application primarily simulates auction logic 
(actions are not legally binding, and real-world payments are not implemented).

#### System Architecture
The system is designed with a separated backend and frontend:
* **Backend (Server):** Provides the application logic as a Web API. It handles data persistence, user authentication, bid validation and current information retrieval.
* **Frontend (Client):** A console-based textual user interface (Console UI). It communicates with the server via the API.

#### Functional Requirements

##### User Management
* **Registration and Authentication:** The system allows the creation of a new user (assigning a unique ID and setting a password) and handles subsequent logins.
* **Unified Role:** Every authenticated user can seamlessly switch between the roles of a Seller and a Buyer.

##### Role: Seller
* **Auction Creation:** The user can instantiate a new auction for a specified item.
* **Supported Auction Types:**
    * *English auction:* Open ascending bids; the highest bid at the end of the time limit wins.
    * *Dutch auction:* The price gradually decreases over time; the first buyer to accept the current price wins and ends the auction.
    * *First-price sealed-bid:* Hidden bids; each participant submits exactly one bid. The highest bid after the time limit expires wins.
* **Parameter Management:** Set the starting price and the time limit of the auction. (Modifying the time limit after the auction starts is restricted exclusively to prolonging).
* **Results Overview:** View the history of their own created auctions, track their ongoing status, and see the final selling price and the winner's identity upon conclusion.

##### Role: Buyer
* **Active Participation:** The ability to place a bid in an ongoing auction. The server dictates whether the bid is valid based on the specific auction type's rules.
* **Search and Discovery:** Filter and search for active auctions via ID. List all active auctions.
* **User History:** Retrieve a list of all auctions the user has participated in, including a specific filter to view only the auctions they have won.

##### Role: Server (Auctioneer)
* **Rule Execution:** The server serves as the sole authority determining bid validity and declaring winners.
* **Timer and Lifecycle Management:** Autonomous termination of auctions upon expiration of the predefined time limit or when specific trigger conditions are met (e.g., a bid is placed in a Dutch auction).

#### Data Model
The core entities include:
* **User:** Identifier (ID), password.
* **Auction:** Identifier, owner ID (Seller), auction type, temporal parameters (start time, end time), starting price, and current status (e.g., Pending, Active, Finished).
* **Bid:** Auction identifier, user ID (Buyer), bid value, and a precise timestamp.

#### Libraries
* **Web Framework:** [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet)
* **Database:** [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)

* **API calls:** [Refit](https://www.nuget.org/packages/Refit)
* **CLI TUI:** [Spectre.Console](https://packages.nuget.org/packages/Spectre.Console/0.57.2-alpha.0.1)

#### Testing
* **Unit testing:** [XUnit](https://www.nuget.org/packages/xunit.v3/)
* **ASP.NET integration testing:** [Microsoft.AspNetCore.Mvc.Testing](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing) 
* **CLI TUI:** [Spectre.Console.Cli.Testing](https://packages.nuget.org/packages/Spectre.Console.Cli.Testing/1.0.0-alpha.0.16)