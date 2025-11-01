# Multi-stage Dockerfile for LinkInBio.Backend
# This builds the F# application and creates a minimal runtime image

# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file first (for better Docker layer caching)
COPY LinkInBio.Backend.fsproj ./

# Restore dependencies
RUN dotnet restore LinkInBio.Backend.fsproj

# Copy all source files
COPY Models/ ./Models/
COPY Database/ ./Database/
COPY Services/ ./Services/
COPY Handlers/ ./Handlers/
COPY Program.fs ./
COPY appsettings.json ./

# Build and publish the application
RUN dotnet publish LinkInBio.Backend.fsproj \
    -c Release \
    -o out \
    --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application from build stage
COPY --from=build /app/out .

# Create a non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "LinkInBio.Backend.dll"]
