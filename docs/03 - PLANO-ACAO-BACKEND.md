# 🎯 Plano de Ação — Backend ARJSYS

> **Documento:** Plano completo de implementação backend
> **Estratégia:** Itens 1 a 3 → integra front → itens 4+ um a um (back→front)

---

## Fase 1 — Engenharia: GrupoProduto (Prioridade 1)

### O que fazer

Cadastro dos níveis de código inteligente, vínculos pai→filho entre grupos, e configuração de paths para documentos/desenhos.

### Arquivos a criar

```
Models/Engenharia/
├── GrupoProduto.cs              → Entidade do grupo (código, descrição, nível, qtd caracteres, path docs)
├── GrupoVinculo.cs              → Relação pai→filho permitida entre grupos
└── Enums/
    └── NivelGrupo.cs            → Enum: Grupo, Subgrupo, Familia

DTOs/Engenharia/
├── GrupoProdutoDTO.cs           → CreateDTO e ResponseDTO do grupo
└── GrupoVinculoDTO.cs           → CreateDTO e ResponseDTO do vínculo

Data/Configurations/Engenharia/
├── GrupoProdutoConfiguration.cs → Índice único por código+nível, constraints
└── GrupoVinculoConfiguration.cs → Índice único pai+filho, FKs

Services/Engenharia/
├── GrupoProdutoService.cs       → CRUD grupos + validação de nível
└── GrupoVinculoService.cs       → CRUD vínculos + validação pai aceita filho

Controllers/Engenharia/
├── GrupoProdutoController.cs    → Endpoints CRUD grupos
└── GrupoVinculoController.cs    → Endpoints CRUD vínculos
```

### Arquivos a alterar

```
Data/AppDbContext.cs              → Adicionar DbSets de GrupoProduto e GrupoVinculo
Program.cs                        → Registrar GrupoProdutoService e GrupoVinculoService
```

### Lógica principal (Services)

- Validar que grupo pai e filho são de níveis consecutivos (Grupo→Subgrupo→Família)
- Impedir vínculo duplicado
- Montar path de documentos: path_raiz/grupo/codigo_produto ou path_custom/codigo_produto
- Endpoint para consultar path montado de um produto
- Gerar sequencial automático ao criar produto (último nível)

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| GET | /api/engenharia/grupos | Lista todos os grupos |
| GET | /api/engenharia/grupos/{id} | Busca grupo por ID |
| GET | /api/engenharia/grupos/nivel/{nivel} | Lista grupos de um nível específico |
| POST | /api/engenharia/grupos | Cria grupo |
| PUT | /api/engenharia/grupos/{id} | Atualiza grupo |
| DELETE | /api/engenharia/grupos/{id} | Remove grupo |
| GET | /api/engenharia/grupo-vinculos/pai/{paiId} | Lista filhos permitidos de um pai |
| POST | /api/engenharia/grupo-vinculos | Cria vínculo |
| DELETE | /api/engenharia/grupo-vinculos/{id} | Remove vínculo |

---

## Fase 2 — Admin: Pessoas, Clientes, Funcionários, Login (Prioridade 2)

### O que fazer

Cadastro base de pessoas, extensões para cliente e funcionário, login simples com hash de senha, e controle de permissões por módulo.

### Arquivos a criar

