# Remaining Production Readiness & SignalR Implementation Tasks

## Overview
This document outlines the remaining tasks to complete the production readiness and SignalR integration for the Outfit Planner platform.

---

## 🔴 Phase 1: Critical Security Fixes (Estimated: 2-3 days) (Done)

### 1. [COMPLETED] Security Audit & HTTPS Enforcement
- **Backend** (`src/OutfitPlanner.Api/Program.cs`):
  - Enforce HTTPS redirection
  - Configure HSTS headers
  - Remove development-only SSL cert trust
- **Frontend** (`src/outfit-planner-ui/src/environments/environment.prod.ts`):
  - Ensure `apiUrl` uses `https://` protocol
  - Verify CORS whitelist restricts to production domains only
- **Validation**: Production API only responds to HTTPS; HTTP requests return 301/308 redirect

### 2. [COMPLETED] Secrets & Credentials Management
- **Backend**:
  - Move `JWT_KEY`, `ConnectionStrings__DefaultConnection` to environment variables or Azure Key Vault
  - Remove hardcoded credentials from `appsettings.Development.json`
  - Add `appsettings.Production.json` to `.gitignore`
- **Validation**: No sensitive data in git history; app runs without `appsettings.Development.json`

### 3. [COMPLETED] Update Vulnerable NuGet Packages
- AutoMapper 12.0.1 → 13.0.1+
- SixLabors.ImageSharp 3.1.6 → 3.1.7+
- Newtonsoft.Json 11.0.1 → 13.0.3+
- **Validation**: `dotnet list package --vulnerable` returns empty

---

## 🟡 Phase 2: High Priority Deployment (Estimated: 4-5 days)

### 4. MonsterASP Deployment Configuration

#### 4A. MonsterASP Dashboard — Manual Configuration
Access your MonsterASP control panel at `https://monsterasp.net` and create/edit the website:

| Setting | Value |
|---------|-------|
| **ASP.NET Core version** | .NET 10 |
| **Application path** | `/api` (the API will be accessible at `https://outfitplanner.runasp.net/api/*`) |
| **Enable WebSocket** | ✅ Checked (required for SignalR hubs) |
| **Enable HTTPS** | ✅ Checked (auto SSL cert via MonsterASP) |

#### 4B. Environment Variables to Set in MonsterASP Dashboard
Navigate to **Settings → Environment Variables** and add these:

| Variable Name | Value / Example | Purpose |
|--------------|----------------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Tells ASP.NET Core to use production config |
| `ASPNETCORE_URLS` | `http://localhost:5000` | Internal binding (MonsterASP reverse-proxies) |
| `ConnectionStrings__DefaultConnection` | `Server=localhost;Database=outfitplanner;User Id=...;Password=...;TrustServerCertificate=True;ConnectRetryCount=3;ConnectRetryTimeout=30;` | Production SQL Server connection string |
| `JWT_KEY` | *(your 256+ bit secret key)* | Symmetric key for JWT token signing |
| `JWT_ISSUER` | `OutfitPlanner` | JWT issuer claim |
| `JWT_AUDIENCE` | `OutfitPlannerUsers` | JWT audience claim |
| `Authentication__Google__ClientId` | *(from Google Cloud Console)* | Google OAuth client ID *(optional)* |
| `Authentication__Google__ClientSecret` | *(from Google Cloud Console)* | Google OAuth client secret *(optional)* |
| `Authentication__Facebook__AppId` | *(from Facebook Dev Console)* | Facebook OAuth app ID *(optional)* |
| `Authentication__Facebook__AppSecret` | *(from Facebook Dev Console)* | Facebook OAuth app secret *(optional)* |
| `Hangfire__ServerName` | `outfitplanner-prod` | Hangfire server identifier |
| `Serilog__WriteTo__0__Name` | `File` | Serilog file sink |
| `Serilog__WriteTo__0__Args__path` | `Logs/outfitplanner-log.txt` | Log file path |
| `Serilog__WriteTo__1__Name` | `Console` | Serilog console sink (captured by MonsterASP) |

> **Note**: Use `__` (double underscore) as the hierarchy separator — ASP.NET Core maps these to `ConfigurationSection:Key` automatically.

#### 4C. Files Deployed to MonsterASP
The `web.config` at `src/OutfitPlanner.Api/web.config` is already configured with:
- WebSocket support enabled via `<webSocket enabled="true" />` and `<handlerSetting name="enableWebSockets" value="true" />`
- `ASPNETCORE_ENVIRONMENT=Production` environment variable set
- URL Rewrite rules to handle the `/api` path prefix
- Security headers (`X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`)

> **Warning**: The `web.config` is deployed alongside the published DLLs. Do NOT manually edit it on MonsterASP — update the source file and redeploy instead.

#### 4D. Database Setup on MonsterASP
1. In MonsterASP dashboard, go to **Databases → SQL Server**
2. Create a new SQL Server database (note the server name, database name, username, password)
3. Copy the connection string into the `ConnectionStrings__DefaultConnection` environment variable
4. The first deployment will auto-apply migrations via `dotnet ef database update` (if configured in CI/CD) or manually via:
   ```
   dotnet ef database update --connection "your_connection_string"
   ```

