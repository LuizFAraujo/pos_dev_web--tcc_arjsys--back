using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Data.Sqlite;

namespace Api_ArjSys_Tcc.Configurations.Sqlite;

/// <summary>
/// Configuração do SQLite como provedor do AppDbContext.
/// Registra o DbContext e aplica PRAGMAs de tuning via interceptor em cada conexão.
/// Isolado em subpasta Sqlite/ pra facilitar troca futura por outro provedor (Postgres etc).
/// </summary>
public static class SqliteConfig
{
    public static void AddSqliteConfig(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseSqlite(connectionString)
                .AddInterceptors(new SqlitePragmaInterceptor()));
    }
}
