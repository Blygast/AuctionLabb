# =============================================================================
# Stage 1 — Build the React frontend
# =============================================================================
FROM node:20-alpine AS frontend-build
WORKDIR /src/client
COPY client/package.json client/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY client/ ./
RUN npm run build

# =============================================================================
# Stage 2 — Build the ASP.NET Core API, embedding the frontend under wwwroot
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Copy csproj files first for layer caching
COPY api/AuctionApi.slnx ./
COPY api/AuctionApi.Core/AuctionApi.Core.csproj   AuctionApi.Core/
COPY api/AuctionApi.Data/AuctionApi.Data.csproj   AuctionApi.Data/
COPY api/AuctionApi/AuctionApi.csproj             AuctionApi/
RUN dotnet restore

# Copy the rest of the source and publish a self-contained release
COPY api/ ./
COPY --from=frontend-build /src/client/dist AuctionApi/wwwroot
RUN dotnet publish AuctionApi/AuctionApi.csproj -c Release -o /app /p:UseAppHost=false

# =============================================================================
# Stage 3 — Runtime image
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Run as a non-root user
RUN useradd --create-home --shell /bin/bash app
USER app

COPY --from=backend-build --chown=app:app /app ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AuctionApi.dll"]