---

### 5. CI/CD Pipeline

#### 5A. GitHub Secrets — What to Add
In your GitHub repository: **Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Value Description |
|-------------|------------------|
| `FTP_SERVER` | MonsterASP FTP hostname (e.g., `ftp.outfitplanner.runasp.net`) |
| `FTP_USERNAME` | MonsterASP FTP username |
| `FTP_PASSWORD` | MonsterASP FTP password |
| `VERCEL_TOKEN` | Vercel API token (generate at https://vercel.com/account/tokens) |
| `VERCEL_ORG_ID` | Vercel organization ID (run `vercel whoami` → team slug) |
| `VERCEL_PROJECT_ID` | Vercel project ID (run `vercel project ls` or find in project settings) |

#### 5B. How to Get Vercel IDs
```bash
# Install Vercel CLI
npm i -g vercel

# Login
vercel login

# Link to your project (creates .vercel/project.json with IDs)
cd src/outfit-planner-ui
vercel link

# Then view the generated .vercel/project.json:
type .vercel\project.json
# Output: {"orgId":"...","projectId":"..."}
```

#### 5C. CI/CD Pipeline File
The pipeline is already created at `.github/workflows/deploy.yml` with two parallel jobs:

**Backend Job (on push to `main`):**
1. `actions/checkout@v4` — Checkout source
2. `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'` — Setup .NET 10
3. `dotnet restore` — Restore NuGet packages
4. `dotnet build --configuration Release --no-restore` — Build
5. `dotnet test --no-build --verbosity normal` — Run tests
6. `dotnet publish --configuration Release --no-build --output ./publish` — Publish
7. `SamKirkland/FTP-Deploy-Action@v4.3.5` — FTP deploy to MonsterASP at `/api/`

**Frontend Job (on push to `main`):**
1. `actions/checkout@v4` — Checkout source
2. `actions/setup-node@v4` with Node 22 + npm cache — Setup Node
3. `npm ci` — Clean install dependencies
4. `npm test -- --watch=false --browsers=ChromeHeadless --no-cache` — Run tests
5. `npm run build -- --configuration production` — Build for production
6. `amondnet/vercel-action@v25` — Deploy to Vercel

#### 5D. Vercel Configuration (`src/outfit-planner-ui/vercel.json`)
Already created with:
- **Framework**: Angular
- **Build command**: `npm run build -- --configuration production`
- **Output directory**: `dist/outfit-planner-ui/browser`
- **Install command**: `npm ci`
- **Rewrites**: All routes → `index.html` (SPA client-side routing)
- **Cache headers**: Assets cached 1 year, service worker uncached, manifest cached 1 hour

---

### 6. Database Production Configuration
- **EF Core Migrations**:
  - Ensure all pending migrations are committed to repository
  - Add migration application step to CI/CD pipeline
  - Test migrations on staging database before production
- **Connection Resiliency**:
  - Enable `EnableRetryOnFailure()` in production DbContext configuration
  - Configure connection string with `ConnectRetryCount=3;ConnectRetryTimeout=30;`
- **Validation**: Migrations run successfully in CI/CD pipeline; connection resiliency tested with simulated downtime

### 7. CORS Production Configuration
- **File**: `src/OutfitPlanner.Api/Program.cs`
- **Changes**:
  - Replace wildcard `AllowAnyOrigin()` with specific production origins (frontend Vercel URL, custom domain)
  - Configure `AllowAnyMethod()` and `AllowAnyHeader()` but restrict origins
  - Allow credentials if using cookie-based auth (or keep JWT in Authorization header without credentials)
- **Validation**: Requests from non-whitelisted origins return 403

---

## 🟡 Phase 3: Medium Priority Items (Estimated: 4-5 days)

### 8. PWA / Service Worker Registration
- **File**: `src/outfit-planner-ui/src/app/app.config.ts`
- **Changes** (already implemented):
  - `import { provideServiceWorker } from '@angular/service-worker';`
  - `provideServiceWorker('ngsw-worker.js', { enabled: !isDevMode(), registrationStrategy: 'registerWhenStable:30000' })`
- **PWA Manifest** (already created at `src/outfit-planner-ui/src/manifest.webmanifest`):
  - App name: "Outfit Planner", short name: "OutfitPlan"
  - Theme color: `#6366f1`, background: `#ffffff`
  - Display: `standalone`
  - Icons: 72x72 through 512x512 referenced from `assets/icons/`
- **Icon Assets Needed**: You must place `.png` icon files at `src/outfit-planner-ui/src/assets/icons/` matching the sizes in the manifest (72x72, 96x96, 128x128, 144x144, 152x152, 192x192, 384x384, 512x512). Use a tool like https://realfavicongenerator.net or https://www.pwabuilder.com to generate them.
- **Service Worker Config** (already exists at `src/outfit-planner-ui/ngsw-config.json`):
  - Caches static assets (`app` group: prefetch CSS/JS/manifest)
  - Lazy-loads media assets (`assets` group: images, fonts)
- **Angular.json**: Ensure `"serviceWorker": true` in the `production` build configuration (check `angular.json`)
- **Validation**: Lighthouse PWA audit passes with score > 90

---

## 📋 Complete Deployment Steps — Step by Step

### Step 1: Set Up MonsterASP
- [ ] Log into MonsterASP dashboard at https://monsterasp.net
- [ ] Create a new website (or use existing)
- [ ] Set **ASP.NET Core version** → `.NET 10`
- [ ] Set **Application path** → `/api`
- [ ] Enable **WebSocket** ✅
- [ ] Enable **HTTPS** ✅
- [ ] Copy the FTP credentials (server, username, password)

### Step 2: Create SQL Server Database on MonsterASP
- [ ] Go to **Databases → SQL Server → Create Database**
- [ ] Note: server name, database name, username, password
- [ ] Build the connection string: `Server=monsterasp-sql.server;Database=outfitplanner;User Id=...;Password=...;TrustServerCertificate=True;ConnectRetryCount=3;ConnectRetryTimeout=30`

### Step 3: Set MonsterASP Environment Variables
- [ ] Go to **Settings → Environment Variables**
- [ ] Add all variables from **Section 4B** above

### Step 4: Create GitHub Secrets
- [ ] Go to GitHub repo → **Settings → Secrets and variables → Actions**
- [ ] Add `FTP_SERVER`, `FTP_USERNAME`, `FTP_PASSWORD`
- [ ] Add `VERCEL_TOKEN`, `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID`

### Step 5: Set Up Vercel Project
- [ ] Go to https://vercel.com → **Add New → Project**
- [ ] Import your GitHub repository
- [ ] Set **Root Directory** → `src/outfit-planner-ui`
- [ ] Set **Framework Preset** → Angular
- [ ] Set **Build Command** → `npm run build -- --configuration production`
- [ ] Set **Output Directory** → `dist/outfit-planner-ui/browser`
- [ ] Add environment variable: `NODE_VERSION=22`
- [ ] Deploy (this first manual deploy creates the project ID)
- [ ] Run `vercel link` locally to get `orgId` and `projectId`

### Step 6: Generate PWA Icons
- [ ] Create a 512x512 PNG app icon
- [ ] Generate resized versions at: 72, 96, 128, 144, 152, 192, 384, 512
- [ ] Place all icons in `src/outfit-planner-ui/src/assets/icons/`

### Step 7: Push to GitHub
- [ ] Commit and push all changes to `main`
- [ ] Monitor the GitHub Actions workflow at **Actions** tab
- [ ] Verify backend deploys to `https://outfitplanner.runasp.net/api/health`
- [ ] Verify frontend deploys to Vercel URL
- [ ] Test CORS: frontend → API requests work

---

## 📋 Testing Checklist

### Production Deployment
- [ ] Backend builds and deploys to MonsterASP successfully
- [ ] Backend health check endpoint (`/health`) returns 200 OK
- [ ] Frontend builds with production environment without errors
- [ ] CORS properly restricts to production domains (non-whitelisted origins blocked)
- [ ] All NuGet packages updated and vulnerability-free (`dotnet list package --vulnerable`)
- [ ] Database migrations apply cleanly to production database
- [ ] SignalR connection established successfully from production frontend
- [ ] HTTPS enforced on all endpoints (HTTP redirects to HTTPS)
- [ ] No sensitive data exposed in API responses or frontend source
- [ ] File uploads work with production storage configuration
- [ ] Authentication flow (register/login/token refresh) works end-to-end
- [ ] Rate limiting active on public endpoints
- [ ] Logging captures errors and requests without exposing secrets
- [ ] CDN/static assets load correctly on production frontend

### Rollback & Monitoring
- [ ] Deployment rollback procedure documented and tested
- [ ] Application Insights / logging dashboard configured
- [ ] Alert rules set for error rate, response time, and uptime
- [ ] Database backup strategy in place and tested

---

## ⏱️ Estimated Timeline

| Phase | Priority | Items | Effort |
|-------|----------|-------|--------|
| Phase 1 | 🔴 Critical | Security fixes | 2-3 days |
| Phase 2 | 🟡 High | Deployment config + CI/CD + DB + CORS | 4-5 days |
| Phase 3 | 🟡 Medium | PWA | 4-5 days |

**Total Estimated Effort: 10-13 days**

---

## 📌 Notes

- **.NET Version**: Target .NET 10 as supported by MonsterASP.
- **SignalR**: WebSocket support enabled in both `web.config` and MonsterASP dashboard. Hubs mapped at `/notifications/hub` and `/social/hub`.
- **Environment Separation**: Maintain separate environment files (`environment.ts`, `environment.prod.ts`) with explicit API URLs. Production URL: `https://outfitplanner.runasp.net/api`.
- **Deployment Order**: Backend should deploy and pass smoke tests before frontend deploys to avoid version mismatches.