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
COPY ["NFe/NFe.Danfe.QuestPdf/NFe.Danfe.QuestPdf.csproj", "NFe/NFe.Danfe.QuestPdf/"]
COPY ["NFe/DFe.Classes/DFe.Classes.csproj", "NFe/DFe.Classes/"]
COPY ["NFe/DFe.Utils/DFe.Utils.csproj", "NFe/DFe.Utils/"]
COPY ["NFe/NFe.Danfe.Base/NFe.Danfe.Base.csproj", "NFe/NFe.Danfe.Base/"]

RUN dotnet restore WebApiCommercial/WebApiCommercial.csproj

COPY . .

RUN dotnet publish WebApiCommercial/WebApiCommercial.csproj -c Release -o /publish

# Etapa 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# 🔧 NÃO INSTALA NENHUMA DEPENDÊNCIA SKIASHARP DO SISTEMA
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        libc6-dev \
        libgdiplus \
        libx11-6 \
        libxcb1 \
        libxext6 \
        libxrender1 \
        libfontconfig1 \
        libfreetype6 \
        openssl \
        tzdata \
        && rm -rf /var/lib/apt/lists/*

# 🔧 REMOVE QUALQUER VERSÃO EXISTENTE DO SKIASHARP
RUN find /usr -name "*libSkiaSharp*" -type f -delete 2>/dev/null || true && \
    find /usr -name "*libHarfBuzzSharp*" -type f -delete 2>/dev/null || true

# 🔧 CONFIGURAÇÃO OPENSSL
RUN sed -i 's/^#\(openssl_conf = openssl_init\)/\1/' /etc/ssl/openssl.cnf 2>/dev/null || true && \
    echo "\n[openssl_init]\nssl_conf = ssl_sect\n\n[ssl_sect]\nsystem_default = system_default_sect\n\n[system_default_sect]\nCipherString = DEFAULT@SECLEVEL=1\n" >> /etc/ssl/openssl.cnf

# 🔧 TIMEZONE BRASÍLIA
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# 🔧 CONFIGURAÇÕES .NET
ENV DOTNET_SYSTEM_NET_SECURITY_PROTOCOLS=Tls12
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV COMPlus_SystemNetHttp_UseSocketsHttpHandler=false
ENV ASPNETCORE_URLS=http://+:80
ENV RENDER=true

# 🔧 CONFIGURAÇÕES CRÍTICAS PARA SKIASHARP
ENV SKIA_SHARP_SKIP_NATIVE_LOADER_CHECK=true
ENV DOTNET_GCHeapHardLimit=1C0000000

WORKDIR /app
EXPOSE 80

# Copia os arquivos publicados
COPY --from=build /publish .

# 🔧 VERIFICA E MOSTRA AS NATIVE LIBS QUE FORAM COPIADAS
RUN echo "=== Native libraries in /app ===" && \
    find /app -name "*.so" -type f 2>/dev/null | grep -E "(SkiaSharp|HarfBuzz)" || echo "No SkiaSharp libs found in /app" && \
    echo "=== Native libraries in /app/runtimes ===" && \
    find /app/runtimes -name "*.so" -type f 2>/dev/null | grep -E "(SkiaSharp|HarfBuzz)" || echo "No SkiaSharp libs found in runtimes" && \
    echo "=== Setting up library paths ==="

# 🔧 COPIA MANUALMENTE AS NATIVE LIBS DO SKIASHARP PARA O DIRETÓRIO DO SISTEMA
RUN if [ -d "/app/runtimes/linux-x64/native" ]; then \
        cp -f /app/runtimes/linux-x64/native/libSkiaSharp.so* /usr/lib/ 2>/dev/null || true; \
        cp -f /app/runtimes/linux-x64/native/libHarfBuzzSharp.so* /usr/lib/ 2>/dev/null || true; \
        echo "Copied native libs from runtimes"; \
    fi && \
    if [ -f "/app/libSkiaSharp.so" ]; then \
        cp -f /app/libSkiaSharp.so* /usr/lib/ 2>/dev/null || true; \
        echo "Copied native libs from app root"; \
    fi && \
    ldconfig 2>/dev/null || true

RUN mkdir -p /app/wwwroot/certs

ENTRYPOINT ["dotnet", "WebApiCommercial.dll"]