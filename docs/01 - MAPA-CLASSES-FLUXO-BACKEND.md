<!-- markdownlint-disable-file -->
# 🗺️ Mapa de Classes e Fluxo — Backend ARJSYS

> **Documento:** Relacionamento entre todas as classes, fluxo de dados e ordem de criação

---

## 1. Fluxo Geral de uma Requisição

Toda requisição HTTP segue o mesmo caminho:

```
[Cliente/Frontend]
        │
        ▼
  Controller          ← Recebe a requisição HTTP, valida formato, devolve resposta
        │
        ▼
    Service            ← Lógica de negócio, validações, cálculos
        │
        ▼
   AppDbContext         ← Acessa o banco de dados via Entity Framework
        │
        ▼
     Model             ← Entidade mapeada para tabela no banco
```

**DTOs** entram e saem no Controller:
- **CreateDTO** → Controller recebe do frontend → passa pro Service → Service converte em Model → salva no banco
- **ResponseDTO** → Service lê Model do banco → converte em ResponseDTO → Controller devolve pro frontend

**Configuration** define as regras do banco (índices, FKs, constraints) para cada Model.

---

## 2. Ordem de Criação ao Implementar Algo Novo

Sempre nesta sequência:

```
1º  Model          → Define a entidade (campos, tipos)
2º  Enum           → Se o Model usa enums, cria junto
3º  DTO            → Define o que entra (Create) e o que sai (Response)
4º  Configuration  → Define regras do banco (índices, FKs, conversões)
5º  Service        → Implementa lógica de negócio
6º  Controller     → Expõe os endpoints HTTP
7º  AppDbContext    → Adiciona DbSet
8º  Program.cs     → Registra Service no AddScoped
9º  Migration      → Gera e aplica no banco
```

---

## 3. Mapa Completo por Módulo

### 3.1 ENGENHARIA — Produtos e BOM (✅ Implementado)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
Produto.cs ◄──────────────────── ProdutoDTO.cs                      ProdutoConfiguration.cs            ProdutoService.cs ◄──────────────── ProdutosController.cs
│  Codigo                          ├── ProdutoCreateDTO                │  Índice único: Codigo            │  GetAll()                         │  GET    /api/engenharia/produtos
│  Descricao                       │   Codigo                          │  MaxLength: Codigo(50)           │  GetById()                        │  GET    /api/engenharia/produtos/{id}
│  DescricaoCompleta               │   Descricao                       │  MaxLength: Descricao(140)       │  Create()                         │  POST   /api/engenharia/produtos
│  Unidade (enum)                  │   DescricaoCompleta               │  Conversão: Unidade→string       │  Update()                         │  PUT    /api/engenharia/produtos/{id}
│  Tipo (enum)                     │   Unidade                         │  Conversão: Tipo→string          │  Delete()                         │  DELETE /api/engenharia/produtos/{id}
│  Peso                            │   Tipo                            │                                  │  ToResponseDTO()                  │
│  Ativo                           │   Peso                            │                                  │                                   │
│  (herda BaseEntity)              │   Ativo                           │                                  │                                   │
│                                  └── ProdutoResponseDTO              │                                  │                                   │
│                                      Id, Codigo, Descricao...        │                                  │                                   │
│                                      CriadoEm, ModificadoEm          │                                  │                                   │
│                                                                      │                                  │                                   │
├─┐                                                                    │                                  │                                   │
│ │ (Produto é referenciado por)                                       │                                  │                                   │
│ ▼                                                                    │                                  │                                   │
EstruturaProduto.cs ◄──────────── EstruturaProdutoDTO.cs             EstruturaProdutoConfiguration.cs   BomService.cs ◄────────────────── BomController.cs
│  ProdutoPaiId ──► Produto        ├── EstruturaProdutoCreateDTO       │  Índice único: Pai+Filho         │  GetByProdutoId()                 │  GET    /api/engenharia/bom/produto/{id}
│  ProdutoFilhoId ──► Produto      │   ProdutoPaiId                    │  FK: ProdutoPai → Restrict       │  GetById()                        │  GET    /api/engenharia/bom/{id}
│  Quantidade                      │   ProdutoFilhoId                  │  FK: ProdutoFilho → Restrict     │  GetAllFlat()                     │  GET    /api/engenharia/bom/flat
│  Posicao                         │   Quantidade                      │                                  │  GetProdutosComEstrutura()        │  GET    /api/engenharia/bom
│  Observacao                      │   Posicao                         │                                  │  Create()                         │  POST   /api/engenharia/bom
│  (herda BaseEntity)              │   Observacao                      │                                  │  Update()                         │  PUT    /api/engenharia/bom/{id}
│                                  ├── EstruturaProdutoResponseDTO     │                                  │  Delete()                         │  DELETE /api/engenharia/bom/{id}
│                                  │   + ProdutoFilhoCodigo            │                                  │  VerificarCiclo() [privado]       │
│                                  │   + ProdutoFilhoDescricao         │                                  │  CalcularProximaPosicao() [priv]  │
│                                  └── EstruturaProdutoFlatDTO         │                                  │  ToResponseDTO()                  │
│                                      + ProdutoPaiCodigo/Descricao    │                                  │                                   │
│                                      + ProdutoFilhoCodigo/Desc/Un    │                                  │                                   │
│                                                                      │                                  │                                   │
Enums/                                                                 │                                  │                                   │
├── UnidadeMedida.cs                                                   │                                  │                                   │
│   UN, PC, CJ, KG, KT, MT...                                          │                                  │                                   │
└── TipoProduto.cs                                                     │                                  │                                   │
    Fabricado, Comprado, MateriaPrima, Revenda, Servico                │                                  │                                   │
