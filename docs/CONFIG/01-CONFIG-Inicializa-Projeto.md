<!-- markdownlint-disable-file -->
# ⚙️ CONFIG: Inicializa projeto ASP.NET Core 10 Web API

## 🛠️ Pré-requisitos

### Software necessário

- **Visual Studio 2026** (v18.0+) com o workload **ASP.NET and web development**
- **.NET 10 SDK** (LTS — suporte até Nov/2028)

### Verificar instalação

```bash
dotnet --version
```

Deve retornar `10.0.xxx`.

Se não tiver instalado, baixar em: https://dotnet.microsoft.com/en-us/download/dotnet/10.0

---

## 📁 Passo 1 — Criar estrutura base do repositório

```bash
mkdir pos_dev_web--tcc_arjsys--back
cd pos_dev_web--tcc_arjsys--back
mkdir docs
```

Criar os arquivos `README.md` e `.gitignore` na raiz.

### README.md

```markdown
# TCC Back - Sistema ARJSYS

Projeto backend (API) desenvolvido como Trabalho de Conclusão de Curso em Pós-graduação em Desenvolvimento Web.

## Estrutura do Projeto

- **app/** - API Web principal (ASP.NET Core 10)
- **docs/** - Documentação do projeto

## Tecnologias Principais

- .NET 10 (LTS)
- C# 14
- ASP.NET Core 10 Web API
- Entity Framework Core 10
- SQLite
```

### .gitignore

Utilizado o mesmo `.gitignore` do frontend, mesclado com regras do .NET:

```gitignore
# ========================================
# .gitignore - ARJSYS (Frontend + Backend)
# ========================================

# ---------- Logs ----------
logs/
*.log
npm-debug.log*
yarn-debug.log*
yarn-error.log*
pnpm-debug.log*
lerna-debug.log*

# ---------- Dependencies ----------
node_modules/

# ---------- .NET Build ----------
[Dd]ebug/
[Rr]elease/
x64/
x86/
bld/
[Bb]in/
[Oo]bj/
[Oo]ut/
[Ll]og/
[Ll]ogs/
artifacts/

# ---------- Frontend Build ----------
dist/
dist-ssr/
_dist/
build/
*.local

# ---------- Visual Studio ----------
.vs/
*.suo
*.user
*.userosscache
*.sln.docstates
*.rsuser
Generated\ Files/
*.Cache
!?*.[Cc]ache/

# ---------- Visual Studio Profiler / Trace ----------
*.psess
*.vsp
*.vspx
*.sap
*.e2e

# ---------- Test Results ----------
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.trx
*.VisualState.xml
TestResult.xml
nunit-*.xml
*.received.*
coverage*.json
coverage*.xml
coverage*.info
*.coverage
*.coveragexml

# ---------- NuGet ----------
**/[Pp]ackages/*
!**/[Pp]ackages/build/
*.nupkg
*.snupkg
.nuget/
project.lock.json
project.fragment.lock.json
*.nuget.props
*.nuget.targets

# ---------- .NET Misc ----------
*.ilk
*.pdb
*.ipdb
*.iobj
*.pgc
*.pgd
*.tmp
*.tmp_proj
*_wpftmp.csproj
*.tlog
*.binlog
MSBuild_Logs/
ScaffoldingReadMe.txt

# ---------- IDE / Editor ----------
.idea/
.vscode/*
!.vscode/settings.json
!.vscode/extensions.json
!.vscode/launch.json
!.vscode/tasks.json
*.ntvs*
*.njsproj
*.sw?
.history/
*.vsix

# ---------- ReSharper / JetBrains ----------
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user

# ---------- Environment ----------
*.env
.env
.env.local
.env.*.local

# ---------- Database ----------
*.db
*.db-shm
*.db-wal
*.mdf
*.ldf
*.ndf

# ---------- OS ----------
.DS_Store
Thumbs.db
~$*
*~

# ---------- pnpm ----------
.pnpm-approve-builds.json
```

