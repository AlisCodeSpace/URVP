# URVP

## Logging (Serilog + Seq)

The backend (`FEA.URVP.Backend`) uses [Serilog](https://serilog.net/) for structured logging. Console logging is always enabled; shipping logs to [Seq](https://datalust.co/seq) is optional and controlled entirely by environment variables.

### How it works

- `Program.cs` builds the Serilog pipeline before the host starts: it clears the default `Microsoft.Extensions.Logging` providers, reads levels from the `Serilog` configuration section, and enriches every log event with `Application`, `EnvironmentName`, `MachineName`, and `ThreadId`.
- The Console sink is always active, so the app logs locally even if Seq is unreachable or unconfigured.
- The Seq sink is added only when `SEQ_SERVER_URL` is set to a non-empty value; `SEQ_API_KEY` is optional and only needed if the target Seq instance requires an API key for ingestion.
- Startup is wrapped in try/catch/finally: successful startup logs an `Information` event, unhandled startup failures log a `Fatal` event, and `Log.CloseAndFlush()` always runs so buffered log events are not lost.
- `app.UseSerilogRequestLogging()` (in `MiddlewareConfiguration.cs`) emits exactly one structured event per HTTP request: `HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms`, at `Error` when an exception occurred, `Warning` when the request took longer than 1000 ms, and `Information` otherwise. The previous custom `RequestLoggingMiddleware` was removed in favor of this to avoid duplicate request-logging events.

### Required environment variables

| Variable | Required | Description |
| --- | --- | --- |
| `SEQ_SERVER_URL` | No | Seq ingestion URL (e.g. `http://localhost:5342`). When unset/empty, the app logs to console only. |
| `SEQ_API_KEY` | No (secret) | API key for Seq ingestion, only needed if the Seq instance requires one. **Never commit this value** — configure it via your host's secret/environment store (e.g. Docker/Kubernetes secrets, IIS app-pool/Azure App Service application settings, CI/CD secret variables). |

Both variables are read as normal **process-level** environment variables first (works out of the box with Docker, Kubernetes, and CI/CD). On Windows/IIS deployments that instead rely on **machine-level** environment variables (Control Panel/System Properties), the app falls back to reading the machine scope if the process scope is empty — note that changes to machine-level variables require an IIS/application-pool (or full IIS) restart to take effect; process-level variables (e.g. set in `web.config`/App Service settings) do not.

### Running Seq locally

A `docker-compose.yml` at the repository root defines a local-only `seq` service:

- Seq UI: <http://localhost:5341>
- Seq ingestion endpoint (`SEQ_SERVER_URL`): <http://localhost:5342>

To start it:

1. Copy `.env.example` to `.env` and set a local `SEQ_ADMIN_PASSWORD` (never commit `.env`; it's gitignored).
2. Run `docker compose up -d seq`.
3. Set `SEQ_SERVER_URL=http://localhost:5342` (and optionally `SEQ_API_KEY`) before running the backend, e.g. in `FEA.URVP.Backend/Properties/launchSettings.json`'s `environmentVariables`, or in your shell before `dotnet run`.

### Verifying a structured test event in Seq

1. Start Seq (see above) and run the backend with `SEQ_SERVER_URL` set.
2. Hit any endpoint (e.g. `GET /`) or wait for the startup log line.
3. Open the Seq UI at <http://localhost:5341> and confirm the event appears, then filter/search using `Application = 'FEA.URVP.Backend'`, `EnvironmentName`, `MachineName`, or `ThreadId` to verify those properties are indexed and searchable.

### Deployment notes

- No IIS/application-pool restart is required to pick up `SEQ_SERVER_URL`/`SEQ_API_KEY` when they are set as **process-level** environment variables (Docker, Kubernetes, Render, Azure App Service application settings, etc.) — the app simply needs to be restarted/redeployed to read the new values, same as any other configuration change.
- An IIS/application-pool (or full IIS) restart **is** required if these variables are instead configured as **machine-level** Windows environment variables, since Windows only refreshes a process's environment block on process start.