```

### 3.2 ENGENHARIA — GrupoProduto (🔲 A fazer)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
GrupoProduto.cs ◄─────────────── GrupoProdutoDTO.cs                  GrupoProdutoConfiguration.cs       GrupoProdutoService.cs ◄────────── GrupoProdutoController.cs
│  Codigo                          ├── GrupoProdutoCreateDTO           │  Índice único: Codigo+Nivel      │  GetAll()                         │  GET    /api/engenharia/grupos
│  Descricao                       │   Codigo                          │  MaxLengths                      │  GetByNivel()                     │  GET    /api/engenharia/grupos/nivel/{n}
│  Nivel (enum)                    │   Descricao                       │  Conversão: Nivel→string         │  GetById()                        │  GET    /api/engenharia/grupos/{id}
│  QtdCaracteres                   │   Nivel                           │                                  │  Create()                         │  POST   /api/engenharia/grupos
│  PathDocumentos                  │   QtdCaracteres                   │                                  │  Update()                         │  PUT    /api/engenharia/grupos/{id}
│  (herda BaseEntity)              │   PathDocumentos                  │                                  │  Delete()                         │  DELETE /api/engenharia/grupos/{id}
│                                  └── GrupoProdutoResponseDTO         │                                  │  MontarPathDocumento()            │
│                                      Id, Codigo, Descricao...        │                                  │                                   │
│                                                                      │                                  │                                   │
├─┐                                                                    │                                  │                                   │
│ │ (GrupoProduto é referenciado por)                                  │                                  │                                   │
│ ▼                                                                    │                                  │                                   │
GrupoVinculo.cs ◄─────────────── GrupoVinculoDTO.cs                 GrupoVinculoConfiguration.cs       GrupoVinculoService.cs ◄────────── GrupoVinculoController.cs
│  GrupoPaiId ──► GrupoProduto     ├── GrupoVinculoCreateDTO           │  Índice único: Pai+Filho         │  GetByPaiId()                     │  GET    /api/engenharia/grupo-vinculos/pai/{id}
│  GrupoFilhoId ──► GrupoProduto   │   GrupoPaiId                      │  FK: GrupoPai → Restrict         │  Create()                         │  POST   /api/engenharia/grupo-vinculos
│  (herda BaseEntity)              │   GrupoFilhoId                    │  FK: GrupoFilho → Restrict       │  Delete()                         │  DELETE /api/engenharia/grupo-vinculos/{id}
│                                  └── GrupoVinculoResponseDTO         │  Validação: níveis consecutivos  │  ValidarNiveis()                  │
│                                      + GrupoPaiCodigo/Descricao      │                                  │                                   │
│                                      + GrupoFilhoCodigo/Descricao    │                                  │                                   │
│                                                                      │                                  │                                   │
Enums/                                                                 │                                  │                                   │
└── NivelGrupo.cs                                                      │                                  │                                   │
    Grupo, Subgrupo, Familia                                           │                                  │                                   │
```

