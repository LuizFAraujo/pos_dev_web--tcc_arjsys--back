<!-- markdownlint-disable-file -->
# ✨ FEATURE: Implementa BOM (Estrutura de Produtos) com DTOs

## 💡 Conceitos aplicados

### DTOs (Data Transfer Objects)

Objetos separados para entrada e saída da API. O controller nunca recebe/retorna o Model do banco diretamente. Isso evita expor propriedades de navegação do EF Core e dá controle total sobre o que entra e sai.

- **CreateDTO** — campos que o frontend envia (POST/PUT)
- **ResponseDTO** — campos que a API retorna (GET)
- **FlatDTO** — formato especial para exibição flat da BOM (pai e filho lado a lado)

### Validação de ciclo recursiva

Antes de inserir um item na estrutura, o BomService percorre a árvore inteira verificando se a inclusão criaria referência circular. Ex: se A contém B, e B contém C, não permite C conter A.

### Cálculo automático de posição

Se a posição não for informada (ou <= 0), o service pega a maior posição existente na estrutura do pai e arredonda para o próximo múltiplo de 10. Ex: se a última posição é 1002, a próxima será 1010.

### Paginação opcional

Os endpoints de listagem aceitam parâmetros `pagina` e `tamanho`. Se não forem passados, retornam todos os registros. O retorno inclui `total`, `pagina`, `tamanho` e `totalPaginas`.

### IEntityTypeConfiguration

As configurações do EF Core (índices, constraints, conversões) foram extraídas do AppDbContext para arquivos separados por entidade. O AppDbContext carrega todos automaticamente com `ApplyConfigurationsFromAssembly`.

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `Models/Engenharia/EstruturaProduto.cs` | Entidade da estrutura de produtos (pai, filho, quantidade, posição) |
| `DTOs/Engenharia/ProdutoDTO.cs` | ProdutoCreateDTO e ProdutoResponseDTO |
| `DTOs/Engenharia/EstruturaProdutoDTO.cs` | EstruturaProdutoCreateDTO, EstruturaProdutoResponseDTO e EstruturaProdutoFlatDTO |
| `Data/Configurations/Engenharia/ProdutoConfiguration.cs` | Config do EF Core para Produto |
| `Data/Configurations/Engenharia/EstruturaProdutoConfiguration.cs` | Config do EF Core para EstruturaProduto (índice único pai+filho, FKs, DeleteBehavior.Restrict) |
| `Services/Engenharia/BomService.cs` | Lógica de negócio da BOM (CRUD, ciclo, posição, flat) |
| `Controllers/Engenharia/BomController.cs` | Endpoints REST da BOM |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Data/AppDbContext.cs` | Adicionado DbSet de EstruturaProdutos, trocado OnModelCreating por ApplyConfigurationsFromAssembly |
| `Services/Engenharia/ProdutoService.cs` | Refatorado para usar DTOs (CreateDTO/ResponseDTO) |
| `Controllers/Engenharia/ProdutosController.cs` | Refatorado para usar DTOs |
| `Program.cs` | Adicionado registro do BomService no AddScoped |

---

## 🌐 Endpoints da BOM

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/engenharia/bom` | Lista produtos que possuem estrutura (paginável) |
| GET | `/api/engenharia/bom/flat` | Lista todas as estruturas com pai e filho lado a lado (paginável) |
| GET | `/api/engenharia/bom/produto/{produtoPaiId}` | Lista filhos diretos de um produto (flat de um pai específico) |
| GET | `/api/engenharia/bom/{id}` | Busca item da estrutura por ID |
| POST | `/api/engenharia/bom` | Cria item na estrutura (com validação de ciclo e posição automática) |
| PUT | `/api/engenharia/bom/{id}` | Atualiza item da estrutura |
| DELETE | `/api/engenharia/bom/{id}` | Remove item da estrutura |

### Paginação (endpoints GET com listagem)

| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|--------|-----------|
| `pagina` | int | 1 | Número da página |
| `tamanho` | int | 0 | Itens por página (0 = todos) |

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
│   │   │   ├── ProdutosController.cs
│   │   │   └── BomController.cs            ← novo
│   │   └── StatusController.cs
│   ├── Data/
│   │   ├── Configurations/
│   │   │   └── Engenharia/
│   │   │       ├── ProdutoConfiguration.cs         ← novo
│   │   │       └── EstruturaProdutoConfiguration.cs ← novo
│   │   └── AppDbContext.cs                  ← alterado
│   ├── Database/
│   │   └── ArjSysDB.db
│   ├── DTOs/
│   │   └── Engenharia/
│   │       ├── ProdutoDTO.cs               ← novo
│   │       └── EstruturaProdutoDTO.cs      ← novo
│   ├── Migrations/
│   │   └── (...)
│   ├── Models/
│   │   ├── BaseEntity.cs
│   │   └── Engenharia/
│   │       ├── Enums/
│   │       │   ├── TipoProduto.cs
│   │       │   └── UnidadeMedida.cs
│   │       ├── Produto.cs
│   │       └── EstruturaProduto.cs         ← novo
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Services/
│   │   └── Engenharia/
│   │       ├── ProdutoService.cs            ← alterado
│   │       └── BomService.cs               ← novo
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
│   ├── 05-REFACTOR-ProdutoService.md
│   └── 06-FEATURE-BOM-DTOs.md              ← este documento
├── README.md
└── .gitignore
```

---
