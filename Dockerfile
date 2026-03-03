# Base Runtime Image
# =========================
# Use the official ASP.NET Core runtime image (lighter than SDK image).
# This image contains only what is needed to RUN the app, not buil
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*
USER app

# Set working directory inside container.
# All next commands will run relative to /app.
WORKDIR /app
# Inform Docker that the container listens on these ports.
# 8080 → main HTTP
# 8081 → optional HTTPS or secondary port
EXPOSE 8080
EXPOSE 8081


# Build Stage (SDK)
# =========================
# Use full .NET SDK image to build the project.
# This image includes compiler and build tools.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Define a build argument (can be overridden during docker build).
# Default is Release mode.
ARG BUILD_CONFIGURATION=Release
# Set working directory for source code.
WORKDIR /src
# Copy only .csproj files first.
# This allows Docker to cache dependency restore step
# and avoid restoring packages every time code changes.
COPY ["GymManagement/GymManagement.Web.csproj", "GymManagement/"]
COPY ["GymManagement.Application/GymManagement.Application.csproj", "GymManagement.Application/"]
COPY ["GymManagement.Domain/GymManagement.Domain.csproj", "GymManagement.Domain/"]
COPY ["GymManagement.Infrastructure/GymManagement.Infrastructure.csproj", "GymManagement.Infrastructure/"]
# Restore NuGet dependencies.
# This downloads all required external packages.
RUN dotnet restore "./GymManagement/GymManagement.Web.csproj"
# Copy the rest of the source code into container.
# This includes controllers, services, configs, etc.
COPY . .


# Move into the Web project directory.
WORKDIR "/src/GymManagement"
# Build the project.
# -c → configuration (Release/Debug)
# -o → output folder
RUN dotnet build "./GymManagement.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build



# Publish Stage
# =========================
# Create a new stage based on the build stage.
# This keeps publish step isolated.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Publish the application.
# publish = optimized compiled output ready for production.
# /p:UseAppHost=false avoids generating OS-specific executable.
RUN dotnet publish "./GymManagement.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# Final Runtime Image
# =========================
# Start from lightweight runtime image again.
# This ensures the final image does NOT contain SDK tools.
FROM base AS final
WORKDIR /app
# Copy published files from publish stage.
# This is called multi-stage build optimization.
# Final image only contains runtime + compiled app.
COPY --from=publish /app/publish .


# Health Check
# =========================
# Docker will periodically check this endpoint.
# If it fails 3 times, container is marked unhealthy.
# --interval → check every 30 seconds
# --timeout → wait max 10 seconds
# --start-period → wait 40 seconds before first check
# --retries → fail after 3 failed attempts
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget -qO- http://localhost:8080/health/live || exit 1


# Entry Point
# =========================
# This is the command that runs when container starts.
# It runs your ASP.NET Core application.
ENTRYPOINT ["dotnet", "GymManagement.Web.dll"]
