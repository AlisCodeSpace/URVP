# Production security

This document describes the deployed security posture of FEA.URVP: the request flow, the
configuration a deployment must supply, what the reverse proxy has to do, the risks we have
accepted, and how to re-verify all of it.

## 1. Topology

Production runs as a **same-origin backend-for-frontend**. One ASP.NET Core process serves both
the statically exported Next.js app and the API, so the browser sees a single origin.

```
Browser
  └─ HTTPS  →  reverse proxy / IIS  (TLS termination, X-Forwarded-*)
                 └─ ASP.NET Core (FEA.URVP.Backend)
                      ├─ exported Next.js app from wwwroot (public assets)
                      ├─ Azure AD OIDC sign-in  → application cookie
                      └─ /api/*  — authorizes every request
```

The browser holds **only** the `FEA.URVP.Auth` session cookie. `SaveTokens` is off, so no id_token
or access token is returned to, or stored by, the frontend. Nothing in the browser is treated as an
authorization input: frontend route guards are user-experience controls only, and every API request
is authorized server-side from the cookie's claims.

### Request pipeline order

`FEA.URVP.Api/Configuration/MiddlewareConfiguration.cs` is the single source of truth. The order
matters:

1. `UseForwardedHeaders` — first, so everything downstream sees the real scheme and client IP.
2. Exception handling — wraps everything after it.
3. Serilog request logging.
4. HTTPS redirection and HSTS.
5. Security headers and CSP (per-response nonce generated here).
6. Static frontend assets.
7. Routing → CORS → rate limiter → authentication → authorization.
8. Antiforgery validation for mutating API requests.
9. Controllers, then the SPA fallback.

## 2. Authentication and authorization

| Control | Behaviour |
| --- | --- |
| Default scheme | `UrvpCookie` for authenticate, sign-in, **and challenge** |
| Session cookie | `HttpOnly`, `Secure`, `Path=/`, `IsEssential`, 8 h sliding |
| `SameSite` | `Strict` when same-origin; `None` only when `Cors:AllowedOrigins` is non-empty |
| API failures | 401 / 403 JSON — never a 302 to an HTML login page |
| Fallback policy | Authenticated user required; public routes opt out with `[AllowAnonymous]` |
| Role / ownership | Enforced in command and query handlers, not in controllers or the frontend |

Challenging the cookie scheme rather than OIDC is what makes an unauthenticated `fetch()` receive a
401 it can act on instead of a cross-origin redirect it cannot follow. Interactive sign-in still
reaches Azure because `AzureAdSsoController` names the OIDC scheme explicitly.

### Anonymous endpoints

This is the complete list. Everything else requires a session.

| Endpoint | Why it is public |
| --- | --- |
| `GET /health/live` | Liveness probe; returns `{"status":"healthy"}` and nothing else |
| `GET /health/ready` | Same minimal body unless the caller is an admin or on a monitoring network |
| `GET /api/auth/status` | The frontend must be able to ask "am I signed in?" before it has a session |
| `GET /api/auth/csrf` | Issues the antiforgery token pair; needed before the first mutation |
| `GET /api/auth/azuread-sso/signin`, `signout` | Sign-in and sign-out cannot require a session |
| `GET /api/auth/dev/signin` | **Development only.** Not registered outside Development |
| `POST /api/security/csp-report` | Browsers post violation reports without credentials |
| `GET /api/news`, `GET /api/news/slug/{slug}` | Public marketing content |
| `GET /api/workshops`, `GET /api/workshops/{id}` | Public marketing content |
| `GET /api/semesters`, `active`, `{id}` | Public cycle dates; contains no personal data |
| `GET /api/files/{id}` | Authorizes per file — see below |
| Exported frontend assets and the SPA fallback | Generated JavaScript and routes are public by definition |

`GET /api/projects` and `GET /api/projects/{id}` were previously anonymous and returned faculty
email addresses through `ProjectDto`. They now require authentication.

`GET /api/files/{id}` is anonymous at the routing layer but authorizes inside
`GetFileByIdQueryHandler`: a file is served to an anonymous caller only when it is classified
public. Private files require ownership or an administrator role, and only public files are
cacheable.

### Azure AD OIDC

- `RequireHttpsMetadata = true` outside Development; normal certificate validation.
- Issuer, audience, lifetime and signing keys are all validated; the issuer is pinned when the
  tenant is a single-tenant GUID.
- Correlation and nonce cookies are `SameSite=None` + `Secure` + `HttpOnly`, which `form_post`
  requires.
