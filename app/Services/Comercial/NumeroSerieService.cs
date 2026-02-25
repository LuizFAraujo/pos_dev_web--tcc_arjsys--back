using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

public class NumeroSerieService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    // Ano de fundação da empresa — usado para calcular a idade
    private const int AnoFundacao = 1966;

    public async Task<List<NumeroSerieResponseDTO>> GetAll(int pagina = 0, int tamanho = 0)
    {
        var query = _context.NumerosSerie
            .Include(n => n.PedidoVenda)
                .ThenInclude(p => p.Cliente)
                    .ThenInclude(c => c.Pessoa)
            .OrderByDescending(n => n.CriadoEm);

        List<NumeroSerie> lista;

        if (pagina > 0 && tamanho > 0)
            lista = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            lista = await query.ToListAsync();

        return lista.Select(ToResponseDTO).ToList();
    }

    public async Task<NumeroSerieResponseDTO?> GetById(int id)
    {
        var ns = await _context.NumerosSerie
            .Include(n => n.PedidoVenda)
                .ThenInclude(p => p.Cliente)
                    .ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(n => n.Id == id);

        return ns == null ? null : ToResponseDTO(ns);
    }

    public async Task<List<NumeroSerieResponseDTO>> GetByPedidoId(int pedidoId)
    {
        return await _context.NumerosSerie
            .Where(n => n.PedidoVendaId == pedidoId)
            .Include(n => n.PedidoVenda)
                .ThenInclude(p => p.Cliente)
                    .ThenInclude(c => c.Pessoa)
            .Select(n => ToResponseDTO(n))
            .ToListAsync();
    }

    public async Task<(NumeroSerieResponseDTO? Item, string? Erro)> Create(NumeroSerieCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == dto.PedidoVendaId);

        if (pedido == null)
            return (null, "Pedido não encontrado");

        if (pedido.Status == StatusPedidoVenda.Orcamento)
            return (null, "Pedido precisa estar aprovado para gerar Número de Série");

        if (pedido.Status == StatusPedidoVenda.Cancelado)
            return (null, "Não é possível gerar N.Série para pedido cancelado");

        var codigo = await GerarCodigo();

        var ns = new NumeroSerie
        {
            Codigo = codigo,
            PedidoVendaId = dto.PedidoVendaId,
            Status = StatusNumeroSerie.Aberto,
            CriadoEm = DateTime.UtcNow
        };

        _context.NumerosSerie.Add(ns);
        await _context.SaveChangesAsync();

        ns.PedidoVenda = pedido;
        return (ToResponseDTO(ns), null);
    }

    public async Task<(bool Sucesso, string? Erro)> AlterarStatus(int id, StatusNumeroSerieDTO dto)
    {
        var ns = await _context.NumerosSerie.FindAsync(id);

        if (ns == null)
            return (false, "Número de Série não encontrado");

        var valido = ValidarTransicaoStatus(ns.Status, dto.NovoStatus);

        if (!valido)
            return (false, $"Transição inválida: {ns.Status} → {dto.NovoStatus}");

        ns.Status = dto.NovoStatus;
        ns.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    private async Task<string> GerarCodigo()
    {
        var agora = DateTime.UtcNow;
        var idadeEmpresa = agora.Year - AnoFundacao;
        var mes = agora.Month;
        var ano = agora.Year % 100; // 2 últimos dígitos

        var prefixo = $"{idadeEmpresa:D2}.{mes:D2}.{ano:D2}";

        var ultimo = await _context.NumerosSerie
            .Where(n => n.Codigo.StartsWith(prefixo))
            .OrderByDescending(n => n.Codigo)
            .FirstOrDefaultAsync();

        var sequencial = 1;

        if (ultimo != null)
        {
            var partes = ultimo.Codigo.Split('.');
            if (partes.Length == 4 && int.TryParse(partes[3], out var num))
                sequencial = num + 1;
        }

        return $"{prefixo}.{sequencial:D5}";
    }

    private static bool ValidarTransicaoStatus(StatusNumeroSerie atual, StatusNumeroSerie novo)
    {
        return (atual, novo) switch
        {
            (StatusNumeroSerie.Aberto, StatusNumeroSerie.EmFabricacao) => true,
            (StatusNumeroSerie.EmFabricacao, StatusNumeroSerie.Concluido) => true,
            (StatusNumeroSerie.Concluido, StatusNumeroSerie.Entregue) => true,
            _ => false
        };
    }

    private static NumeroSerieResponseDTO ToResponseDTO(NumeroSerie n) => new()
    {
        Id = n.Id,
        Codigo = n.Codigo,
        PedidoVendaId = n.PedidoVendaId,
        PedidoVendaCodigo = n.PedidoVenda.Codigo,
        ClienteNome = n.PedidoVenda.Cliente.Pessoa.Nome,
        Status = n.Status,
        CriadoEm = n.CriadoEm,
        ModificadoEm = n.ModificadoEm
    };
}