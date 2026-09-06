# URVP

## Logging (Serilog + Seq)

The backend (`FEA.URVP.Backend`) uses [Serilog](https://serilog.net/) for structured logging. Console logging is always enabled; shipping logs to [Seq](https://datalust.co/seq) is optional and controlled from `appsettings`.

### How it works

- `Program.cs` builds the Serilog pipeline before the host starts: it clears the default `Microsoft.Extensions.Logging` providers, reads levels from the `Serilog` configuration section, and enriches every log event with `Application`, `EnvironmentName`, `MachineName`, and `ThreadId`.
- The Console sink is always active, so the app logs locally even if Seq is unreachable or unconfigured.
- The Seq sink is added only when `Seq:ServerUrl` is set to a non-empty value. `Seq:ApiKey` is optional and only needed if the target Seq instance requires an API key for ingestion.
- After the standard `appsettings.json` / `appsettings.{Environment}.json` files, the host also loads the optional, gitignored `appsettings.{Environment}.local.json`. Put an API key (or any other local-only override) there — never in a committed file.
- Startup is wrapped in try/catch/finally: successful startup logs an `Information` event, unhandled startup failures log a `Fatal` event, and `Log.CloseAndFlush()` always runs so buffered log events are not lost.
- `app.UseSerilogRequestLogging()` (in `MiddlewareConfiguration.cs`) emits exactly one structured event per HTTP request: `HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms`, at `Error` when an exception occurred, `Warning` when the request took longer than 1000 ms, and `Information` otherwise.

### Seq settings

| Setting | Required | Description |
| --- | --- | --- |
| `Seq:ServerUrl` | No | Seq ingestion URL (e.g. `http://localhost:5342`). When unset/empty, the app logs to console only. Development defaults this to the local compose service. |
| `Seq:ApiKey` | No (secret) | API key for Seq ingestion, only if the Seq instance requires one. Store it in `appsettings.{Environment}.local.json` on a developer machine, or in the deployed `appsettings` file on the server. **Never commit this value.** |

### Running Seq locally

A `docker-compose.yml` at the repository root defines a local-only `seq` service:

- Seq UI: <http://localhost:5341> (sign in as `admin` / `change-me-locally` on first run)
- Seq ingestion endpoint (`Seq:ServerUrl`): <http://localhost:5342>

To start it:

1. Run `docker compose up -d seq`.
2. Run the backend with the Development profile. `appsettings.Development.json` already points `Seq:ServerUrl` at the local ingestion endpoint.

### Verifying a structured test event in Seq

1. Start Seq (see above) and run the backend in Development.
2. Hit any endpoint (e.g. `GET /`) or wait for the startup log line.
3. Open the Seq UI at <http://localhost:5341> and confirm the event appears, then filter/search using `Application = 'FEA.URVP.Backend'`, `EnvironmentName`, `MachineName`, or `ThreadId` to verify those properties are indexed and searchable.

### Deployment notes

- Production leaves `Seq:ServerUrl` empty unless a Seq instance is available. To enable it on a host, set `Seq:ServerUrl` (and `Seq:ApiKey` if required) in that host's `appsettings.Production.json` or `appsettings.Production.local.json` and recycle the app pool.
- Changing a committed `appsettings` file takes effect on the next process start.
