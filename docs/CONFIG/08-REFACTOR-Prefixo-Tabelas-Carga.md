# ♻️ REFACTOR: Prefixo tabelas, ajuste grupos de código e carga inicial

## 💡 Tags nos Controllers (Swagger/Scalar)

Os nomes das tabelas no banco (`Engenharia_Produtos`) não aparecem no Swagger — ele mostra o nome do Controller/DTO. Para organizar visualmente os endpoints por módulo, adicionado o atributo `[Tags]` em cada controller:

| Controller | Tag exibida |
|-----------|-------------|
| ProdutosController | `Engenharia - Produtos` |
| BomController | `Engenharia - BOM` |
| GrupoProdutoController | `Engenharia - Grupos` |
| GrupoVinculoController | `Engenharia - Grupo Vínculos` |
| StatusController | `Sistema - Status` |

Os próximos módulos seguirão o mesmo padrão: `Admin - Clientes`, `Comercial - Pedidos`, etc.

---

## 💡 Prefixo nas tabelas

Todas as tabelas passam a ter prefixo do módulo para organização conforme o sistema cresce:

| Tabela | Nome no banco |
|--------|--------------|
| Produtos | `Engenharia_Produtos` |
| Estruturas | `Engenharia_EstruturasProdutos` |
| Grupos | `Engenharia_GruposProdutos` |
| Vínculos | `Engenharia_GruposVinculos` |

Implementado via `builder.ToTable("Engenharia_NomeTabela")` em cada Configuration.
Os próximos módulos seguirão o mesmo padrão: `Admin_`, `Comercial_`, `PCP_`, `Compras_`, `Almoxarifado_`.

---

## 💡 Grupos de Código — Modelo atual (TCC)

### Máscara fixa para o TCC

O código inteligente de produto segue uma máscara fixa de 5 posições:

```
XX.YYY.ZZZZ.NNN.0000
│   │    │    │    │
│   │    │    │    └── Posição 5: Fixo 0000 (placeholder)
│   │    │    └─────── Posição 4: Sequencial automático (3 dígitos)
│   │    └──────────── Posição 3: Coluna3 — selecionável (4 caracteres)
│   └───────────────── Posição 2: Coluna2 — selecionável (3 caracteres)
│
└───────────────────── Posição 1: Coluna1 — grupo pai (2 dígitos)
```

### Nomenclatura

Em vez de nomes fixos (Grupo, Subgrupo, Família), usamos Coluna1, Coluna2 e Coluna3 pois:

- Os códigos existentes na empresa têm formatos variados
- Nem todo código segue a lógica de "grupo → subgrupo → família"
- Coluna é um nome genérico que se adapta a qualquer classificação

### Exemplos reais

| Código completo | Coluna1 | Coluna2 | Coluna3 | Seq | Fixo |
|----------------|---------|---------|---------|-----|------|
| 30.VLT.GM08.008.0000 | 30 (Peça Fabricada) | VLT | GM08 | 008 | 0000 |
| 30.SAC.NI05.001.0000 | 30 (Peça Fabricada) | SAC | NI05 | 001 | 0000 |
| 70.CHP.1020.003.0000 | 70 (Matéria Prima) | CHP | 1020 | 003 | 0000 |
| 10.PCP.GW10.001.0000 | 10 (Produto Final) | PCP | GW10 | 001 | 0000 |

### Vínculos entre colunas

A regra de vínculos garante hierarquia:
- Coluna1 → só pode ter filhos Coluna2
- Coluna2 → só pode ter filhos Coluna3
- Coluna3 → não pode ter filhos (último nível selecionável)

Exemplo: Coluna1 "30" (Peça Fabricada) aceita Coluna2 "SGE", "MEC", "ELT". Coluna2 "SGE" aceita Coluna3 "BM20", "BM30".

---

## 🔮 Melhorias futuras (pós-TCC)

### Configurador dinâmico de máscara

O modelo atual é simplificado com 3 colunas fixas. Na realidade, a empresa possui códigos com formatos diferentes:

```
70.060.3500.021.SIN0      → 5 posições, formatos variados
70.AMB.PLCP.009.0000      → 5 posições padrão
34.ARG.MB02.001.0000      → 5 posições padrão
92.010.CADM.000           → 4 posições, sem o 0000
34.BCB.FC12.001.0000M     → 5 posições + sufixo
```

Para atender todos os casos, seria necessário:

**Tabela `MascaraCodigo`** — uma máscara por grupo Coluna1:
- Id, Coluna1Id, Separador, QtdPosicoes

**Tabela `MascaraCodigoPosicao`** — cada posição da máscara:
- Id, MascaraCodigoId, Ordem, QtdCaracteres, Tipo (Lista, Sequencial, Fixo, Livre)

Isso permitiria:
- Cada Coluna1 ter sua própria estrutura de código
- Quantidade variável de posições (3, 4, 5, 6...)
- Misturar posições selecionáveis, sequenciais, fixas e livres
- Sufixos opcionais
- Validação dinâmica no frontend conforme a máscara configurada

**Estimativa de esforço:** Adiciona pelo menos 2 Models, 2 Services, 2 Controllers e lógica complexa de validação dinâmica. Viável para uma versão 2.0 do sistema.

---

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Models/Engenharia/Enums/NivelGrupo.cs` | Renomeado valores para Coluna1, Coluna2, Coluna3 |
| `Services/Engenharia/GrupoVinculoService.cs` | Ajustada validação de níveis para nomenclatura de colunas |
| `Data/Configurations/Engenharia/ProdutoConfiguration.cs` | Adicionado `ToTable("Engenharia_Produtos")` |
| `Data/Configurations/Engenharia/EstruturaProdutoConfiguration.cs` | Adicionado `ToTable("Engenharia_EstruturasProdutos")` |
| `Data/Configurations/Engenharia/GrupoProdutoConfiguration.cs` | Adicionado `ToTable("Engenharia_GruposProdutos")` |
| `Data/Configurations/Engenharia/GrupoVinculoConfiguration.cs` | Adicionado `ToTable("Engenharia_GruposVinculos")` |
| `Controllers/Engenharia/ProdutosController.cs` | Adicionado `[Tags("Engenharia - Produtos")]` |
| `Controllers/Engenharia/BomController.cs` | Adicionado `[Tags("Engenharia - BOM")]` |
| `Controllers/Engenharia/GrupoProdutoController.cs` | Adicionado `[Tags("Engenharia - Grupos")]` |
| `Controllers/Engenharia/GrupoVinculoController.cs` | Adicionado `[Tags("Engenharia - Grupo Vínculos")]` |
| `Controllers/StatusController.cs` | Adicionado `[Tags("Sistema - Status")]` |

---

## 🗄️ Carga inicial do banco

Banco populado via SQL direto no SQLite com:

| Tabela | Registros | Observação |
|--------|-----------|-----------|
| Engenharia_Produtos | 70 | Fabricados, comprados, matéria-prima |
| Engenharia_EstruturasProdutos | 50+ | Estruturas do Picador 500/700/900, Rotores, Mancais, Peneiras, Esteira |
| Engenharia_GruposProdutos | 50 | 10 Coluna1 + 25 Coluna2 + 15 Coluna3 |
| Engenharia_GruposVinculos | 50 | Vínculos entre Coluna1→Coluna2 e Coluna2→Coluna3 |

---

## ✅ Roteiro de testes

### STATUS
| # | Método | Rota | Esperado |
|---|--------|------|----------|
| 1 | GET | `/api/status` | ✅ 200 |

### PRODUTOS
| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/engenharia/Produtos` | — | ✅ 200 lista 70 produtos |
| 2 | GET | `/api/engenharia/Produtos/1` | — | ✅ 200 PIC-001 |
| 3 | GET | `/api/engenharia/Produtos/999` | — | ✅ 404 |
| 4 | POST | `/api/engenharia/Produtos` | `{"codigo":"TST-001","descricao":"Teste","unidade":"UN","tipo":"Fabricado","ativo":true}` | ✅ 201 |
| 5 | PUT | `/api/engenharia/Produtos/{id}` | `{"codigo":"TST-001","descricao":"Teste ATUALIZADO","unidade":"KG","tipo":"Comprado","ativo":true}` | ✅ 204 |
| 6 | DELETE | `/api/engenharia/Produtos/{id}` | — | ✅ 204 |