- `SaveTokens = false`, `GetClaimsFromUserInfoEndpoint = false`, `UseTokenLifetime = false`.
- Identity-provider exceptions are logged in full server-side and answered with a generic message
  plus a trace identifier.
- Post-login return URLs are validated by `ReturnUrlValidationService`: root-relative paths in
  Production, and configured absolute origins only in Development.

**Flow choice.** Authorization code + PKCE is used whenever `AzureAd:ClientSecret` is set, and is
preferred. Without a secret the handler falls back to `response_type=id_token` with
`response_mode=form_post`, because the AUB app registration is a public client and Azure will not
complete a server-side code exchange without a credential. The application needs identity only —
it calls no downstream Microsoft API, so no access or refresh token is ever requested. The id_token
is POSTed directly to the server, never appears in a URL fragment, and is never visible to
JavaScript. Adding a secret or certificate switches the flow to code + PKCE with no code change.

**Startup requirement.** `AzureAd:TenantId` and `AzureAd:ClientId` are mandatory outside
Development; the app logs a fatal message and refuses to boot without them. This is deliberate: the
OIDC handler is an `IAuthenticationRequestHandler`, so its options are built on *every* request to
test for the callback path. A missing setting discovered lazily inside the options factory used to
return 500 for every route in the application, `/health/live` included, while startup still reported
success. Failing at boot turns that into one legible message.

## 3. CSRF

Cookie authentication means `SameSite` and CORS are defence in depth, not CSRF protection. Every
mutating API request must also carry an antiforgery token.

- `GET /api/auth/csrf` returns `{ headerName, token }` and sets the paired `FEA.URVP.Antiforgery`
  cookie. The cookie is `HttpOnly`, so no token value is ever readable from JavaScript or persisted
  in browser storage.
- The frontend echoes the token in `X-CSRF-TOKEN`.
- `AntiforgeryValidationMiddleware` validates `POST`, `PUT`, `PATCH` and `DELETE` on `/api/*`.
  Failure is a 403 with `antiforgery_validation_failed` and no further detail.
- Safe methods are exempt.

## 4. Security headers and CSP

Applied by `SecurityHeadersMiddleware` to every response:

`X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`,
`X-Frame-Options: DENY`, `Cross-Origin-Opener-Policy: same-origin`, a restrictive
`Permissions-Policy`, and `Cache-Control: no-store` on sensitive dynamic responses.

Two different policies are emitted:

- **HTML documents** — `default-src 'self'; base-uri 'self'; object-src 'none'; script-src 'self'
  'nonce-…'; script-src-attr 'none'; frame-ancestors 'none'; form-action 'self'` plus explicit
  `connect-src`, `img-src`, `font-src`, `frame-src`. There is no `unsafe-inline` in `script-src`.
- **API and asset responses** — `default-src 'none'` with everything locked off, which is stricter
  than the document policy.

A cryptographically random nonce is generated per HTML response, placed in the CSP header, and
injected only into the inline bootstrap `<script>` tags that Next.js emits. `ExportedFrontendProvider`
caches parsed documents and applies the nonce per request; API responses and file downloads are
never buffered for this.

`Security:ContentSecurityPolicy:ReportOnly` switches to `Content-Security-Policy-Report-Only` for a
staging rollout. Extra hosts go in the `ConnectSrc` / `ImgSrc` / `FontSrc` / `FrameSrc` lists rather
than by widening the policy to `https:`.

## 5. Rate limiting

Fixed-window budgets, partitioned by authenticated user id when there is one and by real client IP
otherwise. Rejections return **429** with `Retry-After` and a generic body that discloses neither
the window nor the budget.

| Policy | Default per minute | Applies to |
| --- | --- | --- |
| Global | 600 | all traffic |
| `urvp-auth` | 120 | sign-in initiation, callbacks, CSRF token |
| `urvp-upload` | 30 | file upload |
| `urvp-download` | 240 | file download |
| `urvp-report` | 20 | CSP report ingestion |
| `urvp-public-form` | 20 | public/contact forms |

> **Counters are in-process.** This is only sound for a single-instance deployment. Running more
> than one instance divides every budget by the instance count. Moving to multiple instances
> requires a distributed store (Redis) — see accepted risks.

## 6. Input and file security

- FluentValidation runs in the MediatR pipeline; validation messages are authored for end users and
  are the only exception detail returned in every environment.
- Explicit request-body and multipart size limits are configured.
- Uploads are checked server-side for: non-empty content, per-file and combined size limits,
  **magic-byte MIME detection**, agreement between the detected signature and the file extension,
  and membership of an explicit allow-list. Filenames are sanitized.
