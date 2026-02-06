# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Note the quotes around the project name to handle the space correctly
COPY ["AuthManager Enterprise.csproj", "."]
RUN dotnet restore "AuthManager Enterprise.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "AuthManager Enterprise.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Use quotes for the entry point as well
ENTRYPOINT ["dotnet", "AuthManager Enterprise.dll"]