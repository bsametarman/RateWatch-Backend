FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY . .

RUN dotnet restore "RateWatch.AnalyticsService.csproj"
RUN dotnet publish "RateWatch.AnalyticsService.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RateWatch.AnalyticsService.dll"]