```
Models/Admin/
├── Pessoa.cs                    → Classe base (nome, CPF/CNPJ, telefone, email, endereço, tipo)
├── Cliente.cs                   → Dados extras: razão social, CNPJ, inscrição estadual, contato
├── Funcionario.cs               → Dados extras: cargo, setor, usuario, senhaHash, ativo
├── Permissao.cs                 → Acesso por módulo por funcionário
└── Enums/
    ├── TipoPessoa.cs            → Enum: Cliente, Funcionario, Ambos
    ├── NivelAcesso.cs           → Enum: SemAcesso, Leitura, LeituraEscrita, Admin
    └── ModuloSistema.cs         → Enum: Engenharia, Comercial, PCP, Compras, Almoxarifado, Admin

DTOs/Admin/
├── PessoaDTO.cs                 → CreateDTO e ResponseDTO base
├── ClienteDTO.cs                → CreateDTO e ResponseDTO do cliente
├── FuncionarioDTO.cs            → CreateDTO e ResponseDTO do funcionário
├── PermissaoDTO.cs              → CreateDTO e ResponseDTO de permissão
└── LoginDTO.cs                  → LoginRequestDTO (usuario+senha) e LoginResponseDTO (token/dados)

Data/Configurations/Admin/
├── PessoaConfiguration.cs       → Índice em CPF/CNPJ
├── ClienteConfiguration.cs      → FK para Pessoa
├── FuncionarioConfiguration.cs  → FK para Pessoa, índice único em usuario
└── PermissaoConfiguration.cs    → Índice único funcionário+módulo

Services/Admin/
├── PessoaService.cs             → CRUD base de pessoas
├── ClienteService.cs            → CRUD clientes (herda/usa PessoaService)
├── FuncionarioService.cs        → CRUD funcionários + hash de senha
├── PermissaoService.cs          → CRUD permissões por módulo
└── AuthService.cs               → Login: valida usuario+senha, retorna dados do funcionário

Controllers/Admin/
├── ClientesController.cs        → Endpoints CRUD clientes
├── FuncionariosController.cs    → Endpoints CRUD funcionários
├── PermissoesController.cs      → Endpoints CRUD permissões
└── AuthController.cs            → POST /api/admin/auth/login
```

### Arquivos a alterar

```
Data/AppDbContext.cs              → Adicionar DbSets
Program.cs                        → Registrar Services
```

### Lógica principal (Services)

- Pessoa base compartilhada: Cliente e Funcionário referenciam a mesma Pessoa
- Hash de senha com BCrypt ou SHA256+salt no cadastro do funcionário
- Login simples: valida usuario+senha, retorna dados do funcionário e permissões
- Permissão por módulo: cada funcionário tem um registro por módulo com nível de acesso
- Validar CPF/CNPJ único
- Impedir excluir pessoa vinculada a pedidos

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| GET | /api/admin/clientes | Lista clientes |
| GET | /api/admin/clientes/{id} | Busca cliente |
| POST | /api/admin/clientes | Cria cliente |
| PUT | /api/admin/clientes/{id} | Atualiza cliente |
| DELETE | /api/admin/clientes/{id} | Remove cliente |
| GET | /api/admin/funcionarios | Lista funcionários |
| GET | /api/admin/funcionarios/{id} | Busca funcionário |
| POST | /api/admin/funcionarios | Cria funcionário |
| PUT | /api/admin/funcionarios/{id} | Atualiza funcionário |
| DELETE | /api/admin/funcionarios/{id} | Remove funcionário |
| GET | /api/admin/permissoes/funcionario/{funcId} | Lista permissões de um funcionário |
| POST | /api/admin/permissoes | Cria/atualiza permissão |
| DELETE | /api/admin/permissoes/{id} | Remove permissão |
| POST | /api/admin/auth/login | Login (usuario+senha) |

---

## Fase 3 — Comercial: Pedido de Venda + Número de Série (Prioridade 3)

### O que fazer

Pedido de venda vinculado ao cliente, com itens, status controlado, geração de número de série ao aprovar.

### Arquivos a criar

