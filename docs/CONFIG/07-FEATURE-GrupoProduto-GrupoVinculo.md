<!-- markdownlint-disable-file -->
# ✨ FEATURE: Implementa GrupoProduto e GrupoVinculo

## 💡 Conceitos aplicados

### Código Inteligente de Produto

Os produtos seguem uma classificação hierárquica em 4 níveis: `XX.YYY.ZZZZ.NNNN` (Grupo.Subgrupo.Família.Sequencial). Cada nível é cadastrado na tabela GrupoProduto com seu código, descrição e quantidade de caracteres.

### Vínculos entre Grupos

Funciona como a BOM — tabela flat informando quais filhos cada pai aceita. Validação obrigatória: só permite vínculos entre níveis consecutivos (Grupo→Subgrupo, Subgrupo→Família). Impede vínculo duplicado e auto-referência.

### Path de Documentos

Cada grupo pode ter um path customizado para documentos/desenhos. Se não informado, usa o path raiz global + código do grupo. O método `MontarPathDocumento()` no service resolve a hierarquia automaticamente.

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `Models/Engenharia/Enums/NivelGrupo.cs` | Enum: Grupo, Subgrupo, Familia |
| `Models/Engenharia/GrupoProduto.cs` | Entidade do grupo (herda BaseEntity) |
| `Models/Engenharia/GrupoVinculo.cs` | Relação pai→filho entre grupos |
| `DTOs/Engenharia/GrupoProdutoDTO.cs` | GrupoProdutoCreateDTO e ResponseDTO |
| `DTOs/Engenharia/GrupoVinculoDTO.cs` | GrupoVinculoCreateDTO e ResponseDTO |
| `Data/Configurations/Engenharia/GrupoProdutoConfiguration.cs` | Índice único código+nível, conversão enum→string |
| `Data/Configurations/Engenharia/GrupoVinculoConfiguration.cs` | Índice único pai+filho, FKs Restrict |
| `Services/Engenharia/GrupoProdutoService.cs` | CRUD + validação + montagem path docs |
| `Services/Engenharia/GrupoVinculoService.cs` | CRUD + validação níveis consecutivos |
| `Controllers/Engenharia/GrupoProdutoController.cs` | Endpoints CRUD grupos |
| `Controllers/Engenharia/GrupoVinculoController.cs` | Endpoints CRUD vínculos |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Data/AppDbContext.cs` | Adicionado DbSets de GruposProdutos e GruposVinculos |
| `Program.cs` | Registrado GrupoProdutoService e GrupoVinculoService no AddScoped |

---

## 🌐 Endpoints

### GrupoProduto

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/engenharia/GrupoProduto` | Lista todos os grupos |
| GET | `/api/engenharia/GrupoProduto/nivel/{nivel}` | Lista por nível (Grupo, Subgrupo, Familia) |
| GET | `/api/engenharia/GrupoProduto/{id}` | Busca por ID |
| POST | `/api/engenharia/GrupoProduto` | Cria grupo |
| PUT | `/api/engenharia/GrupoProduto/{id}` | Atualiza grupo |
| DELETE | `/api/engenharia/GrupoProduto/{id}` | Remove (se não tiver vínculos) |

### GrupoVinculo

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/engenharia/GrupoVinculo` | Lista todos os vínculos |
| GET | `/api/engenharia/GrupoVinculo/pai/{paiId}` | Lista filhos permitidos de um pai |
| POST | `/api/engenharia/GrupoVinculo` | Cria vínculo (valida níveis consecutivos) |
| DELETE | `/api/engenharia/GrupoVinculo/{id}` | Remove vínculo |

---

## ✅ Validações implementadas

- Código + Nível únicos (não pode ter dois "10" no nível Grupo)
- Quantidade de caracteres > 0
- Vínculo: pai ≠ filho
- Vínculo: níveis consecutivos obrigatórios (Grupo→Subgrupo, Subgrupo→Família)
- Vínculo duplicado impedido
- Exclusão de grupo impedida se houver vínculos

---
