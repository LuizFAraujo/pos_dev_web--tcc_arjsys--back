using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

public class PedidoVendaService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<PedidoVendaResponseDTO>> GetAll(int pagina = 0, int tamanho = 0)
    {
        var query = _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .OrderByDescending(p => p.Data);

        List<PedidoVenda> pedidos;

        if (pagina > 0 && tamanho > 0)
            pedidos = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            pedidos = await query.ToListAsync();

        var resultado = new List<PedidoVendaResponseDTO>();

        foreach (var p in pedidos)
        {
            var itens = await _context.PedidosVendaItens
                .Where(i => i.PedidoVendaId == p.Id)
                .Include(i => i.Produto)
                .ToListAsync();

            resultado.Add(ToResponseDTO(p, itens));
        }

        return resultado;
    }

    public async Task<PedidoVendaResponseDTO?> GetById(int id)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return null;

        var itens = await _context.PedidosVendaItens
            .Where(i => i.PedidoVendaId == id)
            .Include(i => i.Produto)
            .ToListAsync();

        return ToResponseDTO(pedido, itens);
    }

    /// <summary>
    /// Cria Pedido de Venda. Status inicial: Aguardando (venda futura) ou EmAndamento (default).
    /// </summary>
    public async Task<(PedidoVendaResponseDTO? Item, string? Erro)> Create(PedidoVendaCreateDTO dto)
    {
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);

        if (cliente == null)
            return (null, "Cliente não encontrado");

        // Validar status inicial
        var statusInicial = dto.Status ?? StatusPedidoVenda.EmAndamento;

        if (statusInicial != StatusPedidoVenda.Aguardando && statusInicial != StatusPedidoVenda.EmAndamento)
            return (null, "Status inicial deve ser Aguardando ou EmAndamento");

        var codigo = await GerarCodigo();

        var pedido = new PedidoVenda
        {
            Codigo = codigo,
            ClienteId = dto.ClienteId,
            Status = statusInicial,
            Data = DateTime.UtcNow,
            Observacoes = dto.Observacoes,
            CriadoEm = DateTime.UtcNow
        };

        _context.PedidosVenda.Add(pedido);
        await _context.SaveChangesAsync();

        await _context.Entry(pedido).Reference(p => p.Cliente).LoadAsync();
        await _context.Entry(pedido.Cliente).Reference(c => c.Pessoa).LoadAsync();

        return (ToResponseDTO(pedido, []), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, PedidoVendaCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Aguardando && pedido.Status != StatusPedidoVenda.EmAndamento)
            return (false, "Só é possível editar pedidos com status Aguardando ou Em Andamento");

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);

        if (cliente == null)
            return (false, "Cliente não encontrado");

        pedido.ClienteId = dto.ClienteId;
        pedido.Observacoes = dto.Observacoes;
        pedido.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Altera status do PV com validação de transição.
    /// Automação: PV Aguardando → EmAndamento muda NS Aguardando vinculados para EmAndamento.
    /// Automação: PV → Cancelado muda NS vinculados (que não sejam Entregue) para Cancelado.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> AlterarStatus(int id, StatusPedidoVendaDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        var transicaoValida = ValidarTransicaoStatus(pedido.Status, dto.NovoStatus);

        if (!transicaoValida)
            return (false, $"Transição inválida: {pedido.Status} → {dto.NovoStatus}");

        var statusAnterior = pedido.Status;
        pedido.Status = dto.NovoStatus;
        pedido.ModificadoEm = DateTime.UtcNow;

        // Automação: PV Aguardando → EmAndamento → NS Aguardando viram EmAndamento
        if (statusAnterior == StatusPedidoVenda.Aguardando && dto.NovoStatus == StatusPedidoVenda.EmAndamento)
        {
            var nsAguardando = await _context.NumerosSerie
                .Where(n => n.PedidoVendaId == id && n.Status == StatusNumeroSerie.Aguardando)
                .ToListAsync();

            foreach (var ns in nsAguardando)
            {
                ns.Status = StatusNumeroSerie.EmAndamento;
                ns.ModificadoEm = DateTime.UtcNow;
            }
        }

        // Automação: PV → Cancelado → NS que não são Entregue viram Cancelado
        if (dto.NovoStatus == StatusPedidoVenda.Cancelado)
        {
            var nsParaCancelar = await _context.NumerosSerie
                .Where(n => n.PedidoVendaId == id && n.Status != StatusNumeroSerie.Entregue)
                .ToListAsync();

            foreach (var ns in nsParaCancelar)
            {
                ns.Status = StatusNumeroSerie.Cancelado;
                ns.ModificadoEm = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Aguardando)
            return (false, "Só é possível excluir pedidos com status Aguardando");

        var temNS = await _context.NumerosSerie.AnyAsync(n => n.PedidoVendaId == id);

        if (temNS)
            return (false, "Não é possível excluir pedido com Número de Série gerado");

        var itens = await _context.PedidosVendaItens.Where(i => i.PedidoVendaId == id).ToListAsync();
        _context.PedidosVendaItens.RemoveRange(itens);
        _context.PedidosVenda.Remove(pedido);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private async Task<string> GerarCodigo()
    {
        var agora = DateTime.UtcNow;
        var prefixo = $"PV.{agora:yyyy}.{agora:MM}";

        var ultimo = await _context.PedidosVenda
            .Where(p => p.Codigo.StartsWith(prefixo))
            .OrderByDescending(p => p.Codigo)
            .FirstOrDefaultAsync();

        var sequencial = 1;

        if (ultimo != null)
        {
            var partes = ultimo.Codigo.Split('.');
            if (partes.Length == 4 && int.TryParse(partes[3], out var num))
                sequencial = num + 1;
        }

        return $"{prefixo}.{sequencial:D4}";
    }

    private static bool ValidarTransicaoStatus(StatusPedidoVenda atual, StatusPedidoVenda novo)
    {
        return (atual, novo) switch
        {
            (StatusPedidoVenda.Aguardando, StatusPedidoVenda.EmAndamento) => true,
            (StatusPedidoVenda.Aguardando, StatusPedidoVenda.Cancelado) => true,
            (StatusPedidoVenda.EmAndamento, StatusPedidoVenda.Pausado) => true,
            (StatusPedidoVenda.EmAndamento, StatusPedidoVenda.Concluido) => true,
            (StatusPedidoVenda.EmAndamento, StatusPedidoVenda.Cancelado) => true,
            (StatusPedidoVenda.Pausado, StatusPedidoVenda.EmAndamento) => true,
            (StatusPedidoVenda.Pausado, StatusPedidoVenda.Cancelado) => true,
            (StatusPedidoVenda.Concluido, StatusPedidoVenda.AguardandoEntrega) => true,
            (StatusPedidoVenda.AguardandoEntrega, StatusPedidoVenda.Entregue) => true,

            // Retorno (correção de erro)
            (StatusPedidoVenda.Concluido, StatusPedidoVenda.EmAndamento) => true,
            (StatusPedidoVenda.AguardandoEntrega, StatusPedidoVenda.Concluido) => true,
            (StatusPedidoVenda.AguardandoEntrega, StatusPedidoVenda.EmAndamento) => true,
            _ => false
        };
    }

    private static PedidoVendaResponseDTO ToResponseDTO(PedidoVenda p, List<PedidoVendaItem> itens) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        ClienteId = p.ClienteId,
        ClienteNome = p.Cliente.Pessoa.Nome,
        Status = p.Status,
        Data = p.Data,
        Observacoes = p.Observacoes,
        Total = itens.Sum(i => i.Quantidade * i.PrecoUnitario),
        Itens = itens.Select(i => new PedidoVendaItemResponseDTO
        {
            Id = i.Id,
            PedidoVendaId = i.PedidoVendaId,
            ProdutoId = i.ProdutoId,
            ProdutoCodigo = i.Produto.Codigo,
            ProdutoDescricao = i.Produto.Descricao,
            Quantidade = i.Quantidade,
            PrecoUnitario = i.PrecoUnitario,
            Subtotal = i.Quantidade * i.PrecoUnitario,
            CriadoEm = i.CriadoEm
        }).ToList(),
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}