```
Models/Comercial/
├── PedidoVenda.cs               → Código auto, clienteId, status, data, observações
├── PedidoVendaItem.cs           → PedidoId, ProdutoId, quantidade, preço unitário
├── NumeroSerie.cs               → Código formato empresa, pedidoId, status
└── Enums/
    ├── StatusPedidoVenda.cs     → Enum: Orcamento, Aprovado, EmProducao, Concluido, Entregue, Cancelado
    └── StatusNumeroSerie.cs     → Enum: Aberto, EmFabricacao, Concluido, Entregue

DTOs/Comercial/
├── PedidoVendaDTO.cs            → CreateDTO, ResponseDTO, ResumoDTO (pra listagem)
├── PedidoVendaItemDTO.cs        → CreateDTO e ResponseDTO
└── NumeroSerieDTO.cs            → CreateDTO e ResponseDTO

Data/Configurations/Comercial/
├── PedidoVendaConfiguration.cs       → Índice único em código, FK para Cliente
├── PedidoVendaItemConfiguration.cs   → FK para Pedido e Produto
└── NumeroSerieConfiguration.cs       → Índice único em código, FK para Pedido

Services/Comercial/
├── PedidoVendaService.cs        → CRUD + geração código auto + controle status + geração N.Série
├── PedidoVendaItemService.cs    → CRUD itens do pedido
└── NumeroSerieService.cs        → CRUD + geração código formato empresa + importação Excel futura

Controllers/Comercial/
├── PedidoVendaController.cs     → Endpoints CRUD + ações de status
├── PedidoVendaItensController.cs → Endpoints CRUD itens
└── NumeroSerieController.cs     → Endpoints CRUD
```

### Arquivos a alterar

```
Data/AppDbContext.cs              → Adicionar DbSets
Program.cs                        → Registrar Services
```

### Lógica principal (Services)

- Código automático: PV.AAAA.MM.NNNN — ano e mês da data, sequencial do mês
- Transição de status validada (não pode pular etapas, não pode voltar)
- Ao aprovar pedido → gera Número de Série automaticamente
- Número de Série formato: II.MM.AA.NNNNN (idade empresa.mês.ano.sequencial)
- Impedir excluir pedido com N.Série gerado
- Calcular total do pedido (soma itens × preço)

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| GET | /api/comercial/pedidos | Lista pedidos (paginável) |
| GET | /api/comercial/pedidos/{id} | Busca pedido com itens |
| POST | /api/comercial/pedidos | Cria pedido |
| PUT | /api/comercial/pedidos/{id} | Atualiza pedido |
| PATCH | /api/comercial/pedidos/{id}/status | Altera status |
| DELETE | /api/comercial/pedidos/{id} | Remove pedido |
| GET | /api/comercial/pedidos/{pedidoId}/itens | Lista itens do pedido |
| POST | /api/comercial/pedidos/{pedidoId}/itens | Adiciona item |
| PUT | /api/comercial/pedidos/{pedidoId}/itens/{id} | Atualiza item |
| DELETE | /api/comercial/pedidos/{pedidoId}/itens/{id} | Remove item |
| GET | /api/comercial/numero-serie | Lista todos N.Série (paginável) |
| GET | /api/comercial/numero-serie/{id} | Busca N.Série |
| GET | /api/comercial/numero-serie/pedido/{pedidoId} | N.Série de um pedido |

---

## ⏸️ PAUSA — Integração Frontend (Fases 1 a 3)

Após implementar as Fases 1 a 3, pausa no backend para:
- Integrar frontend React com os endpoints criados
- Criar telas de cadastro, listagem e navegação
- Testar fluxo completo: cadastrar grupo → produto → cliente → pedido → N.Série

---

## Fase 4 — Almoxarifado: Estoque + Movimentações (Prioridade 4)

### Arquivos a criar

```
Models/Almoxarifado/
├── MovimentacaoEstoque.cs       → ProdutoId, tipo (entrada/saída/ajuste), qtd, data, referência
└── Enums/
    └── TipoMovimentacao.cs      → Enum: Entrada, Saida, Ajuste

DTOs/Almoxarifado/
├── MovimentacaoDTO.cs           → CreateDTO e ResponseDTO
└── EstoqueDTO.cs                → ResponseDTO (saldo calculado por produto)

Data/Configurations/Almoxarifado/
└── MovimentacaoConfiguration.cs → FK para Produto

Services/Almoxarifado/
├── EstoqueService.cs            → Consulta saldo (soma movimentações), saldo por produto
└── MovimentacaoService.cs       → CRUD movimentações + validação (saída não pode deixar negativo)

Controllers/Almoxarifado/
├── EstoqueController.cs         → GET saldo por produto, GET saldo geral
└── MovimentacoesController.cs   → CRUD movimentações
```

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| GET | /api/almoxarifado/estoque | Saldo de todos os produtos (paginável) |
| GET | /api/almoxarifado/estoque/produto/{produtoId} | Saldo de um produto |
| GET | /api/almoxarifado/movimentacoes | Lista movimentações (paginável) |
| GET | /api/almoxarifado/movimentacoes/produto/{produtoId} | Movimentações de um produto |
| POST | /api/almoxarifado/movimentacoes | Registra movimentação |
| DELETE | /api/almoxarifado/movimentacoes/{id} | Estorna movimentação |

