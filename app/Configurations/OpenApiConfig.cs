using Microsoft.OpenApi;

namespace Api_ArjSys_Tcc.Configurations;

/// <summary>
/// Configuração do OpenAPI (Swagger/Scalar).
/// Define a ordem de exibição das Tags (seções) na documentação.
/// Ao adicionar novos módulos/controllers, incluir a Tag aqui para
/// controlar a posição na documentação. Tags não listadas aparecem no final.
/// </summary>
public static class OpenApiConfig
{
    public static void AddOpenApiConfig(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // Ordem de exibição no Swagger e Scalar
                var ordemDesejada = new[]
                {
                    "Sistema - Status",

                    "Admin - Auth",
                    "Admin - Clientes",
                    "Admin - Funcionários",
                    "Admin - Permissões",

                    "Engenharia - Produtos",
                    "Engenharia - BOM",
                    "Engenharia - Grupos",
                    "Engenharia - Grupo Vínculos",
                    "Engenharia - Configurações",
                    "Engenharia - Path Documentos",

                    "Comercial - Pedidos de Venda",
                    "Comercial - Itens do Pedido",
                    "Comercial - Número de Série",
                    
                };

                document.Tags = ordemDesejada
                    .Select(nome => new OpenApiTag { Name = nome })
                    .ToHashSet();

                return Task.CompletedTask;
            });
        });
    }
}