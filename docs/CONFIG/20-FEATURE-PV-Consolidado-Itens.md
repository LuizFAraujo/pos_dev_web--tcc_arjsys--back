# 20 - FEATURE - Consolidação PV + Itens em chamada única + Edição em status avançado

## Resumo

Atende pedido do front-end. Resolve 3 problemas:

1. **Atomicidade**: criar/editar PV + itens em 1 chamada, não N+1
2. **Regra "PV sem itens"**: reforçada no back (antes só o front validava)
3. **Edição em status avançado**: permite alterar itens mesmo após produção iniciar, mas exige justificativa e rastreabilidade

## Mudanças principais

### POST `/PedidoVenda` agora aceita `itens: [...]`

```json
POST /api/comercial/PedidoVenda
{
  "clienteId": 1,
  "tipo": "Normal",
  "itens": [
    { "quantidade": 1, "descricao": "..." },
    { "quantidade": 2, "descricao": "..." }
  ]
}
```

- Lista **obrigatória** com pelo menos 1 item → senão 400
- Tudo em transação EF Core: erro em qualquer item = rollback completo

### PUT `/PedidoVenda/{id}` agora é "replace full"

Recebe o shape consolidado com cabeçalho + itens + justificativa (condicional).

**Diff automático:**
- Item com `id` no payload → `UPDATE`
- Item sem `id` no payload → `INSERT`
- Item no banco ausente do payload → `DELETE`

**Retorna 200 OK com `PedidoVendaResponseDTO`** (antes era 204 No Content).
Justificado: o front economiza um GET subsequente, e itens novos precisam do ID gerado.

### Novo evento `ItensAlterados`

Adicionado ao enum `EventoPedidoVenda`. Registrado no histórico quando itens são alterados em status avançado.

Formato do campo `justificativa` no histórico: `"<motivo do usuário> [+N adicionados, -M removidos, K alterados]"`.

### Regras de edição por status

| Status do PV | Edita itens? | Justificativa? | Histórico? | Notifica? |
|---|---|---|---|---|
| AguardandoNS, RecebidoNS, AguardandoRetorno, Liberado | Sim | Não | Não | Não |
| Andamento, Concluido, AEntregar, Pausado | Sim | **Obrigatória** | `ItensAlterados` | **Eng + Prod + Almox** |
| Entregue, Devolvido, Cancelado, Reaberto | Não | — | — | — |

**Almoxarifado só é notificado** se status é `Andamento` ou `Pausado` (nos status `Concluido`/`AEntregar` o material já foi consumido).

### Endpoints individuais mantidos

`POST/PUT/DELETE /api/comercial/PedidoVenda/{id}/itens[/{itemId}]` continuam funcionando, com as mesmas regras:

- POST/PUT ganham campo `justificativa` no body
- DELETE ganha query param `?justificativa=...`
- Em status inicial: ignoram justificativa
- Em status avançado: obrigatória
- Em status terminal: 400

## Shapes novos

### `PedidoVendaCreateDTO` (modificado)
```csharp
public class PedidoVendaCreateDTO
{
    public int ClienteId { get; set; }
    public TipoPedidoVenda Tipo { get; set; }
    public DateTime? Data { get; set; }
    public DateTime? DataEntrega { get; set; }
    public string? Observacoes { get; set; }
    public List<PedidoVendaItemCreateDTO> Itens { get; set; } = []; // OBRIGATÓRIO >=1
}
```

### `PedidoVendaUpdateDTO` (NOVO)
```csharp
public class PedidoVendaUpdateDTO
{
    public int ClienteId { get; set; }
    public TipoPedidoVenda Tipo { get; set; }
    public DateTime? Data { get; set; }
    public DateTime? DataEntrega { get; set; }
    public string? Observacoes { get; set; }
    public List<PedidoVendaItemUpsertDTO> Itens { get; set; } = [];
    public string? Justificativa { get; set; } // obrigatória em status avançado
}
```

### `PedidoVendaItemUpsertDTO` (NOVO)
```csharp
public class PedidoVendaItemUpsertDTO
{
    public int? Id { get; set; } // null/0 = novo; preenchido = atualizar
    public decimal Quantidade { get; set; }
    public string Descricao { get; set; } = "";
    public string? Observacao { get; set; }
}
```

### `PedidoVendaItemCreateDTO` (modificado — ganhou justificativa)
```csharp
public class PedidoVendaItemCreateDTO
{
    public decimal Quantidade { get; set; }
    public string Descricao { get; set; } = "";
    public string? Observacao { get; set; }
    public string? Justificativa { get; set; } // obrigatória em status avançado
}
```

## Arquivos afetados

| Ação | Path |
|---|---|
| SUBSTITUIR | `app/Models/Comercial/Enums/EventoPedidoVenda.cs` |
| SUBSTITUIR | `app/DTOs/Comercial/PedidoVendaDTO.cs` |
| SUBSTITUIR | `app/DTOs/Comercial/PedidoVendaItemDTO.cs` |
| SUBSTITUIR | `app/Services/Comercial/PedidoVendaService.cs` |
| SUBSTITUIR | `app/Services/Comercial/PedidoVendaItemService.cs` |
| SUBSTITUIR | `app/Controllers/Comercial/PedidoVendaController.cs` |
| SUBSTITUIR | `app/Controllers/Comercial/PedidoVendaItensController.cs` |
| SUBSTITUIR | `app/Tests/http/comercial_01_pedidos-venda.http` |
| SUBSTITUIR | `app/Tests/http/comercial_02_itens-pedido.http` |
| NOVO | `docs/CONFIG/20-FEATURE-PV-Consolidado-Itens.md` |

**Nenhuma migration necessária.** Enum `EventoPedidoVenda` é mapeado como string, só adicionei valor novo.

## Como aplicar

```powershell
# 1. Colar todos os arquivos
# 2. Build de sanidade
dotnet build app

# 3. Rodar
arj-api
```

## Cenários de teste

Ver `.http` de comercial_01 e comercial_02. Principais:

**comercial_01:**
1. POST com 2 itens → 201, PV criado, 1 evento Criado no histórico
2. POST com `itens: []` → 400 "Pedido de venda deve ter ao menos um item."
3. POST com erro no item 2 → 400 + rollback (PV não criado)
4. PUT em Liberado com diff → 200 com shape atualizado
5. PUT em Andamento COM justificativa → 200 + evento ItensAlterados + 3 notificações
6. PUT em Andamento SEM justificativa → 400
7. PUT em Entregue → 400 (status terminal)

**comercial_02:**
1. POST item em Liberado sem justificativa → 200
2. POST item em Andamento sem justificativa → 400
3. POST item em Andamento com justificativa → 200 + notificações
4. DELETE item em Andamento `?justificativa=x` → 204 + notificações
5. DELETE item em Entregue → 400
