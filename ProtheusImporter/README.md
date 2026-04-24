# ProtheusImporter

Console app pra importar dados do Protheus (CSVs SB1, SG1) para o banco SQLite do ARJSYS.

Projeto paralelo ao `app/` e `SeedRunner/`, referencia os Models e o `AppDbContext` do projeto principal via `ProjectReference` (não duplica código).

---

## Pra que serve

Popular o ambiente de teste paralelo com dados reais extraídos do ERP Protheus. Diferente do SeedRunner (que só roda `.sql` pré-definidos), este projeto:

- Lê CSVs do Protheus (Windows-1252, separador `;`, header detectado automaticamente).
- Valida colunas obrigatórias e avisa se o formato não bater.
- Cruza códigos do Protheus com produtos já cadastrados no ARJ (necessário para SG1).
- Faz UPSERT (insere novos, atualiza existentes mantendo o mesmo Id) ou reset completo da tabela.
- Filtra SG1 pela **Rev. Final máxima** de cada código pai (só a revisão mais nova vai pro ARJ).
- Mostra preview em tela antes de gravar, aceita confirmar ou cancelar.
- Gera relatório `.log` com resumo, repetidos agrupados e rejeições detalhadas.

---

## Como rodar

### Desenvolvimento

```bash
cd ProtheusImporter
dotnet run
```

### Via BUILD.bat

Script na raiz do projeto, menu interativo com 3 opções:

1. **Self-contained** (`.exe` único ~70MB, não precisa .NET instalado) — pra distribuir.
2. **Framework-dependent** (`.exe` ~1MB, exige .NET 10 instalado) — pra máquinas que já têm .NET.
3. **Build Debug** (só compila, não publica).

Edite a variável `AMBIENTE` no topo do `.bat` (`TRABALHO` ou `CASA`) pra alternar entre `dotnet.CMD` e `dotnet`.

### Publicar standalone manualmente

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Saída: `bin/Release/net10.0/win-x64/publish/ProtheusImporter.exe`. Copie o `.exe` + `importer-config.ini` pra máquina de destino.

---

## Configuração — `importer-config.ini`

Fica ao lado do executável. Dividido em seções:

### `[Banco]`
- `DbPath` — caminho absoluto ou relativo do `ArjSysDB.db`. Vazio = `..\app\Database\ArjSysDB.db`.

### `[CSV]`
- `Separador` — padrão `;`.
- `ArquivoSB1`, `ArquivoSG1` — nome dos CSVs. Vazio = prompt pede na hora.
- `PastaCSV` — pasta base (absoluta ou relativa). Vazio = pasta do `.exe`.
- `MaxLinhasBuscaHeader` — quantas linhas o parser tenta pra achar o cabeçalho antes de desistir.

### `[Importacao]`
- `BatchSize` — registros por `SaveChanges`. Padrão 500.
- `SomarSequenciasRepetidas` — SG1: quando pai+filho aparecem com Sequências diferentes (caso legítimo do Protheus), como o ARJ ainda não guarda Sequência na Estrutura:
  - `true` = soma as Quantidades e grava 1 linha consolidada (recomendado por enquanto)
  - `false` ou vazio = mantém só a última (pra quando ARJ já tiver Sequência)

### `[Confirmacoes]`
- `ConfirmarAntesDeImportar` — se `false`, pula o prompt de confirmação.
- `ConfirmarReset`, `ConfirmarUpsert` — permite ligar/desligar por modo.

### `[Perigo]`
- `PermitirResetCascata` — habilita modo que apaga dependências em cascata. Ver seção "Reset em cascata" abaixo.

### `[Relatorio]`
- `GerarRelatorio` — `true` gera `.log`, `false`/vazio não gera.
- `CaminhoRelatorio` — pasta onde salvar. Vazio = pasta do `.exe`.
- `NomeRelatorio` — nome do arquivo (ex.: `ProtheusImporter.log`).
- `Sobrescrever` — `true` substitui a cada execução; `false` faz append com divisor entre rodadas.

