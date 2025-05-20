

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.sln .
COPY Transaction.Api/*.csproj ./Transaction.Api/
COPY Antifraud.Common/*.csproj ./Antifraud.Common/
COPY Antifraud.Dto/*.csproj ./Antifraud.Dto/
COPY Antifraud.Service/*.csproj ./Antifraud.Service/
COPY Antifraud.Model/*.csproj ./Antifraud.Model/
COPY Antifraud.Repository/*.csproj ./Antifraud.Repository/
COPY Antifraud.Core.Repository/*.csproj ./Antifraud.Core.Repository/

RUN dotnet restore Transaction.Api/Transaction.API.csproj

COPY Transaction.Api/. ./Transaction.Api/
COPY Antifraud.Common/. ./Antifraud.Common/
COPY Antifraud.Dto/. ./Antifraud.Dto/
COPY Antifraud.Service/. ./Antifraud.Service/
COPY Antifraud.Model/. ./Antifraud.Model/
COPY Antifraud.Repository/. ./Antifraud.Repository/
COPY Antifraud.Core.Repository/. ./Antifraud.Core.Repository/

RUN dotnet build -c Release Transaction.Api/Transaction.API.csproj

RUN dotnet publish Transaction.Api/Transaction.API.csproj -c Release -o /app/publish

# Etapa final (asp)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish . 
EXPOSE 80
ENTRYPOINT ["dotnet", "Transaction.API.dll"]