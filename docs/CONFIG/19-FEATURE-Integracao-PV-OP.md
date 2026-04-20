# 19 - FEATURE - Integração automática PV ↔ OP + Notificações

## Resumo

Fecha o ciclo PV → Produção com transições automáticas de status e notificações automáticas. Também permite **OP sem PV** (produção pra estoque).

## Mudanças no modelo

### `OrdemProducao.PedidoVendaId` agora é **opcional**

- **Com PV:** OP atende um pedido específico. Validações de status do PV aplicam.
- **Sem PV:** OP independente — produção pra estoque. Sem validações de PV.

Controle de estoque em si **não** está implementado — fica pra fase futura.

### Criação de OP Master com PV

Só permitido se PV está em: `Liberado`, `Andamento` ou `Pausado`.

- Liberado: fluxo natural (primeira OP)
- Andamento: adicionar OPs extras enquanto produz
- Pausado: Produção tem autonomia pra adiantar mesmo com PV pausado

## Transições automáticas

### OP → Andamento automático no PV
Quando qualquer OP (Master ou Filha) ligada a um PV muda pra `Andamento`:
- Se PV está em `Liberado` → PV vai automaticamente pra `Andamento`
- Evento `ProducaoIniciada` registrado no histórico do PV

### OP Master → Concluida automática no PV
Quando uma Master é concluída (manual ou auto-concluída via apontamento):
- Se **todas as Masters do PV** estão em `Concluida` ou `Cancelada`
- E pelo menos uma está `Concluida`
- E PV está em `Andamento`
→ PV vai automaticamente pra `Concluido`
→ Evento `ProducaoConcluida` no histórico
→ Notificação pro Comercial ("PV pronto pra expedição")

## Bloqueio de entrega

**PV não pode ir pra `Entregue`** se existe alguma OP Master do PV em status `Pendente`, `Andamento` ou `Pausada`. Retorna 400.

Solução: concluir ou cancelar as Masters pendentes antes.

## Notificações automáticas

| Evento | Destino | Tipo | Mensagem |
|---|---|---|---|
| PV criado em Liberado (Normal) | Producao | Info | "PV {codigo} liberado — programar OPs" |
| PV vai pra Liberado (PreVenda aprovada) | Producao | Info | idem |
| OP Master criada | Almoxarifado | Info | "Reservar material para {produto}" |
| OP (qualquer) concluída | Producao | Sucesso | "OP {codigo} concluída" |
| Todas Masters do PV concluídas | Comercial | Sucesso | "PV pronto para expedição" |
| PV cancelado com OP ativa | Producao | Aviso | "Avaliar cancelamento das OPs" |
| PV pausado com OP ativa | Producao | Aviso | "Avaliar pausa das OPs" |

Todas as notificações incluem `OrigemTabela` e `OrigemId` pra rastreabilidade.

## Arquivos afetados

### Models
- `app/Models/Producao/OrdemProducao.cs` — SUBSTITUIR (PedidoVendaId opcional)

### DTOs
- `app/DTOs/Producao/OrdemProducaoDTO.cs` — SUBSTITUIR (Create/Response com PV opcional)

### Configurations (EF)
- `app/Data/Configurations/Producao/OrdemProducaoConfiguration.cs` — SUBSTITUIR (FK PV opcional)

### Services
- `app/Services/Producao/OrdemProducaoService.cs` — SUBSTITUIR (NotificacaoService injetado, integrações com PV, PV opcional nas validações)
- `app/Services/Comercial/PedidoVendaService.cs` — SUBSTITUIR (NotificacaoService injetado, bloqueio de entrega, notificações automáticas)

### Testes
- `app/Tests/http/producao_01_ordens.http` — SUBSTITUIR (cenários de integração + OP de estoque)

## Como aplicar

```powershell
# 1. Colar todos os arquivos
# 2. Migration (PedidoVendaId vira nullable)
dotnet ef migrations add PvOpIntegracao --project app

# 3. Atualizar banco
dotnet ef database update --project app

# 4. Rodar API
arj-api
```

Testar cenários no `.http` producao_01 (com atenção nos comentários de integração).

## Cenários de teste

1. **Fluxo feliz automático:** cria PV Normal → cria Master → inicia filha → PV vai pra Andamento auto → aponta tudo → Master auto-conclui → PV vai pra Concluido auto → notifica Comercial
2. **OP de estoque:** cria Master sem PedidoVendaId → sem efeito em PV → aparece como `ehEstoque: true`
3. **Bloqueio de entrega:** PV com Master Pendente → tentar Entregue → 400
4. **Cancelar PV com OPs ativas:** notificação pra Producao avaliar
5. **Master Pausada:** Produção pode criar OPs extras mesmo com PV Pausado

## Ciclo completo v3 concluído

Fase 1: Refactor Comercial (novos status, tipos, fluxo por tipo)
Fase 2: Notificações (infra genérica)
Fase 3a: NS vinculado a Produto BOM + explosão de BOM
Fase 3b: Módulo Produção (OPs Master/Filha, snapshot, apontamentos)
Fase 3c: Integração PV ↔ OP + notificações automáticas ← **essa**

O backend v3 está fechado funcionalmente. Próximos passos ficam no frontend.
