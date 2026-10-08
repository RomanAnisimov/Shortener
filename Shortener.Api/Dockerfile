# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Shortener.Api/Shortener.Api.csproj", "Shortener.Api/"]
COPY ["Shortener.Data/Shortener.Data.csproj", "Shortener.Data/"]
COPY ["Shortener.Infrastructure/Shortener.Infrastructure.csproj", "Shortener.Infrastructure/"]
COPY ["Shortener.Application/Shortener.Application.csproj", "Shortener.Application/"]
COPY ["Shortener.Shared/Shortener.Shared.csproj", "Shortener.Shared/"]
COPY ["Shortener.Repository/Shortener.Repository.csproj", "Shortener.Repository/"]
COPY ["Shortener.Services/Shortener.Services.csproj", "Shortener.Services/"]
RUN dotnet restore "./Shortener.Api/Shortener.Api.csproj"
COPY . .
WORKDIR "/src/Shortener.Api"
RUN dotnet build "./Shortener.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Shortener.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shortener.Api.dll"]