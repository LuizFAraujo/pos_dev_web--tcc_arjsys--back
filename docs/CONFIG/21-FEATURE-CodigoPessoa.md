# 21 - FEATURE - Código humano único em Pessoa

## Resumo

Pessoa ganha campo `Codigo` string com prefixo por tipo:

- **Cliente** → `CLI-0001`, `CLI-0002`, ...
- **Funcionario** → `FUN-0001`, `FUN-0002`, ...
- **Fornecedor** (futuro) → `FOR-0001`, ...

Sequencial independente por tipo. Gerado automaticamente pelo back. Não aceito no payload.

## Modelo

```csharp
public class Pessoa : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;  // NOVO
    public string Nome { get; set; } = string.Empty;
    // ... demais ...
}
```

### Configuration EF
- `Codigo` NOT NULL, MaxLength 10, **unique index**

### Enum `TipoPessoa`
- Ganha `Fornecedor` (slot reservado mesmo sem entidade implementada)

## Geração do código

Helper estático `CodigoPessoaGenerator` em `app/Services/Admin/`:
- Query `MAX(SUBSTRING(Codigo, 5))` entre pessoas com mesmo prefixo
- Próximo = MAX + 1, formatado `D4` (4 dígitos)

### Concorrência

**Retry simples** em `DbUpdateException` (unique index violado) até 3 tentativas. Após limpar o change tracker entre tentativas. Suficiente pra cargas locais; escalar depois se necessário.

## Decisões tomadas

| Decisão | Escolha |
|---|---|
| Concorrência | Retry simples até 3 tentativas |
| Enum `TipoPessoa` | Adiciona `Fornecedor` agora (slot reservado) |
| Filtro `?busca=` | Adiciona em Cliente e Funcionário agora |
| `Codigo` no POST/PUT | **Ignorado silenciosamente** — back gera |
| Exposição em DTOs relacionados | `ClienteCodigo` no PV/NS/OP; `FuncionarioCodigo` em Permissão |
| Seed | Códigos explícitos nos INSERTs pra determinismo |

## Busca

**Cliente:** `GET /api/admin/Clientes?busca=texto` — filtra em nome, codigo, cpfCnpj, cidade
**Funcionario:** `GET /api/admin/Funcionarios?busca=texto` — filtra em nome, codigo, usuario, cargo

## Arquivos afetados

### Models / Enums
| Ação | Path |
|---|---|
| SUBSTITUIR | `app/Models/Admin/Pessoa.cs` |
| SUBSTITUIR | `app/Models/Admin/Enums/TipoPessoa.cs` |

### Configurations
| Ação | Path |
|---|---|
| SUBSTITUIR | `app/Data/Configurations/Admin/PessoaConfiguration.cs` |

### Helper
| Ação | Path |
|---|---|
| NOVO | `app/Services/Admin/CodigoPessoaGenerator.cs` |

### DTOs
| Ação | Path | O que muda |
|---|---|---|
| SUBSTITUIR | `app/DTOs/Admin/ClienteDTO.cs` | Response ganha `Codigo` |
| SUBSTITUIR | `app/DTOs/Admin/FuncionarioDTO.cs` | Response ganha `Codigo` |
| SUBSTITUIR | `app/DTOs/Admin/PermissaoDTO.cs` | Response ganha `FuncionarioCodigo` |
| SUBSTITUIR | `app/DTOs/Comercial/PedidoVendaDTO.cs` | Response ganha `ClienteCodigo` |
| SUBSTITUIR | `app/DTOs/Comercial/NumeroSerieDTO.cs` | Response ganha `ClienteCodigo` |
| SUBSTITUIR | `app/DTOs/Producao/OrdemProducaoDTO.cs` | Response ganha `ClienteCodigo` (nullable) |

### Services
| Ação | Path |
|---|---|
| SUBSTITUIR | `app/Services/Admin/ClienteService.cs` |
| SUBSTITUIR | `app/Services/Admin/FuncionarioService.cs` |
| SUBSTITUIR | `app/Services/Admin/PermissaoService.cs` |
| PATCH | `app/Services/Comercial/PedidoVendaService.cs` (1 linha em `ToResponseDTO`) |
| PATCH | `app/Services/Comercial/NumeroSerieService.cs` (1 linha em `ToResponseDTO`) |
| PATCH | `app/Services/Producao/OrdemProducaoService.cs` (1 linha em `ToResponseDTO`) |

### Controllers
| Ação | Path |
|---|---|
| SUBSTITUIR | `app/Controllers/Admin/ClientesController.cs` |
| SUBSTITUIR | `app/Controllers/Admin/FuncionariosController.cs` |

