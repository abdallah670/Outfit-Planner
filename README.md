# Outfit Planner

**Solve the daily "what to wear" problem.**

Outfit Planner is an intelligent wardrobe management system that generates outfit suggestions by analyzing your clothes against real-time weather, specific occasions, and your personal style preferences.

## Key Features

- **Smart Suggestions:** Automated outfit generation based on weather (e.g., "It's raining, wear this jacket") and occasion ("Business Casual").
- **Wardrobe Management:** Digital catalog of your clothes with AI-assisted tagging.
- **Social Validation:** Create polls to get outfit feedback from friends before you go out.
- **Wear Analytics:** Track cost-per-wear and identify underutilized items.

## Tech Stack

- **Backend:** ASP.NET Core 9 Web API
  - SQL Server & Entity Framework Core
  - ASP.NET Identity Auth
  - Clean Architecture with CQRS (MediatR)
- **Frontend:** Angular 17+
  - NgRx State Management
  - Bootstrap 5 / Angular Material UI

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- SQL Server

### Setup

1.  **Database:** Update the connection string in `src/OutfitPlanner.Api/appsettings.json`.
2.  **Migrations:** Run `dotnet ef database update -p src/OutfitPlanner.Infrastructure -s src/OutfitPlanner.Api`.
3.  **Backend:** Run `dotnet run` from the `src/OutfitPlanner.Api` directory.
4.  **Frontend:** Navigate to `src/outfit-planner-ui`, run `npm install`, then `npm start`.
