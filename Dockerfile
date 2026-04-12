# Stage 1: Build & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо тільки csproj і робимо restore
COPY SupportBot.csproj ./
RUN dotnet restore ./SupportBot.csproj

# Копіюємо решту проекту
COPY . ./
WORKDIR /src
# Publish одразу
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SupportBot.dll"]