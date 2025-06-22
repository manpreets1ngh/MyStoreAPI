# Use .NET 8 runtime for the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use .NET 8 SDK for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["ApplicationToSellThings.APIs/ApplicationToSellThings.APIs.csproj", "ApplicationToSellThings.APIs/"]
RUN dotnet restore "ApplicationToSellThings.APIs/ApplicationToSellThings.APIs.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/ApplicationToSellThings.APIs"

# Build the project in Release mode
RUN dotnet build "ApplicationToSellThings.APIs.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ApplicationToSellThings.APIs.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image for the final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApplicationToSellThings.APIs.dll"]
