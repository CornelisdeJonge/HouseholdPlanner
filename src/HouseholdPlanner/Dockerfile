# File: src/HouseholdPlanner/Dockerfile

# Stage 1: Build & publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore first for better caching
COPY src/HouseholdPlanner/HouseholdPlanner.csproj ./HouseholdPlanner/
RUN dotnet restore ./HouseholdPlanner/HouseholdPlanner.csproj

# Copy the remaining source code
COPY src/HouseholdPlanner/. ./HouseholdPlanner/
WORKDIR /src/HouseholdPlanner

# Build (optional, but keeps publish faster and gives earlier failures)
RUN dotnet build "HouseholdPlanner.csproj" \
    -c Release \
    -o /app/build \
    /p:RunAnalyzers=false \
    /p:UseSharedCompilation=false

# Publish (trimmed, analyzer-less, container-friendly)
RUN dotnet publish "HouseholdPlanner.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:RunAnalyzers=false \
    /p:UseSharedCompilation=false \
    /p:BuildingInsideDocker=true \
    --no-restore

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Environment: listen on 8080 (Kestrel) and run non-Development by default
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_ENVIRONMENT=Production

# Copy published output
COPY --from=build /app/publish .

# Expose Kestrel port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "HouseholdPlanner.dll"]