---

## Fase 5 — PCP: Necessidade de Material + Ordens de Produção (Prioridade 5)

### Arquivos a criar

```
Models/PCP/
├── NecessidadeMaterial.cs       → NumeroSerieId, ProdutoId, tipo (comprar/fabricar/projetar), qtd necessária, qtd estoque
├── OrdemProducao.cs             → Código auto, NumeroSerieId, ProdutoId, qtd, status, funcionarioId, data prevista
└── Enums/
    ├── TipoNecessidade.cs       → Enum: Comprar, Fabricar, Projetar
    └── StatusOrdemProducao.cs   → Enum: Aberta, EmProducao, Revisao, Concluida, Suspensa

DTOs/PCP/
├── NecessidadeMaterialDTO.cs    → ResponseDTO (gerado automaticamente, não tem Create manual)
└── OrdemProducaoDTO.cs          → CreateDTO e ResponseDTO

Data/Configurations/PCP/
├── NecessidadeMaterialConfiguration.cs → FKs
└── OrdemProducaoConfiguration.cs       → Índice único código, FKs

Services/PCP/
├── NecessidadeMaterialService.cs → Explosão da BOM recursiva, cruzamento com estoque, separação por tipo
└── OrdemProducaoService.cs       → CRUD + geração código auto + controle status + geração a partir da necessidade

Controllers/PCP/
├── NecessidadeMaterialController.cs → GET necessidades de um N.Série, POST gerar necessidades
└── OrdemProducaoController.cs       → CRUD + ações de status
```

### Lógica principal

- **Explosão da BOM:** Percorre recursivamente a estrutura do produto, acumula quantidades de cada item
- **Cruzamento com estoque:** Para cada item, consulta saldo e calcula o que falta
- **Separação:** Itens comprados → lista de compra. Fabricados com BOM → lista de fabricação + matérias-primas calculadas. Fabricados sem BOM → lista de desenvolvimento
- **Cálculo de matéria-prima:** Soma quantidades por item base (ex: 45 KG de chapa X, 12 MT de tubo Y)
- **Geração de OPs:** Cada item fabricado vira uma OP com código automático

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| POST | /api/pcp/necessidades/gerar/{numeroSerieId} | Explode BOM e gera necessidades |
| GET | /api/pcp/necessidades/numero-serie/{nsId} | Lista necessidades de um N.Série |
| GET | /api/pcp/necessidades/numero-serie/{nsId}/comprar | Só itens a comprar |
| GET | /api/pcp/necessidades/numero-serie/{nsId}/fabricar | Só itens a fabricar |
| GET | /api/pcp/necessidades/numero-serie/{nsId}/projetar | Só itens sem BOM |
| GET | /api/pcp/necessidades/numero-serie/{nsId}/materias-primas | Cálculo de MP acumulada |
| GET | /api/pcp/ordens | Lista OPs (paginável) |
| GET | /api/pcp/ordens/{id} | Busca OP |
| POST | /api/pcp/ordens/gerar/{numeroSerieId} | Gera OPs a partir da necessidade |
| PATCH | /api/pcp/ordens/{id}/status | Altera status da OP |

---

## Fase 6 — Compras: Pedido de Compra (Prioridade 6)

