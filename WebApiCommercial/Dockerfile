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

# Publica a API
RUN dotnet publish WebApiCommercial/WebApiCommercial.csproj -c Release -o /publish

# Etapa 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        openssl \
        curl && \
    update-ca-certificates

# ✔ ICP Brasil (OBRIGATÓRIO)
RUN curl -L https://acraiz.icpbrasil.gov.br/ACRAIZ.cer \
    -o /usr/local/share/ca-certificates/icpbrasil.crt && \
    update-ca-certificates

# ✔ cadeia da SEFAZ GO (opcional, mas ajuda)
RUN echo | openssl s_client -showcerts -connect nfe.sefaz.go.gov.br:443 2>/dev/null \
    | awk '/BEGIN CERTIFICATE/,/END CERTIFICATE/{ print }' \
    > /usr/local/share/ca-certificates/sefaz-go.crt && \
    update-ca-certificates

WORKDIR /app

EXPOSE 80
EXPOSE 443

# Copia os arquivos publicados
COPY --from=build /publish .

# Copia as pastas estáticas necessárias
COPY --from=build /src/WebApiCommercial/NFSchemas ./NFSchemas

# Copia a pasta wwwroot, mas exclui a pasta certs (será gerenciada pelo volume)
COPY --from=build /src/WebApiCommercial/wwwroot ./wwwroot

# Remove a pasta certs que veio do código fonte (se existir)
# para garantir que o volume seja usado
RUN rm -rf ./wwwroot/certs 2>/dev/null || true

# Cria o diretório onde o volume será montado
RUN mkdir -p /app/wwwroot/certs

# Configura a porta que o Render espera
ENV ASPNETCORE_URLS=http://+:80
ENV RENDER=true

ENTRYPOINT ["dotnet", "WebApiCommercial.dll"]