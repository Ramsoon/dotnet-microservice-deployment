FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .

# Create a non-root user and ensure the app directory is owned by them
RUN groupadd -r appuser && useradd -r -g appuser -d /app -s /bin/sh appuser
RUN chown -R appuser:appuser /app

EXPOSE 8080

# Run the app as the non-root user
USER appuser

ENTRYPOINT ["dotnet","user-service.dll"]