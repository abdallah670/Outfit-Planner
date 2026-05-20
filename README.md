# Outfit Planner

A full-stack wardrobe management and outfit planning application with social features, weather integration, AI-powered outfit suggestions, and an admin panel.

## Features

### 📱 Core Features
- **Wardrobe Management** — Add, categorize, and manage clothing items with images, tags, and metadata
- **Outfit Builder** — Create outfits by combining clothing items, with drag-and-drop interface
- **Daily Suggestions** — AI-powered outfit recommendations based on weather, calendar, and style preferences
- **Calendar** — Schedule outfits for events and get weather-based suggestions
- **Wear Tracking** — Log when you wear items and track usage statistics

### 🌐 Social Features
- **Community Feed** — Share outfits and polls with other users
- **Polls** — Create validation polls asking the community for outfit opinions
- **Reactions & Comments** — Like, react, and comment on posts
- **Follow System** — Follow other users and see their posts in your feed
- **Trending Outfits** — Algorithm calculates trending outfits based on engagement

### 🔐 Auth & Security
- **Email/Password Registration** — With email verification flow
- **Google & Facebook OAuth** — Social login with account linking
- **JWT Authentication** — With refresh token rotation
- **Password Reset** — Email-based password reset with secure tokens
- **Role-Based Access** — Admin and Planner roles with different permissions

### 🛠️ Admin Panel
- Dashboard with real-time analytics
- User management
- Content moderation (posts, polls, outfits)
- System settings and audit logs
- Data export and backup management

### 🔧 Infrastructure
- **Background Jobs** — Hangfire for scheduled tasks (trending calculation, account unlocking)
- **Caching** — Memory cache for searches and frequently accessed data
- **Image Processing** — Automatic thumbnail generation (thumb, medium, large)
- **Weather Integration** — OpenWeatherMap API for weather-based outfit suggestions
- **Email Service** — SMTP for verification, password reset, and notifications

## Tech Stack

### Backend
- **.NET 10** — API, Application, Domain, Persistence, Infrastructure layers
- **Entity Framework Core** — ORM with SQL Server
- **ASP.NET Core Identity** — Authentication & authorization
- **Hangfire** — Background job processing
- **Serilog** — Structured logging
- **AutoMapper** — Object mapping
- **FluentValidation** — Request validation
- **Swagger** — API documentation

### Frontend
- **Angular 19** — Standalone components, signals, reactive forms
- **NgRx** — State management
- **Angular Material** — UI components (dialogs, forms, tables, snackbars)
- **SweetAlert2** — Alert and confirmation dialogs
- **RxJS** — Reactive programming

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full instance)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

## Project Structure

```
Outfit-Planner/
├── src/
│   ├── OutfitPlanner.Api/          # REST API endpoints, middleware
│   │   ├── Controllers/            # API controllers
│   │   ├── Middleware/             # Request logging, exception handling
│   │   └── appsettings.json        # Application configuration
│   ├── OutfitPlanner.Application/  # Business logic, DTOs, handlers
│   │   ├── Features/               # CQRS feature folders
│   │   ├── Models/                 # Settings & request/response models
│   │   ├── Contracts/              # Interfaces & abstractions
│   │   └── Profiles/               # AutoMapper profiles
│   ├── OutfitPlanner.Domain/       # Entity models, enums, value objects
│   ├── OutfitPlanner.Infrastructure/ # External services (email, weather, storage)
│   └── OutfitPlanner.Persistence/  # EF Core DbContext, migrations, repositories
├── tests/
│   ├── OutfitPlanner.Application.IntegrationTests/
│   └── OutfitPlanner.Application.UnitTests/
└── src/outfit-planner-ui/          # Angular frontend
    └── src/app/
        ├── core/                   # Services, guards, interceptors, state
        ├── data/                   # Datasources, repositories, models
        ├── domain/                 # Entities, use cases
        └── presentation/           # Pages & components
```

## Setup Instructions

### 1. Database Setup (SQL Server)

Ensure SQL Server is running. The default connection string in `appsettings.json` uses Windows Authentication with `Server=.` (local instance). If you use a named instance or SQL authentication, update accordingly.

### 2. Environment Variables (Secrets)

**⚠️ Important:** The `appsettings.json` file contains placeholders for sensitive values. You must set these as environment variables for the app to work with real services.

Run the following in an **Administrator Command Prompt**:

```batch
:: JWT Signing Key (change to any secure 32+ char string)
setx JwtSettings__Key "ThisIsASecure256BitKeyForOutfitPlannerAPI12345"

:: Weather API Key (get from https://home.openweathermap.org/api_keys)
setx WeatherApi__ApiKey "your-openweathermap-api-key"

:: Background Removal API Key (get from https://remove.bg)
setx BackgroundRemoval__ApiKey "your-removebg-api-key"

:: Google OAuth (create at https://console.cloud.google.com)
setx Authentication__Google__ClientId "your-client-id"
setx Authentication__Google__ClientSecret "your-client-secret"

:: Facebook OAuth (create at https://developers.facebook.com)
setx Authentication__Facebook__AppId "your-app-id"
setx Authentication__Facebook__AppSecret "your-app-secret"

:: Email SMTP (for verification & password reset emails)
setx EmailSettings__SmtpUsername "your-email@gmail.com"
setx EmailSettings__SmtpPassword "your-gmail-app-password"
```

