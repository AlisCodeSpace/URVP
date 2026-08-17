# syntax=docker/dockerfile:1
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore FEA.URVP.Backend.csproj
RUN dotnet publish FEA.URVP.Backend.csproj -c Release -o /app/publish --no-restore \
    /p:UseAppHost=false \
    /p:ServerGarbageCollection=false

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Render's 512MB sandbox: diagnostics IPC and server GC often SIGSEGV (exit 139).
ENV DOTNET_EnableDiagnostics=0 \
    COMPlus_EnableDiagnostics=0 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_gcServer=0 \
    DOTNET_GCHeapCount=1 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=10000 \
    ASPNETCORE_URLS=http://0.0.0.0:10000

COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["dotnet", "FEA.URVP.Backend.dll"]
