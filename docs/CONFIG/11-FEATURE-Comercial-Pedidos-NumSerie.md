<!-- markdownlint-disable-file -->
# ✨ FEATURE: Implementa módulo Comercial (Pedidos de Venda, Itens, Número de Série)

## 💡 Conceitos aplicados

### Código automático do Pedido de Venda

Formato: `PV.AAAA.MM.NNNN` — ano e mês capturados da data de criação, sequencial incrementa dentro do mês.
Exemplo: PV.2026.02.0001, PV.2026.02.0002, etc.

### Número de Série — formato da empresa

Formato: `II.MM.AA.NNNNN` — idade da empresa (ano atual - 1966), mês, ano (2 dígitos), sequencial.
Exemplo: 60.02.26.00001 (empresa com 60 anos, fevereiro de 2026, primeiro N.Série do mês).

Só é gerado quando o pedido está com status Aprovado ou superior. Pedidos em Orçamento ou Cancelados não geram N.Série.

### Controle de status com transições validadas

O backend valida que as transições de status seguem o fluxo correto:

**Pedido de Venda:**
```
Orçamento → Aprovado → Em Produção → Concluído → Entregue
    ↓           ↓
 Cancelado   Cancelado
```

**Número de Série:**
```
Aberto → Em Fabricação → Concluído → Entregue
```

Transições fora do fluxo retornam erro 400.

### Proteção de integridade

- Só edita/exclui pedido em status Orçamento
- Só adiciona/edita/remove itens em pedidos Orçamento
- Não exclui pedido com N.Série gerado
- Produto duplicado impedido no mesmo pedido
- Total do pedido calculado automaticamente (soma dos subtotais dos itens)

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `Models/Comercial/Enums/StatusPedidoVenda.cs` | Enum: Orcamento, Aprovado, EmProducao, Concluido, Entregue, Cancelado |
| `Models/Comercial/Enums/StatusNumeroSerie.cs` | Enum: Aberto, EmFabricacao, Concluido, Entregue |
| `Models/Comercial/PedidoVenda.cs` | Código auto, ClienteId, Status, Data, Observações |
| `Models/Comercial/PedidoVendaItem.cs` | PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario |
| `Models/Comercial/NumeroSerie.cs` | Código formato empresa, PedidoVendaId, Status |
| `DTOs/Comercial/PedidoVendaDTO.cs` | CreateDTO, ResponseDTO (com itens e total), StatusDTO |
| `DTOs/Comercial/PedidoVendaItemDTO.cs` | CreateDTO, ResponseDTO (com subtotal) |
| `DTOs/Comercial/NumeroSerieDTO.cs` | CreateDTO, ResponseDTO, StatusDTO |
| `Data/Configurations/Comercial/PedidoVendaConfiguration.cs` | Comercial_PedidosVenda, FK Cliente Restrict |
| `Data/Configurations/Comercial/PedidoVendaItemConfiguration.cs` | Comercial_PedidosVendaItens, FK Cascade/Restrict |
| `Data/Configurations/Comercial/NumeroSerieConfiguration.cs` | Comercial_NumerosSerie, FK PedidoVenda Restrict |
| `Services/Comercial/PedidoVendaService.cs` | CRUD + código auto + status + total |
| `Services/Comercial/PedidoVendaItemService.cs` | CRUD itens + validações |
| `Services/Comercial/NumeroSerieService.cs` | CRUD + código formato empresa + status |
| `Controllers/Comercial/PedidoVendaController.cs` | Endpoints CRUD + PATCH status |
| `Controllers/Comercial/PedidoVendaItensController.cs` | Endpoints CRUD itens aninhados |
| `Controllers/Comercial/NumeroSerieController.cs` | Endpoints CRUD + PATCH status |
| `tests/http/10-comercial-pedidos-venda.http` | 12 testes de pedidos |
| `tests/http/11-comercial-itens-pedido.http` | 10 testes de itens |
| `tests/http/12-comercial-numero-serie.http` | 13 testes de N.Série |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Data/AppDbContext.cs` | Adicionado DbSets: PedidosVenda, PedidosVendaItens, NumerosSerie |
| `Program.cs` | Registrado PedidoVendaService, PedidoVendaItemService, NumeroSerieService |

---

## 🗄️ Tabelas no banco

| Tabela | Prefixo | Registros na carga |
|--------|---------|-------------------|
| `Comercial_PedidosVenda` | Comercial_ | 20 |
| `Comercial_PedidosVendaItens` | Comercial_ | 34 |
| `Comercial_NumerosSerie` | Comercial_ | 14 |

---

## 🌐 Endpoints

### Comercial - Pedidos de Venda

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/comercial/PedidoVenda` | Lista todos (paginável) |
| GET | `/api/comercial/PedidoVenda/{id}` | Busca por ID (com itens e total) |
| POST | `/api/comercial/PedidoVenda` | Cria pedido (código auto) |
| PUT | `/api/comercial/PedidoVenda/{id}` | Atualiza (só Orçamento) |
| PATCH | `/api/comercial/PedidoVenda/{id}/status` | Altera status (transição validada) |
| DELETE | `/api/comercial/PedidoVenda/{id}` | Remove (só Orçamento, sem N.Série) |

### Comercial - Itens do Pedido

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/comercial/PedidoVenda/{pedidoId}/itens` | Lista itens do pedido |
| POST | `/api/comercial/PedidoVenda/{pedidoId}/itens` | Adiciona item (só Orçamento) |
| PUT | `/api/comercial/PedidoVenda/{pedidoId}/itens/{id}` | Atualiza item (só Orçamento) |
| DELETE | `/api/comercial/PedidoVenda/{pedidoId}/itens/{id}` | Remove item (só Orçamento) |

### Comercial - Número de Série

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/comercial/NumeroSerie` | Lista todos (paginável) |
| GET | `/api/comercial/NumeroSerie/{id}` | Busca por ID |
| GET | `/api/comercial/NumeroSerie/pedido/{pedidoId}` | N.Série de um pedido |
| POST | `/api/comercial/NumeroSerie` | Gera N.Série (pedido deve estar aprovado) |
| PATCH | `/api/comercial/NumeroSerie/{id}/status` | Altera status (transição validada) |

---

## 🧪 Roteiro de testes (.http)

| Arquivo | Módulo | Testes |
|---------|--------|--------|
| 10-comercial-pedidos-venda.http | Pedidos | 12 |
| 11-comercial-itens-pedido.http | Itens | 10 |
| 12-comercial-numero-serie.http | N.Série | 13 |
| **Total Fase 3** | | **35** |

### Total acumulado de testes .http

| Fase | Testes |
|------|--------|
| Status | 1 |
| Engenharia | 38 |
| Admin | 28 |
| Comercial | 35 |
| **Total** | **102** |

---
