# ⚙️ CONFIG: Adiciona Swagger UI, Scalar e ajustes gerais

## 📦 Passo 1 — Instalar pacotes NuGet

No terminal, dentro da pasta `app`:

```bash
dotnet add package Swashbuckle.AspNetCore
```

```bash
dotnet add package Scalar.AspNetCore
```

### O que cada pacote faz

| Pacote | Função |
|--------|--------|
| `Swashbuckle.AspNetCore` | Swagger UI — visualizador interativo clássico da API |
| `Scalar.AspNetCore` | Scalar — visualizador moderno e bonito da API |

### Como funciona

O .NET 10 já gera o documento OpenAPI nativamente (`AddOpenApi()` / `MapOpenApi()`). O Swagger UI e o Scalar são apenas **visualizadores** que leem esse documento e mostram uma interface interativa no navegador onde é possível testar os endpoints (GET, POST, PUT, DELETE).

---

## 🗂️ Passo 2 — Criar pasta Configurations

Criar a pasta `Configurations` dentro de `app`.

A ideia é isolar as configurações em arquivos separados usando **extension methods**, mantendo o `Program.cs` limpo.

### SwaggerConfig

Arquivo: `Configurations/SwaggerConfig.cs`

```csharp
namespace Api_ArjSys_Tcc.Configurations;

public static class SwaggerConfig
{
    public static WebApplication UseSwaggerConfig(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "ARJSYS API v1");
                options.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}
```

### ScalarConfig

Arquivo: `Configurations/ScalarConfig.cs`

```csharp
using Scalar.AspNetCore;

namespace Api_ArjSys_Tcc.Configurations;

public static class ScalarConfig
{
    public static WebApplication UseScalarConfig(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("ARJSYS API");
                options.WithTheme(ScalarTheme.BluePlanet);
            });
        }

        return app;
    }
}
```

### O que são extension methods

São métodos estáticos que "estendem" uma classe existente. No caso, estamos adicionando os métodos `UseSwaggerConfig()` e `UseScalarConfig()` ao `WebApplication`. Assim, no `Program.cs` basta chamar uma linha para cada configuração.

---

## 🔧 Passo 3 — Atualizar o Program.cs

Arquivo: `Program.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Configurations;

var builder = WebApplication.CreateBuilder(args);

// ===== Serviços =====

// Controllers — habilita o uso de controllers na API
// JsonStringEnumConverter — permite enviar/receber enums como texto ("PC", "KG") em vez de números (0, 1)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// OpenAPI — gera o documento JSON que descreve todos os endpoints da API
builder.Services.AddOpenApi();

// Entity Framework + SQLite — ORM para acesso ao banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS — permite o frontend (React/Vite) fazer requisições para esta API
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

// ===== Pipeline de Middleware =====

// OpenAPI — expõe o documento JSON em /openapi/v1.json (apenas em Development)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Swagger UI — visualizador interativo da API em /swagger
app.UseSwaggerConfig();

// Scalar — visualizador moderno da API em /scalar/v1
app.UseScalarConfig();

// HTTPS — redireciona requisições HTTP para HTTPS
app.UseHttpsRedirection();

// CORS — aplica a política de acesso do frontend
app.UseCors("AllowFrontend");

// Authorization — habilita autenticação/autorização (preparado para JWT futuro)
app.UseAuthorization();

// Controllers — mapeia as rotas dos controllers
app.MapControllers();

// Inicia a aplicação
app.Run();
```

### O que mudou em relação ao anterior

| Alteração | Motivo |
|-----------|--------|
| `using Api_ArjSys_Tcc.Configurations` | Importa os extension methods de configuração |
| `AddJsonOptions` com `JsonStringEnumConverter` | Enums são enviados/recebidos como texto ("PC", "Comprado") em vez de números (0, 1) |
| `app.UseSwaggerConfig()` | Chama a configuração do Swagger isolada |
| `app.UseScalarConfig()` | Chama a configuração do Scalar isolada |

---

## 📦 Passo 4 — Ajustar Model Produto