### BOM
| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/engenharia/Bom` | — | ✅ 200 produtos com estrutura |
| 2 | GET | `/api/engenharia/Bom?pagina=1&tamanho=2` | — | ✅ 200 paginado |
| 3 | GET | `/api/engenharia/Bom/flat` | — | ✅ 200 flat completa |
| 4 | GET | `/api/engenharia/Bom/flat?pagina=1&tamanho=5` | — | ✅ 200 flat paginada |
| 5 | GET | `/api/engenharia/Bom/produto/1` | — | ✅ 200 filhos do Picador 500 |
| 6 | GET | `/api/engenharia/Bom/1` | — | ✅ 200 item id 1 |
| 7 | POST | `/api/engenharia/Bom` | `{"produtoPaiId":1,"produtoFilhoId":19,"quantidade":1}` | ✅ 201 posição auto |
| 8 | POST | `/api/engenharia/Bom` | `{"produtoPaiId":1,"produtoFilhoId":4,"quantidade":1}` | ❌ 400 duplicado |
| 9 | POST | `/api/engenharia/Bom` | `{"produtoPaiId":4,"produtoFilhoId":1,"quantidade":1}` | ❌ 400 ciclo |
| 10 | POST | `/api/engenharia/Bom` | `{"produtoPaiId":1,"produtoFilhoId":1,"quantidade":1}` | ❌ 400 auto-ref |
| 11 | PUT | `/api/engenharia/Bom/{id}` | `{"id":{id},"produtoPaiId":1,"produtoFilhoId":19,"quantidade":3,"posicao":90,"observacao":"Teste PUT"}` | ✅ 204 |
| 12 | DELETE | `/api/engenharia/Bom/{id}` | — | ✅ 204 |

### GRUPO PRODUTO
| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/engenharia/GrupoProduto` | — | ✅ 200 lista 50 grupos |
| 2 | GET | `/api/engenharia/GrupoProduto/nivel/Coluna1` | — | ✅ 200 lista 10 |
| 3 | GET | `/api/engenharia/GrupoProduto/nivel/Coluna2` | — | ✅ 200 lista 25 |
| 4 | GET | `/api/engenharia/GrupoProduto/nivel/Coluna3` | — | ✅ 200 lista 15 |
| 5 | GET | `/api/engenharia/GrupoProduto/1` | — | ✅ 200 grupo "10" |
| 6 | POST | `/api/engenharia/GrupoProduto` | `{"codigo":"TS","descricao":"Teste","nivel":"Coluna1","qtdCaracteres":2,"ativo":true}` | ✅ 201 |
| 7 | POST | `/api/engenharia/GrupoProduto` | `{"codigo":"10","descricao":"Dup","nivel":"Coluna1","qtdCaracteres":2,"ativo":true}` | ❌ 400 duplicado |
| 8 | PUT | `/api/engenharia/GrupoProduto/{id}` | `{"codigo":"TS","descricao":"Teste ATUALIZADO","nivel":"Coluna1","qtdCaracteres":2,"ativo":true}` | ✅ 204 |
| 9 | DELETE | `/api/engenharia/GrupoProduto/{id}` | (grupo sem vínculo) | ✅ 204 |
| 10 | DELETE | `/api/engenharia/GrupoProduto/1` | (grupo com vínculo) | ❌ 400 possui vínculos |

