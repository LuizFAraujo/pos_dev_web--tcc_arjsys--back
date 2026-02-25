<!-- markdownlint-disable-file -->
# ♻️ REFACTOR: Cria ProdutoService e separa lógica do controller

## 💡 Por que separar em Services

O controller deve ser responsável apenas por receber requisições HTTP e devolver respostas. A lógica de negócio (validações, cálculos, regras) fica no service.

Benefícios:

- **Responsabilidade única** — cada classe faz uma coisa
- **Regras futuras** — ex: antes de deletar um produto, verificar se está em alguma BOM. Essa regra fica no service
- **Padronização** — todos os módulos seguem o mesmo fluxo: Controller → Service → Banco
- **Testabilidade** — services podem ser testados isoladamente sem HTTP

---

## 📁 Passo 1 — Criar estrutura de pastas

Criar dentro de `app`:

```
Services/
└── Engenharia/
```

---

## 🔧 Passo 2 — Criar o ProdutoService

Arquivo: `Services/Engenharia/ProdutoService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class ProdutoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<Produto>> GetAll()
    {
        return await _context.Produtos.ToListAsync();
    }

    public async Task<Produto?> GetById(int id)
    {
        return await _context.Produtos.FindAsync(id);
    }

    public async Task<Produto> Create(Produto produto)
    {
        produto.CriadoEm = DateTime.UtcNow;
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task<bool> Update(int id, Produto produto)
    {
        var existente = await _context.Produtos.FindAsync(id);

        if (existente == null)
            return false;

        existente.Codigo = produto.Codigo;
        existente.Descricao = produto.Descricao;
        existente.DescricaoCompleta = produto.DescricaoCompleta;
        existente.Unidade = produto.Unidade;
        existente.Tipo = produto.Tipo;
        existente.Peso = produto.Peso;
        existente.Ativo = produto.Ativo;
        existente.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return false;

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

### Anatomia do Service

| Elemento | Função |
|----------|--------|
| `AppDbContext _context` | Acesso ao banco via injeção de dependência |
| `GetAll` / `GetById` | Leitura de dados |
| `Create` | Cria produto e preenche `CriadoEm` automaticamente |
| `Update` | Atualiza campos e preenche `ModificadoEm` automaticamente |
| `Delete` | Remove produto do banco |
| Retorno `bool` | O service retorna `true/false` indicando sucesso; o controller decide qual HTTP status devolver |

---

## 🎮 Passo 3 — Refatorar o ProdutosController

O controller fica magro — apenas recebe, chama o service e devolve.

Arquivo: `Controllers/Engenharia/ProdutosController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
public class ProdutosController(ProdutoService service) : ControllerBase
{
    private readonly ProdutoService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<Produto>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetById(int id)
    {
        var produto = await _service.GetById(id);

        if (produto == null)
            return NotFound();

        return produto;
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> Create(Produto produto)
    {
        var criado = await _service.Create(produto);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Produto produto)
    {
        if (id != produto.Id)
            return BadRequest();

        var atualizado = await _service.Update(id, produto);

        if (!atualizado)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _service.Delete(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }
}
```

### Antes vs Depois

| Antes | Depois |
|-------|--------|
| Controller acessava `AppDbContext` diretamente | Controller chama `ProdutoService` |
| Lógica de negócio misturada com HTTP | Lógica isolada no service |
| Construtor recebia `AppDbContext` | Construtor recebe `ProdutoService` |

---

## 📝 Passo 4 — Registrar o Service no Program.cs

Adicionar no `Program.cs`, logo após o bloco do CORS:

```csharp
// Services — registro dos serviços de negócio (injeção de dependência)
builder.Services.AddScoped<ProdutoService>();
```

E adicionar o using no topo:

```csharp
using Api_ArjSys_Tcc.Services.Engenharia;
```

### O que é AddScoped

O `AddScoped` cria uma instância do service **por requisição HTTP**. Cada request recebe sua própria instância, que é descartada ao final. Outras opções seriam `AddTransient` (nova instância a cada injeção) e `AddSingleton` (uma instância pra toda a aplicação). Para services que acessam banco, `AddScoped` é o padrão.

---

## 🔤 Primary Constructor (C# 14)

O Visual Studio sugere usar primary constructor (`IDE0290`). É a sintaxe moderna do C# 14, mais enxuta.

### Antes (construtor tradicional)

```csharp
public class ProdutoService
{
    private readonly AppDbContext _context;

    public ProdutoService(AppDbContext context)
    {
        _context = context;
    }
}
```

### Depois (primary constructor)

```csharp
public class ProdutoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;
}
```

Mesma funcionalidade, menos código. Aplicado tanto no service quanto no controller.

---

## ✅ Teste — Verificar funcionamento

Rodar com **Ctrl+F5** e testar no Swagger:

- **GET** `/api/engenharia/produtos` — deve retornar os produtos cadastrados
- **POST** `/api/engenharia/produtos` — deve criar produto normalmente
- **PUT** `/api/engenharia/produtos/{id}` — deve atualizar
- **DELETE** `/api/engenharia/produtos/{id}` — deve remover

O comportamento externo é idêntico ao anterior — a mudança é apenas interna (organização do código).

---

## 📂 Estrutura após este commit

```
pos_dev_web--tcc_arjsys--back/
├── app/
│   ├── Configurations/
│   │   ├── SwaggerConfig.cs
│   │   └── ScalarConfig.cs
│   ├── Controllers/
│   │   ├── Engenharia/
│   │   │   └── ProdutosController.cs       ← refatorado
│   │   └── StatusController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Database/
│   │   └── ArjSysDB.db
│   ├── DTOs/
│   ├── Migrations/
│   │   └── (...)
│   ├── Models/
│   │   ├── BaseEntity.cs
│   │   └── Engenharia/
│   │       ├── Enums/
│   │       │   ├── TipoProduto.cs
│   │       │   └── UnidadeMedida.cs
│   │       └── Produto.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Services/
│   │   └── Engenharia/
│   │       └── ProdutoService.cs           ← novo
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── app.csproj
│   ├── Api_ArjSys_Tcc.http
│   └── Program.cs                          ← alterado
├── docs/
│   ├── 01-CONFIG-Inicializa-Projeto.md
│   ├── 02-CONFIG-DbContext-CORS-Status.md
│   ├── 03-FEATURE-Models-CRUD-Produtos.md
│   ├── 04-CONFIG-Swagger-Scalar-Ajustes.md
│   └── 05-REFACTOR-ProdutoService.md       ← este documento
├── README.md
└── .gitignore
```