- The browser's `Content-Type` is never trusted. A text file renamed `.pdf`, a PNG renamed `.pdf`,
  and a PDF renamed `.png` are all rejected.
- SVG is rejected outright. MIME detection is type validation, not malware scanning.

## 7. Data protection, database, secrets

- Data Protection keys are persisted to the database via `PersistKeysToDbContext<AppDbContext>()`
  with the application name `FEA.URVP.Backend`, so cookies and antiforgery tokens survive a restart
  and are shared across instances.
- All data access goes through EF Core with parameterized queries. No SQL is built from user input.
- The connection string requires `Encrypt=True`. `TrustServerCertificate=true` is set **only** in
  `appsettings.Development.json`; startup logs an error if it appears outside Development.
- `AllowedHosts` is restricted to the real production domains, so Host-header spoofing returns 400.
- Production explicitly overrides every development flag: `Auth:EnableDevSignIn: false`,
  `Database:ApplyMigrationsOnStartup: false`, `Database:SeedCatalogsOnStartup: false`.
- Demo email sign-in is *hard*-disabled outside Development: `DevSignInPolicy` ignores the
  configuration value in Production entirely, and the controller is not registered.

## 8. Administrative surfaces

- The OpenAPI schema is **not registered at all** in Production. It is anonymous in Development and
  requires an authenticated administrator anywhere in between (for example Staging).
- Detailed readiness output requires an administrator session or a caller inside
  `Security:Health:MonitoringNetworks`. Anonymous callers get `{"status":"healthy"}` with no
  dependency names, URLs, or exception text.
- CSP report ingestion is rate-limited and size-limited, and logged bodies are sanitized and
  truncated.

## 9. Required configuration

Secrets must come from the platform's secret store or protected environment variables — never from
a source-controlled `appsettings` file, a `NEXT_PUBLIC_*` variable, the frontend bundle, a CI log,
or a committed compose file.

### Mandatory

| Variable | Notes |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Must be set explicitly per deployment (`Production` / `Staging`) |
| `ConnectionStrings__SqlServerConnection` | Must include `Encrypt=True` and must **not** include `TrustServerCertificate=true` |
| `AzureAd__TenantId` | Directory (tenant) GUID. App refuses to boot without it |
| `AzureAd__ClientId` | Application (client) GUID. App refuses to boot without it |
| `AllowedHosts` | Real hostnames, semicolon-separated. No wildcard |

### Recommended

| Variable | Notes |
| --- | --- |
| `AzureAd__ClientSecret` | Enables authorization code + PKCE instead of the `id_token` fallback |
| `AzureAd__CallbackPath` | Defaults to `/signin-oidc-ad`; must match the app registration redirect URI |
| `Security__TrustedProxies__KnownProxies__0` | Reverse-proxy IP allowed to set `X-Forwarded-*` |
| `Security__TrustedProxies__KnownNetworks__0` | CIDR alternative to the above |
| `Security__Health__MonitoringNetworks__0` | CIDR permitted to read detailed readiness |
| `SEQ_SERVER_URL`, `SEQ_API_KEY` | Structured log sink |

### Deliberately left empty in Production

| Setting | Effect |
| --- | --- |
| `Cors:AllowedOrigins` | Empty ⇒ all cross-origin requests denied. Correct for same-origin BFF |
| `Security:Hsts:IncludeSubDomains` | Enable only once every subdomain serves HTTPS |
| `Security:Hsts:Preload` | Enable only after confirming hstspreload.org requirements; hard to reverse |
| `Security:TrustedProxies:TrustAnyProxy` | Must stay `false` behind a proxy with a stable address |

Setting `Cors:AllowedOrigins` to a non-empty list switches the session cookie to
`SameSite=None`, which weakens CSRF defence in depth. Only do it for a genuinely split-origin
deployment.

## 10. Reverse proxy requirements

1. **Terminate TLS** and forward `X-Forwarded-Proto`, `X-Forwarded-Host` and `X-Forwarded-For`.
2. **Register the proxy address** in `Security:TrustedProxies:KnownProxies` or `KnownNetworks`.
   Untrusted sources are ignored, so an unregistered proxy makes the app see the proxy's IP as the
   client IP — which collapses every rate-limit partition into one.