### GRUPO VÍNCULO
| # | Método | Rota | Body/Params | Esperado |
|---|--------|------|-------------|----------|
| 1 | GET | `/api/engenharia/GrupoVinculo` | — | ✅ 200 lista 50 vínculos |
| 2 | GET | `/api/engenharia/GrupoVinculo/pai/1` | — | ✅ 200 filhos de Coluna1 "10" |
| 3 | GET | `/api/engenharia/GrupoVinculo/pai/3` | — | ✅ 200 filhos de Coluna1 "30" |
| 4 | POST | `/api/engenharia/GrupoVinculo` | `{"grupoPaiId":3,"grupoFilhoId":16}` | ✅ 200 vínculo criado |
| 5 | POST | `/api/engenharia/GrupoVinculo` | `{"grupoPaiId":3,"grupoFilhoId":16}` | ❌ 400 duplicado |
| 6 | POST | `/api/engenharia/GrupoVinculo` | `{"grupoPaiId":1,"grupoFilhoId":36}` | ❌ 400 nível errado (Coluna1→Coluna3) |
| 7 | POST | `/api/engenharia/GrupoVinculo` | `{"grupoPaiId":1,"grupoFilhoId":1}` | ❌ 400 auto-ref |
| 8 | DELETE | `/api/engenharia/GrupoVinculo/{id}` | — | ✅ 204 |

---

## 📂 Estrutura após este commit

```
pos_dev_web--tcc_arjsys--back/
├── app/
│   ├── Configurations/
│   │   ├── SwaggerConfig.cs
│   │   └── ScalarConfig.cs
│   ├── Controllers/
│   │   ├── Engenharia/
│   │   │   ├── ProdutosController.cs
│   │   │   ├── BomController.cs
│   │   │   ├── GrupoProdutoController.cs
│   │   │   └── GrupoVinculoController.cs
│   │   └── StatusController.cs
│   ├── Data/
│   │   ├── Configurations/Engenharia/
│   │   │   ├── ProdutoConfiguration.cs          ← alterado
│   │   │   ├── EstruturaProdutoConfiguration.cs ← alterado
│   │   │   ├── GrupoProdutoConfiguration.cs     ← alterado
│   │   │   └── GrupoVinculoConfiguration.cs     ← alterado
│   │   └── AppDbContext.cs
│   ├── Database/
│   │   └── ArjSysDB.db                          ← recriado e populado
│   ├── DTOs/Engenharia/
│   │   ├── ProdutoDTO.cs
│   │   ├── EstruturaProdutoDTO.cs
│   │   ├── GrupoProdutoDTO.cs
│   │   └── GrupoVinculoDTO.cs
│   ├── Migrations/
│   │   └── CriacaoInicial                       ← recriado
│   ├── Models/Engenharia/
│   │   ├── Enums/
│   │   │   ├── NivelGrupo.cs                    ← alterado
│   │   │   ├── TipoProduto.cs
│   │   │   └── UnidadeMedida.cs
│   │   ├── Produto.cs
│   │   ├── EstruturaProduto.cs
│   │   ├── GrupoProduto.cs
│   │   └── GrupoVinculo.cs
│   ├── Services/Engenharia/
│   │   ├── ProdutoService.cs
│   │   ├── BomService.cs
│   │   ├── GrupoProdutoService.cs
│   │   └── GrupoVinculoService.cs               ← alterado
│   ├── Properties/launchSettings.json
│   ├── appsettings.json
│   ├── app.csproj
│   └── Program.cs
├── docs/
│   ├── CONFIG/
│   │   ├── ProdutoService.cs
│	│   ├── 01-CONFIG-Inicializa-Projeto.md
│	│   ├── 02-CONFIG-DbContext-CORS-Status.md
│	│   ├── 03-FEATURE-Models-CRUD-Produtos.md
│	│   ├── 04-CONFIG-Swagger-Scalar-Ajustes.md
│	│   ├── 05-REFACTOR-ProdutoService.md
│	│   ├── 06-FEATURE-BOM-DTOs.md
│	│   ├── 07-FEATURE-GrupoProduto-GrupoVinculo.md
│	│   └── 08-REFACTOR-Prefixo-Tabelas-Carga.md
│   ├── SQL/
│	│   ├── SEED_Engenharia_EstruturasProdutos.SQL
│	│   ├── SEED_Engenharia_GruposProdutos.SQL
│	│   ├── SEED_Engenharia_GruposVinculos.SQL
│	│   └── SEED_Engenharia_Produtos.SQL
│   ├── 01 - MAPA-CLASSES-FLUXO-BACKEND.md
│   ├── 02 - PLANEJAMENTO-GERAL-ARJSYS.md
│   └── 03 - PLANO-ACAO-BACKEND.md
├── README.md
└── .gitignore
```

---
