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

RUN dotnet restore WebApiCommercial/WebApiCommercial.csproj

COPY . .

RUN dotnet publish WebApiCommercial/WebApiCommercial.csproj -c Release -o /publish

# Etapa 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# 🔧 CONFIGURAÇÃO DE TIMEZONE (BRASÍLIA)
ENV TZ=America/Sao_Paulo
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        openssl \
        libssl3 \
        tzdata && \
    ln -fs /usr/share/zoneinfo/America/Sao_Paulo /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    update-ca-certificates --fresh

# 🔧 Configuração OpenSSL para algoritmos legados
RUN sed -i 's/^#\(openssl_conf = openssl_init\)/\1/' /etc/ssl/openssl.cnf 2>/dev/null || true && \
    echo "\n[openssl_init]\nssl_conf = ssl_sect\n\n[ssl_sect]\nsystem_default = system_default_sect\n\n[system_default_sect]\nCipherString = DEFAULT@SECLEVEL=1\n" >> /etc/ssl/openssl.cnf

# Configurações .NET
ENV DOTNET_SYSTEM_NET_SECURITY_PROTOCOLS=Tls12
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV COMPlus_SystemNetHttp_UseSocketsHttpHandler=false
ENV ASPNETCORE_URLS=http://+:80
ENV RENDER=true

WORKDIR /app
EXPOSE 80

COPY --from=build /publish .
COPY --from=build /src/WebApiCommercial/NFSchemas ./NFSchemas
COPY --from=build /src/WebApiCommercial/wwwroot ./wwwroot
RUN rm -rf ./wwwroot/certs 2>/dev/null || true && mkdir -p /app/wwwroot/certs

ENTRYPOINT ["dotnet", "WebApiCommercial.dll"]