### 3.3 ADMIN — Pessoas, Clientes, Funcionários (🔲 A fazer)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
Pessoa.cs (base)                   PessoaDTO.cs                       PessoaConfiguration.cs             PessoaService.cs                   (não tem controller próprio)
│  Nome                            │  Dados comuns                     │  Índice: CpfCnpj                 │  CRUD base                        │
│  CpfCnpj                         │                                   │                                  │                                   │
│  Telefone                        │                                   │                                  │                                   │
│  Email                           │                                   │                                  │                                   │
│  Endereco                        │                                   │                                  │                                   │
│  Tipo (enum)                     │                                   │                                  │                                   │
│  (herda BaseEntity)              │                                   │                                  │                                   │
│                                  │                                   │                                  │                                   │
├─┐                                │                                   │                                  │                                   │
│ │ (Pessoa é estendida por)       │                                   │                                  │                                   │
│ ▼                                │                                   │                                  │                                   │
Cliente.cs ◄──────────────────── ClienteDTO.cs                       ClienteConfiguration.cs            ClienteService.cs ◄────────────── ClientesController.cs
│  PessoaId ──► Pessoa             ├── ClienteCreateDTO                │  FK: Pessoa                      │  GetAll()                         │  GET    /api/admin/clientes
│  RazaoSocial                     │   Nome, CpfCnpj...                │                                  │  GetById()                        │  GET    /api/admin/clientes/{id}
│  InscricaoEstadual               │   RazaoSocial                     │                                  │  Create() → cria Pessoa + Cliente │  POST   /api/admin/clientes
│  ContatoComercial                │   InscricaoEstadual               │                                  │  Update()                         │  PUT    /api/admin/clientes/{id}
│  (herda BaseEntity)              └── ClienteResponseDTO              │                                  │  Delete()                         │  DELETE /api/admin/clientes/{id}
│                                      Id, Nome, RazaoSocial...        │                                  │                                   │
│                                                                      │                                  │                                   │
Funcionario.cs ◄─────────────── FuncionarioDTO.cs                    FuncionarioConfiguration.cs        FuncionarioService.cs ◄──────────── FuncionariosController.cs
│  PessoaId ──► Pessoa             ├── FuncionarioCreateDTO            │  FK: Pessoa                      │  GetAll()                         │  GET    /api/admin/funcionarios
│  Cargo                           │   Nome, CpfCnpj...                │  Índice único: Usuario           │  GetById()                        │  GET    /api/admin/funcionarios/{id}
│  Setor                           │   Cargo, Setor                    │                                  │  Create() → cria Pessoa + Func   │  POST   /api/admin/funcionarios
│  Usuario                         │   Usuario, Senha                  │                                  │  Update()                         │  PUT    /api/admin/funcionarios/{id}
│  SenhaHash                       └── FuncionarioResponseDTO          │                                  │  Delete()                         │  DELETE /api/admin/funcionarios/{id}
│  (herda BaseEntity)                  Id, Nome, Cargo, Usuario...     │                                  │  HashSenha() [privado]            │
│                                      (nunca retorna senha!)          │                                  │                                   │
│                                                                      │                                  │                                   │
├─┐                                                                    │                                  │                                   │
│ ▼                                                                    │                                  │                                   │
Permissao.cs ◄───────────────── PermissaoDTO.cs                      PermissaoConfiguration.cs          PermissaoService.cs ◄──────────── PermissoesController.cs
│  FuncionarioId ──► Funcionario   ├── PermissaoCreateDTO              │  Índice único: Func+Modulo       │  GetByFuncionarioId()             │  GET    /api/admin/permissoes/func/{id}
│  Modulo (enum)                   │   FuncionarioId                   │  Conversões: enums→string        │  Create()                         │  POST   /api/admin/permissoes
│  Nivel (enum)                    │   Modulo                          │                                  │  Delete()                         │  DELETE /api/admin/permissoes/{id}
│  (herda BaseEntity)              │   Nivel                           │                                  │  VerificarPermissao()             │
│                                  └── PermissaoResponseDTO            │                                  │                                   │
│                                                                      │                                  │                                   │
(sem Model próprio)                LoginDTO.cs                        (sem Config)                       AuthService.cs ◄───────────────── AuthController.cs
                                   ├── LoginRequestDTO                                                    │  Login()                          │  POST   /api/admin/auth/login
                                   │   Usuario                                                            │  ValidarSenha() [privado]         │
                                   │   Senha                                                              │                                   │
                                   └── LoginResponseDTO                                                   │                                   │
                                       FuncionarioId, Nome, Permissoes                                    │                                   │
                                                                                                          │                                   │
