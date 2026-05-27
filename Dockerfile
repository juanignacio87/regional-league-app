FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["RegionalLeagueApp.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]
COPY ["RegionalLeagueApp.Domain/RegionalLeagueApp.Domain.csproj", "RegionalLeagueApp.Domain/"]
COPY ["RegionalLeagueApp.Application/RegionalLeagueApp.Application.csproj", "RegionalLeagueApp.Application/"]
COPY ["RegionalLeagueApp.Infrastructure/RegionalLeagueApp.Infrastructure.csproj", "RegionalLeagueApp.Infrastructure/"]
COPY ["RegionalLeagueApp.Web/RegionalLeagueApp.Web.csproj", "RegionalLeagueApp.Web/"]
COPY ["RegionalLeagueApp.Tests/RegionalLeagueApp.Tests.csproj", "RegionalLeagueApp.Tests/"]

RUN dotnet restore "RegionalLeagueApp.sln"

COPY . .
RUN dotnet publish "RegionalLeagueApp.Web/RegionalLeagueApp.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot/uploads/clubs

ENTRYPOINT ["dotnet", "RegionalLeagueApp.Web.dll"]
