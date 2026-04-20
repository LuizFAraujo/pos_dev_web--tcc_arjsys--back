# 16 - FEATURE - Notificações

## Resumo

Infraestrutura de notificações genéricas direcionadas a **módulos** do sistema (não a usuários individuais). Qualquer parte do sistema pode criar uma notificação para um módulo destinatário; o front lista/marca como lida.

Nesta fase **apenas a infra** é entregue (tabela, service, endpoints, seed). O disparo automático a partir de eventos (ex: PV liberado → notifica Produção) será acoplado em fases seguintes.

## Ajuste de base: `PCP` → `Producao`

O enum `ModuloSistema` tinha o valor `PCP`. Foi renomeado para `Producao` (mais direto, em português). Impacta:

- `app/Models/Admin/Enums/ModuloSistema.cs`
- `docs/SQL/SEED_ADMIN_04_PERMISSOES.SQL` (todas as linhas com `'PCP'`)

## Modelo

### Tabela: `Admin_Notificacoes`

| Campo | Tipo | Obs |
|---|---|---|
| `Id` | int | PK |
| `ModuloDestino` | string(30) | enum `ModuloSistema` convertido |
| `Tipo` | string(20) | enum `TipoNotificacao`: Info / Sucesso / Aviso / Erro |
| `Titulo` | string(100) | obrigatório |
| `Mensagem` | string(500) | obrigatório |
| `Lida` | bool | default false |
| `DataLeitura` | DateTime? | null enquanto não lida |
| `OrigemTabela` | string(100)? | rastreabilidade (ex: `Comercial_PedidosVenda`) |
| `OrigemId` | int? | id do registro de origem |
| `CriadoEm` | DateTime | BaseEntity |
| `ModificadoEm` | DateTime? | BaseEntity |

**Índice:** `(ModuloDestino, Lida)` — acelera a query principal "não lidas do módulo X".

## Endpoints

Todos em `api/admin/Notificacoes`:

| Método | Rota | Função |
|---|---|---|
| GET | `?modulo=X` | Lista por módulo (query params: `lidas=true/false`, `pagina`, `tamanho`) |
| GET | `/{id}` | Busca por ID |
| GET | `/nao-lidas/contagem?modulo=X` | Só o número (pra badge) |
| POST | `/` | Cria notificação |
| PATCH | `/{id}/lida` | Marca como lida |
| PATCH | `/modulo/{modulo}/marcar-todas-lidas` | Marca todas como lidas |
| DELETE | `/{id}` | Exclui |

## Arquivos afetados

### Models / Enums
- `app/Models/Admin/Enums/ModuloSistema.cs` — SUBSTITUIR (rename PCP → Producao)
- `app/Models/Admin/Enums/TipoNotificacao.cs` — NOVO
- `app/Models/Admin/Notificacao.cs` — NOVO

### DTOs
- `app/DTOs/Admin/NotificacaoDTO.cs` — NOVO

### Configurations (EF)
- `app/Data/Configurations/Admin/NotificacaoConfiguration.cs` — NOVO

### Services
- `app/Services/Admin/NotificacaoService.cs` — NOVO

### Controllers
- `app/Controllers/Admin/NotificacoesController.cs` — NOVO

### Infra
- `app/Data/AppDbContext.cs` — SUBSTITUIR (adiciona `DbSet<Notificacao>`)
- `app/Program.cs` — SUBSTITUIR (registra `NotificacaoService`)

### Testes
- `app/Tests/http/20-admin-notificacoes.http` — NOVO

### Seeds
- `docs/SQL/SEED_ADMIN_04_PERMISSOES.SQL` — SUBSTITUIR (PCP → Producao)
- `docs/SQL/SEED_ADMIN_05_NOTIFICACOES.SQL` — NOVO
- `SeedRunner/seed-order.txt` — ALTERAR (adicionar linha do novo seed)

## Como aplicar

Como a Fase 1 regenerou o banco recentemente, aqui basta:

```powershell
# 1. Colar todos os arquivos nos paths
# 2. Adicionar migration
dotnet ef migrations add AddNotificacoes --project app

# 3. Atualizar banco (adiciona a tabela)
dotnet ef database update --project app

# 4. Re-rodar seeds (PCP → Producao + novo SEED_ADMIN_05)
arj-reset
arj-seed
```

Depois testar com `.http` 20.

## Próximos passos (Fase 3)

- Módulo Produção (tabela `Producao_OrdensProducao`)
- Disparo automático de notificações a partir de eventos do PV (Liberado → notifica Produção; Concluído → notifica Comercial; etc.)
