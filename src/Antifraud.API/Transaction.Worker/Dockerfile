FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.sln .
COPY Transaction.Worker/*.csproj ./Transaction.Worker/
COPY Antifraud.Common/*.csproj ./Antifraud.Common/
COPY Antifraud.Dto/*.csproj ./Antifraud.Dto/
COPY Antifraud.Service/*.csproj ./Antifraud.Service/
COPY Antifraud.Model/*.csproj ./Antifraud.Model/
COPY Antifraud.Repository/*.csproj ./Antifraud.Repository/
COPY Antifraud.Core.Repository/*.csproj ./Antifraud.Core.Repository/

RUN dotnet restore Transaction.Worker/Transaction.Worker.csproj

COPY Transaction.Worker/. ./Transaction.Worker/
COPY Antifraud.Common/. ./Antifraud.Common/
COPY Antifraud.Dto/. ./Antifraud.Dto/
COPY Antifraud.Service/. ./Antifraud.Service/
COPY Antifraud.Model/. ./Antifraud.Model/
COPY Antifraud.Repository/. ./Antifraud.Repository/
COPY Antifraud.Core.Repository/. ./Antifraud.Core.Repository/

RUN dotnet build -c Release Transaction.Worker/Transaction.Worker.csproj

RUN dotnet publish Transaction.Worker/Transaction.Worker.csproj -c Release -o /app/publish

# Etapa final (asp)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish . 
ENTRYPOINT ["dotnet", "Transaction.Worker.dll"]