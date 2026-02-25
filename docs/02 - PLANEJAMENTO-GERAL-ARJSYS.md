<!-- markdownlint-disable-file -->
# 📋 Planejamento Geral — Sistema ERP ARJSYS

> **Documento:** Visão geral do sistema, módulos, entidades e plano de ação
> **Projeto:** TCC — Pós-graduação em Desenvolvimento Web

---

## 1. Visão Geral do Sistema

O ARJSYS é um sistema ERP voltado para indústrias de máquinas agrícolas e metalúrgicas. Ele cobre o fluxo completo desde o pedido do cliente até a entrega do produto fabricado, passando por engenharia, planejamento, compras, estoque e produção.

### Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| Frontend | React 19, TypeScript, Vite 7, TailwindCSS 4.1, TanStack Router, Zustand |
| Backend | ASP.NET Core 10, Entity Framework Core, SQLite |
| Documentação API | Swagger UI + Scalar |

---

## 2. Módulos do Sistema

O sistema é organizado por setores/módulos que representam áreas reais de uma indústria.

### 2.1 Engenharia

Responsável pela definição técnica dos produtos — o que é cada produto, como é composto e onde ficam seus documentos/desenhos.

| Entidade | Situação | Descrição |
|----------|----------|-----------|
| **Produtos** | ✅ Implementado | Cadastro de todos os itens (fabricados, comprados, matéria-prima, revenda, serviço) |
| **BOM (Estrutura)** | ✅ Implementado | Bill of Materials — define quais itens compõem um produto (relação pai/filho hierárquica) |
| **GrupoProduto** | 🔲 A fazer | Níveis de classificação do código inteligente do produto (grupo, subgrupo, família) |
| **GrupoVinculo** | 🔲 A fazer | Define quais subgrupos podem ser usados dentro de cada grupo pai |
| **Documentos/Desenhos** | 🔲 A fazer | Configuração de paths para pastas de desenhos e documentos por grupo |

### 2.2 Comercial

Responsável pelo relacionamento com o cliente — pedidos de venda e números de série.

| Entidade | Situação | Descrição |
|----------|----------|-----------|
| **PedidoVenda** | 🔲 A fazer | Pedido do cliente com código automático, status, observações do comercial |
| **PedidoVendaItem** | 🔲 A fazer | Itens do pedido (produto, quantidade, preço unitário) |
| **NumeroSerie** | 🔲 A fazer | Identificador único de fabricação, vinculado ao pedido, com formato específico da empresa |

### 2.3 Admin (Administração)

Responsável pelo cadastro de pessoas (clientes e funcionários) e controle de acesso ao sistema.

| Entidade | Situação | Descrição |
|----------|----------|-----------|
| **Pessoa** | 🔲 A fazer | Classe base com dados comuns (nome, CPF/CNPJ, telefone, email, endereço) |
| **Cliente** | 🔲 A fazer | Dados extras do cliente (razão social, CNPJ, inscrição estadual, contato comercial) |
| **Funcionario** | 🔲 A fazer | Dados extras do funcionário (cargo, setor, usuário, senha, nível de acesso) |
| **Permissões** | 🔲 A fazer | Controle de acesso por módulo (leitura, escrita, sem acesso) por funcionário |

### 2.4 PCP (Planejamento e Controle de Produção)

Responsável por transformar os pedidos em ordens de fabricação — o que comprar, o que fabricar, o que projetar.

| Entidade | Situação | Descrição |
|----------|----------|-----------|
| **NecessidadeMaterial** | 🔲 A fazer | Explosão automática da BOM: lista o que precisa comprar, fabricar e projetar |
| **OrdemProducao** | 🔲 A fazer | Ordem de fabricação (OP) com código automático e status |
| **Kanban** | 🔲 A fazer | Visualização das OPs por status (A Fazer, Fazendo, Revisão, Concluído) |

### 2.5 Compras

Responsável pela aquisição de itens necessários para a produção.

| Entidade | Situação | Descrição |
|----------|----------|-----------|
| **PedidoCompra** | 🔲 A fazer | Pedido de compra com código automático, gerado a partir da necessidade de material |
| **PedidoCompraItem** | 🔲 A fazer | Itens do pedido de compra (produto, quantidade, fornecedor) |

### 2.6 Almoxarifado

Responsável pelo controle de estoque dos produtos, principalmente os comprados.

| Entidade | Situação | Descrição |
|----------|----------|-----------|
| **Estoque** | 🔲 A fazer | Saldo por produto (calculado pela soma das movimentações) |
| **Movimentacao** | 🔲 A fazer | Registros de entrada, saída e ajuste com referência à origem (compra, produção, manual) |

---

## 3. Códigos Inteligentes de Produto

### 3.1 Formato

Os produtos seguem um código hierárquico no formato:

```
XX.YYY.ZZZZ.NNNN
│   │    │     │
│   │    │     └── Sequencial (gerado automático)
│   │    └──────── Família/Modelo (ex: GW10, BM20, 1020)
│   └───────────── Subgrupo (ex: PCP, SGE, CHP)
│
└───────────────── Grupo principal (10, 30, 70)
```