**Restart VS Code** after running the above commands for the changes to take effect.

### 3. Run Database Migrations

```bash
cd Outfit-Planner
dotnet ef database update --project src/OutfitPlanner.Persistence --startup-project src/OutfitPlanner.Api
```

This creates the database and applies all migrations.

### 4. Run the Backend

```bash
cd src/OutfitPlanner.Api
dotnet run
```

The API will start at:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001`
- **Swagger UI:** `http://localhost:5000/swagger`

The first run will seed the database with sample data (users, clothing items, outfits, polls, etc.).

### Default Seed Users

| Username | Email | Password | Role |
|----------|-------|----------|------|
| `admin` | `admin@example.com` | `Password123!` | Admin |
| `StyleMaven92` | `stylemaven92@example.com` | `Password123!` | Planner |
| `Fashionista_A` | `alex@example.com` | `Password123!` | Planner |
| `ChicExplorer` | `chic@example.com` | `Password123!` | Planner |
| `TrendSetter` | `trend@example.com` | `Password123!` | Planner |
| `UrbanVibes` | `urban@example.com` | `Password123!` | Planner |

### 5. Run the Frontend

```bash
cd src/outfit-planner-ui
npm install
ng serve
```

The frontend will be available at `http://localhost:4200`.

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/login` | Login with email & password |
| POST | `/api/Auth/register` | Register new user |
| POST | `/api/Auth/refresh` | Refresh JWT token |
| GET | `/api/Auth/google` | Google OAuth login |
| GET | `/api/Auth/facebook` | Facebook OAuth login |
| POST | `/api/Auth/forgot-password` | Request password reset |
| POST | `/api/Auth/reset-password` | Reset password with token |
| POST | `/api/Auth/verify-email` | Verify email with code |
| POST | `/api/Auth/resend-verification` | Resend verification email |

### Wardrobe
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Wardrobe/items` | Get clothing items |
| POST | `/api/Wardrobe/item` | Add clothing item |
| PUT | `/api/Wardrobe/item/{id}` | Update clothing item |
| DELETE | `/api/Wardrobe/item/{id}` | Delete clothing item |

### Outfits
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Outfits` | Get user's outfits |
| POST | `/api/Outfits` | Create outfit |
| GET | `/api/Outfits/{id}` | Get outfit by ID |
| PUT | `/api/Outfits/{id}` | Update outfit |
| DELETE | `/api/Outfits/{id}` | Delete outfit |
| POST | `/api/Outfits/{id}/wear` | Record outfit wear |

### Social Feed
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Feed` | Get community feed |
| POST | `/api/Feed/post` | Create feed post |
| POST | `/api/Feed/poll` | Create poll post |
| POST | `/api/Feed/{id}/react` | React to a post |
| POST | `/api/Feed/{id}/comment` | Comment on a post |
| GET | `/api/Feed/trending` | Get trending outfits |

### Admin
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Admin/dashboard` | Dashboard stats |
| GET | `/api/Admin/users` | User management |
| GET | `/api/Admin/posts` | Content moderation |
| GET | `/api/Admin/analytics/realtime` | Real-time analytics |
| GET | `/api/Admin/audit-logs` | Audit logs |

Full API documentation is available at `/swagger` when running.

## Configuration

### `appsettings.json` Structure

| Section | Description | Secrets? |
|---------|-------------|----------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection | ❌ (local dev) |
| `JwtSettings` | JWT signing key & issuer | ✅ Env var |
| `WeatherApi` | OpenWeatherMap API | ✅ Env var |
| `BackgroundRemoval` | Remove.bg API | ✅ Env var |
| `Authentication:Google` | Google OAuth | ✅ Env var |
| `Authentication:Facebook` | Facebook OAuth | ✅ Env var |
| `EmailSettings` | SMTP credentials | ✅ Env var |
| `AI` | GCP API key | ✅ Env var |
| `BackupSettings` | Backup configuration | ❌ |
| `CacheSettings` | Memory cache config | ❌ |
| `MaintenanceSettings` | Maintenance mode | ❌ |
| `ServiceManagementSettings` | Health checks | ❌ |
| `UserActivitySettings` | Activity tracking | ❌ |

### Environment Variable Naming

.NET uses `__` (double underscore) as a hierarchy separator. For example, `Authentication:Google:ClientSecret` in JSON becomes `Authentication__Google__ClientSecret` as an environment variable.

## Testing Password Reset (Development)

Without SMTP configured, the password reset link is printed to the **server console**:

```
=== PASSWORD RESET LINK ===
http://localhost:4200/reset-password?token=847291&email=user@example.com
===========================
```

Copy the link and paste it in your browser to test the flow.

## Background Jobs

The app uses **Hangfire** with the SQL Server storage provider. The dashboard is available at `/hangfire` when running.

### Scheduled Jobs

| Job Name | Schedule | Description |
|----------|----------|-------------|
| `daily-trending-calculation` | Daily at midnight UTC | Recalculates trending outfits |
| `auto-unlock-accounts` | Every 5 minutes | Unlocks accounts after lockout period |

## License

This project is for educational and demonstration purposes.