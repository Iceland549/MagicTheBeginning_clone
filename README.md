
# Magic The Beginning (MTB)

## Overview
Magic The Beginning (MTB) is a backend-first microservices architecture inspired by **Magic: The Gathering**.  
It provides authentication, card/deck management, and game orchestration, with persistence in **SQL Server** and **MongoDB**.  

Frontend: lightweight React client for testing gameplay.

---

## Architecture
- **Auth Service** : User management, JWT auth (SQL Server)  
- **Card Service** : Cards management & external Scryfall integration (MongoDB)  
- **Deck Service** : Player decks (MongoDB)  
- **Game Service** : Game sessions & rules engine (MongoDB)  
- **API Gateway (Ocelot)** : Single entrypoint to services  
- **Frontend (React)** : UI for playtesting  

---

## Tech Stack
- **.NET 8 / ASP.NET Core** (microservices)  
- **React** (frontend)  
- **SQL Server** (auth)  
- **MongoDB** (cards, decks, games)  
- **Ocelot** (API gateway)  
- **Docker Compose** (containerization)  
- **xUnit** (tests)  
- **GitHub Actions** (CI/CD)  

---

## Getting Started

### With Docker (recommended)
```bash
git clone https://github.com/iceland549/MagicTheBeginning_clone.git
cd MagicTheBeginning_clone
docker-compose up --build
````

Services available:

Gateway : [http://localhost:5000](http://localhost:5000)
Auth : [http://localhost:5001/swagger](http://localhost:5001/swagger)
Card : [http://localhost:5002/swagger](http://localhost:5002/swagger)
Deck : [http://localhost:5003/swagger](http://localhost:5003/swagger)
Game : [http://localhost:5004/swagger](http://localhost:5004/swagger)
Frontend : [http://localhost:3000](http://localhost:3000)

---

### Local Dev (without Docker)

Configure SQL Server connection in `appsettings.Development.json`:

```json
{
  "UseLocalDb": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=MOA\\MSSQLSERVER01;Database=AuthDb;...",
    "DefaultConnection_Local": "Server=MOA\\MSSQLSERVER01;Database=AuthDbLocal;..."
  }
}
```

Create/update DB:

```bash
dotnet ef database update
```

Run services:

```bash
dotnet run --project Services/Auth/Auth.csproj
dotnet run --project Services/Card/Card.csproj
dotnet run --project Services/Deck/Deck.csproj
dotnet run --project Services/Game/Game.csproj
dotnet run --project Gateway/MTBGateway.csproj
```

Frontend:

```bash
cd Frontend && npm install && npm start
```

---

## Tests

```bash
dotnet test
```

---

## CI/CD

* **GitHub Actions** : build, test, coverage
* Artifacts available per commit (published services)

---

## Project Status

* Authentication & JWT operational
* Microservices persistence (SQL Server + MongoDB)
* Scryfall integration
* Unit/integration tests base
* Gameplay engine in progress

---

## Author

Developed by Denis CHAU