Enums/                                                                                                    │                                   │
├── TipoPessoa.cs                                                                                         │                                   │
│   Cliente, Funcionario, Ambos                                                                           │                                   │
├── NivelAcesso.cs                                                                                        │                                   │
│   SemAcesso, Leitura, LeituraEscrita, Admin                                                             │                                   │
└── ModuloSistema.cs                                                                                      │                                   │
    Engenharia, Comercial, PCP, Compras, Almoxarifado, Admin                                              │                                   │
```

### 3.4 COMERCIAL — Pedidos e Número de Série (🔲 A fazer)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
PedidoVenda.cs ◄─────────────── PedidoVendaDTO.cs                   PedidoVendaConfiguration.cs        PedidoVendaService.cs ◄──────────── PedidoVendaController.cs
│  Codigo (auto)                   ├── PedidoVendaCreateDTO            │  Índice único: Codigo            │  GetAll()                         │  GET    /api/comercial/pedidos
│  ClienteId ──► Cliente           │   ClienteId                       │  FK: Cliente                     │  GetById()                        │  GET    /api/comercial/pedidos/{id}
│  Status (enum)                   │   Observacoes                     │  Conversão: Status→string        │  Create() → gera código auto      │  POST   /api/comercial/pedidos
│  Data                            └── PedidoVendaResponseDTO          │                                  │  Update()                         │  PUT    /api/comercial/pedidos/{id}
│  Observacoes                         Id, Codigo, ClienteNome...      │                                  │  AlterarStatus()                  │  PATCH  /api/comercial/pedidos/{id}/status
│  (herda BaseEntity)                  Status, Data, Total             │                                  │  Delete()                         │  DELETE /api/comercial/pedidos/{id}
│                                                                      │                                  │  GerarCodigo() [privado]          │
│                                                                      │                                  │  GerarNumeroSerie() [privado]     │
├─┐                                                                    │                                  │                                   │
│ ▼                                                                    │                                  │                                   │
PedidoVendaItem.cs ◄─────────── PedidoVendaItemDTO.cs               PedidoVendaItemConfiguration.cs    PedidoVendaItemService.cs ◄──────── PedidoVendaItensController.cs
│  PedidoVendaId ──► PedidoVenda   ├── PedidoVendaItemCreateDTO        │  FK: PedidoVenda                 │  GetByPedidoId()                  │  GET    /api/comercial/pedidos/{id}/itens
│  ProdutoId ──► Produto           │   ProdutoId                       │  FK: Produto                     │  Create()                         │  POST   /api/comercial/pedidos/{id}/itens
│  Quantidade                      │   Quantidade                      │                                  │  Update()                         │  PUT    /api/comercial/pedidos/{id}/itens/{id}
│  PrecoUnitario                   │   PrecoUnitario                   │                                  │  Delete()                         │  DELETE /api/comercial/pedidos/{id}/itens/{id}
│  (herda BaseEntity)              └── PedidoVendaItemResponseDTO      │                                  │                                   │
│                                      + ProdutoCodigo/Descricao       │                                  │                                   │
│                                                                      │                                  │                                   │
NumeroSerie.cs ◄─────────────── NumeroSerieDTO.cs                    NumeroSerieConfiguration.cs        NumeroSerieService.cs ◄──────────── NumeroSerieController.cs
│  Codigo (auto formato empresa)   ├── NumeroSerieCreateDTO            │  Índice único: Codigo            │  GetAll()                         │  GET    /api/comercial/numero-serie
│  PedidoVendaId ──► PedidoVenda   │   PedidoVendaId                   │  FK: PedidoVenda                 │  GetById()                        │  GET    /api/comercial/numero-serie/{id}
│  Status (enum)                   └── NumeroSerieResponseDTO          │  Conversão: Status→string        │  GetByPedidoId()                  │  GET    /api/comercial/numero-serie/pedido/{id}
│  (herda BaseEntity)                  Id, Codigo, PedidoId, Status    │                                  │  Create() → gera código           │  POST   /api/comercial/numero-serie
│                                                                      │                                  │  AlterarStatus()                  │  PATCH  /api/comercial/numero-serie/{id}/status
│                                                                      │                                  │  GerarCodigo() [privado]          │
Enums/                                                                 │                                  │                                   │
├── StatusPedidoVenda.cs                                               │                                  │                                   │
│   Orcamento, Aprovado, EmProducao, Concluido, Entregue, Cancelado    │                                  │                                   │
└── StatusNumeroSerie.cs                                               │                                  │                                   │
    Aberto, EmFabricacao, Concluido, Entregue                          │                                  │                                   │
```

