# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files for dependency restore
COPY src/CinemaBooking.Domain/CinemaBooking.Domain.csproj src/CinemaBooking.Domain/
COPY src/CinemaBooking.Application/CinemaBooking.Application.csproj src/CinemaBooking.Application/
COPY src/CinemaBooking.Infrastructure/CinemaBooking.Infrastructure.csproj src/CinemaBooking.Infrastructure/
COPY src/CinemaBooking.Api/CinemaBooking.Api.csproj src/CinemaBooking.Api/

RUN dotnet restore src/CinemaBooking.Api/CinemaBooking.Api.csproj

# Copy source code and publish
COPY src/ src/

RUN dotnet publish src/CinemaBooking.Api/CinemaBooking.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "CinemaBooking.Api.dll"]
