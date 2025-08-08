# SalesFlow
📌 Configuração Completa do Ambiente de Desenvolvimento
1️⃣ Instalação do SDK .NET 6.0 + ASP.NET Core (Backend - API)
🔹 Download: .NET 6.0 SDK
🔹 Verificação:

bash
dotnet --version  # Deve retornar 6.0.x
🔹 Criação de um projeto ASP.NET Core (API):

bash
dotnet new webapi -n NomeDaApi
🔹 Extras (opcional):

Visual Studio 2022 (Community/Professional) para desenvolvimento avançado em .NET.

Extensão no VSCode: C# (OmniSharp) para melhor suporte.

2️⃣ Instalação do Node.js 14.20.1 + NPM (Frontend)
🔹 Download: Node.js 14.20.1
🔹 Verificação:

bash
node --version  # v14.20.1
npm --version   # 6.14.17 (ou superior)
🔹 Gerenciador de versões (recomendado):

NVM (Node Version Manager) para alternar entre versões:

bash
nvm install 14.20.1
nvm use 14.20.1
3️⃣ Visual Studio Code (Editor Principal)
🔹 Download: VS Code
🔹 Extensões recomendadas:

Frontend:

ESLint (análise de código JavaScript).

Prettier (formatação automática).

Live Server (visualização rápida).

Backend:

C# (OmniSharp).

REST Client (para testar APIs).

Banco de Dados:

SQL Server (mssql) (extensão para conexão com SQL Server).

4️⃣ Git (Versionamento de Código)
🔹 Download: Git
🔹 Configuração inicial:

bash
git config --global user.name "Seu Nome"
git config --global user.email "seu@email.com"
🔹 Comandos úteis:

bash
git init          # Inicializa um repositório
git clone <url>   # Clona um repositório remoto
git status        # Verifica alterações
5️⃣ SQL Server + SQL Server Management Studio (SSMS)
🔹 Download SQL Server:

Developer Edition (gratuito para desenvolvimento): SQL Server Download
🔹 Download SSMS: SQL Server Management Studio
🔹 Configuração pós-instalação:

Criar um banco de dados local para testes.

Configurar conexão no projeto .NET via appsettings.json:

json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=NomeDoBanco;Trusted_Connection=True;"
}
🔧 Configuração Adicional
✔ Proxy/corporativo: Se estiver em rede restrita, configure:

bash
npm config set proxy http://user:senha@proxy:porta
git config --global http.proxy http://user:senha@proxy:porta
✔ Yarn (opcional):

bash
npm install -g yarn
yarn --version
✔ Docker (opcional): Para containerização da API e frontend.

✅ Checklist Pós-Instalação
.NET 6.0 + ASP.NET Core instalado (dotnet --version).

Node.js 14.20.1 + NPM configurados (node --version).

VS Code com extensões essenciais.

Git configurado com usuário/e-mail.

SQL Server + SSMS instalados e funcionando.
