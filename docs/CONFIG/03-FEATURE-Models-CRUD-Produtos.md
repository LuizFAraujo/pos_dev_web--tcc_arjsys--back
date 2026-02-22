# ✨ FEATURE: Implementa Models base e CRUD de Produtos

## 🏗️ Passo 1 — Criar a BaseEntity

Classe abstrata que serve de base para todas as entidades do sistema. Toda tabela terá esses campos automaticamente.

Arquivo: `Models/BaseEntity.cs`

```csharp
namespace Api_ArjSys_Tcc.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ModificadoEm { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
```

### Sobre os campos

| Campo | Tipo | Observação |
|-------|------|------------|
| `Id` | `int` | Chave primária, auto-incremento |
| `CriadoEm` | `DateTime` | Preenchido automaticamente com UTC |
| `ModificadoEm` | `DateTime?` | Nullable — preenchido apenas na atualização |
| `CriadoPor` | `string?` | Nullable — será preenchido quando houver autenticação |
| `ModificadoPor` | `string?` | Nullable — idem |

### Por que `abstract`

A classe não pode ser instanciada diretamente. Ela existe apenas para ser herdada por outras entidades (Produto, BOM, etc.).

---

## 📏 Passo 2 — Criar os Enums

Criar a estrutura de pastas: `Models/Engenharia/Enums/`

### UnidadeMedida

Arquivo: `Models/Engenharia/Enums/UnidadeMedida.cs`

```csharp
namespace Api_ArjSys_Tcc.Models.Engenharia.Enums;

public enum UnidadeMedida
{
    UN,     // Unidade
    PC,     // Peça
    CJ,     // Conjunto
    KG,     // Quilograma
    KT,     // Kit
    MT,     // Metro
    M2,     // Metro Quadrado
    M3,     // Metro Cúbico
    LT,     // Litro
    ML,     // Mililitro
    MM,     // Milímetro
    CM,     // Centímetro
    TN      // Tonelada
}
```

### TipoProduto

Arquivo: `Models/Engenharia/Enums/TipoProduto.cs`

```csharp
namespace Api_ArjSys_Tcc.Models.Engenharia.Enums;

public enum TipoProduto
{
    Fabricado,
    Comprado,
    MateriaPrima,
    Revenda,
    Servico
}
```

### Por que enum e não tabela no banco

Para listas pequenas e estáveis (unidades e tipos não mudam com frequência), enum é mais simples — não precisa de CRUD extra, migration, nem tela de cadastro. Se futuramente precisar de mais flexibilidade, a troca para tabela é simples.

### Padrão de siglas

Todas as siglas de UnidadeMedida seguem 2 caracteres, com exceção de M2 e M3 que já são naturalmente 2 caracteres (alfanuméricos).

---