---

## Fluxo de uso

1. Menu principal: digite `1` (SB1), `2` (SG1), `3` (Ver config), `4` (Sair).
2. App resolve o CSV via `.ini` ou prompta o caminho.
3. Modo de importação: `1` Atualizar (UPSERT) ou `2` Resetar.
4. App analisa o CSV e mostra preview (novos, atualizações, repetidos, rejeições).
5. Você confirma ou cancela.
6. App grava em batches com barra de progresso.
7. Se `GerarRelatorio=true`, escreve o `.log`.

**Ordem importa:** SB1 antes de SG1. Sem produtos cadastrados, o SG1 rejeita tudo por não achar pai/filho em Produtos.

---

## Colunas esperadas

### SB1 → Produtos

**Obrigatórias:** `Codigo`, `Descricao`
**Opcionais:** `Blq. de Tela`, `Unidade`, `Tipo`

Mapeamentos:
- `Blq. de Tela` → `Ativo`: vazio/`0` → `true`; `1` → `false`.
- `Unidade`: se bater com enum `UnidadeMedida` do ARJ, usa; senão `UN`.
- `Tipo`: `MP`→`MateriaPrima`, `PA`/`PI`→`Fabricado`, `MC`→`Comprado`, `SV`→`Servico`, `RE`→`Revenda`, outro → `Comprado`.

**Chave natural:** `Codigo` (case-sensitive, preserva caracteres crus do CSV — espaços, acentos estranhos etc. aparecem no ARJ como vieram, pra você identificar cadastros quebrados no Protheus).

### SG1 → EstruturaProduto

**Obrigatórias:** `Código`, `Ordem Item`, `Sequência`, `Quantidade`, `Componente`, `Rev. Final`

Mapeamentos:
- `Código` → busca Id em Produtos → `ProdutoPaiId`
- `Componente` → busca Id em Produtos → `ProdutoFilhoId`
- `Ordem Item` → `Posicao` (int)
- `Quantidade` → `Quantidade` (decimal, aceita `1,5` ou `1.5`)
- `Sequência` → usada só pra detectar repetidos reais, não vai pro ARJ
- `Rev. Final` → usada só pra filtrar, não vai pro ARJ

**Chave natural:** par `(ProdutoPaiId, ProdutoFilhoId)`.

---

## Regras específicas do SG1

O importador processa o CSV em fases:

1. **Lê tudo** sem validar em Produtos (só descarta pai/filho vazios).
2. **Filtra por Rev. Final:** agrupa por código pai, acha a Rev. Final **máxima**, descarta todas as linhas com rev. menor (silenciosamente — entra no resumo como "linhas descartadas", não como rejeição).
3. **Valida** pai e filho em Produtos. Linhas com código não encontrado vão pro log com status `ENCONTRADO` / `NAO ENCONTRADO` pra cada lado.
4. **Detecta repetidos reais** pela tríade `(pai, filho, sequência)` dentro da rev. máxima. Mesmo trio duas vezes = repetido (vai pro log).
5. **Consolida por par** `(pai, filho)`: se a mesma combinação aparece com Sequências diferentes e `SomarSequenciasRepetidas=true`, soma as Quantidades em 1 linha.
6. **UPSERT por `(ProdutoPaiId, ProdutoFilhoId)`:** existente no banco → atualiza Quantidade/Posicao; novo → insere.

---

## Modos de importação

### Atualizar (UPSERT) — padrão recomendado
- Novos são inseridos, existentes são atualizados mantendo o `Id` e `CriadoEm` originais.
- Só `ModificadoEm` e `ModificadoPor` mudam ("ProtheusImporter").
- Seguro em qualquer momento.

### Resetar
- `DELETE` na tabela alvo antes de inserir.
- Se tabela tem FKs apontando pra ela (ex.: Produtos é referenciado por Estruturas/NS/OP), o `DELETE` estoura — use UPSERT ou o modo cascata abaixo.

