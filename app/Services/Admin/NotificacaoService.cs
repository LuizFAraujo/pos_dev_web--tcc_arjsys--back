using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.Models.Admin.Enums;
using Api_ArjSys_Tcc.DTOs.Admin;

namespace Api_ArjSys_Tcc.Services.Admin;

/// <summary>
/// Serviço de notificações genéricas por módulo.
/// Fase 2 entrega apenas a infraestrutura (CRUD + marcar lida). O disparo automático
/// a partir de eventos de outros módulos será acoplado em fases seguintes.
/// </summary>
public class NotificacaoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista notificações filtradas por módulo. Opcionalmente só as não lidas.
    /// Suporta paginação.
    /// </summary>
    public async Task<List<NotificacaoResponseDTO>> GetByModulo(
        ModuloSistema modulo,
        bool? lidas = null,
        int pagina = 0,
        int tamanho = 0)
    {
        var query = _context.Notificacoes.Where(n => n.ModuloDestino == modulo);

        if (lidas.HasValue)
            query = query.Where(n => n.Lida == lidas.Value);

        var ordenada = query.OrderByDescending(n => n.CriadoEm);

        List<Notificacao> lista;

        if (pagina > 0 && tamanho > 0)
            lista = await ordenada.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            lista = await ordenada.ToListAsync();

        return lista.Select(ToResponseDTO).ToList();
    }

    /// <summary>
    /// Busca notificação por ID.
    /// </summary>
    public async Task<NotificacaoResponseDTO?> GetById(int id)
    {
        var n = await _context.Notificacoes.FindAsync(id);
        return n == null ? null : ToResponseDTO(n);
    }

    /// <summary>
    /// Conta notificações não lidas de um módulo (atalho usado pelo front pra badge).
    /// </summary>
    public async Task<int> ContarNaoLidas(ModuloSistema modulo)
    {
        return await _context.Notificacoes
            .CountAsync(n => n.ModuloDestino == modulo && !n.Lida);
    }

    /// <summary>
    /// Cria notificação. Valida título e mensagem obrigatórios.
    /// </summary>
    public async Task<(NotificacaoResponseDTO? Item, string? Erro)> Create(NotificacaoCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return (null, "Título é obrigatório");

        if (string.IsNullOrWhiteSpace(dto.Mensagem))
            return (null, "Mensagem é obrigatória");

        var n = new Notificacao
        {
            ModuloDestino = dto.ModuloDestino,
            Tipo = dto.Tipo,
            Titulo = dto.Titulo.Trim(),
            Mensagem = dto.Mensagem.Trim(),
            OrigemTabela = string.IsNullOrWhiteSpace(dto.OrigemTabela) ? null : dto.OrigemTabela.Trim(),
            OrigemId = dto.OrigemId,
            Lida = false,
            CriadoEm = DateTime.UtcNow
        };

        _context.Notificacoes.Add(n);
        await _context.SaveChangesAsync();

        return (ToResponseDTO(n), null);
    }

    /// <summary>
    /// Marca uma notificação como lida. Idempotente (não reescreve se já estava lida).
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> MarcarLida(int id)
    {
        var n = await _context.Notificacoes.FindAsync(id);

        if (n == null)
            return (false, null);

        if (!n.Lida)
        {
            n.Lida = true;
            n.DataLeitura = DateTime.UtcNow;
            n.ModificadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return (true, null);
    }

    /// <summary>
    /// Marca todas as notificações não lidas de um módulo como lidas.
    /// Retorna a quantidade afetada.
    /// </summary>
    public async Task<int> MarcarTodasLidas(ModuloSistema modulo)
    {
        var agora = DateTime.UtcNow;

        var naoLidas = await _context.Notificacoes
            .Where(n => n.ModuloDestino == modulo && !n.Lida)
            .ToListAsync();

        foreach (var n in naoLidas)
        {
            n.Lida = true;
            n.DataLeitura = agora;
            n.ModificadoEm = agora;
        }

        if (naoLidas.Count > 0)
            await _context.SaveChangesAsync();

        return naoLidas.Count;
    }

    /// <summary>
    /// Exclui notificação.
    /// </summary>
    public async Task<bool> Delete(int id)
    {
        var n = await _context.Notificacoes.FindAsync(id);

        if (n == null)
            return false;

        _context.Notificacoes.Remove(n);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Converte entidade em DTO de resposta.
    /// </summary>
    private static NotificacaoResponseDTO ToResponseDTO(Notificacao n) => new()
    {
        Id = n.Id,
        ModuloDestino = n.ModuloDestino,
        Tipo = n.Tipo,
        Titulo = n.Titulo,
        Mensagem = n.Mensagem,
        Lida = n.Lida,
        DataLeitura = n.DataLeitura,
        OrigemTabela = n.OrigemTabela,
        OrigemId = n.OrigemId,
        CriadoEm = n.CriadoEm,
        ModificadoEm = n.ModificadoEm
    };
}
