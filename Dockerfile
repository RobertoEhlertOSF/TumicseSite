FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TumicseSite/TumicseSite.csproj TumicseSite/
RUN dotnet restore TumicseSite/TumicseSite.csproj

COPY . .
RUN dotnet publish TumicseSite/TumicseSite.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Render
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TumicseSite.dll"]