### 3.2 Exemplos

| Código | Significado |
|--------|-----------|
| `10.PCP.GW10.0001` | Grupo 10 (Produto Final), Subgrupo PCP, Família GW10, Item 0001 |
| `30.SGE.BM20.0001` | Grupo 30 (Peça Fabricada), Subgrupo SGE, Família BM20, Item 0001 |
| `70.CHP.1020.0001` | Grupo 70 (Matéria Prima), Subgrupo CHP, Família 1020, Item 0001 |

### 3.3 Cadastro de Grupos

Cada nível é cadastrado separadamente:

- **Nível 1 (Grupo):** 10, 30, 70, etc. — informando descrição e quantidade de caracteres
- **Nível 2 (Subgrupo):** PCP, SGE, CHP, etc. — cadastrado dentro de cada grupo
- **Nível 3 (Família):** GW10, BM20, 1020, etc. — cadastrado dentro de cada subgrupo
- **Nível 4 (Sequencial):** Gerado automaticamente ao cadastrar produto

### 3.4 Vínculos entre Grupos

Funciona como a BOM — uma tabela flat informando quais filhos cada pai aceita:

| Grupo Pai | Subgrupo Filho Permitido |
|-----------|-------------------------|
| 30 | SGE |
| 30 | MEC |
| 30 | CHP |
| 10 | PCP |
| 10 | GW |

Ao criar um produto, o sistema filtra os subgrupos disponíveis conforme o grupo selecionado.

---

## 4. Documentos e Desenhos

### 4.1 Onde fica

A configuração de documentos/desenhos fica no módulo **Engenharia**, vinculada ao cadastro de GrupoProduto.

### 4.2 Estrutura de Paths

Existe uma configuração global no sistema com o path raiz onde ficam os documentos. Cada grupo principal pode ter um path customizado ou usar o padrão.

**Hierarquia de resolução do path:**

1. Grupo tem path customizado? → Usa `path_custom/codigo_produto/`
2. Grupo não tem path customizado? → Usa `path_raiz/codigo_grupo/codigo_produto/`

**Exemplo prático:**

```
Path raiz global: \\servidor\CODIGOS\

Grupo 30 (sem path custom):
  → \\servidor\CODIGOS\30\30.SGE.BM20.0001\

Grupo 10 (com path custom: \\servidor\PROJETOS_FINAIS\):
  → \\servidor\PROJETOS_FINAIS\10.PCP.GW10.0001\

Grupo 70 (sem path custom):
  → \\servidor\CODIGOS\70\70.CHP.1020.0001\
```

### 4.3 No Frontend

- Botão "Abrir Documentos" no cadastro de produto
- Se a pasta existir, abre e mostra preview dos arquivos (imagens, PDFs)
- Se não existir, informa que não há documentos cadastrados
- O backend lista os arquivos da pasta e retorna pro frontend exibir

---

## 5. Códigos Automáticos (Documentos Comerciais e Produção)

Todos os documentos do sistema seguem um padrão de código automático com ano, mês e sequencial.

| Entidade | Formato | Exemplo |
|----------|---------|---------|
| Pedido de Venda | PV.AAAA.MM.NNNN | PV.2026.02.0001 |
| Pedido de Compra | PC.AAAA.MM.NNNN | PC.2026.02.0001 |
| Ordem de Produção | OP.AAAA.MM.NNNN | OP.2026.02.0001 |
| Número de Série | II.MM.AA.NNNNN | 60.07.24.02793 |

### Geração

- **Ano e mês:** Capturados automaticamente pela data de criação
- **Sequencial:** Próximo número disponível dentro do mês, incrementado automaticamente
- **Número de Série:** Formato específico da empresa — idade da empresa . mês fabricação . ano fabricação . sequencial

### Status de Documentos

Cada entidade com controle de fluxo possui status com transições válidas (controladas no backend):

| Entidade | Fluxo de Status |
|----------|----------------|
| Pedido de Venda | Orçamento → Aprovado → Em Produção → Concluído → Entregue (ou Cancelado) |
| Pedido de Compra | Aberto → Enviado → Parcial → Recebido (ou Cancelado) |
| Ordem de Produção | Aberta → Em Produção → Revisão → Concluída (ou Suspensa) |
| Número de Série | Aberto → Em Fabricação → Concluído → Entregue |

---

## 6. Fluxo Geral do Sistema

O fluxo completo de um pedido do início ao fim:

