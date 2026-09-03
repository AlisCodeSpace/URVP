# syntax=docker/dockerfile:1
#
# Single image for the same-origin BFF: the exported Next.js app is baked into the backend's
# wwwroot so the browser only ever talks to one origin. The build context is the repository
# root, not FEA.URVP.Backend, because the frontend sources live outside it.

# ---------------------------------------------------------------------------
# Frontend: static export
# ---------------------------------------------------------------------------
FROM --platform=linux/amd64 node:22-alpine AS frontend
WORKDIR /src

# npm ci (not npm install) so the build is reproducible from package-lock.json.
COPY FEA.URVP.Frontend/package.json FEA.URVP.Frontend/package-lock.json ./
RUN npm ci

COPY FEA.URVP.Frontend/ ./
RUN npm run build

# ---------------------------------------------------------------------------
# Backend: publish
# ---------------------------------------------------------------------------
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

# --locked-mode fails the build if packages.lock.json disagrees with the project files, so a
# transitive dependency cannot drift between the tested build and the shipped one.
RUN dotnet restore FEA.URVP.Backend/FEA.URVP.Backend.csproj --locked-mode

RUN dotnet publish FEA.URVP.Backend/FEA.URVP.Backend.csproj -c Release -o /app/publish --no-restore \
    /p:UseAppHost=false \
    /p:ServerGarbageCollection=false

# ---------------------------------------------------------------------------
# Runtime
# ---------------------------------------------------------------------------
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Render's 512MB sandbox: diagnostics IPC and server GC often SIGSEGV (exit 139).
# Disable config-file inotify watchers (user limit 128) so CreateBuilder can start.
ENV DOTNET_EnableDiagnostics=0 \
    COMPlus_EnableDiagnostics=0 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_gcServer=0 \
    DOTNET_GCHeapCount=1 \
    DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE=false \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=10000 \
    ASPNETCORE_URLS=http://0.0.0.0:10000

COPY --from=build /app/publish .

# Served by FrontendHostingConfiguration; Security:Frontend:RootPath defaults to wwwroot.
COPY --from=frontend /src/out ./wwwroot

EXPOSE 10000
ENTRYPOINT ["dotnet", "FEA.URVP.Backend.dll"]
