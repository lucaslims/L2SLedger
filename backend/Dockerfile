# ── Stage 1: Build & Publish ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

# Build-time metadata (injected by CI)
ARG BUILD_VERSION=0.0.0-local
ARG BUILD_SHA=unknown

WORKDIR /src

# Copy nuget config and solution for layer-cached restore
COPY nuget.config ./
COPY L2SLedger.sln ./
COPY src/L2SLedger.Domain/L2SLedger.Domain.csproj              src/L2SLedger.Domain/
COPY src/L2SLedger.Application/L2SLedger.Application.csproj    src/L2SLedger.Application/
COPY src/L2SLedger.Infrastructure/L2SLedger.Infrastructure.csproj src/L2SLedger.Infrastructure/
COPY src/L2SLedger.API/L2SLedger.API.csproj                    src/L2SLedger.API/

RUN dotnet restore src/L2SLedger.API/L2SLedger.API.csproj

# Copy remaining source and publish
COPY src/ src/
WORKDIR /src/src/L2SLedger.API
RUN dotnet publish -c Release -o /app/publish --no-restore \
    /p:Version="${BUILD_VERSION}" \
    /p:InformationalVersion="${BUILD_VERSION}+${BUILD_SHA}"

# ── Stage 2: Runtime ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# OCI image metadata
LABEL org.opencontainers.image.title="L2SLedger API" \
      org.opencontainers.image.description="L2SLedger Backend API (.NET 9)" \
      org.opencontainers.image.source="https://github.com/l2s-ledger/cash-flow" \
      org.opencontainers.image.vendor="L2SLedger"

# Security: create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

# Harden: disable .NET diagnostics and telemetry in production
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    COMPlus_EnableDiagnostics=0

COPY --from=build --chown=appuser:appgroup /app/publish .

RUN chown appuser:appgroup /app -R

# Switch to non-root user
USER appuser

# .NET 8+ defaults to port 8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "L2SLedger.API.dll"]
