using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api_ArjSys_Tcc.Data.Sqlite;

/// <summary>
/// Interceptor que aplica PRAGMAs de tuning do SQLite em cada conexão aberta.
/// WAL (journal_mode) é persistente no arquivo; os demais são por conexão e
/// precisam ser reaplicados sempre que o pool abre uma nova.
/// </summary>
public class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    private const string PragmaScript = @"
        PRAGMA journal_mode = WAL;
        PRAGMA synchronous = NORMAL;
        PRAGMA cache_size = -64000;
        PRAGMA temp_store = MEMORY;
        PRAGMA mmap_size = 268435456;
    ";

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = PragmaScript;
        cmd.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = PragmaScript;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
