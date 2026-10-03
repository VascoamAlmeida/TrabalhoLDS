# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY src/LojaMaquinas.Helper/LojaMaquinas.Helper.csproj ./src/LojaMaquinas.Helper/
COPY src/LojaMaquinas.Business/LojaMaquinas.Business.csproj ./src/LojaMaquinas.Business/
COPY src/LojaMaquinas.Data/LojaMaquinas.Data.csproj ./src/LojaMaquinas.Data/
COPY src/LojaMaquinas.WebApp/LojaMaquinas.WebApp.csproj ./src/LojaMaquinas.WebApp/

# Restore dependencies (only WebApp, without solution file to avoid test projects)
RUN dotnet restore src/LojaMaquinas.WebApp/LojaMaquinas.WebApp.csproj

# Copy everything else
COPY . ./

# Build and publish
RUN dotnet publish src/LojaMaquinas.WebApp/LojaMaquinas.WebApp.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Production uses SQLite at /data/lojamaquinas.db.
RUN mkdir -p /data
VOLUME ["/data"]

# Set environment to Production (uses SQLite)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "LojaMaquinas.WebApp.dll"]
