<!-- markdownlint-disable-file -->
# ✨ FEATURE: Configurações de Engenharia, TemDocumento e Varredura de Documentos

## 💡 Conceitos aplicados

### Configurações editáveis pelo usuário

A tabela `Engenharia_Configuracoes` armazena chave/valor. Inicialmente com a chave `PathRaizDocumentos` que define o caminho raiz padrão para documentos/desenhos.

O usuário configura pela tela do sistema — não precisa editar arquivos de configuração. Novas chaves podem ser adicionadas conforme necessidade.

### Path de documentos — lógica de resolução

Cada grupo Coluna1 pode ter um `PathDocumentos` customizado. Se não tiver, usa o `PathRaizDocumentos` global + código do grupo.

```
Produto: 30.VLT.GM08.008.0000
Grupo Coluna1: 30

Se grupo 30 tem PathDocumentos = "D:\Outros Docs\30\"
  → D:\Outros Docs\30\30.VLT.GM08.008.0000\

Se grupo 30 NÃO tem PathDocumentos
  → usa PathRaizDocumentos = "D:\Codigos\"
  → D:\Codigos\30\30.VLT.GM08.008.0000\
```

Dentro da pasta do produto, o sistema procura arquivo com o mesmo nome do código (qualquer extensão):
- `30.VLT.GM08.008.0000.pdf` ✅
- `30.VLT.GM08.008.0000.dwg` ✅
- `outro_nome.pdf` ❌ (nome diferente do código)

### Varredura de documentos

O endpoint de varredura percorre os produtos cadastrados, verifica se existe a pasta e o arquivo correspondente no path configurado, e atualiza o campo `TemDocumento` automaticamente.

- **Sem parâmetro:** varredura total em todos os produtos
- **Com prefixo:** filtra por código (ex: `?prefixo=30` varre só produtos começando com "30")

A varredura atualiza tanto pra `true` (encontrou) quanto pra `false` (não encontrou mais).

### TemDocumento no frontend

O campo `TemDocumento` permite ao frontend:
- Habilitar/desabilitar botão de abrir desenho
- Filtrar visualização só de produtos com documento
- Exibir miniatura do documento (preview)

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `Models/Engenharia/ConfiguracaoEngenharia.cs` | Entidade chave/valor (herda BaseEntity) |
| `DTOs/Engenharia/ConfiguracaoEngenhariaDTO.cs` | CreateDTO e ResponseDTO |
| `DTOs/Engenharia/VarreduraDocumentosResultDTO.cs` | Resultado da varredura |
| `Data/Configurations/Engenharia/ConfiguracaoEngenhariaConfiguration.cs` | Tabela Engenharia_Configuracoes, índice único Chave |
| `Services/Engenharia/ConfiguracaoEngenhariaService.cs` | CRUD configurações |
| `Controllers/Engenharia/ConfiguracaoEngenhariaController.cs` | Endpoints CRUD configurações |
| `tests/http/13-engenharia-configuracoes.http` | 9 testes de configurações e path |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Models/Engenharia/Produto.cs` | Adicionado campo `TemDocumento` (bool) |
| `DTOs/Engenharia/ProdutoDTO.cs` | Adicionado `TemDocumento` em Create e Response |
| `Services/Engenharia/ProdutoService.cs` | Adicionado `TemDocumento` no CRUD + método `VarrerDocumentos` |
| `Services/Engenharia/GrupoProdutoService.cs` | Refatorado `MontarPathDocumento` para buscar path raiz do banco |
| `Controllers/Engenharia/ProdutosController.cs` | Adicionado endpoint `POST varredura-documentos` |
| `Controllers/Engenharia/GrupoProdutoController.cs` | Adicionado endpoint `GET path-documento` |
| `Data/AppDbContext.cs` | Adicionado DbSet ConfiguracoesEngenharia |
| `Program.cs` | Registrado ConfiguracaoEngenhariaService |
| `Configurations/OpenApiConfig.cs` | Adicionada tag "Engenharia - Configurações" |
| `tests/http/02-engenharia-produtos.http` | Atualizado com TemDocumento e varredura |

---

## 🗄️ Tabela no banco

| Tabela | Registros na carga |
|--------|-------------------|
| `Engenharia_Configuracoes` | 1 (PathRaizDocumentos) |

---

## 🌐 Endpoints novos

### Engenharia - Configurações

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/engenharia/ConfiguracaoEngenharia` | Lista todas |
| GET | `/api/engenharia/ConfiguracaoEngenharia/{chave}` | Busca por chave |
| POST | `/api/engenharia/ConfiguracaoEngenharia` | Cria configuração |
| PUT | `/api/engenharia/ConfiguracaoEngenharia/{id}` | Atualiza valor |
| DELETE | `/api/engenharia/ConfiguracaoEngenharia/{id}` | Remove |

### Engenharia - Produtos (novos)

| Método | Rota | Ação |
|--------|------|------|
| POST | `/api/engenharia/Produtos/varredura-documentos` | Varredura total |
| POST | `/api/engenharia/Produtos/varredura-documentos?prefixo=30` | Varredura por prefixo |

### Engenharia - Grupos (novo)

| Método | Rota | Ação |
|--------|------|------|
| GET | `/api/engenharia/GrupoProduto/{id}/path-documento/{codigoProduto}` | Monta path completo |

---
