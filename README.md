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
| `scripts/iis/` | Classic Release PowerShell (`Deploy-IisSite.ps1`), same pattern as RICH Connect. Not a build artifact. |
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
- **Azure AD SSO**: AUB staff and students via the app registration. Add redirect URI `https://localhost:7222/signin-oidc`. Each environment file supplies its own `AzureAd:ClientId`. Role (Student / Faculty) is resolved against AUB LDAP, not token group claims. Admins are the addresses in that environment's `AdminEmails` list, which is empty unless overridden.

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

The backend always logs to the console. It sends events to Seq only when `SEQ_SERVER_URL` is set. The frontend does not talk to Seq.

`ASPNETCORE_ENVIRONMENT` picks `appsettings.Development.json`, `appsettings.Staging.json`, or `appsettings.Production.json`, and Serilog stamps that same value on every event as `EnvironmentName`. It does not choose the Seq server. The server is whichever URL is in the machine variable on that box. Those URLs and API keys are not in git: not in `appsettings.*.json`, `web.config`, or pipeline YAML. Each environment file's `Serilog` section sets minimum levels only.

On Windows, `SEQ_SERVER_URL` and `SEQ_API_KEY` are read from machine scope (`HKLM\...\Session Manager\Environment`). A user variable, a value exported in the current shell, and `launchSettings.json` are ignored. On Linux (including Render) they are read from the process environment. `SEQ_API_KEY` is optional; Seq accepts a null key when ingestion is open.

Every event is enriched with `Application`, `EnvironmentName`, `MachineName`, and `ThreadId`. `UseSerilogRequestLogging` writes `HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms`. Over 1000 ms is a warning; an exception is an error. `Log.CloseAndFlush()` runs in a `finally`, so the last events are pushed on shutdown.

| Where it runs | Config file | Seq |
| --- | --- | --- |
| `dotnet run` on your machine (`https://localhost:7222`) | `appsettings.Development.json` | `docker-compose.yml` (`datalust/seq:2024.1`) |
| IIS on the staging Windows host (`https://urvp-staging.aub.edu.lb`) | `appsettings.Staging.json` | Machine `SEQ_SERVER_URL` on that host. Seq itself is not in this repo. |
| IIS on the production Windows host (`https://urvp.aub.edu.lb`) | `appsettings.Production.json` | Same hook, on the production host. |

Local Seq UI: <http://localhost:5341>. Ingestion accepts events on that same URL. <http://localhost:5342> is the dedicated ingestion port. The first-run admin password is `DevPassword123` (username `admin`) and applies only to an empty `seq-data` volume. Change it if the container is reachable beyond your machine. Do not reuse it on an AUB server.

```powershell
docker compose up -d seq
[System.Environment]::SetEnvironmentVariable("SEQ_SERVER_URL", "http://localhost:5341", "Machine")
# After creating an ingest API key in the Seq UI (Settings → API Keys):
[System.Environment]::SetEnvironmentVariable("SEQ_API_KEY", "<ingest-key>", "Machine")
```

`GetEnvironmentVariable(..., Machine)` reads the registry, so a new `dotnet run` sees the value without inheriting it from the parent shell. `ASPNETCORE_ENVIRONMENT=Development` is already set in `Properties/launchSettings.json`.

On each AUB Windows host, set the two variables at machine scope, then `iisreset`. The worker inherits its environment from Windows Process Activation Service, which read it at service start, so an app-pool recycle alone often keeps the old environment. Use a separate ingest key per environment, restricted to ingest. `ASPNETCORE_ENVIRONMENT` for Staging and Production is written to `applicationHost.config` by `scripts/iis/Set-AspNetCoreEnvironment.ps1`; it is not a machine variable.

In the Seq UI, filter with `Application = 'FEA.URVP.Backend'` and `EnvironmentName = 'Development'` (or `Staging` / `Production`). `MachineName` separates hosts that share one Seq server.

## CI/CD and environments

Azure Pipelines (`azure-pipelines.yml`) builds on `ubuntu-latest`, publishes a `win-x64` backend, copies the Next.js export into `wwwroot`, and publishes `Build.zip` as **`app-release` only**. The zip is the same shape as RICH Connect: `FEA.URVP\FEA.URVP.Backend\` (dll, `web.config`, `wwwroot`). The classic Release extracts it to `C:\inetpub\wwwroot\`.

**Classic Release:** empty job named **Setup IIS**, artifact **Build** → this pipeline’s `app-release`. Agent pool = same as RICH Connect. One **PowerShell** task: paste `scripts/iis/Deploy-IisSite.ps1`. For production, change `$siteDnsName` to `urvp.aub.edu.lb` and `$environmentName` to `Production`.

| Branch | Build | Release |
| --- | --- | --- |
| `Dev` | CI + package | Do not deploy |
| `Staging` | CI + package | Staging IIS (`urvp-staging.aub.edu.lb`) |
| `Master` | CI + package | Production IIS (`urvp.aub.edu.lb`) |

Health: `GET /health/live` is anonymous and does not touch SQL. `GET /health/ready` requires an administrator or a configured monitoring network, and reuses a cached database result.

A Docker image (`Dockerfile` at the repo root) and `render.yaml` exist for a same-origin container deploy. AUB production is IIS, not Render.

## Configuration notes

- `appsettings.Development.json` is **not** copied on publish (`CopyToPublishDirectory=Never`).
- Staging/Production require `AzureAd:TenantId` and `AzureAd:ClientId` or the process refuses to start.
- Connection strings outside Development must use `Encrypt=True` and must not use `TrustServerCertificate=true`.
- Do not put secrets in `NEXT_PUBLIC_*` variables; they are inlined into the static export.

Production security (cookies, CSRF, CSP, rate limits, file uploads, reverse-proxy requirements): [docs/SECURITY.md](docs/SECURITY.md).
