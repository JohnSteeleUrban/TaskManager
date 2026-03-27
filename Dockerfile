# Stage 1: Build the Vue frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY taskmanager-frontend/package*.json ./
RUN npm ci
COPY taskmanager-frontend/ ./
RUN npm run build

# Stage 2: Build the .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /app
COPY TaskManager.Api/*.csproj ./TaskManager.Api/
RUN dotnet restore TaskManager.Api/TaskManager.Api.csproj
COPY TaskManager.Api/ ./TaskManager.Api/
RUN dotnet publish TaskManager.Api/TaskManager.Api.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN mkdir -p /data
COPY --from=api-build /app/publish ./
COPY --from=frontend-build /app/frontend/dist ./wwwroot/

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