### Arquivos a criar

```
Models/Compras/
├── PedidoCompra.cs              → Código auto, status, data, observações
├── PedidoCompraItem.cs          → PedidoCompraId, ProdutoId, quantidade
└── Enums/
    └── StatusPedidoCompra.cs    → Enum: Aberto, Enviado, Parcial, Recebido, Cancelado

DTOs/Compras/
├── PedidoCompraDTO.cs           → CreateDTO e ResponseDTO
└── PedidoCompraItemDTO.cs       → CreateDTO e ResponseDTO

Data/Configurations/Compras/
├── PedidoCompraConfiguration.cs      → Índice único código
└── PedidoCompraItemConfiguration.cs  → FKs

Services/Compras/
├── PedidoCompraService.cs       → CRUD + código auto + status + gerar a partir da necessidade
└── PedidoCompraItemService.cs   → CRUD itens

Controllers/Compras/
├── PedidoCompraController.cs    → CRUD + status
└── PedidoCompraItensController.cs → CRUD itens
```

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| GET | /api/compras/pedidos | Lista pedidos de compra (paginável) |
| GET | /api/compras/pedidos/{id} | Busca pedido |
| POST | /api/compras/pedidos | Cria pedido |
| POST | /api/compras/pedidos/gerar/{numeroSerieId} | Gera PC a partir da necessidade |
| PATCH | /api/compras/pedidos/{id}/status | Altera status |
| POST | /api/compras/pedidos/{id}/receber | Recebe itens → entrada no estoque |

---

## Fase 7 — Produção: Kanban (Prioridade 7)

### O que fazer

Não é uma entidade nova — é uma **visualização** das Ordens de Produção por status. O backend já tem tudo na Fase 5.

### Arquivos a criar

```
Controllers/PCP/
└── KanbanController.cs          → Endpoints específicos para a visualização Kanban

Services/PCP/
└── KanbanService.cs             → Agrupa OPs por status, permite mover entre colunas
```

### Endpoints

| Método | Rota | Ação |
|--------|------|------|
| GET | /api/pcp/kanban | OPs agrupadas por status (colunas do Kanban) |
| GET | /api/pcp/kanban/numero-serie/{nsId} | Kanban filtrado por N.Série |
| PATCH | /api/pcp/kanban/{opId}/mover | Move OP para outro status |

### Observação

O grosso do Kanban é frontend (drag and drop, colunas visuais, cards). O backend só fornece dados agrupados e salva mudanças de status.

---

## Resumo de Arquivos por Fase

| Fase | Models | DTOs | Configs | Services | Controllers | Total |
|------|--------|------|---------|----------|-------------|-------|
| 1 - Engenharia | 3 | 2 | 2 | 2 | 2 | **11** |
| 2 - Admin | 5 | 5 | 4 | 5 | 4 | **23** |
| 3 - Comercial | 4 | 3 | 3 | 3 | 3 | **16** |
| 4 - Almoxarifado | 2 | 2 | 1 | 2 | 2 | **9** |
| 5 - PCP | 3 | 2 | 2 | 2 | 2 | **11** |
| 6 - Compras | 3 | 2 | 2 | 2 | 2 | **11** |
| 7 - Kanban | 0 | 0 | 0 | 1 | 1 | **2** |
| **Total** | **20** | **16** | **14** | **17** | **16** | **83** |

---

## Migrations

Cada fase gera uma migration nova. Não é necessário apagar tudo a cada vez — a partir de agora usamos migrations incrementais:

| Fase | Migration |
|------|-----------|
| 1 | `AdicionarGrupoProdutoEVinculos` |
| 2 | `AdicionarPessoasClientesFuncionarios` |
| 3 | `AdicionarPedidoVendaNumeroSerie` |
| 4 | `AdicionarEstoqueMovimentacoes` |
| 5 | `AdicionarNecessidadeOrdemProducao` |
| 6 | `AdicionarPedidoCompra` |
| 7 | Sem migration (usa OPs existentes) |
