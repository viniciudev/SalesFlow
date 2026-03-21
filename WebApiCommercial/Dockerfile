# Etapa 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copia os arquivos de projeto
COPY ["WebApiCommercial/WebApiCommercial.csproj", "WebApiCommercial/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["Repository/Repository.csproj", "Repository/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["NFe/NFe.AppTeste.NetCore/NFe.AppTeste.NetCore.csproj", "NFe/NFe.AppTeste.NetCore/"]
COPY ["NFe/NFe.Classes/NFe.Classes.csproj", "NFe/NFe.Classes/"]
COPY ["NFe/NFe.Utils/NFe.Utils.csproj", "NFe/NFe.Utils/"]
COPY ["NFe/NFe.Servicos/NFe.Servicos.csproj", "NFe/NFe.Servicos/"]
COPY ["NFe/NFe.Wsdl.Standard/NFe.Wsdl.Standard.csproj", "NFe/NFe.Wsdl.Standard/"]

# Restaura dependências
RUN dotnet restore WebApiCommercial/WebApiCommercial.csproj

# Copia todo o código
COPY . .

# Publica a API com verbosidade para ver erros detalhados
RUN dotnet publish WebApiCommercial/WebApiCommercial.csproj -c Release -o /publish --verbosity diag

# Etapa 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

EXPOSE 80
EXPOSE 443

COPY --from=build /publish .

# Configura a porta que o Render espera
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "WebApiCommercial.dll"]