### Reset em cascata (só SB1)
- Controlado pela flag `PermitirResetCascata` em `[Perigo]` do `.ini`.
- Se `true` e você escolher modo Resetar no SB1: app mostra tabela vermelha com todas as tabelas afetadas + contagem, pede confirmação digitando `SIM` (maiúsculo), e aí apaga em ordem: `Producao_OrdemProducaoHistorico` → `Producao_OrdensProducaoItens` → `Producao_OrdensProducao` → `Comercial_NumerosSerie` → `Engenharia_EstruturasProdutos` → `Engenharia_Produtos`. Zera `sqlite_sequence` ao fim.
- **Use apenas em fase de teste/implantação inicial.** Em produção, mantenha `false`.

---

## Relatório `.log`

Formato:

```
============================================================
SG1 - Estruturas / BOM
============================================================
 Data/hora:     2026-04-24 15:42:13
 Arquivo CSV:   C:\PRG\TC\CSV\SG1.csv
 Modo:          Atualizar (UPSERT)
 Duracao total: 8.42s

 RESUMO
 ---
   Linhas lidas no CSV..........:   70.218
   Inseridos....................:   68.942
   Atualizados..................:        0
   Repetidos no CSV.............:      123
   Rejeitados...................:       28

 FILTRO DE REVISAO (Rev. Final)
 ---
 Apenas a maior Rev. Final de cada codigo pai foi importada.
   Codigos-pai distintos........:    8.342
   Pais com multiplas revisoes..:    1.203
   Linhas descartadas (rev antiga):  4.852

 REPETIDOS NO CSV (123)
 ---
- CÓDIGO: 10.ABS.TO02.001.0000 - COMP.: 39.ABS.TO02.001.0001 - SEQ.: 001
  LINHAS: 6, 7

 NÃO ENCONTRADOS REFERENCIA EM SB1 (PRODUTOS) (28)
 ---
------ LINHA: 68142
  CÓDIGO PAI: 10.ASA.MB02.019.0000 (ENCONTRADO)
  COMPONENTE: 72.0.ZBSR.6M15.00000 (NÃO ENCONTRADO)
```

Com `Sobrescrever=false`, cada nova execução é anexada ao arquivo com linha em branco dupla separando.

---

## Estrutura do projeto

```
ProtheusImporter/
├── ProtheusImporter.csproj
├── BUILD.bat
├── importer-config.ini
├── Program.cs
├── README.md
├── Core/
│   ├── CsvImporter.cs        # engine genérico <TEntity, TKey> com UPSERT e dedup
│   ├── CsvReader.cs          # parser CSV (header detection, aspas, separador configurável)
│   ├── ImportOptions.cs      # options passadas aos importers
│   ├── ImportResult.cs       # resultado da execução + tipos ItemRepetido / RejeicaoSG1
│   ├── IniConfig.cs          # leitor do .ini (seções, chaves, tipos)
│   └── RelatorioLog.cs       # gerador do arquivo .log
├── Importers/
│   ├── ImportadorSB1.cs      # SB1 → Produto (+ reset em cascata)
│   └── ImportadorSG1.cs      # SG1 → EstruturaProduto (com filtro Rev. Final, dedup por tríade)
└── UI/
    └── MenuPrincipal.cs      # menu interativo Spectre.Console
```

---

## Pontos importantes

- **Caracteres preservados como vêm:** sem `.Trim()` em `Codigo`/`Descricao`/`Componente`/`Código`. Se o Protheus tem cadastro quebrado (espaço no meio, acento estranho), vai aparecer no ARJ do jeito que está — intencional, pra você identificar.
- **Ordem: SB1 → SG1.** Sempre.
- **Transação única por importação:** erro no meio faz rollback total, banco fica íntegro.
- **Progresso em tempo real** via Spectre.Console (barra + % + ETA).
- **CSV não pode estar aberto** em Excel/Notepad durante a importação (Windows trava o arquivo).