### 3.5 ALMOXARIFADO — Estoque (🔲 A fazer)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
MovimentacaoEstoque.cs ◄───────── MovimentacaoDTO.cs                 MovimentacaoConfiguration.cs       MovimentacaoService.cs ◄────────── MovimentacoesController.cs
│  ProdutoId ──► Produto           ├── MovimentacaoCreateDTO           │  FK: Produto                     │  GetByProdutoId()                 │  GET    /api/almoxarifado/movimentacoes/prod/{id}
│  Tipo (enum)                     │   ProdutoId                       │  Conversão: Tipo→string          │  Create()                         │  POST   /api/almoxarifado/movimentacoes
│  Quantidade                      │   Tipo                            │                                  │  Estornar()                       │  DELETE /api/almoxarifado/movimentacoes/{id}
│  Data                            │   Quantidade                      │                                  │                                   │
│  Referencia                      │   Referencia                      │                                  │                                   │
│  (herda BaseEntity)              └── MovimentacaoResponseDTO         │                                  │                                   │
│                                                                      │                                  │                                   │
(sem Model próprio — saldo é      EstoqueDTO.cs                       (sem Config)                       EstoqueService.cs ◄────────────── EstoqueController.cs
 calculado pela soma das           └── EstoqueResponseDTO                                                 │  GetSaldoTodos()                  │  GET    /api/almoxarifado/estoque
 movimentações)                        ProdutoId, Codigo, Descricao                                       │  GetSaldoProduto()                │  GET    /api/almoxarifado/estoque/prod/{id}
                                       Unidade, SaldoAtual                                                │                                   │
                                                                                                          │                                   │
Enums/                                                                                                    │                                   │
└── TipoMovimentacao.cs                                                                                   │                                   │
    Entrada, Saida, Ajuste                                                                                │                                   │
```

### 3.6 PCP — Necessidade de Material + Ordens de Produção (🔲 A fazer)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
NecessidadeMaterial.cs ◄─────── NecessidadeMaterialDTO.cs             NecessidadeMaterialConfig.cs       NecessidadeMaterialService.cs ◄── NecessidadeMaterialController.cs
│  NumeroSerieId ──► NumeroSerie   └── NecessidadeMaterialResponseDTO  │  FK: NumeroSerie                 │  Gerar() → explode BOM            │  POST   /api/pcp/necessidades/gerar/{nsId}
│  ProdutoId ──► Produto               ProdutoId, Codigo, Descricao    │  FK: Produto                     │  GetByNumeroSerieId()             │  GET    /api/pcp/necessidades/ns/{nsId}
│  Tipo (enum)                         Tipo, QtdNecessaria             │  Conversão: Tipo→string          │  GetComprar()                     │  GET    /api/pcp/necessidades/ns/{nsId}/comprar
│  QtdNecessaria                       QtdEstoque, QtdFaltante         │                                  │  GetFabricar()                    │  GET    /api/pcp/necessidades/ns/{nsId}/fabricar
│  (herda BaseEntity)                                                  │                                  │  GetProjetar()                    │  GET    /api/pcp/necessidades/ns/{nsId}/projetar
│                                                                      │                                  │  GetMateriasPrimas()              │  GET    /api/pcp/necessidades/ns/{nsId}/mp
│                                                                      │                                  │  ExplodirBom() [privado]          │
│                                                                      │                                  │                                   │
OrdemProducao.cs ◄───────────── OrdemProducaoDTO.cs                   OrdemProducaoConfiguration.cs      OrdemProducaoService.cs ◄────────── OrdemProducaoController.cs
│  Codigo (auto)                   ├── OrdemProducaoCreateDTO          │  Índice único: Codigo            │  GetAll()                         │  GET    /api/pcp/ordens
│  NumeroSerieId ──► NumeroSerie   │   NumeroSerieId                   │  FK: NumeroSerie                 │  GetById()                        │  GET    /api/pcp/ordens/{id}
│  ProdutoId ──► Produto           │   ProdutoId                       │  FK: Produto                     │  GerarPorNumeroSerie()            │  POST   /api/pcp/ordens/gerar/{nsId}
│  Quantidade                      │   Quantidade                      │  FK: Funcionario                 │  AlterarStatus()                  │  PATCH  /api/pcp/ordens/{id}/status
│  Status (enum)                   │   FuncionarioId                   │  Conversão: Status→string        │  GerarCodigo() [privado]          │
│  FuncionarioId ──► Funcionario   │   DataPrevista                    │                                  │                                   │
│  DataPrevista                    └── OrdemProducaoResponseDTO        │                                  │                                   │
│  (herda BaseEntity)                  Id, Codigo, Produto, Status...  │                                  │                                   │
│                                                                      │                                  │                                   │
Enums/                                                                 │                                  │                                   │
├── TipoNecessidade.cs                                                 │                                  │                                   │
│   Comprar, Fabricar, Projetar                                        │                                  │                                   │
└── StatusOrdemProducao.cs                                             │                                  │                                   │
    Aberta, EmProducao, Revisao, Concluida, Suspensa                   │                                  │                                   │
```

