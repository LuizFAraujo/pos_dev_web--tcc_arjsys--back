using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço do Número de Série.
/// NS não tem tipo nem status próprios - herda do PV vinculado (exibição readonly).
/// Relação 1:1 com PV.
/// Criação manual só para PV tipo PreVenda em status AguardandoNS.
/// PV tipo Normal recebe NS automaticamente via Ordem de Produção (Fase 3b).
/// Produto vinculado deve ser BOM (ter pelo menos 1 filho em EstruturasProdutos).
///
/// Pré-requisito: ConfiguracaoEmpresa.Configurado = true.
/// Sem isso, qualquer Create de NS é rejeitado com erro descritivo.
///
/// Código:
/// - Formato: II.MM.AA.NNNNN (idade da empresa, mês, ano, sequencial 5 dígitos).
/// - AnoFundacao vem da ConfiguracaoEmpresa (Admin) - antes era const hardcoded.
/// - Sequencial NNNNN é único GLOBAL (entre todos os NS, qualquer prefixo).
/// - Pode ser informado manualmente no Create (valida formato + unicidade).
/// - Não pode ser alterado no Update (NumeroSerieUpdateDTO não tem campo Codigo).
///
/// Transição automática de status:
/// - Após Create bem-sucedido, o PV vinculado é transicionado de AguardandoNS para
///   RecebidoNS via PedidoVendaService.AlterarStatus (registra histórico com evento
///   NsRecebido). Sem justificativa - é fluxo natural.
/// </summary>
public class NumeroSerieService(
    AppDbContext context,
    ConfiguracaoEmpresaService configEmpresa,
    PedidoVendaService pedidoService)
{
    private readonly AppDbContext _context = context;
    private readonly ConfiguracaoEmpresaService _configEmpresa = configEmpresa;
    private readonly PedidoVendaService _pedidoService = pedidoService;

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
    /// - ConfiguracaoEmpresa.Configurado deve ser true (ano de fundação setado);
    /// - PV deve ser do tipo PreVenda;
    /// - PV deve estar em status AguardandoNS;
    /// - 1 PV = 1 NS (rejeita se já existe NS vinculado);
    /// - Se ProdutoId informado, Produto deve existir e ter BOM;
    /// - Se Codigo informado, valida formato e unicidade (completo + sequencial isolado);
    /// - Se Codigo omitido, gera automaticamente.
    ///
    /// Após criar o NS, transiciona o PV de AguardandoNS para RecebidoNS
    /// (via PedidoVendaService.AlterarStatus - registra histórico, evento NsRecebido).
    /// Se a transição falhar, o NS já foi persistido e a falha é reportada como erro
    /// (caso raro: transição é válida e não exige justificativa).
    /// </summary>
    public async Task<(NumeroSerieResponseDTO? Item, string? Erro)> Create(NumeroSerieCreateDTO dto)
    {
        // Pré-requisito: empresa precisa ter o ano de fundação confirmado.
        var configurado = await _configEmpresa.IsConfigurado();
        if (!configurado)
            return (null, "Configure o ano de fundação da empresa antes de emitir Números de Série.");

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

        // Carrega TODOS os códigos existentes em uma única query - usado tanto pelo
        // ValidarCodigoManual quanto pelo GerarCodigo (evita duas queries pesadas).
        var codigosExistentes = await _context.NumerosSerie
            .Select(n => n.Codigo)
            .ToListAsync();

        string codigo;

        if (!string.IsNullOrWhiteSpace(dto.Codigo))
        {
            // Manual: valida formato + unicidade do código completo + unicidade do sequencial isolado
            var (ok, erro) = ValidarCodigoManual(dto.Codigo, codigosExistentes);
            if (!ok)
                return (null, erro);
            codigo = dto.Codigo;
        }
        else
        {
            // Automático: monta prefixo a partir da config + acha próximo sequencial global
            codigo = await GerarCodigo(codigosExistentes);
        }

        var ns = new NumeroSerie
        {
            Codigo = codigo,
            PedidoVendaId = dto.PedidoVendaId,
            ProdutoId = dto.ProdutoId,
            CriadoEm = DateTime.UtcNow
        };

        _context.NumerosSerie.Add(ns);

        // Sincroniza Projeto do PV: se o NS tem produto, ele é o Projeto BOM
        // do PV. Mantém PedidoVenda.ProdutoBomId em fase com o NS pra Produção
        // ler num lugar só (sem precisar de fallback NS).
        if (dto.ProdutoId.HasValue && pedido.ProdutoBomId != dto.ProdutoId)
        {
            pedido.ProdutoBomId = dto.ProdutoId;
            pedido.ModificadoEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Transição automática: AguardandoNS → RecebidoNS.
        // PV chegou aqui obrigatoriamente em AguardandoNS (validado acima),
        // então a transição é sempre aplicável.
        var (transOk, transErro) = await _pedidoService.AlterarStatus(
            pedido.Id,
            new StatusPedidoVendaDTO { NovoStatus = StatusPedidoVenda.RecebidoNS });

        if (!transOk)
            return (null, $"NS criado, mas falha ao transicionar PV para RecebidoNS: {transErro}");

        // Recarrega com includes pra response
        return await GetById(ns.Id) is { } response
            ? (response, null)
            : (null, "Erro ao recarregar NS criado");
    }

    /// <summary>
    /// Atualiza o Produto vinculado ao NS. Valida que o Produto tem BOM.
    /// Codigo NÃO pode ser alterado aqui - não há campo no UpdateDTO.
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

        // Sincroniza Projeto do PV vinculado.
        var pedido = await _context.PedidosVenda.FindAsync(ns.PedidoVendaId);
        if (pedido != null && pedido.ProdutoBomId != dto.ProdutoId)
        {
            pedido.ProdutoBomId = dto.ProdutoId;
            pedido.ModificadoEm = DateTime.UtcNow;
        }

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
    /// Valida código informado manualmente:
    /// - Formato: II.MM.AA.NNNNN (4 partes separadas por ponto, larguras 2/2/2/5, todas numéricas).
    /// - MM no intervalo 01..12.
    /// - Código completo único (não pode existir igual no banco).
    /// - Sequencial NNNNN único globalmente - não pode coincidir com sequencial de
    ///   nenhum outro NS, mesmo que o prefixo seja diferente.
    /// </summary>
    private static (bool Ok, string? Erro) ValidarCodigoManual(string codigo, List<string> codigosExistentes)
    {
        var partes = codigo.Split('.');
        if (partes.Length != 4)
            return (false, "Código inválido. Formato esperado: II.MM.AA.NNNNN");

        if (partes[0].Length != 2 || !int.TryParse(partes[0], out _))
            return (false, "Idade da empresa inválida (II = 2 dígitos numéricos)");

        if (partes[1].Length != 2 || !int.TryParse(partes[1], out var mes) || mes < 1 || mes > 12)
            return (false, "Mês inválido (MM = 01..12)");

        if (partes[2].Length != 2 || !int.TryParse(partes[2], out _))
            return (false, "Ano inválido (AA = 2 dígitos numéricos)");

        if (partes[3].Length != 5 || !int.TryParse(partes[3], out _))
            return (false, "Sequencial inválido (NNNNN = 5 dígitos numéricos)");

        // Código completo já existe?
        if (codigosExistentes.Contains(codigo))
            return (false, $"Código '{codigo}' já está em uso.");

        // Sequencial isolado já apareceu em algum outro código?
        var sequencialNovo = partes[3];
        foreach (var existente in codigosExistentes)
        {
            var p = existente.Split('.');
            if (p.Length == 4 && p[3] == sequencialNovo)
                return (false, $"Sequencial '{sequencialNovo}' já foi usado em outro Número de Série.");
        }

        return (true, null);
    }

    /// <summary>
    /// Gera o próximo código no formato II.MM.AA.NNNNN.
    /// AnoFundacao vem da ConfiguracaoEmpresa.
    /// Sequencial NNNNN é o próximo após o MAIOR sequencial GLOBAL existente
    /// (não reinicia por mês/ano/idade - a série é única e crescente).
    /// </summary>
    private async Task<string> GerarCodigo(List<string> codigosExistentes)
    {
        var anoFundacao = await _configEmpresa.GetAnoFundacao();
        var agora = DateTime.UtcNow;
        var idadeEmpresa = agora.Year - anoFundacao;
        var mes = agora.Month;
        var ano = agora.Year % 100;

        var prefixo = $"{idadeEmpresa:D2}.{mes:D2}.{ano:D2}";

        // Acha o maior sequencial GLOBAL - qualquer prefixo conta.
        int maiorSeq = 0;
        foreach (var c in codigosExistentes)
        {
            var p = c.Split('.');
            if (p.Length == 4 && int.TryParse(p[3], out var num) && num > maiorSeq)
                maiorSeq = num;
        }

        var proximoSeq = maiorSeq + 1;
        return $"{prefixo}.{proximoSeq:D5}";
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
