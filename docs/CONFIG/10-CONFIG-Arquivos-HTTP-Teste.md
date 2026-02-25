<!-- markdownlint-disable-file -->
# 🧪 CONFIG: Arquivos HTTP de teste para API

## 💡 Como funciona

### VS Code

1. Instalar extensão **REST Client** (por Huachao Mao)
2. Abrir qualquer arquivo `.http`
3. Clicar em **Send Request** acima de cada `###`
4. Resposta aparece numa aba ao lado

**Variável centralizada** em `.vscode/settings.json` (raiz do projeto):

```json
{
  "rest-client.environmentVariables": {
    "$shared": {
      "host": "http://localhost:7000"
    }
  }
}
```

### Visual Studio

1. Suporte nativo a `.http` (VS 2022 17.6+)
2. Abrir o arquivo → botão verde de play em cada requisição
3. Selecionar o ambiente **dev** no dropdown
4. Resposta aparece embaixo

**Variável centralizada** em `app/tests/http/http-client.env.json`:

```json
{
  "dev": {
    "host": "http://localhost:7000"
  }
}
```

### Resumo

| IDE | Extensão | Arquivo de variáveis |
|-----|----------|---------------------|
| VS Code | REST Client (Huachao Mao) | `.vscode/settings.json` |
| Visual Studio | Nativo (17.6+) | `app/tests/http/http-client.env.json` |

Todos os arquivos `.http` usam `{{host}}` — se a porta mudar, altera no arquivo correspondente ao seu IDE.

---

## 📁 Arquivos criados

```
.vscode/
└── settings.json                    ← Variável host (VS Code)

app/tests/http/
├── http-client.env.json             ← Variável host (Visual Studio)
├── 01-status.http                   ← Health check (1 teste)
├── 02-engenharia-produtos.http      ← CRUD Produtos (6 testes)
├── 03-engenharia-bom.http           ← CRUD BOM + validações (12 testes)
├── 04-engenharia-grupos.http        ← CRUD Grupos + validações (11 testes)
├── 05-engenharia-vinculos.http      ← CRUD Vínculos + validações (9 testes)
├── 06-admin-clientes.http           ← CRUD Clientes (7 testes)
├── 07-admin-funcionarios.http       ← CRUD Funcionários + validações (9 testes)
├── 08-admin-permissoes.http         ← CRUD Permissões + validações (8 testes)
└── 09-admin-auth.http               ← Login válido/inválido (6 testes)
```

## 📊 Total de testes

| Arquivo | Módulo | Testes |
|---------|--------|--------|
| 01-status | Sistema | 1 |
| 02-engenharia-produtos | Engenharia | 6 |
| 03-engenharia-bom | Engenharia | 12 |
| 04-engenharia-grupos | Engenharia | 11 |
| 05-engenharia-vinculos | Engenharia | 9 |
| 06-admin-clientes | Admin | 7 |
| 07-admin-funcionarios | Admin | 9 |
| 08-admin-permissoes | Admin | 8 |
| 09-admin-auth | Admin | 6 |
| **Total** | | **69** |

---

## ⚠️ Observações

- Alguns testes de PUT e DELETE precisam de ajuste manual no ID (comentado no arquivo)
- Testes que esperam erro (400, 401, 404) estão marcados com `→` no comentário
- A API precisa estar rodando antes de executar os testes
- Novos módulos (Comercial, PCP, etc.) terão seus arquivos adicionados conforme implementação
