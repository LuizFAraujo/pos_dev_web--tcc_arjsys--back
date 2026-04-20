# 17 - REFACTOR - NS vinculado a Produto BOM

## Resumo

Refatoração no Número de Série para transformar o campo `CodigoProjeto` (string solta) em uma **FK para `Engenharia_Produtos`** (`ProdutoId`). O NS passa a ser o elo entre o PV (comercial) e o Produto BOM (engenharia/produção).

Além disso, adiciona endpoint de **explosão de BOM** no módulo Engenharia — retorna todos os itens folha consolidados com quantidade total somada.

## Mudanças conceituais

- `NumeroSerie.CodigoProjeto` (string) → **`NumeroSerie.ProdutoId`** (int?, FK)
- Produto vinculado ao NS deve ser **BOM** (ter pelo menos 1 filho em `EstruturasProdutos`)
- Pesquisa atravessa PV ↔ NS ↔ Produto

## Novo endpoint

`GET /api/engenharia/Bom/produto/{id}/explosao`

Desce recursivamente em todos os níveis da estrutura, multiplica as quantidades pelo caminho e soma as ocorrências do mesmo produto em ramos diferentes. Retorna 1 linha por item **folha** (produtos que não têm filhos) com a quantidade total.

Exemplo:
```
PIC-500 (BOM explodida):
  Parafuso M10:  47 un  (soma de todas as ocorrências em todos os níveis)
  Motor 50CV:     1 un
  Rotor-001:      1 un  // se Rotor-001 não tiver BOM, aparece
  Chapa 3/16:   8.5 kg
  ...
```

Itens intermediários (com filhos próprios) **não aparecem** no resultado — só as folhas.

## Validações

- `ProdutoId` é **opcional** no NS (pode criar sem e preencher depois via `PUT`)
- Se informado, produto deve existir, estar ativo e ter BOM
- Regras antigas de criação de NS continuam (só PV tipo PreVenda em AguardandoNS)

## Codificação de OP (só documentado, implementação nas próximas fases)

- Pai: `OP.AAAA.MM.NNNN`
- Filha: `OP.AAAA.MM.NNNN/NNNN` (reaproveita o código do pai + sequencial após a barra)

## Arquivos afetados

### Models
- `app/Models/Comercial/NumeroSerie.cs` — SUBSTITUIR (troca `CodigoProjeto` por `ProdutoId`)

### DTOs
- `app/DTOs/Comercial/NumeroSerieDTO.cs` — SUBSTITUIR (`produtoId` em vez de `codigoProjeto`, response inclui `produtoCodigo` + `produtoDescricao`)
- `app/DTOs/Engenharia/BomExplosaoDTO.cs` — NOVO

### Configurations (EF)
- `app/Data/Configurations/Comercial/NumeroSerieConfiguration.cs` — SUBSTITUIR (FK pra Produto, remove MaxLength de CodigoProjeto)

### Services
- `app/Services/Comercial/NumeroSerieService.cs` — SUBSTITUIR (validação de Produto BOM)
- `app/Services/Engenharia/BomService.cs` — SUBSTITUIR (adiciona método `GetExplosao`)

### Controllers
- `app/Controllers/Engenharia/BomController.cs` — SUBSTITUIR (adiciona endpoint `/produto/{id}/explosao`)

### Testes
- `app/Tests/http/12-comercial-numero-serie.http` — SUBSTITUIR (cenários com `produtoId`)
- `app/Tests/http/03-engenharia-bom.http` — SUBSTITUIR (adiciona cenários de explosão)

### Seeds
- `docs/SQL/SEED_COMERCIAL_03_NUMEROSSERIE.SQL` — SUBSTITUIR (`ProdutoId` em vez de `CodigoProjeto`; coerente com seed 01 v3)

## Como aplicar

```powershell
# 1. Colar todos os arquivos nos paths
# 2. Adicionar migration
dotnet ef migrations add NsComProduto --project app

# 3. Atualizar banco
dotnet ef database update --project app

# 4. Re-rodar seeds (o SEED 03 mudou)
arj-reset
arj-seed
```

Testar com `.http` 03 (explosao) e `.http` 12 (NS com ProdutoId).

## Próximas fases

- **Fase 3b:** módulo Produção — OrdemProducao (tabela + CRUD + OP Master/Filha + alocação de itens da BOM explodida)
- **Fase 3c:** integração OP → PV (status automático) e notificações automáticas