### Seed
| Ação | Path |
|---|---|
| SUBSTITUIR | `docs/SQL/SEED_ADMIN_01_PESSOAS.SQL` (INSERTs com coluna `Codigo`) |

### .http
| Ação | Path |
|---|---|
| SUBSTITUIR | `app/Tests/http/admin_01_clientes.http` |
| SUBSTITUIR | `app/Tests/http/admin_02_funcionarios.http` |

### Migration
- NOVO: `CodigoPessoa` (ver seção abaixo)

## Migration — passo a passo

```bash
# 1. Colar todos os arquivos novos/substituídos
# 2. Gerar migration
dotnet ef migrations add CodigoPessoa --project app
```

Como o EF Core vai gerar automaticamente só a adição da coluna `Codigo` + índice único, mas **a coluna é NOT NULL** e já tem dados existentes, precisamos:

### Editar manualmente a migration gerada

Abrir o arquivo `app/Migrations/*_CodigoPessoa.cs` e no método `Up()`:

1. Substituir o `AddColumn<string>(..., nullable: false, defaultValue: "")` por `nullable: true` temporariamente
2. Adicionar comando SQL de data migration
3. Alterar coluna para NOT NULL depois

**Exemplo do `Up()` ajustado:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Adiciona coluna como nullable temporariamente
    migrationBuilder.AddColumn<string>(
        name: "Codigo",
        table: "Admin_Pessoas",
        type: "TEXT",
        maxLength: 10,
        nullable: true);

    // 2. Data migration: popular códigos usando ROW_NUMBER por Tipo
    migrationBuilder.Sql(@"
        UPDATE Admin_Pessoas
        SET Codigo = 'CLI-' || printf('%04d', (
            SELECT COUNT(*) FROM Admin_Pessoas AS p2
            WHERE p2.Tipo = 'Cliente' AND p2.Id <= Admin_Pessoas.Id
        ))
        WHERE Tipo = 'Cliente';
    ");

    migrationBuilder.Sql(@"
        UPDATE Admin_Pessoas
        SET Codigo = 'FUN-' || printf('%04d', (
            SELECT COUNT(*) FROM Admin_Pessoas AS p2
            WHERE p2.Tipo = 'Funcionario' AND p2.Id <= Admin_Pessoas.Id
        ))
        WHERE Tipo = 'Funcionario';
    ");

    // 3. Altera coluna pra NOT NULL agora que tem dados
    migrationBuilder.AlterColumn<string>(
        name: "Codigo",
        table: "Admin_Pessoas",
        type: "TEXT",
        maxLength: 10,
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "TEXT",
        oldMaxLength: 10,
        oldNullable: true);

    // 4. Cria índice único
    migrationBuilder.CreateIndex(
        name: "IX_Admin_Pessoas_Codigo",
        table: "Admin_Pessoas",
        column: "Codigo",
        unique: true);
}
```

### Alternativa mais simples: resetar seed

Como o projeto já tá em dev e os seeds foram atualizados com códigos explícitos:

```bash
# 1. Gerar a migration (deixa ela como EF gerou — com NOT NULL direto)
dotnet ef migrations add CodigoPessoa --project app

# 2. Resetar banco e repovoar com o novo seed
arj-reset
arj-seed

# 3. Rodar API
arj-api
```

**Escolha do Luiz:** como o seed já foi atualizado, essa segunda via é mais limpa. Usar só a primeira se o banco local tiver dados manuais que precisam preservar.

## Cenários de teste

Ver `admin_01_clientes.http` e `admin_02_funcionarios.http`. Principais:

1. `POST /Clientes` → retorna `codigo: "CLI-0016"` (próximo após 15 do seed)
2. `POST /Clientes` com `codigo: "CLI-9999"` no body → ignora, gera o próximo normalmente
3. `GET /Clientes/1` → retorna `codigo: "CLI-0001"`
4. `GET /Clientes?busca=CLI-0001` → 1 resultado exato
5. `GET /Clientes?busca=CLI-00` → múltiplos (prefixo match)
6. `GET /Clientes?busca=Maringa` → busca funciona por cidade
7. `POST /Funcionarios` → retorna `codigo: "FUN-0011"`
8. `GET /PedidoVenda/7` → response inclui `clienteCodigo`
9. `GET /NumeroSerie` → response inclui `clienteCodigo`
10. `GET /OrdemProducao/1` → response inclui `clienteCodigo` (ou null se OP de estoque)
