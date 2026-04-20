# 15 - REFACTOR - Comercial

## Resumo

Refatoração do módulo Comercial para alinhar com o modelo de negócio real:

- **1 PV = 1 NS** (unique index em `PedidoVendaId`)
- **Tipo (Normal/PreVenda)** pertence ao PV e define o fluxo
- **Itens são descrição livre** (sem FK com `Produto`, sem preço)
- **Justificativa obrigatória** em Pausar, Cancelar, Reabrir, Devolver e retrocesso
- **Fluxo depende do tipo**: PreVenda passa por 3 status adicionais antes do fluxo comum
- **Bloqueios rígidos**: Entregue só aceita Devolvido; Cancelado só aceita Reaberto

## Tipos

| Tipo | Status inicial | Uso |
|------|----------------|-----|
| `Normal` | `Liberado` | Venda regular, vai direto pra produção |
| `PreVenda` | `AguardandoNS` | Libera NS antes de aprovação de financiamento |

## Status do PV

### Fluxo PreVenda (3 status iniciais)

```
AguardandoNS → RecebidoNS → AguardandoRetorno → Liberado
```

- `AguardandoNS` — PV criado, aguardando Engenharia gerar NS
- `RecebidoNS` — NS gerado pela Engenharia, Comercial avalia com o cliente
- `AguardandoRetorno` — proposta/NS entregues ao cliente, aguarda retorno

### Fluxo comum (Normal inicia aqui)

```
Liberado → Andamento → Concluido → AEntregar → Entregue
```

- `Liberado` — Comercial liberou para produção
- `Andamento` — Produção iniciou a OP
- `Concluido` — Produção finalizou a OP
- `AEntregar` — Comercial liberou para expedição
- `Entregue` — Entrega confirmada (terminal do fluxo normal)

### Status especiais (manuais, com justificativa + data)

- `Pausado` — fluxo interrompido temporariamente
- `Cancelado` — PV cancelado (proibido em Entregue)
- `Reaberto` — PV saiu do Cancelado; permanece assim até Comercial mover manualmente para `Liberado`
- `Devolvido` — só pode vir de `Entregue`

## Regras de transição

| De → Para | Permitido? | Obs |
|-----------|------------|-----|
| Qualquer → `Pausado` | ✅ (exceto de `Entregue`) | Justificativa obrigatória |
| Qualquer → `Cancelado` | ✅ (exceto de `Entregue`) | Justificativa obrigatória |
| `Cancelado` → `Reaberto` | ✅ | Única saída de `Cancelado`; justificativa obrigatória |
| `Cancelado` → qualquer outro | ❌ | Só `Reaberto` |
| `Entregue` → `Devolvido` | ✅ | Única saída de `Entregue`; justificativa obrigatória |
| `Entregue` → `Pausado`/`Cancelado` | ❌ | Bloqueado |
| `Devolvido` a partir de não-`Entregue` | ❌ | Só vale em cima de `Entregue` |
| `Reaberto` a partir de não-`Cancelado` | ❌ | Só vale em cima de `Cancelado` |
| Retrocesso no fluxo (destino antes da origem) | ✅ | Justificativa obrigatória |
| Avanço normal no fluxo | ✅ | Sem justificativa |

## Justificativa obrigatória

| Transição | Obrigatória? |
|-----------|--------------|
| Qualquer → `Pausado` | ✅ |
| Qualquer → `Cancelado` | ✅ |
| `Cancelado` → `Reaberto` | ✅ |
| `Entregue` → `Devolvido` | ✅ |
| Retrocesso (nível destino < nível origem) | ✅ |
| Avanço normal | ❌ |
| Saída de `Pausado` (retomada) | ❌ |
| `Reaberto` → `Liberado` | ❌ |

### Níveis do fluxo (para detectar retrocesso)

```
AguardandoNS       = 0
RecebidoNS         = 1
AguardandoRetorno  = 2
Liberado           = 3
Andamento          = 4
Concluido          = 5
AEntregar          = 6
Entregue           = 7
```

Status especiais (`Pausado`, `Cancelado`, `Reaberto`, `Devolvido`) ficam fora do fluxo linear — retrocesso só é detectado entre dois status de fluxo.

## Eventos no histórico

| Transição | Evento registrado |
|-----------|-------------------|
| Criação do PV | `Criado` |
| `AguardandoNS` → `RecebidoNS` | `NsRecebido` |
| `RecebidoNS` → `AguardandoRetorno` | `RetornoSolicitado` |
| `AguardandoRetorno` → `Liberado` | `Aprovado` |
| `Liberado` → `Andamento` | `ProducaoIniciada` |
| `Andamento` → `Concluido` | `ProducaoConcluida` |
| `Concluido` → `AEntregar` | `LiberadoEntrega` |
| `AEntregar` → `Entregue` | `Entregue` |
| Qualquer → `Pausado` | `Pausado` |
| `Pausado` → qualquer | `Retomado` |
| Qualquer → `Cancelado` | `Cancelado` |
| `Cancelado` → `Reaberto` | `Reaberto` |
| `Entregue` → `Devolvido` | `Devolvido` |

