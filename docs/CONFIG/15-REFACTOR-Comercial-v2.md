# 15 - REFACTOR - Comercial v2

## Resumo

Refatoração profunda do módulo Comercial para alinhar com o modelo de negócio real da indústria: **1 PV = 1 NS**, **tipo (Normal/VendaFutura) pertence ao PV**, **itens são descrição livre**, **justificativa obrigatória** em pausar/cancelar/retroceder/reabrir.

## Mudanças de conceito

- **Tipo (Normal/VendaFutura)** migrou de `NumeroSerie` para `PedidoVenda`
- **Status do NS removido** — NS herda e exibe status do PV vinculado
- **1 PV = 1 NS** garantido por unique index em `PedidoVendaId`
- **Itens do PV viraram descrição livre** — sem FK com `Produto`, sem preço
- **Justificativa obrigatória** em pausar, cancelar, retroceder e reabrir
- **Retrocesso universal** — qualquer status pode voltar pra qualquer anterior
- **Reabertura** — Cancelado pode voltar pra qualquer status (com justificativa)
- **DataEntrega** adicionada no PV (combinada na venda, opcional)
- **Data da venda** mantida (negócio, editável) — distinta de `CriadoEm` (auditoria)

## Arquivos criados / alterados

### Models
- `app/Models/Comercial/Enums/TipoPedidoVenda.cs` — NOVO
- `app/Models/Comercial/Enums/EventoPedidoVenda.cs` — adiciona `Reaberto`
- `app/Models/Comercial/Enums/StatusNumeroSerie.cs` — **EXCLUIR**
- `app/Models/Comercial/Enums/TipoNumeroSerie.cs` — **EXCLUIR**
- `app/Models/Comercial/PedidoVenda.cs` — adiciona `Tipo` e `DataEntrega`
- `app/Models/Comercial/PedidoVendaItem.cs` — `Descricao` + `Observacao` (substitui `ProdutoId` + `PrecoUnitario`)
- `app/Models/Comercial/NumeroSerie.cs` — remove `Tipo` e `Status`
- `app/Models/Comercial/PedidoVendaHistorico.cs` — adiciona `StatusAnterior`, `StatusNovo`, renomeia `Observacao` → `Justificativa`

### DTOs
- `app/DTOs/Comercial/PedidoVendaDTO.cs` — `Tipo`, `Data`, `DataEntrega`, `StatusPedidoVendaDTO` com `Justificativa`; remove `Total` do response
- `app/DTOs/Comercial/PedidoVendaItemDTO.cs` — `Descricao` + `Observacao`
- `app/DTOs/Comercial/NumeroSerieDTO.cs` — response com `pvTipo`/`pvStatus`/`pvDataEntrega`; `NumeroSerieUpdateDTO` novo
- `app/DTOs/Comercial/PedidoVendaHistoricoDTO.cs` — `StatusAnterior`, `StatusNovo`, `Justificativa`

### Configurations (EF)
- `app/Data/Configurations/Comercial/PedidoVendaConfiguration.cs` — conversão `Tipo` string
- `app/Data/Configurations/Comercial/PedidoVendaItemConfiguration.cs` — nova estrutura
- `app/Data/Configurations/Comercial/NumeroSerieConfiguration.cs` — unique index em `PedidoVendaId`
- `app/Data/Configurations/Comercial/PedidoVendaHistoricoConfiguration.cs` — conversão enums + `Justificativa`

### Services
- `app/Services/Comercial/PedidoVendaService.cs` — regras de justificativa, retrocesso universal, reabertura, mapeamento de evento
- `app/Services/Comercial/PedidoVendaItemService.cs` — descrição livre
- `app/Services/Comercial/NumeroSerieService.cs` — sem `Tipo`/`Status`, 1:1, `Update`; remove `AlterarStatus`

### Controllers
- `app/Controllers/Comercial/PedidoVendaController.cs` — mantém padrão
- `app/Controllers/Comercial/PedidoVendaItensController.cs` — mantém padrão
- `app/Controllers/Comercial/NumeroSerieController.cs` — remove `PATCH /status`, adiciona `PUT /{id}`

### Migration
- `app/Migrations/20260418000000_RefatorarComercialV2.cs` — migration consolidada

### Testes
- `app/Tests/http/10-comercial-pedidos-venda.http` — cenários de justificativa, retrocesso, reabertura
- `app/Tests/http/11-comercial-itens-pedido.http` — descrição livre
- `app/Tests/http/12-comercial-numero-serie.http` — NS 1:1, `PUT` de edição

## Regras de justificativa

A justificativa é obrigatória quando:

| Transição | Obrigatória? |
|-----------|--------------|
| Qualquer → Pausado | ✅ |
| Qualquer → Cancelado | ✅ |
| Cancelado → Qualquer (reabertura) | ✅ |
| Retrocesso (nível destino < nível origem) | ✅ |
| Avanço normal | ❌ (opcional) |

**Níveis de status** (para detectar retrocesso):
```
Cancelado       = -1 (terminal especial)
Aguardando      =  0
EmAndamento     =  1
Pausado         =  1 (mesmo nível, transição lateral)
Concluido       =  2
AguardandoEntrega = 3
Entregue        =  4
```

## Mapeamento de evento no histórico

| Transição | Evento registrado |
|-----------|-------------------|
| Criação do PV | `Criado` |
| Aguardando → EmAndamento | `Aprovado` |
| Pausado → EmAndamento | `Retomado` |
| Qualquer → Pausado | `Pausado` |
| Qualquer → Cancelado | `Cancelado` |
| Cancelado → Qualquer | `Reaberto` |
| Qualquer → Concluido | `Concluido` |
| Qualquer → AguardandoEntrega | `AguardandoEntrega` |
| Qualquer → Entregue | `Entregue` |

## Como aplicar

1. Colar todos os arquivos nos caminhos correspondentes
2. **EXCLUIR** arquivos antigos:
   - `app/Models/Comercial/Enums/StatusNumeroSerie.cs`
   - `app/Models/Comercial/Enums/TipoNumeroSerie.cs`
3. (Opcional, recomendado) Apagar a migration consolidada `20260418000000_RefatorarComercialV2.cs` e rodar:
   ```powershell
   dotnet ef migrations add RefatorarComercialV2
   ```
   Se o EF não gerar as instruções `Sql(...)` que populam `Descricao`, copiar do meu arquivo.
4. Como o banco é de dev/genérico, pode simplesmente:
   ```powershell
   # Apagar o banco e recriar do zero
   rm app/Database/ArjSysDB.db
   dotnet ef database update
   # Re-rodar os SEEDs de SQL depois
   ```
5. Validar com os arquivos `.http` 10, 11 e 12.

## Impacto no frontend

Quando for mexer no front, os pontos de atenção são:

- Form do PV ganha `tipo` (Normal/VendaFutura) e `dataEntrega`
- Grid de itens muda: `quantidade | descrição | observação` (tudo texto livre)
- Busca de produto no item do PV **some**
- Modal de mudança de status do PV ganha campo `justificativa` com validação condicional (obrigatória em pausar/cancelar/retroceder/reabrir)
- Form do NS some tipo/status; exibe tipo/status/dataEntrega do PV como readonly
- Tela de NS não tem mais ação de "alterar status"
- `GET /api/comercial/NumeroSerie/pedido/{pvId}` passa a retornar objeto ou 404 (não mais lista)
- Histórico do PV passa a ter colunas `statusAnterior → statusNovo` + `justificativa`
