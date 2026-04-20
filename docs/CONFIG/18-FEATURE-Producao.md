# 18 - FEATURE - Módulo Produção (Ordens de Produção)

## Resumo

Módulo Produção para planejamento e controle de Ordens de Produção (OP).

Cada OP é gerada a partir de um PV + Produto BOM, com hierarquia Master/Filha reflectindo a estrutura do produto:

- **OP Master** — liga ao PV + Produto raiz da BOM (ex: PIC-500)
- **OP Filha** — produto específico da estrutura do Master (ex: Rotor, Motor, Parafusos)

Master e Filhas têm **status independentes**. O andamento geral da Master é derivado das filhas (consultar a lista).

## Codificação

- **Master:** `OP.AAAA.MM.NNNN`
- **Filha:** `OP.AAAA.MM.NNNN/NNNN` (reaproveita código do Master + sequencial após `/`)

## Modelo

### Tabela `Producao_OrdensProducao`

| Campo | Tipo | Obs |
|---|---|---|
| `Id`, `Codigo` | int, string(30) | Código único |
| `PedidoVendaId` | int | FK PV |
| `ProdutoId` | int | FK Produto (raiz na Master; item da BOM na Filha) |
| `OrdemPaiId` | int? | null = Master; preenchido = Filha |
| `Status` | string(20) | enum `StatusOrdemProducao` |
| `DataInicio`, `DataFim` | DateTime? | preenchidos automaticamente ao mudar status |
| `Observacoes` | string(500)? | |

**Cascade:** deletar Master apaga filhas.

### Tabela `Producao_OrdensProducaoItens`

| Campo | Tipo | Obs |
|---|---|---|
| `Id`, `OrdemProducaoId`, `ProdutoId` | int | |
| `QuantidadePlanejada` | decimal | **snapshot da BOM no momento da criação (imutável por fluxo automático)** |
| `QuantidadeProduzida` | decimal | cresce via apontamentos |
| `Observacao` | string(500)? | |

**Índice único:** `(OrdemProducaoId, ProdutoId)` — mesmo produto não duplica na mesma OP.

### Tabela `Producao_OrdemProducaoHistorico`

Log de eventos: Criada, Iniciada, Pausada, Retomada, Concluida, Cancelada, Apontamento.
Inclui `StatusAnterior`, `StatusNovo`, `Justificativa`, `Detalhe`, `DataHora`.

## Status e transições

```
Pendente → Andamento → Concluida (terminal)
Pendente → Cancelada (terminal)
Andamento → Pausada → Andamento
Andamento → Cancelada
Pausada → Cancelada
```

**Justificativa obrigatória** em `Pausada` e `Cancelada`.

**Auto-conclusão:** quando todos os itens atingem `QuantidadeProduzida >= QuantidadePlanejada`, OP passa para `Concluida` automaticamente (evento registrado no histórico).

## Regras de criação

### Master
- PV e Produto devem existir
- Produto deve ter BOM (ser pai em EstruturasProdutos)

### Filha
- Master deve existir e não estar Cancelada
- Filha de Filha é proibido (somente 1 nível de hierarquia)
- Produto deve estar na BOM explodida do Master
- Mesmo Produto não pode ter 2 Filhas ativas na mesma Master
- QuantidadePlanejada é **snapshot** (calculado na criação, imutável automaticamente)

## Endpoints

| Método | Rota | Função |
|---|---|---|
| GET | `/` | Lista todas |
| GET | `/{id}` | Detalhe (itens + filhas) |
| GET | `/pedido/{id}` | OPs de um PV |
| GET | `/{id}/status-producao` | Consolidado com % |
| GET | `/{id}/divergencia-bom` | Compara OP × BOM atual |
| GET | `/{id}/historico` | Eventos |
| POST | `/master` | Cria Master |
| POST | `/filha` | Cria Filha |
| PUT | `/{id}` | Edita observações |
| PATCH | `/{id}/status` | Muda status |
| PATCH | `/{id}/itens/{itemId}/apontar` | Aponta produção (+N no produzido) |
| DELETE | `/{id}` | Exclui (só Pendente, sem apontamentos) |

## Divergência OP × BOM

Se a BOM do Produto raiz mudar depois da OP ser criada, o endpoint `/{id}/divergencia-bom` mostra as diferenças. A Produção decide:

- **Editar a OP** (ajusta quantidade planejada) — edição manual via `PUT` é permitida (Fase futura se necessário)
- **Criar OP complementar** (nova Filha só pra diferença)

## Arquivos afetados

### Models / Enums
- `app/Models/Producao/Enums/StatusOrdemProducao.cs` — NOVO
- `app/Models/Producao/Enums/EventoOrdemProducao.cs` — NOVO
- `app/Models/Producao/OrdemProducao.cs` — NOVO
- `app/Models/Producao/OrdemProducaoItem.cs` — NOVO
- `app/Models/Producao/OrdemProducaoHistorico.cs` — NOVO

### DTOs
- `app/DTOs/Producao/OrdemProducaoDTO.cs` — NOVO

### Configurations (EF)
- `app/Data/Configurations/Producao/OrdemProducaoConfiguration.cs` — NOVO
- `app/Data/Configurations/Producao/OrdemProducaoItemConfiguration.cs` — NOVO
- `app/Data/Configurations/Producao/OrdemProducaoHistoricoConfiguration.cs` — NOVO

### Services
- `app/Services/Producao/OrdemProducaoService.cs` — NOVO

### Controllers
- `app/Controllers/Producao/OrdemProducaoController.cs` — NOVO

### Infra
- `app/Data/AppDbContext.cs` — SUBSTITUIR (adiciona 3 DbSets de Produção)
- `app/Program.cs` — SUBSTITUIR (registra `OrdemProducaoService`)

### Testes
- `app/Tests/http/30-producao-ordens.http` — NOVO

## Como aplicar

```powershell
# 1. Colar todos os arquivos nos paths
# 2. Gerar migration
dotnet ef migrations add ModuloProducao --project app

# 3. Atualizar banco
dotnet ef database update --project app

# 4. API
arj-api
```

Testar cenários no `.http` 30 (sem seed pronto — cria OPs via requests).

## Próxima fase (3c)

- Integração automática OP → PV: primeira OP em Andamento → PV vai pra Andamento; todas as OPs da Master Concluidas → PV vai pra Concluido
- Notificações automáticas via módulo Notificações (Fase 2): PV Liberado → notifica Producao; OP Concluida → notifica Comercial; etc.