## 📦 Passo 3 — Criar o Model Produto

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
    public string? CodigoBarras { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; } = true;
}
```

### Campos do Produto

| Campo | Tipo | Obrigatório | Observação |
|-------|------|:-----------:|------------|
| `Codigo` | `string` | ✅ | Código único do produto (máx 50 caracteres) |
| `Descricao` | `string` | ✅ | Descrição curta (máx 140 caracteres) |
| `DescricaoCompleta` | `string?` | ☐ | Descrição longa sem limite |
| `Unidade` | `UnidadeMedida` | ✅ | Padrão: UN |
| `Tipo` | `TipoProduto` | ✅ | Padrão: Fabricado |
| `Peso` | `decimal?` | ☐ | Em quilogramas |
| `CodigoBarras` | `string?` | ☐ | EAN/UPC |
| `Observacao` | `string?` | ☐ | Texto livre |
| `Ativo` | `bool` | ✅ | Padrão: true |

Além desses, herda de BaseEntity: `Id`, `CriadoEm`, `ModificadoEm`, `CriadoPor`, `ModificadoPor`.

---

## 🗄️ Passo 4 — Registrar no AppDbContext

Arquivo: `Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Codigo).HasMaxLength(50);
            entity.Property(p => p.Descricao).HasMaxLength(140);
            entity.Property(p => p.Unidade).HasConversion<string>().HasMaxLength(2);
            entity.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        });
    }
}
```

### Configurações aplicadas

| Configuração | O que faz |
|-------------|-----------|
| `HasIndex(...).IsUnique()` | Código do produto não pode repetir no banco |
| `HasMaxLength(50)` | Limita Codigo a 50 caracteres |
| `HasMaxLength(140)` | Limita Descricao a 140 caracteres |
| `HasConversion<string>()` | Salva enums como texto no banco ("KG", "UN") em vez de números (0, 1, 2) |

---

## 🗃️ Passo 5 — Gerar Migration e Banco

### Instalar ferramenta EF (se necessário)

```bash
dotnet tool install --global dotnet-ef
```

### Gerar migration

```bash
dotnet ef migrations add CriarTabelaProdutos
```

### Aplicar no banco

```bash
dotnet ef database update
```

Isso cria o arquivo `ArjSys.db` na raiz do projeto app.

### Connection string utilizada

No `appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Data Source=ArjSys.db"
}
```

### Comandos úteis de referência

| Comando | Função |
|---------|--------|
| `dotnet ef migrations add Nome` | Cria nova migration |
| `dotnet ef database update` | Aplica migrations no banco |
| `dotnet ef migrations remove` | Remove última migration |
| `dotnet ef database drop` | Apaga o banco |

---

## 🎮 Passo 6 — Criar o ProdutosController

Criar a pasta `Engenharia` dentro de `Controllers`.

Arquivo: `Controllers/Engenharia/ProdutosController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
    {
        return await _context.Produtos.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetById(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return NotFound();

        return produto;
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> Create(Produto produto)
    {
        produto.CriadoEm = DateTime.UtcNow;
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Produto produto)
    {
        if (id != produto.Id)
            return BadRequest();

        var existente = await _context.Produtos.FindAsync(id);

        if (existente == null)
            return NotFound();

        existente.Codigo = produto.Codigo;
        existente.Descricao = produto.Descricao;
        existente.DescricaoCompleta = produto.DescricaoCompleta;
        existente.Unidade = produto.Unidade;
        existente.Tipo = produto.Tipo;
        existente.Peso = produto.Peso;
        existente.CodigoBarras = produto.CodigoBarras;
        existente.Observacao = produto.Observacao;
        existente.Ativo = produto.Ativo;
        existente.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return NotFound();

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

### Endpoints disponíveis

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/engenharia/produtos` | Lista todos os produtos |
| GET | `/api/engenharia/produtos/{id}` | Busca produto por ID |
| POST | `/api/engenharia/produtos` | Cria novo produto |
| PUT | `/api/engenharia/produtos/{id}` | Atualiza produto existente |
| DELETE | `/api/engenharia/produtos/{id}` | Remove produto |

### Anatomia do Controller

| Elemento | Função |
|----------|--------|
| `[Route("api/engenharia/[controller]")]` | Rota base inclui o módulo Engenharia |
| `AppDbContext _context` | Injetado automaticamente pelo ASP.NET (DI) |
| `async/await` | Operações assíncronas para não travar a API |
| `CreatedAtAction` | Retorna 201 com location header apontando para o recurso criado |
| `NoContent()` | Retorna 204 — operação bem-sucedida sem corpo de resposta |
| `NotFound()` | Retorna 404 quando o produto não existe |

---

## ✅ Teste — Verificar funcionamento

Rodar com **Ctrl+F5** e acessar:

```
https://localhost:7001/api/engenharia/produtos
```

Resposta esperada: `[]` (lista vazia — nenhum produto cadastrado ainda).

---

## 📂 Estrutura após este commit

```
pos_dev_web--tcc_arjsys--back/
├── app/
│   ├── Controllers/
│   │   ├── Engenharia/
│   │   │   └── ProdutosController.cs       ← novo
│   │   └── StatusController.cs
│   ├── Data/
│   │   └── AppDbContext.cs                  ← alterado
│   ├── DTOs/
│   ├── Migrations/
│   │   └── (arquivos gerados pelo EF)       ← novo
│   ├── Models/
│   │   ├── BaseEntity.cs                    ← novo
│   │   └── Engenharia/
│   │       ├── Enums/
│   │       │   ├── TipoProduto.cs           ← novo
│   │       │   └── UnidadeMedida.cs         ← novo
│   │       └── Produto.cs                   ← novo
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── app.csproj
│   ├── ArjSys.db                            ← novo (banco SQLite)
│   ├── Api_ArjSys_Tcc.http
│   └── Program.cs
├── docs/
│   ├── 01-CONFIG-Inicializa-Projeto.md
│   ├── 02-CONFIG-DbContext-CORS-Status.md
│   └── 03-FEATURE-Models-CRUD-Produtos.md   ← este documento
├── README.md
└── .gitignore
```

---