3. **Use one real-IP header consistently.** The rate limiter partitions on
   `Connection.RemoteIpAddress` after forwarded-header processing, so the proxy must send the same
   `X-Forwarded-For` chain the app is configured to trust.

   `ForwardLimit` stays at **1** — one trusted hop — even when
   `Security:TrustedProxies:TrustAnyProxy` is enabled. This matters: with no limit the middleware
   walks the entire `X-Forwarded-For` chain and honours the leftmost entry, so any caller could
   prepend a header and choose the IP the application sees. That hands an anonymous attacker a
   fresh rate-limit partition on every request, silently disabling rate limiting. Consuming a
   single hop takes only the rightmost entry, which is the one the edge appended, and ignores
   anything the client supplied. Raise `ForwardLimit` only if there genuinely is more than one
   trusted hop, and only by the exact number of them.
4. **Strip fingerprinting headers** at the proxy as well: `Server`, `X-Powered-By`,
   `X-AspNet-Version`, `X-AspNetMvc-Version`. Kestrel's `Server` header is already disabled in
   `Program.cs`.
5. **Do not add a second set of security headers.** ASP.NET Core is authoritative for CSP, and a
   proxy-added CSP would either conflict with, or strip, the per-response nonce.
6. **Serve the app at the origin root.** The exported frontend uses root-relative asset and API
   paths.
7. If the platform terminates TLS and exposes only a plain-HTTP port to the container, set
   `Security:Https:RedirectToHttps=false` to avoid a redirect loop. The app already infers this from
   the presence of a `PORT` environment variable.

## 11. Deployment

**One service, not two.** `render.yaml` declares a single web service. `FEA.URVP.Backend/Dockerfile`
builds the Next.js export in a Node stage and copies it into the published app's `wwwroot`, so the
image serves both halves from one origin. The build context is the repository root, because the
frontend sources live outside `FEA.URVP.Backend`.

The previous blueprint ran `urvp-api` and `urvp-web` as separate services. That was replaced: a
split origin forces the session cookie to `SameSite=None` and requires opening CORS, which
reintroduces exactly the cross-site request forgery exposure the antiforgery layer exists to close.
It was also broken independently — `next start` does not work with `output: 'export'`.

- For a local or IIS build, `npm run build:deploy` in `FEA.URVP.Frontend` runs the export and
  copies it to `FEA.URVP.Backend/wwwroot`. That directory is gitignored; it is a build artifact.
- Reproducible installs are enforced: `npm ci`, and `dotnet restore --locked-mode` against the
  `packages.lock.json` files generated by `RestorePackagesWithLockFile` in `Directory.Build.props`.
- **Do not apply database migrations during application startup.**
  `Database:ApplyMigrationsOnStartup` is `false` in Production and in `render.yaml`. Run migrations
  as a controlled release step with a backup taken first and a tested rollback path.
- `.github/workflows/ci.yml` runs build, test, and dependency and artifact audits on push and pull
  request with `permissions: contents: read`. It deliberately does not deploy.
- Staging deployment should trigger only on push/merge to the staging branch, never from a
  pull-request workflow, and workflow permissions should be least-privilege. Never force-push a
  protected deployment branch.
- `.dockerignore` keeps `appsettings.Development.json`, `.env`, and any locally published
  `wwwroot` out of the image.

## 12. Accepted risks

| Risk | Why accepted | Trigger to revisit |
| --- | --- | --- |
| **Rate-limit counters are in-process.** | Deployment is documented as single-instance. | Any scale-out. Move to Redis before adding a second instance. |
| **`id_token` / `form_post` when no client secret is set.** | The AUB app registration is a public client. No access or refresh token is ever requested, and the token never reaches JavaScript. Weaker than code + PKCE only in lacking proof-of-possession on the authorization response, which the correlation and nonce cookies mitigate. | Add a secret or certificate to the registration; the code already prefers code + PKCE. |
| **`style-src 'unsafe-inline'`.** | Next.js emits inline `<style>` for critical CSS, and style injection is far lower impact than script injection. `script-src` has no `unsafe-inline`. | If Next.js gains nonce support for style tags. |
| **Detailed readiness is admin-only by default.** | `Security:Health:MonitoringNetworks` is empty, so an external monitor sees only `healthy`/`unhealthy`. | Add the monitoring CIDR when a monitoring system needs dependency detail. |
| **No CI deployment pipeline in this repository.** | The deployment target is managed outside the repo. `.github/workflows/ci.yml` runs build, test and dependency audits, but does not deploy. | Add the deployment workflow with least-privilege permissions and an explicit `ASPNETCORE_ENVIRONMENT`. |
| **`Security:TrustedProxies:TrustAnyProxy` is `true` on Render.** | Render's edge addresses are not stable, so they cannot be pinned. `ForwardLimit` stays 1, so only the edge-appended `X-Forwarded-For` entry is honoured and a client cannot choose its own IP. | Any host with a stable proxy address — pin it in `KnownProxies` and set this back to `false`. |
| **24 `react-hooks/set-state-in-effect` lint errors.** | Pre-existing component patterns newly flagged by a linter upgrade, not security defects. The one in `AuthProvider` is a false positive — the `setState` runs inside a subscription callback, which the rule's own guidance endorses. | Address as normal frontend maintenance. |
| **`xunit` 2.9.3 is marked deprecated** in favour of xunit.v3. | Test-only dependency with no known vulnerability. | Migrate when convenient. |
| **No secret scanning or SAST/CodeQL yet.** | Requires organisation-level configuration. | Enable CodeQL and secret scanning on the repository. |

