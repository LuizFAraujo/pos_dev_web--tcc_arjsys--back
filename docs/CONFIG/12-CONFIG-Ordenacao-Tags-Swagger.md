<!-- markdownlint-disable-file -->
# ⚙️ CONFIG: Ordenação das Tags no Swagger e Scalar

## 💡 Como funciona

O arquivo `OpenApiConfig.cs` define a ordem de exibição das seções (Tags) na documentação do Swagger e Scalar. Tags não listadas aparecem no final automaticamente.

### Ordem definida

1. Sistema - Status
2. Admin - Auth
3. Admin - Clientes
4. Admin - Funcionários
5. Admin - Permissões
6. Engenharia - Produtos
7. Engenharia - BOM
8. Engenharia - Grupos
9. Engenharia - Grupo Vínculos
10. Comercial - Pedidos de Venda
11. Comercial - Itens do Pedido
12. Comercial - Número de Série

### Para adicionar novos módulos

Editar o array `ordemDesejada` em `Configurations/OpenApiConfig.cs` e incluir a nova Tag na posição desejada. Tags que não estiverem na lista aparecem no final da documentação.

---

## 📁 Arquivos criados

| Arquivo | Função |
|---------|--------|
| `Configurations/OpenApiConfig.cs` | Ordenação das Tags na documentação Swagger/Scalar |

## 📝 Arquivos alterados

| Arquivo | Alteração |
|---------|-----------|
| `Program.cs` | Trocado `AddOpenApi()` por `AddOpenApiConfig()` |
| `Api_ArjSys_Tcc.csproj` | Adicionado pacote Microsoft.OpenApi 2.4.1 |

---
