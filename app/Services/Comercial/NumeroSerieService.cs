using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço do Número de Série.
/// NS não tem tipo nem status próprios — herda do PV vinculado (exibição readonly).
/// Relação 1:1 com PV.
/// Criação manual só para PV tipo PreVenda em status AguardandoNS.
/// PV tipo Normal recebe NS automaticamente via Ordem de Produção (Fase 3b).
/// Produto vinculado deve ser BOM (ter pelo menos 1 filho em EstruturasProdutos).
/// </summary>
public class NumeroSerieService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    // Ano de fundação da empresa — usado para calcular a idade
    private const int AnoFundacao = 1966;

    /// <summary>
    /// Lista todos os NS com dados do PV e Produto vinculados. Suporta paginação.
    /// </summary>
    public async Task<List<NumeroSerieResponseDTO>> GetAll(int pagina = 0, int tamanho = 0)
    {
        var query = _context.NumerosSerie
            .Include(n => n.PedidoVenda)
                .ThenInclude(p => p.Cliente)
                    .ThenInclude(c => c.Pessoa)
            .Include(n => n.Produto)
            .OrderByDescending(n => n.CriadoEm);

        List<NumeroSerie> lista;

        if (pagina > 0 && tamanho > 0)
            lista = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            lista = await query.ToListAsync();

        return lista.Select(ToResponseDTO).ToList();
    }

    /// <summary>
    /// Busca NS por ID com dados do PV e Produto vinculados.
    /// </summary>
    public async Task<NumeroSerieResponseDTO?> GetById(int id)
    {
        var ns = await _context.NumerosSerie
            .Include(n => n.PedidoVenda)
                .ThenInclude(p => p.Cliente)
                    .ThenInclude(c => c.Pessoa)
            .Include(n => n.Produto)
            .FirstOrDefaultAsync(n => n.Id == id);

        return ns == null ? null : ToResponseDTO(ns);
    }

    /// <summary>
    /// Busca o NS vinculado a um PV (relação 1:1). Retorna null se o PV não tiver NS.
    /// </summary>
    public async Task<NumeroSerieResponseDTO?> GetByPedidoId(int pedidoId)
    {
        var ns = await _context.NumerosSerie
            .Include(n => n.PedidoVenda)
                .ThenInclude(p => p.Cliente)
                    .ThenInclude(c => c.Pessoa)
            .Include(n => n.Produto)
            .FirstOrDefaultAsync(n => n.PedidoVendaId == pedidoId);

        return ns == null ? null : ToResponseDTO(ns);
    }

    /// <summary>
    /// Cria NS vinculado a um PV. Regras:
    /// - PV deve ser do tipo PreVenda;
    /// - PV deve estar em status AguardandoNS;
    /// - 1 PV = 1 NS (rejeita se já existe NS vinculado);
    /// - Se ProdutoId informado, Produto deve existir e ter BOM.
    /// </summary>
    public async Task<(NumeroSerieResponseDTO? Item, string? Erro)> Create(NumeroSerieCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == dto.PedidoVendaId);

        if (pedido == null)
            return (null, "Pedido não encontrado");

        if (pedido.Tipo != TipoPedidoVenda.PreVenda)
            return (null, "Criação manual de NS só é permitida para PV do tipo PreVenda");

        if (pedido.Status != StatusPedidoVenda.AguardandoNS)
            return (null, "PV PreVenda só aceita NS em status AguardandoNS");

        var jaExiste = await _context.NumerosSerie.AnyAsync(n => n.PedidoVendaId == dto.PedidoVendaId);

        if (jaExiste)
            return (null, "Este pedido já possui um Número de Série vinculado");

        if (dto.ProdutoId.HasValue)
        {
            var erroProduto = await ValidarProdutoBom(dto.ProdutoId.Value);
            if (erroProduto != null)
                return (null, erroProduto);
        }

        var codigo = await GerarCodigo();

        var ns = new NumeroSerie
        {
            Codigo = codigo,
            PedidoVendaId = dto.PedidoVendaId,
            ProdutoId = dto.ProdutoId,
            CriadoEm = DateTime.UtcNow
        };

        _context.NumerosSerie.Add(ns);
        await _context.SaveChangesAsync();

        // Recarrega com includes pra response
        return await GetById(ns.Id) is { } response
            ? (response, null)
            : (null, "Erro ao recarregar NS criado");
    }

    /// <summary>
    /// Atualiza o Produto vinculado ao NS. Valida que o Produto tem BOM.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int id, NumeroSerieUpdateDTO dto)
    {
        var ns = await _context.NumerosSerie.FindAsync(id);

        if (ns == null)
            return (false, null);

        if (dto.ProdutoId.HasValue)
        {
            var erroProduto = await ValidarProdutoBom(dto.ProdutoId.Value);
            if (erroProduto != null)
                return (false, erroProduto);
        }

        ns.ProdutoId = dto.ProdutoId;
        ns.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Valida que o produto existe e é um BOM (tem pelo menos 1 filho em EstruturasProdutos).
    /// Retorna null se OK, ou mensagem de erro se inválido.
    /// </summary>
    private async Task<string?> ValidarProdutoBom(int produtoId)
    {
        var produto = await _context.Produtos.FindAsync(produtoId);

        if (produto == null)
            return "Produto não encontrado";

        if (!produto.Ativo)
            return "Produto está inativo";

        var temBom = await _context.EstruturasProdutos
            .AnyAsync(e => e.ProdutoPaiId == produtoId);

        if (!temBom)
            return "Produto precisa ter estrutura (BOM) para ser vinculado ao NS";

        return null;
    }

    /// <summary>
    /// Gera o próximo código no formato II.MM.AA.NNNNN.
    /// </summary>
    private async Task<string> GerarCodigo()
    {
        var agora = DateTime.UtcNow;
        var idadeEmpresa = agora.Year - AnoFundacao;
        var mes = agora.Month;
        var ano = agora.Year % 100;

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

    /// <summary>
    /// Converte entidade em DTO de resposta, incluindo dados readonly do PV e do Produto.
    /// </summary>
    private static NumeroSerieResponseDTO ToResponseDTO(NumeroSerie n) => new()
    {
        Id = n.Id,
        Codigo = n.Codigo,
        PedidoVendaId = n.PedidoVendaId,
        PedidoVendaCodigo = n.PedidoVenda?.Codigo ?? string.Empty,
        ClienteCodigo = n.PedidoVenda?.Cliente?.Pessoa?.Codigo ?? string.Empty,
        ClienteNome = n.PedidoVenda?.Cliente?.Pessoa?.Nome ?? string.Empty,
        PvTipo = n.PedidoVenda?.Tipo ?? TipoPedidoVenda.Normal,
        PvStatus = n.PedidoVenda?.Status ?? StatusPedidoVenda.Liberado,
        PvDataEntrega = n.PedidoVenda?.DataEntrega,
        ProdutoId = n.ProdutoId,
        ProdutoCodigo = n.Produto?.Codigo,
        ProdutoDescricao = n.Produto?.Descricao,
        CriadoEm = n.CriadoEm,
        ModificadoEm = n.ModificadoEm
    };
}