### 3.7 COMPRAS — Pedido de Compra (🔲 A fazer)

```
Model                              DTO                                 Config                             Service                            Controller
─────                              ───                                 ──────                             ───────                            ──────────
PedidoCompra.cs ◄────────────── PedidoCompraDTO.cs                   PedidoCompraConfiguration.cs       PedidoCompraService.cs ◄────────── PedidoCompraController.cs
│  Codigo (auto)                   ├── PedidoCompraCreateDTO           │  Índice único: Codigo            │  GetAll()                         │  GET    /api/compras/pedidos
│  Status (enum)                   │   Observacoes                     │  Conversão: Status→string        │  GetById()                        │  GET    /api/compras/pedidos/{id}
│  Data                            └── PedidoCompraResponseDTO         │                                  │  Create()                         │  POST   /api/compras/pedidos
│  Observacoes                         Id, Codigo, Status, Itens...    │                                  │  GerarPorNecessidade()            │  POST   /api/compras/pedidos/gerar/{nsId}
│  (herda BaseEntity)                                                  │                                  │  AlterarStatus()                  │  PATCH  /api/compras/pedidos/{id}/status
│                                                                      │                                  │  Receber() → mov. estoque         │  POST   /api/compras/pedidos/{id}/receber
├─┐                                                                    │                                  │                                   │
│ ▼                                                                    │                                  │                                   │
PedidoCompraItem.cs ◄──────────── PedidoCompraItemDTO.cs             PedidoCompraItemConfiguration.cs   PedidoCompraItemService.cs ◄────── PedidoCompraItensController.cs
│  PedidoCompraId ──► PedidoCompra ├── PedidoCompraItemCreateDTO       │  FK: PedidoCompra                │  GetByPedidoId()                  │  GET    /api/compras/pedidos/{id}/itens
│  ProdutoId ──► Produto           │   ProdutoId                       │  FK: Produto                     │  Create()                         │  POST   /api/compras/pedidos/{id}/itens
│  Quantidade                      │   Quantidade                      │                                  │  Update()                         │  PUT    /api/compras/pedidos/{id}/itens/{id}
│  (herda BaseEntity)              └── PedidoCompraItemResponseDTO     │                                  │  Delete()                         │  DELETE /api/compras/pedidos/{id}/itens/{id}
│                                      + ProdutoCodigo/Descricao       │                                  │                                   │
│                                                                      │                                  │                                   │
Enums/                                                                 │                                  │                                   │
└── StatusPedidoCompra.cs                                              │                                  │                                   │
    Aberto, Enviado, Parcial, Recebido, Cancelado                      │                                  │                                   │
```

### 3.8 PCP — Kanban (🔲 A fazer)

```
(Sem Models novos — usa OrdemProducao existente)

Service                            Controller
───────                            ──────────
KanbanService.cs ◄────────────── KanbanController.cs
│  GetKanban()                      │  GET    /api/pcp/kanban
│  GetByNumeroSerie()               │  GET    /api/pcp/kanban/ns/{nsId}
│  MoverCard()                      │  PATCH  /api/pcp/kanban/{opId}/mover
│  AgruparPorStatus() [privado]     │
```

---

## 4. Classes Compartilhadas