Removidos os campos `CodigoBarras` e `Observacao` que não serão utilizados.

Arquivo: `Models/Engenharia/Produto.cs`

```csharp
using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.Models.Engenharia;

public class Produto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DescricaoCompleta { get; set; }
    public UnidadeMedida Unidade { get; set; } = UnidadeMedida.UN;
    public TipoProduto Tipo { get; set; } = TipoProduto.Fabricado;
    public decimal? Peso { get; set; }
    public bool Ativo { get; set; } = true;
}
```

Também remover as referências no método `Update` do `ProdutosController.cs` (linhas com `CodigoBarras` e `Observacao`).

---

## 💾 Passo 5 — Mover banco para pasta Database

### Alterar connection string

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
    "DefaultConnection": "Data Source=Database/ArjSysDB.db"
  }
}
```

### Criar a pasta

Criar a pasta `Database` dentro de `app`. O EF Core cria o arquivo `.db` automaticamente, mas não cria a pasta.

### Recriar migration e banco do zero

Apagar a pasta `Migrations` e o arquivo `.db` antigo (se existir), depois:

```bash
dotnet ef migrations add CriarTabelaProdutos
```

```bash
dotnet ef database update
```

---

## 🌐 Passo 6 — Ajustar launchSettings.json

Arquivo: `Properties/launchSettings.json`

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:7000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:7000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

O `launchUrl` foi alterado para `swagger` — ao rodar a aplicação, o navegador abre direto na interface do Swagger UI.

Para usar o Scalar, basta trocar manualmente para `scalar/v1` na barra de endereço.

---

## ✅ Teste — Verificar funcionamento

Rodar com **Ctrl+F5**.

### URLs disponíveis

| Interface | URL |
|-----------|-----|
| Swagger UI | `https://localhost:7001/swagger` |
| Scalar | `https://localhost:7001/scalar/v1` |
| OpenAPI JSON | `https://localhost:7001/openapi/v1.json` |
| Status | `https://localhost:7001/api/status` |
| Produtos | `https://localhost:7001/api/engenharia/produtos` |

### Testar criação de produto via Swagger

No Swagger UI, clicar em **POST /api/engenharia/produtos** → **Try it out** → colar no body:

```json
{
  "codigo": "PRD-001",
  "descricao": "Parafuso Sextavado M10x50",
  "unidade": "PC",
  "tipo": "Comprado",
  "peso": 0.035,
  "ativo": true
}
```

Clicar em **Execute**. Resposta esperada: HTTP 201 com o produto criado incluindo `id` e `criadoEm` preenchidos automaticamente.

---

## 📂 Estrutura após este commit

```
pos_dev_web--tcc_arjsys--back/
├── app/
│   ├── Configurations/
│   │   ├── SwaggerConfig.cs                ← novo
│   │   └── ScalarConfig.cs                 ← novo
│   ├── Controllers/
│   │   ├── Engenharia/
│   │   │   └── ProdutosController.cs       ← alterado
│   │   └── StatusController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Database/
│   │   └── ArjSysDB.db                     ← novo (movido)
│   ├── DTOs/
│   ├── Migrations/
│   │   └── (recriados do zero)
│   ├── Models/
│   │   ├── BaseEntity.cs
│   │   └── Engenharia/
│   │       ├── Enums/
│   │       │   ├── TipoProduto.cs
│   │       │   └── UnidadeMedida.cs
│   │       └── Produto.cs                  ← alterado
│   ├── Properties/
│   │   └── launchSettings.json             ← alterado
│   ├── appsettings.json                    ← alterado
│   ├── appsettings.Development.json
│   ├── app.csproj
│   ├── Api_ArjSys_Tcc.http
│   └── Program.cs                          ← alterado
├── docs/
│   ├── 01-CONFIG-Inicializa-Projeto.md
│   ├── 02-CONFIG-DbContext-CORS-Status.md
│   ├── 03-FEATURE-Models-CRUD-Produtos.md
│   └── 04-CONFIG-Swagger-Scalar-Ajustes.md ← este documento
├── README.md
└── .gitignore
```
