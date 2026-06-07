FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .

# Create a non-root user and ensure the app directory is owned by them
RUN groupadd -r app && useradd -r -g app -d /app -s /bin/sh app
RUN chown -R app:app /app

EXPOSE 8080

# Run the app as the non-root user
USER app

ENTRYPOINT ["dotnet","task-service.dll"]