FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY LinkInBio.Backend.fsproj ./
RUN dotnet restore

# Copy source files
COPY . ./

# Build and publish
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "LinkInBio.Backend.dll"]