```
CLIENTE FAZ PEDIDO
        │
        ▼
[Comercial] Pedido de Venda (PV.2026.02.0001)
        │
        ▼ (aprovação)
[Comercial] Gera Número de Série (60.02.26.00001)
        │
        ▼
[PCP] Explosão da BOM — monta necessidade de material
        │
        ├──► Itens COMPRADOS → verifica estoque
        │       │
        │       ├── Tem em estoque → reserva
        │       └── Não tem → gera Pedido de Compra (PC.2026.02.0001)
        │               │
        │               ▼
        │           [Compras] Compra e recebe
        │               │
        │               ▼
        │           [Almoxarifado] Entrada no estoque
        │
        ├──► Itens FABRICADOS com BOM → gera Ordens de Produção (OP.2026.02.0001)
        │       │
        │       ├── Lista matérias-primas necessárias (KG chapa, MT tubo, etc.)
        │       └── [Produção/Kanban] A Fazer → Fazendo → Revisão → Concluído
        │
        └──► Itens FABRICADOS sem BOM → lista para desenvolvimento/projeto na Engenharia
                │
                └── Engenharia cria BOM → volta pro fluxo de fabricação
```

---

## 7. Responsabilidades Back vs Front

| Responsabilidade | Backend | Frontend |
|-----------------|---------|---------|
| Regras de negócio | ✅ Toda lógica | — |
| Validações | ✅ Ciclos, duplicados, transições de status | Validação visual básica |
| Geração de códigos | ✅ Automático | Exibe o código gerado |
| Explosão da BOM | ✅ Cálculo recursivo | Exibe o resultado |
| Cálculo de necessidade | ✅ Soma quantidades, verifica estoque | Exibe listas separadas |
| Saldo de estoque | ✅ Soma de movimentações | Exibe saldo |
| Controle de status | ✅ Valida transições | Botões de ação |
| Listagem de documentos | ✅ Lista arquivos da pasta | Preview de imagens/PDFs |
| Paginação | ✅ Fornece dados paginados | Controle de páginas |
| Filtros e ordenação | ✅ Ordenação base | Filtros visuais, reordenação por coluna |
| Kanban | ✅ Salva posição e status | Drag and drop, visual |
| Login | ✅ Autenticação, hash de senha | Tela de login |
| Permissões | ✅ Verifica acesso | Esconde/mostra menus |

---

## 8. Estrutura de Pastas do Backend

```
app/
├── Configurations/          ← configs Swagger, Scalar
├── Controllers/
│   ├── Engenharia/          ← Produtos, BOM, GrupoProduto
│   ├── Comercial/           ← PedidoVenda, NumeroSerie
│   ├── Admin/               ← Pessoa, Cliente, Funcionario, Login
│   ├── PCP/                 ← NecessidadeMaterial, OrdemProducao
│   ├── Compras/             ← PedidoCompra
│   └── Almoxarifado/        ← Estoque, Movimentacao
├── Data/
│   ├── Configurations/
│   │   ├── Engenharia/
│   │   ├── Comercial/
│   │   ├── Admin/
│   │   ├── PCP/
│   │   ├── Compras/
│   │   └── Almoxarifado/
│   └── AppDbContext.cs
├── Database/
│   └── ArjSysDB.db
├── DTOs/
│   ├── Engenharia/
│   ├── Comercial/
│   ├── Admin/
│   ├── PCP/
│   ├── Compras/
│   └── Almoxarifado/
├── Models/
│   ├── BaseEntity.cs
│   ├── Engenharia/
│   ├── Comercial/
│   ├── Admin/
│   ├── PCP/
│   ├── Compras/
│   └── Almoxarifado/
├── Services/
│   ├── Engenharia/
│   ├── Comercial/
│   ├── Admin/
│   ├── PCP/
│   ├── Compras/
│   └── Almoxarifado/
└── Program.cs
```

---

## 9. Plano de Ação — Ordem de Implementação

### Prioridade Alta (Essencial pro TCC)

| # | Módulo | O que | Depende de |
|---|--------|-------|-----------|
| 1 | Engenharia | GrupoProduto + GrupoVinculo + Paths | Produtos (já existe) |
| 2 | Admin | Pessoa + Cliente + Funcionário + Login simples | — |
| 3 | Comercial | Pedido de Venda + Itens + Número de Série | Cliente (#2) |
| 4 | Almoxarifado | Estoque + Movimentações | Produtos (já existe) |

### Prioridade Média (Diferencial)

| # | Módulo | O que | Depende de |
|---|--------|-------|-----------|
| 5 | PCP | Necessidade de Material + Ordens de Produção | BOM + Estoque (#4) |
| 6 | Compras | Pedido de Compra | Necessidade (#5) |

### Prioridade Baixa (Impressiona na apresentação)

| # | Módulo | O que | Depende de |
|---|--------|-------|-----------|
| 7 | PCP | Kanban de Produção | OPs (#5) |

### Observações sobre o prazo

- Itens 1 a 4 formam o sistema funcional mínimo
- Item 5 (PCP) é o coração do sistema e o maior diferencial acadêmico — vale investir tempo
- Itens 6 e 7 podem ser simplificados se o prazo apertar
- A integração frontend ↔ backend deve acontecer em paralelo conforme os módulos forem ficando prontos

---

## 10. Dados Existentes (Importação Futura)

Existe uma planilha Excel com números de série já cadastrados contendo: produto, cliente, cidade, etc. A importação será feita via endpoint de carga ou script após a estrutura do banco estar pronta. Não é prioridade no momento — o sistema deve estar funcional primeiro.