## 13. Verification

Commands actually executed against this implementation.

```bash
# Backend build and tests (154 tests, 108 of them security tests)
dotnet build FEA.URVP.Backend/FEA.URVP.Api/FEA.URVP.Api.csproj
dotnet test  FEA.URVP.Tests/FEA.URVP.Tests.csproj

# Reproducible restore, exactly as CI runs it
dotnet restore --locked-mode

# Dependency audits
dotnet list package --vulnerable --include-transitive   # no vulnerable packages
dotnet list package --deprecated                        # xunit 2.9.3 only

# Frontend
cd FEA.URVP.Frontend
npm ci
npm run lint
npm run build          # static export, every route prerendered
npm audit              # 0 vulnerabilities
npm run publish:backend

# Production artifact inspection — all expected to find nothing
ls -R out | grep '\.map$'
grep -rl 'sourceMappingURL' out
grep -rlE 'ClientSecret|client_secret|BEGIN .*PRIVATE KEY|AccountKey=' out
grep -rhoE 'NEXT_PUBLIC_[A-Z_]+' out | sort -u
```

Runtime checks, and what they returned:

| Check | Result |
| --- | --- |
| CSP nonce in header matches the nonce in the HTML | identical, per response |
| `script-src` without `unsafe-inline` | confirmed |
| `Server` / `X-Powered-By` headers | absent |
| `GET /api/projects` anonymous | 401 |
| `GET /api/users` anon / student / admin | 401 / 403 / 200 |
| `POST /api/news` student vs admin, identical body | 403 vs 400 validation |
| Forged `X-User-Role: Admin` header | ignored; still 403 |
| Mutation with no `X-CSRF-TOKEN` | 403 `antiforgery_validation_failed` |
| Mutation with a bogus token | 403 |
| Mutation with a valid token | passes antiforgery, reaches validation |
| Session cookie flags | `HttpOnly`, `Secure`, `Path=/`, 8 h expiry |
| Antiforgery cookie `SameSite` | `strict` in Production, `none` in split-origin dev |
| Text file renamed `.pdf` | 400, rejected on magic bytes |
| PNG bytes renamed `.pdf` | 400 |
| PDF bytes renamed `.png` | 400 |
| SVG upload | 400 |
| 26 requests to `/api/security/csp-report` | 20 × 204 then 6 × 429 with `Retry-After: 60` |
| Same 26 requests with a rotating forged `X-Forwarded-For` behind a constant edge IP | still 20 × 204 then 6 × 429; limiter partitioned on the edge IP and ignored every forgery |
| `/health/live`, `/health/ready` anonymous | `{"status":"healthy"}`, no dependency detail |
| HSTS on the production hostname | `max-age=31536000` (absent on `localhost` by design) |
| `Host: evil.example` | 400 |
| `Origin: https://evil.example` | no `Access-Control-Allow-Origin` in the response |
| Swagger / OpenAPI / Scalar in Production | 404 — not registered |
| Demo sign-in in Production | 404, and startup logs that the flag was ignored |
| Azure AD metadata failure | generic `Invalid operation` + trace id to the user; full `IDX20803` detail server-side |
| Production boot without `AzureAd__TenantId` | refuses to start with a fatal message |
| Unmatched paths such as `/missing-chunk.js` | 404, with no authentication-failure log entry |

To repeat the runtime checks locally:

```powershell
cd FEA.URVP.Backend
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ASPNETCORE_URLS="https://localhost:7443"
$env:AllowedHosts="localhost;urvp.aub.edu.lb"
$env:AzureAd__TenantId="<tenant-guid>"
$env:AzureAd__ClientId="<client-guid>"
dotnet run --no-launch-profile --project FEA.URVP.Backend.csproj

# then, in another shell
curl -sk -D - -o NUL -H "Host: urvp.aub.edu.lb" https://localhost:7443/
```
