# FEA Undergraduate Research Volunteer Program (URVP)

Web application for the American University of Beirut Faculty of Engineering and Architecture **Undergraduate Research Volunteer Program**. Students submit research profiles, faculty post volunteer projects, both sides rank preferences, and administrators run matching, manage cycles, and publish news and workshops.

Production is a **same-origin backend-for-frontend**: one ASP.NET Core process serves the statically exported Next.js app and the API, so the browser talks to a single origin (`https://urvp.aub.edu.lb`). Staging is `https://urvp-staging.aub.edu.lb`.

## Architecture

```
Browser
  └─ HTTPS → reverse proxy / IIS
               └─ ASP.NET Core (FEA.URVP.Backend)
                    ├─ exported Next.js app from wwwroot
                    ├─ Azure AD OIDC → HttpOnly session cookie (FEA.URVP.Auth)
                    └─ /api/*  (authorization is always server-side)
```

Local development is the exception: Next.js runs on `https://localhost:3000` and the API on `https://localhost:7222`. That split-origin topology is development-only. Production never ships a Next.js server; `next.config.ts` uses `output: "export"`.

Role and ownership checks live in command/query handlers, not in the frontend. Frontend route guards are UX only.

## Repository layout

| Path | Role |
| --- | --- |
| `FEA.URVP.Backend/` | Host web project (`net10.0`) and nested libraries |
| `FEA.URVP.Backend/FEA.URVP.Api/` | Controllers, middleware, auth, OpenAPI |
| `FEA.URVP.Backend/FEA.URVP.Application/` | Commands, queries, validators (MediatR + FluentValidation) |
| `FEA.URVP.Backend/FEA.URVP.Domain/` | Entities, enums, catalogs |
| `FEA.URVP.Backend/FEA.URVP.Infrastructure/` | EF Core, SQL Server, LDAP, email, files |
| `FEA.URVP.Frontend/` | Next.js 16 App Router (static export) |
| `FEA.URVP.Tests/` | xUnit tests |
| `scripts/sql/` | Schema and catalog SQL for environments that do not migrate on startup |
| `scripts/iis/` | IIS publish helpers used by the classic Azure DevOps Release |
| `docs/SECURITY.md` | Production security posture (required reading for deploy) |
| `azure-pipelines.yml` | Azure DevOps Build (CI + `app-release` zip). IIS is a separate Release. |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20 or later (CI uses 22.16.0)
- SQL Server (local instance is enough for Development)
- [Docker](https://docs.docker.com/get-docker/) only if you want Seq locally

## Local development

Development applies EF migrations and seeds catalogs, news, workshops, a default semester, and demo auth accounts on startup (`Database:ApplyMigrationsOnStartup` and `Database:SeedCatalogsOnStartup` in `appsettings.Development.json`).

1. Create a local SQL Server database named `FEA_URVP_Dev`, or override `ConnectionStrings:SqlServerConnection` in a gitignored `FEA.URVP.Backend/appsettings.Development.local.json`.
2. (Optional) Start Seq: `docker compose up -d seq`.
3. Start the API from the repository root:

   ```powershell
   dotnet run --project FEA.URVP.Backend/FEA.URVP.Backend.csproj --launch-profile https
   ```

   API: <https://localhost:7222> (also binds HTTP on <http://localhost:5280>).

4. Start the frontend:

   ```powershell
   cd FEA.URVP.Frontend
   npm ci
   npm run dev
   ```

   App: <https://localhost:3000> (`next dev` uses `--experimental-https`).

`appsettings.Development.json` already allows CORS from those frontend origins. In production, `Cors:AllowedOrigins` is empty and the session cookie is `SameSite=Strict`.

To preview the same-origin layout locally (API serves the export from `wwwroot`):

```powershell
cd FEA.URVP.Frontend
npm run build:deploy
```

Then run the backend and open <https://localhost:7222>. `Security:Frontend:Enabled` is `false` in Development, so set it to `true` in a local override if you want the host to serve `wwwroot`.

### Sign-in

- **Demo accounts** (Development only; never registered in Production): `faculty@urvp.com`, `student@urvp.com`, `admin@urvp.com`. Use the sign-in page; there is no password.
- **Azure AD SSO**: AUB staff and students via the app registration. Add redirect URI `https://localhost:7222/signin-oidc-ad`. Role (Student / Faculty) is resolved against AUB LDAP, not token group claims. Admins are the addresses in `AdminEmails`.

Store secrets (Seq API key, Azure AD client secret, non-dev connection strings) in `appsettings.{Environment}.local.json`. That file is gitignored and loaded after the committed `appsettings`.

### OpenAPI

The schema is mapped at `/openapi/v1.json` outside Production. It is anonymous in Development and requires an authenticated administrator in Staging. It is not registered in Production.

## Database

Development uses EF Core `Migrate()` on startup against `FEA_URVP_Dev`. Staging and Production **must not** migrate or seed on startup; apply schema as a controlled release step (backup first).

Hand-run SQL scripts when you are not using automatic migrations:

- `scripts/sql/01_CreateSchema.sql` — schema plus `__EFMigrationsHistory` rows
- `scripts/sql/02_SeedCatalogs.sql` — research interests and activity types

Add a new migration from the repository root:

```powershell
dotnet ef migrations add <Name> `
  --project FEA.URVP.Backend/FEA.URVP.Infrastructure `
  --startup-project FEA.URVP.Backend
```

## Tests and audits

```powershell
dotnet test FEA.URVP.sln
```

CI also restores with `--locked-mode` (`packages.lock.json` per project), runs `dotnet list package --vulnerable --include-transitive`, and in the frontend `npm ci`, `npm run audit:ci`, and a production export that is rejected if source maps or secret-shaped strings appear in `out/`.

```powershell
cd FEA.URVP.Frontend
npm run lint
npm run audit:ci
```

## Logging (Serilog + Seq)

The backend uses [Serilog](https://serilog.net/). Console logging is always on; shipping to [Seq](https://datalust.co/seq) is optional.

- `Program.cs` builds the pipeline before the host starts, clears default logging providers, reads levels from the `Serilog` section, and enriches events with `Application`, `EnvironmentName`, `MachineName`, and `ThreadId`.
- The Seq sink is added only when `Seq:ServerUrl` is non-empty. `Seq:ApiKey` is optional.
- `app.UseSerilogRequestLogging()` emits one structured event per HTTP request: `HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms` (`Error` on exception, `Warning` if slower than 1000 ms, otherwise `Information`).

| Setting | Required | Description |
| --- | --- | --- |
| `Seq:ServerUrl` | No | Ingestion URL (e.g. `http://localhost:5342`). Empty ⇒ console only. Development defaults to the local compose service. |
| `Seq:ApiKey` | No (secret) | Only if the Seq instance requires one. Put it in `appsettings.{Environment}.local.json`. **Never commit it.** |

Local Seq (`docker-compose.yml`):

- UI: <http://localhost:5341> (first run: `admin` / `change-me-locally`)
- Ingestion (`Seq:ServerUrl`): <http://localhost:5342>

```powershell
docker compose up -d seq
```

Filter in Seq with `Application = 'FEA.URVP.Backend'`. Production leaves `Seq:ServerUrl` empty unless a Seq instance is available; set it (and `Seq:ApiKey` if required) on the host and recycle the app pool.

## CI/CD and environments

Azure Pipelines (`azure-pipelines.yml`) builds on `ubuntu-latest`, publishes a `win-x64` backend, copies the Next.js export into `wwwroot`, and publishes `Build.zip` as `app-release`. IIS deploy is a **classic Release pipeline** in Azure DevOps (same split as RICH Connect), not a YAML stage.

| Branch | Build | Release |
| --- | --- | --- |
| `Dev` | CI + package | Do not deploy |
| `Staging` | CI + package | Staging IIS (`urvp-staging.aub.edu.lb`) |
| `Master` | CI + package | Production IIS (`urvp.aub.edu.lb`) |

Health: `GET /health/live` and `GET /health/ready`. Anonymous callers get `{"status":"healthy"}` with no dependency detail.

A Docker image (`FEA.URVP.Backend/Dockerfile`, context = repo root) and `render.yaml` exist for a same-origin container deploy. AUB production is IIS, not Render.

## Configuration notes

- `appsettings.Development.json` is **not** copied on publish (`CopyToPublishDirectory=Never`).
- Staging/Production require `AzureAd:TenantId` and `AzureAd:ClientId` or the process refuses to start.
- Connection strings outside Development must use `Encrypt=True` and must not use `TrustServerCertificate=true`.
- Do not put secrets in `NEXT_PUBLIC_*` variables; they are inlined into the static export.

Production security (cookies, CSRF, CSP, rate limits, file uploads, reverse-proxy requirements): [docs/SECURITY.md](docs/SECURITY.md).