```
Models/
└── BaseEntity.cs                  → Id, CriadoEm, ModificadoEm, CriadoPor, ModificadoPor
                                     (TODAS as entidades herdam desta)

Data/
└── AppDbContext.cs                 → DbSets de TODAS as entidades
                                     ApplyConfigurationsFromAssembly (carrega todas as Configs)

Configurations/
├── SwaggerConfig.cs               → Configuração do Swagger UI
└── ScalarConfig.cs                → Configuração do Scalar

Controllers/
└── StatusController.cs            → Health check da API

Program.cs                         → Registra TODOS os Services no AddScoped
                                     Configura middleware (CORS, Auth, Swagger, Scalar)
```

---

## 5. Referências entre Módulos

```
Engenharia                 Admin                    Comercial                PCP                      Compras          Almoxarifado
──────────                 ─────                    ─────────                ───                      ───────          ────────────
Produto ◄─────────────────────────────────────────── PedidoVendaItem         NecessidadeMaterial       PedidoCompraItem  MovimentacaoEstoque
    │                                                     │                       │                        │                 │
    ├── EstruturaProduto                                  │                       │                        │                 │
    │                      Cliente ◄──────────────── PedidoVenda                  │                        │                 │
    ├── GrupoProduto                                      │                       │                        │                 │
    │                                                     ▼                       │                        │                 │
    │                                                NumeroSerie ◄─────────── NecessidadeMaterial          │                 │
    │                                                     │                   OrdemProducao                │                 │
    │                      Funcionario ◄──────────────────┼───────────────── OrdemProducao                 │                 │
    │                                                     │                       │                        │                 │
    │                                                     │                       ▼                        │                 │
    │                                                     │               (explode BOM do Produto)         │                 │
    │                                                     │                       │                        │                 │
    │                                                     │                       ├── tipo Comprar ────► PedidoCompra        │
    │                                                     │                       │                        │                 │
    │                                                     │                       │                        └── Receber ────► Movimentacao
    │                                                     │                       │                                          │
    │                                                     │                       └── tipo Fabricar ─► OrdemProducao         │
    │                                                     │                                               │                  │
    │                                                     │                                               └── Concluir ───► Movimentacao
```

---

## 6. Resumo — Checklist de Implementação

### ✅ Já implementado
- [x] BaseEntity
- [x] Produto + ProdutoDTO + ProdutoConfiguration + ProdutoService + ProdutosController
- [x] EstruturaProduto + DTOs + Configuration + BomService + BomController
- [x] Enums: UnidadeMedida, TipoProduto
- [x] AppDbContext com ApplyConfigurationsFromAssembly
- [x] SwaggerConfig + ScalarConfig
- [x] StatusController

### 🔲 Fase 1 — Engenharia: GrupoProduto
- [ ] GrupoProduto + NivelGrupo enum
- [ ] GrupoVinculo
- [ ] DTOs, Configs, Services, Controllers

### 🔲 Fase 2 — Admin: Pessoas
- [ ] Pessoa + TipoPessoa enum
- [ ] Cliente
- [ ] Funcionario + NivelAcesso + ModuloSistema enums
- [ ] Permissao
- [ ] AuthService + LoginDTO
- [ ] DTOs, Configs, Services, Controllers

### 🔲 Fase 3 — Comercial: Pedidos
- [ ] PedidoVenda + StatusPedidoVenda enum
- [ ] PedidoVendaItem
- [ ] NumeroSerie + StatusNumeroSerie enum
- [ ] DTOs, Configs, Services, Controllers

### ⏸️ PAUSA — Integração Frontend

### 🔲 Fase 4 — Almoxarifado
- [ ] MovimentacaoEstoque + TipoMovimentacao enum
- [ ] EstoqueService (saldo calculado)
- [ ] DTOs, Configs, Services, Controllers

### 🔲 Fase 5 — PCP
- [ ] NecessidadeMaterial + TipoNecessidade enum
- [ ] OrdemProducao + StatusOrdemProducao enum
- [ ] DTOs, Configs, Services, Controllers

### 🔲 Fase 6 — Compras
- [ ] PedidoCompra + StatusPedidoCompra enum
- [ ] PedidoCompraItem
- [ ] DTOs, Configs, Services, Controllers

### 🔲 Fase 7 — Kanban
- [ ] KanbanService + KanbanController (usa OrdemProducao)
