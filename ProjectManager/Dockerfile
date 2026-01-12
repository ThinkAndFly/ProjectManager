# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files (paths are relative to build context = rootfolder)
COPY ProjectManager/ProjectManager.Presentation.API.csproj ProjectManager/
COPY ProjectManager.Application/ProjectManager.Application.csproj ProjectManager.Application/
COPY ProjectManager.Domain/ProjectManager.Domain.csproj ProjectManager.Domain/
COPY ProjectManager.Infraestructure/ProjectManager.Infraestructure.DataAccess.csproj ProjectManager.Infraestructure/
COPY ProjectManager.Infraestructure.Pulumi/ProjectManager.Infraestructure.Pulumi.csproj ProjectManager.Infraestructure.Pulumi/
COPY ProjectManager.Infraestructure.Messaging/ProjectManager.Infraestructure.Messaging.csproj ProjectManager.Infraestructure.Messaging/

# Restore from API project folder
WORKDIR /src/ProjectManager
RUN dotnet restore "./ProjectManager.Presentation.API.csproj"

# Copy the rest of the solution
WORKDIR /src
COPY . .

# Build API project
WORKDIR /src/ProjectManager
RUN dotnet build "./ProjectManager.Presentation.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/ProjectManager
RUN dotnet publish "./ProjectManager.Presentation.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectManager.Presentation.API.dll"]