## Edição e exclusão

**Edição do PV** permitida apenas nos status iniciais:
- `AguardandoNS`, `RecebidoNS`, `AguardandoRetorno`, `Liberado`

**Exclusão do PV** permitida apenas em:
- `AguardandoNS` ou `Liberado`, e **sem NS vinculado**

## Arquivos afetados

### Models
- `app/Models/Comercial/Enums/StatusPedidoVenda.cs`
- `app/Models/Comercial/Enums/TipoPedidoVenda.cs`
- `app/Models/Comercial/Enums/EventoPedidoVenda.cs`
- `app/Models/Comercial/PedidoVenda.cs` — `Tipo`, `DataEntrega`
- `app/Models/Comercial/PedidoVendaItem.cs` — `Descricao` + `Observacao` (descrição livre)
- `app/Models/Comercial/NumeroSerie.cs` — sem `Tipo`/`Status` (herda do PV)
- `app/Models/Comercial/PedidoVendaHistorico.cs` — `StatusAnterior`, `StatusNovo`, `Justificativa`

### DTOs
- `app/DTOs/Comercial/PedidoVendaDTO.cs` — `Tipo`, `Data`, `DataEntrega`, `StatusPedidoVendaDTO` com `Justificativa`
- `app/DTOs/Comercial/PedidoVendaItemDTO.cs` — `Descricao` + `Observacao`
- `app/DTOs/Comercial/NumeroSerieDTO.cs` — response com `pvTipo`/`pvStatus`/`pvDataEntrega`
- `app/DTOs/Comercial/PedidoVendaHistoricoDTO.cs` — `StatusAnterior`, `StatusNovo`, `Justificativa`

### Configurations (EF)
- `app/Data/Configurations/Comercial/PedidoVendaConfiguration.cs` — conversão `Tipo`/`Status` string
- `app/Data/Configurations/Comercial/PedidoVendaItemConfiguration.cs` — descrição livre
- `app/Data/Configurations/Comercial/NumeroSerieConfiguration.cs` — unique index em `PedidoVendaId`
- `app/Data/Configurations/Comercial/PedidoVendaHistoricoConfiguration.cs` — enums string + `Justificativa`

### Services
- `app/Services/Comercial/PedidoVendaService.cs` — transições, bloqueios, justificativa, eventos
- `app/Services/Comercial/PedidoVendaItemService.cs` — descrição livre
- `app/Services/Comercial/NumeroSerieService.cs` — 1:1 com PV, sem `Tipo`/`Status`

### Controllers
- `app/Controllers/Comercial/PedidoVendaController.cs`
- `app/Controllers/Comercial/PedidoVendaItensController.cs`
- `app/Controllers/Comercial/NumeroSerieController.cs`

### Testes
- `app/Tests/http/10-comercial-pedidos-venda.http`
- `app/Tests/http/11-comercial-itens-pedido.http`
- `app/Tests/http/12-comercial-numero-serie.http`

### Seeds
- `docs/SQL/SEED_COMERCIAL_01_PEDIDOSVENDA.SQL`
- `docs/SQL/SEED_COMERCIAL_04_PEDIDOVENDAHISTORICO.SQL`

## Como aplicar

Como o banco é de dev, o processo é destrutivo e limpo:

```powershell
# 1. Apagar todas as migrations e o banco
Remove-Item -Recurse -Force app/Migrations
Remove-Item app/Database/ArjSysDB.db -ErrorAction SilentlyContinue

# 2. Gerar migration inicial a partir do modelo atual
dotnet ef migrations add Inicial --project app

# 3. Criar o banco
dotnet ef database update --project app

# 4. Rodar os SEEDs atualizados (pelo SeedRunner ou menu .bat)
```

Depois validar com `.http` 10, 11 e 12.

## Impacto no frontend

Pontos a ajustar quando o backend estiver no ar:

- Form do PV: `tipo` agora é `Normal` / `PreVenda` (renomear de `VendaFutura`)
- Status list precisa cobrir os 12 novos status
- Cores/ícones por status (os 3 iniciais da PreVenda, os 5 do fluxo comum, os 4 especiais)
- Modal de mudança de status: campo `justificativa` com validação condicional
- Botões de ação precisam respeitar os bloqueios:
  - `Entregue` mostra só "Devolver"
  - `Cancelado` mostra só "Reabrir"
  - `Reaberto` mostra só "Liberar" (manual)
- Grid de PVs: coluna Status com 12 estados (não 6)
- Histórico continua com `statusAnterior → statusNovo + justificativa`
