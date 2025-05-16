# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Antifraud.Worker/Antifraud.Worker.csproj", "Antifraud.Worker/"]
COPY ["Antifraud.Service/Antifraud.Service.csproj", "Antifraud.Service/"]
COPY ["Antifraud.Common/Antifraud.Common.csproj", "Antifraud.Common/"]
COPY ["Antifraud.Dto/Antifraud.Dto.csproj", "Antifraud.Dto/"]
COPY ["Antifraud.Model/Antifraud.Model.csproj", "Antifraud.Model/"]
COPY ["Antifraud.Repository/Antifraud.Repository.csproj", "Antifraud.Repository/"]
COPY ["Antrifraud.Core.Repository/Antifraud.Core.Repository.csproj", "Antrifraud.Core.Repository/"]
RUN dotnet restore "./Antifraud.Worker/Antifraud.Worker.csproj"
COPY . .
WORKDIR "/src/Antifraud.Worker"
RUN dotnet build "./Antifraud.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Antifraud.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Antifraud.Worker.dll"]