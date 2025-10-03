# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY . .


RUN dotnet restore "RateWatch.AuthService.csproj"
RUN dotnet publish "RateWatch.AuthService.csproj" -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RateWatch.AuthService.dll"]