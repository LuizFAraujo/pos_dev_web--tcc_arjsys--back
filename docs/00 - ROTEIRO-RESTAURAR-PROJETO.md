<!-- markdownlint-disable-file -->
# 🖥️ Roteiro — Configurar e Rodar o Backend em Nova Máquina

> Seguir os passos na ordem. Os blocos **⚠️ SE DER ERRO** são opcionais — execute apenas se o passo anterior falhar.

---

## Pré-requisitos

- .NET 10 SDK instalado
- Git instalado

### Verificar .NET

```bash
dotnet --version
```

Deve retornar `10.x.xxx`. Se não tiver instalado: https://dotnet.microsoft.com/download/dotnet/10.0

---

## Passo 1 — Clonar o repositório

```bash
git clone <url-do-repositorio>
cd pos_dev_web--tcc_arjsys--back/app
```

---

## Passo 2 — Restaurar dependências (NuGet)

```bash
dotnet restore Api_ArjSys_Tcc.slnx
```

✅ Deve finalizar sem erros.

---

## Passo 3 — Aplicar Migrations (criar o banco SQLite)

```bash
dotnet ef database update
```

✅ Deve exibir as tabelas criadas e finalizar com `Done.`

---

### ⚠️ SE DER ERRO no Passo 3

O `dotnet ef` global pode não encontrar o SDK em máquinas com instalação customizada do .NET.  
Nesse caso, instale o ef como **ferramenta local** do projeto:

**3a. Criar o manifesto de ferramentas locais** *(só na primeira vez nessa máquina)*

```bash
dotnet new tool-manifest
```

**3b. Instalar o dotnet-ef localmente** *(só na primeira vez nessa máquina)*

```bash
dotnet tool install dotnet-ef
```

**3c. Tentar novamente**

```bash
dotnet ef database update
```

> O arquivo `dotnet-tools.json` gerado no passo 3a fica salvo na pasta `app`.  
> Nas próximas vezes nessa máquina, pule direto para o passo 3c.

---

## Passo 4 — Rodar a API

### Modo normal (roda uma vez)

```bash
dotnet run
```

### Modo desenvolvimento (reload automático ao salvar)

```bash
dotnet watch
```

✅ A API estará disponível nas URLs exibidas no terminal (ex: `http://localhost:5XXX`).

---

## Outros comandos úteis

### Verificar se compila sem rodar

```bash
dotnet build Api_ArjSys_Tcc.csproj
```

> Incluir o nome do `.csproj` evita erro caso haja mais de um projeto na pasta.

### Criar nova migration após alterar Models

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

---

## Referência rápida

| Comando | O que faz |
|---|---|
| `dotnet restore` | Baixa os pacotes NuGet |
| `dotnet build Api_ArjSys_Tcc.csproj` | Compila sem rodar |
| `dotnet run` | Compila e roda |
| `dotnet watch` | Roda com reload automático |
| `dotnet ef database update` | Aplica migrations pendentes |
| `dotnet ef migrations add Nome` | Cria nova migration |

---

## No Visual Studio

| Ação | Como fazer |
|---|---|
| Rodar com hot reload | Botão **▶ Play** ou **F5** |
| Só compilar | **Ctrl+Shift+B** ou menu **Build → Build Solution** |

> No Visual Studio o hot reload já vem ativo por padrão — não precisa de `dotnet watch`.
