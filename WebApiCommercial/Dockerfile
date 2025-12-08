# Etapa 1 — Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo
COPY . .

# Restaura dependências
RUN dotnet restore WebApiCommercial/WebApiCommercial.csproj

# Publica a API
RUN dotnet publish WebApiCommercial/WebApiCommercial.csproj -c Release -o /publish

# Etapa 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApiCommercial.dll"]
