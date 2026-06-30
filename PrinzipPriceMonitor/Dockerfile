FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore "PrinzipPriceMonitor.csproj"

COPY . .

RUN dotnet publish "PrinzipPriceMonitor.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /app/data
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/prinzip_monitor.db"

EXPOSE 8080
ENTRYPOINT ["dotnet", "PrinzipPriceMonitor.dll"]