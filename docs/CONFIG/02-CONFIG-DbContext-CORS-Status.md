<!-- markdownlint-disable-file -->
# ⚙️ CONFIG: Configura DbContext, SQLite, CORS e StatusController

## 🗄️ Passo 1 — Criar o AppDbContext

Arquivo: `Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace Api_ArjSys_Tcc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

### O que é o DbContext

O `AppDbContext` é a classe principal do Entity Framework Core. Ele representa a sessão com o banco de dados e é responsável por:

- Mapear classes C# (Models) para tabelas do banco
- Gerenciar conexões e transações
- Executar queries e salvar dados

Os `DbSet<>` serão adicionados conforme os Models forem criados nos próximos passos.

---

## 📝 Passo 2 — Configurar o appsettings.json

Arquivo: `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=arjsys.db"
  }
}
```

### O que foi alterado

- Adicionado `Microsoft.EntityFrameworkCore: Information` no logging — permite ver as queries SQL geradas pelo EF Core no console durante desenvolvimento
- Adicionado `ConnectionStrings.DefaultConnection` — define o arquivo SQLite `arjsys.db` que será criado na raiz do projeto app ao rodar as migrations

---

## 🔧 Passo 3 — Configurar o Program.cs

Arquivo: `Program.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Entity Framework + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS - liberar acesso do frontend (Vite porta 5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### O que foi adicionado

| Trecho | Função |
|--------|--------|
| `using Api_ArjSys_Tcc.Data` | Importa o namespace do DbContext |
| `AddDbContext<AppDbContext>` | Registra o EF Core com SQLite no container de injeção de dependência |
| `GetConnectionString("DefaultConnection")` | Lê a connection string do appsettings.json |
| `AddCors` / `AllowFrontend` | Permite o frontend React (porta 5173) fazer requisições à API |
| `UseCors("AllowFrontend")` | Ativa o CORS no pipeline de middleware |

### O que é CORS

CORS (Cross-Origin Resource Sharing) permite que o frontend e backend se comuniquem mesmo estando em portas diferentes. Sem ele, o navegador bloqueia as requisições por segurança. A política `AllowFrontend` libera a porta padrão do Vite (5173). Ao integrar com o frontend, ajustar a porta se necessário.

---

## 🏥 Passo 4 — Criar o StatusController

Arquivo: `Controllers/StatusController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Api_ArjSys_Tcc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "OK",
            Aplicacao = "ARJSYS API",
            Versao = "1.0.0",
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### O que é esse controller

Um endpoint de **health check** — serve para verificar rapidamente se a API está rodando. Retorna um JSON simples com status, nome e versão.

### Anatomia do controller

| Elemento | Função |
|----------|--------|
| `[ApiController]` | Habilita comportamentos automáticos da API (validação, binding) |
| `[Route("api/[controller]")]` | Define a rota como `/api/status` (pega o nome da classe sem "Controller") |
| `ControllerBase` | Classe base para API (sem suporte a views, diferente de `Controller`) |
| `[HttpGet]` | Responde a requisições GET |
| `Ok(...)` | Retorna HTTP 200 com o objeto serializado em JSON |

---

## 🌐 Passo 5 — Ajustar portas no launchSettings.json

Arquivo: `Properties/launchSettings.json`

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "api/status",
      "applicationUrl": "http://localhost:7000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "api/status",
      "applicationUrl": "https://localhost:7001;http://localhost:7000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### O que foi alterado

| Antes | Depois | Motivo |
|-------|--------|--------|
| Portas aleatórias (7205/5196) | Portas redondas (7000/7001) | Mais fácil de lembrar |
| `launchBrowser: false` | `launchBrowser: true` | Abre navegador automaticamente ao rodar |
| Sem `launchUrl` | `launchUrl: "api/status"` | Já abre direto no endpoint de status |

### Portas da API

| Protocolo | URL |
|-----------|-----|
| HTTP | `http://localhost:7000` |
| HTTPS | `https://localhost:7001` |

---

## ✅ Teste — Verificar funcionamento

Rodar com **Ctrl+F5** no Visual Studio e acessar:

```
https://localhost:7001/api/status
```

Resposta esperada:

```json
{
  "status": "OK",
  "aplicacao": "ARJSYS API",
  "versao": "1.0.0",
  "timestamp": "2026-02-16T..."
}
```

---

## 📂 Estrutura após este commit

```
pos_dev_web--tcc_arjsys--back/
├── app/
│   ├── Controllers/
│   │   └── StatusController.cs         ← novo
│   ├── Data/
│   │   └── AppDbContext.cs             ← novo
│   ├── DTOs/
│   ├── Models/
│   ├── Properties/
│   │   └── launchSettings.json         ← alterado
│   ├── appsettings.json                ← alterado
│   ├── appsettings.Development.json
│   ├── app.csproj
│   ├── Api_ArjSys_Tcc.http
│   └── Program.cs                      ← alterado
├── docs/
│   ├── 01-CONFIG-Inicializa-Projeto.md
│   └── 02-CONFIG-DbContext-CORS-Status.md  ← este documento
├── README.md
└── .gitignore
```

---
