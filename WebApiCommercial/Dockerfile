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

# 🔧 CONFIGURAÇÃO CRÍTICA: Ativar algoritmos legados no OpenSSL
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        openssl \
        libssl3 \
        wget && \
    # Ativa o módulo de algoritmos legados
    sed -i 's/^#\(openssl_conf = openssl_init\)/\1/' /etc/ssl/openssl.cnf && \
    echo "\n[openssl_init]\nssl_conf = ssl_sect\n\n[ssl_sect]\nsystem_default = system_default_sect\n\n[system_default_sect]\nCipherString = DEFAULT@SECLEVEL=1\n" >> /etc/ssl/openssl.cnf && \
    # Instala e ativa o provedor legado (suporte a Triple DES e SHA1)
    mkdir -p /usr/local/ssl && \
    echo "openssl_conf = openssl_init" > /usr/local/ssl/openssl.cnf && \
    echo "[openssl_init]" >> /usr/local/ssl/openssl.cnf && \
    echo "providers = provider_sect" >> /usr/local/ssl/openssl.cnf && \
    echo "[provider_sect]" >> /usr/local/ssl/openssl.cnf && \
    echo "default = default_sect" >> /usr/local/ssl/openssl.cnf && \
    echo "legacy = legacy_sect" >> /usr/local/ssl/openssl.cnf && \
    echo "[default_sect]" >> /usr/local/ssl/openssl.cnf && \
    echo "activate = 1" >> /usr/local/ssl/openssl.cnf && \
    echo "[legacy_sect]" >> /usr/local/ssl/openssl.cnf && \
    echo "activate = 1" >> /usr/local/ssl/openssl.cnf && \
    # Configura ambiente para usar esta configuração
    export OPENSSL_CONF=/usr/local/ssl/openssl.cnf

# Instala certificados ICP-Brasil
RUN wget -q -O /usr/local/share/ca-certificates/icpbrasilv10.crt \
    https://acraiz.icpbrasil.gov.br/ICP-Brasilv10.crt && \
    update-ca-certificates

# Configurações .NET
ENV DOTNET_SYSTEM_NET_SECURITY_PROTOCOLS=Tls12
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV COMPlus_SystemNetHttp_UseSocketsHttpHandler=false
ENV OPENSSL_CONF=/usr/local/ssl/openssl.cnf
ENV ASPNETCORE_URLS=http://+:80
ENV RENDER=true

WORKDIR /app
EXPOSE 80

COPY --from=build /publish .
COPY --from=build /src/WebApiCommercial/NFSchemas ./NFSchemas
COPY --from=build /src/WebApiCommercial/wwwroot ./wwwroot
RUN rm -rf ./wwwroot/certs 2>/dev/null || true && mkdir -p /app/wwwroot/certs

ENTRYPOINT ["dotnet", "WebApiCommercial.dll"]