---

## 🌐 Passo 2 — Criar o projeto Web API

### Via Visual Studio 2026

1. Abrir Visual Studio 2026
2. **Create a new project**
3. Buscar **ASP.NET Core Web API**
4. **Next**
5. Project name: `app`
6. Location: navegar até `pos_dev_web--tcc_arjsys--back`
7. **Next**
8. Configurações:
   - Estrutura: **.NET 10.0 (Suporte de Longo Prazo)**
   - Tipo de autenticação: **Nenhum**
   - ✅ Configurar para HTTPS
   - ☐ Habilitar o suporte a contêineres
   - ✅ Habilitar o suporte a OpenAPI
   - ☐ Não use instruções de nível superior
   - ✅ Usar controles
   - ☐ Inscrever-se na orquestração do Aspire
9. **Criar**

### Via Terminal (alternativa)

```bash
cd pos_dev_web--tcc_arjsys--back
dotnet new webapi --use-controllers -o app
```

### Resultado gerado

```
app/
├── Controllers/
│   └── WeatherForecastController.cs    ← será removido
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── app.csproj
├── app.http
├── Program.cs
└── WeatherForecast.cs                  ← será removido
```

---

## 📦 Passo 3 — Instalar pacotes NuGet

### Via Terminal

Navegar até a pasta `app`:

```bash
cd app
```

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Via Visual Studio

**Tools → NuGet Package Manager → Manage NuGet Packages for Solution**

Buscar e instalar:
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

### O que cada pacote faz

| Pacote | Função |
|--------|--------|
| `EntityFrameworkCore` | ORM principal — mapeia classes C# para tabelas do banco |
| `EntityFrameworkCore.Sqlite` | Provider para banco SQLite |
| `EntityFrameworkCore.Design` | Necessário para gerar migrations via CLI |
| `EntityFrameworkCore.Tools` | Comandos `dotnet ef` no terminal |

---

## 📁 Passo 4 — Criar estrutura de pastas

Dentro de `app/`, criar as pastas:

### Via Solution Explorer (Visual Studio)

Clique direito no projeto `app` → **Add** → **New Folder**

Criar:
- `Data` — DbContext e configurações do banco
- `Models` — Entidades / Models do banco
- `DTOs` — Data Transfer Objects (entrada e saída da API)

### Via Terminal

```bash
mkdir Data Models DTOs
```

---

## 🧹 Passo 5 — Remover arquivos de exemplo

Apagar os arquivos gerados pelo template que não serão utilizados:

- `WeatherForecast.cs` (raiz do projeto app)
- `Controllers/WeatherForecastController.cs`

### Via Solution Explorer

Clique direito em cada arquivo → **Delete**

### Via Terminal

```bash
rm WeatherForecast.cs
rm Controllers/WeatherForecastController.cs
```

---

## ✅ Resultado final — Program.cs (inalterada)

O `Program.cs` gerado pelo template já está limpo e não precisa de alteração neste momento:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 📂 Estrutura final após este commit

```
pos_dev_web--tcc_arjsys--back/
├── app/
│   ├── Controllers/            ← vazia (exemplo removido)
│   ├── Data/                   ← nova
│   ├── DTOs/                   ← nova
│   ├── Models/                 ← nova
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── app.csproj
│   ├── app.http
│   └── Program.cs
├── docs/
│   └── 01-CONFIG-Inicializa-Projeto.md  ← este documento
├── README.md
└── .gitignore
```

---

## 🔗 Tecnologias utilizadas

| Tecnologia | Versão | Tipo |
|------------|--------|------|
| .NET | 10.0 (LTS) | Runtime |
| C# | 14 | Linguagem |
| ASP.NET Core | 10.0 | Framework Web |
| Entity Framework Core | 10.0 | ORM |
| SQLite | — | Banco de dados |
| Visual Studio | 2026 (v18.x) | IDE |

---
