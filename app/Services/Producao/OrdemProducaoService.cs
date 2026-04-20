using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Producao;
using Api_ArjSys_Tcc.Models.Producao.Enums;
using Api_ArjSys_Tcc.DTOs.Producao;

namespace Api_ArjSys_Tcc.Services.Producao;

/// <summary>
/// Serviço de Ordem de Produção.
/// Gerencia CRUD, hierarquia Master/Filha, transições de status,
/// apontamentos de produção e divergências com BOM.
/// Master e Filhas têm status independentes.
/// </summary>
public class OrdemProducaoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    // ========================================================================
    // LISTAGEM
    // ========================================================================

    /// <summary>
    /// Lista todas as OPs. Suporta paginação.
    /// </summary>
    public async Task<List<OrdemProducaoResponseDTO>> GetAll(int pagina = 0, int tamanho = 0)
    {
        var query = _context.OrdensProducao
            .Include(o => o.PedidoVenda).ThenInclude(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .Include(o => o.Produto)
            .Include(o => o.OrdemPai)
            .OrderByDescending(o => o.CriadoEm);

        List<OrdemProducao> lista;

        if (pagina > 0 && tamanho > 0)
            lista = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            lista = await query.ToListAsync();

        var resultado = new List<OrdemProducaoResponseDTO>();
        foreach (var op in lista)
            resultado.Add(await ToResponseDTO(op));

        return resultado;
    }

    /// <summary>
    /// Busca OP por ID, com itens e filhas.
    /// </summary>
    public async Task<OrdemProducaoResponseDTO?> GetById(int id)
    {
        var op = await _context.OrdensProducao
            .Include(o => o.PedidoVenda).ThenInclude(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .Include(o => o.Produto)
            .Include(o => o.OrdemPai)
            .FirstOrDefaultAsync(o => o.Id == id);

        return op == null ? null : await ToResponseDTO(op);
    }

    /// <summary>
    /// Lista OPs de um Pedido de Venda (Master + Filhas).
    /// </summary>
    public async Task<List<OrdemProducaoResponseDTO>> GetByPedidoVenda(int pedidoVendaId)
    {
        var lista = await _context.OrdensProducao
            .Include(o => o.PedidoVenda).ThenInclude(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .Include(o => o.Produto)
            .Include(o => o.OrdemPai)
            .Where(o => o.PedidoVendaId == pedidoVendaId)
            .OrderBy(o => o.Codigo)
            .ToListAsync();

        var resultado = new List<OrdemProducaoResponseDTO>();
        foreach (var op in lista)
            resultado.Add(await ToResponseDTO(op));

        return resultado;
    }

    // ========================================================================
    // CRIAÇÃO — MASTER
    // ========================================================================

    /// <summary>
    /// Cria OP Master. Liga ao PV + Produto raiz.
    /// Produto deve ter BOM (ser pai em EstruturasProdutos).
    /// </summary>
    public async Task<(OrdemProducaoResponseDTO? Item, string? Erro)> CriarMaster(OrdemProducaoMasterCreateDTO dto)
    {
        var pv = await _context.PedidosVenda.FindAsync(dto.PedidoVendaId);
        if (pv == null)
            return (null, "Pedido de Venda não encontrado");

        var produto = await _context.Produtos.FindAsync(dto.ProdutoId);
        if (produto == null)
            return (null, "Produto não encontrado");

        if (!produto.Ativo)
            return (null, "Produto está inativo");

        var temBom = await _context.EstruturasProdutos.AnyAsync(e => e.ProdutoPaiId == dto.ProdutoId);
        if (!temBom)
            return (null, "Produto deve ter BOM (estrutura) para virar OP Master");

        var codigo = await GerarCodigoMaster();
        var agora = DateTime.UtcNow;

        var op = new OrdemProducao
        {
            Codigo = codigo,
            PedidoVendaId = dto.PedidoVendaId,
            ProdutoId = dto.ProdutoId,
            OrdemPaiId = null,
            Status = StatusOrdemProducao.Pendente,
            Observacoes = dto.Observacoes,
            CriadoEm = agora
        };

        _context.OrdensProducao.Add(op);
        await _context.SaveChangesAsync();

        RegistrarEvento(op.Id, EventoOrdemProducao.Criada, null, StatusOrdemProducao.Pendente, null, null);
        await _context.SaveChangesAsync();

        return await GetById(op.Id) is { } response
            ? (response, null)
            : (null, "Erro ao carregar OP criada");
    }

    // ========================================================================
    // CRIAÇÃO — FILHA
    // ========================================================================

    /// <summary>
    /// Cria OP Filha vinculada a uma Master.
    /// Regras:
    /// - Master deve existir;
    /// - Produto deve existir na BOM explodida da Master;
    /// - Mesmo Produto não pode já ter OP Filha ativa nessa Master;
    /// - QuantidadePlanejada do item é snapshot da BOM.
    /// </summary>
    public async Task<(OrdemProducaoResponseDTO? Item, string? Erro)> CriarFilha(OrdemProducaoFilhaCreateDTO dto)
    {
        var master = await _context.OrdensProducao.FindAsync(dto.OrdemPaiId);
        if (master == null)
            return (null, "OP Master não encontrada");

        if (master.OrdemPaiId != null)
            return (null, "Só é possível criar filha a partir de uma OP Master (OP Filha não pode ter sub-filhas)");

        if (master.Status == StatusOrdemProducao.Cancelada)
            return (null, "Não é possível criar filha de OP Master cancelada");

        var produto = await _context.Produtos.FindAsync(dto.ProdutoId);
        if (produto == null)
            return (null, "Produto não encontrado");

        // Valida que o Produto existe na BOM explodida da Master
        var explosao = await ExplodirBom(master.ProdutoId);
        if (!explosao.TryGetValue(dto.ProdutoId, out var qtdNaBom))
            return (null, $"Produto não faz parte da BOM do produto raiz da Master ({master.ProdutoId})");

        // Verifica se já existe filha ativa pra esse Produto na mesma Master
        var jaExiste = await _context.OrdensProducao
            .AnyAsync(o => o.OrdemPaiId == master.Id
                        && o.ProdutoId == dto.ProdutoId
                        && o.Status != StatusOrdemProducao.Cancelada);

        if (jaExiste)
            return (null, "Já existe OP Filha ativa para este Produto nesta Master");

        var codigoFilha = await GerarCodigoFilha(master.Codigo, master.Id);
        var agora = DateTime.UtcNow;

        var op = new OrdemProducao
        {
            Codigo = codigoFilha,
            PedidoVendaId = master.PedidoVendaId,
            ProdutoId = dto.ProdutoId,
            OrdemPaiId = master.Id,
            Status = StatusOrdemProducao.Pendente,
            Observacoes = dto.Observacoes,
            CriadoEm = agora
        };

        _context.OrdensProducao.Add(op);
        await _context.SaveChangesAsync();

        // Cria o item principal (snapshot da BOM)
        var item = new OrdemProducaoItem
        {
            OrdemProducaoId = op.Id,
            ProdutoId = dto.ProdutoId,
            QuantidadePlanejada = qtdNaBom,
            QuantidadeProduzida = 0,
            CriadoEm = agora
        };
        _context.OrdensProducaoItens.Add(item);

        RegistrarEvento(op.Id, EventoOrdemProducao.Criada, null, StatusOrdemProducao.Pendente, null, null);
        await _context.SaveChangesAsync();

        return await GetById(op.Id) is { } response
            ? (response, null)
            : (null, "Erro ao carregar OP criada");
    }

    // ========================================================================
    // EDIÇÃO / EXCLUSÃO
    // ========================================================================

    /// <summary>
    /// Atualiza observações da OP. Permitido em qualquer status exceto Concluida e Cancelada.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int id, OrdemProducaoUpdateDTO dto)
    {
        var op = await _context.OrdensProducao.FindAsync(id);
        if (op == null)
            return (false, null);

        if (op.Status == StatusOrdemProducao.Concluida || op.Status == StatusOrdemProducao.Cancelada)
            return (false, "Não é possível editar OP Concluida ou Cancelada");

        op.Observacoes = dto.Observacoes;
        op.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Exclui OP. Permitido apenas em Pendente, sem produção apontada.
    /// Se for Master, apaga filhas em cascata (Pendentes).
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var op = await _context.OrdensProducao.FindAsync(id);
        if (op == null)
            return (false, null);

        if (op.Status != StatusOrdemProducao.Pendente)
            return (false, "Só é possível excluir OP em status Pendente");

        var temApontamento = await _context.OrdensProducaoItens
            .AnyAsync(i => i.OrdemProducaoId == id && i.QuantidadeProduzida > 0);
        if (temApontamento)
            return (false, "Não é possível excluir OP com produção já apontada");

        // Se é Master, verifica filhas
        if (op.OrdemPaiId == null)
        {
            var filhasNaoPendentes = await _context.OrdensProducao
                .AnyAsync(o => o.OrdemPaiId == id && o.Status != StatusOrdemProducao.Pendente);
            if (filhasNaoPendentes)
                return (false, "Não é possível excluir Master com filhas em andamento/concluídas");
        }

        _context.OrdensProducao.Remove(op);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    // ========================================================================
    // TRANSIÇÃO DE STATUS
    // ========================================================================

    /// <summary>
    /// Altera status da OP. Regras:
    /// - Concluida e Cancelada são terminais;
    /// - Pausada/Cancelada exigem justificativa;
    /// - Pendente → Andamento, Andamento → Pausada/Concluida/Cancelada, Pausada → Andamento/Cancelada.
    /// Ao passar para Andamento, registra DataInicio.
    /// Ao passar para Concluida ou Cancelada, registra DataFim.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> AlterarStatus(int id, OrdemProducaoStatusDTO dto)
    {
        var op = await _context.OrdensProducao.FindAsync(id);
        if (op == null)
            return (false, null);

        var anterior = op.Status;
        var novo = dto.NovoStatus;

        if (anterior == novo)
            return (false, $"A OP já está com status {novo}");

        var (valida, erro) = TransicaoPermitida(anterior, novo);
        if (!valida)
            return (false, erro);

        var exigeJustificativa = novo == StatusOrdemProducao.Pausada || novo == StatusOrdemProducao.Cancelada;
        var justificativa = dto.Justificativa?.Trim();

        if (exigeJustificativa && string.IsNullOrWhiteSpace(justificativa))
            return (false, "Justificativa é obrigatória para esta transição de status");

        op.Status = novo;
        op.ModificadoEm = DateTime.UtcNow;

        if (novo == StatusOrdemProducao.Andamento && op.DataInicio == null)
            op.DataInicio = DateTime.UtcNow;

        if (novo == StatusOrdemProducao.Concluida || novo == StatusOrdemProducao.Cancelada)
            op.DataFim = DateTime.UtcNow;

        var evento = MapearEvento(anterior, novo);
        RegistrarEvento(id, evento, anterior, novo, justificativa, null);

        await _context.SaveChangesAsync();
        return (true, null);
    }

    // ========================================================================
    // APONTAMENTO DE PRODUÇÃO
    // ========================================================================

    /// <summary>
    /// Aponta produção num item da OP. Soma na QuantidadeProduzida.
    /// Rejeita se:
    /// - OP não está em Andamento;
    /// - Quantidade &lt;= 0;
    /// - Ultrapassa QuantidadePlanejada.
    /// Se todos os itens ficam com Produzida == Planejada, OP vai auto para Concluida.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Apontar(int opId, int itemId, OrdemProducaoApontamentoDTO dto)
    {
        var op = await _context.OrdensProducao.FindAsync(opId);
        if (op == null)
            return (false, null);

        if (op.Status != StatusOrdemProducao.Andamento)
            return (false, "Só é possível apontar produção em OP com status Andamento");

        var item = await _context.OrdensProducaoItens
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrdemProducaoId == opId);
        if (item == null)
            return (false, "Item não encontrado nesta OP");

        if (dto.Quantidade <= 0)
            return (false, "Quantidade deve ser maior que zero");

        var nova = item.QuantidadeProduzida + dto.Quantidade;
        if (nova > item.QuantidadePlanejada)
            return (false, $"Quantidade ultrapassa o planejado ({item.QuantidadePlanejada})");

        item.QuantidadeProduzida = nova;
        item.ModificadoEm = DateTime.UtcNow;

        var produto = await _context.Produtos.FindAsync(item.ProdutoId);
        var detalhe = $"{produto?.Codigo ?? "?"} +{dto.Quantidade} (total {nova}/{item.QuantidadePlanejada})";
        RegistrarEvento(opId, EventoOrdemProducao.Apontamento, null, null, dto.Observacao, detalhe);

        await _context.SaveChangesAsync();

        // Auto-conclusão: todos os itens com Produzida == Planejada
        var itens = await _context.OrdensProducaoItens
            .Where(i => i.OrdemProducaoId == opId)
            .ToListAsync();

        if (itens.Count > 0 && itens.All(i => i.QuantidadeProduzida >= i.QuantidadePlanejada))
        {
            op.Status = StatusOrdemProducao.Concluida;
            op.DataFim = DateTime.UtcNow;
            op.ModificadoEm = DateTime.UtcNow;
            RegistrarEvento(opId, EventoOrdemProducao.Concluida, StatusOrdemProducao.Andamento,
                StatusOrdemProducao.Concluida, "Auto-concluída: todos os itens atingiram o planejado", null);
            await _context.SaveChangesAsync();
        }

        return (true, null);
    }

    // ========================================================================
    // STATUS DE PRODUÇÃO (percentual)
    // ========================================================================

    /// <summary>
    /// Retorna o status de produção consolidado da OP (cada item com planejada/produzida/percentual).
    /// </summary>
    public async Task<OrdemProducaoStatusProducaoDTO?> GetStatusProducao(int id)
    {
        var op = await _context.OrdensProducao.FindAsync(id);
        if (op == null)
            return null;

        var itens = await _context.OrdensProducaoItens
            .Include(i => i.Produto)
            .Where(i => i.OrdemProducaoId == id)
            .OrderBy(i => i.Produto.Codigo)
            .ToListAsync();

        var itensDto = itens.Select(ToItemResponseDTO).ToList();

        decimal totalPlanejado = itens.Sum(i => i.QuantidadePlanejada);
        decimal totalProduzido = itens.Sum(i => i.QuantidadeProduzida);
        decimal percentualTotal = totalPlanejado > 0
            ? Math.Round(totalProduzido / totalPlanejado * 100m, 2)
            : 0m;

        return new OrdemProducaoStatusProducaoDTO
        {
            OrdemProducaoId = op.Id,
            Codigo = op.Codigo,
            Status = op.Status,
            Itens = itensDto,
            PercentualTotal = percentualTotal,
            TudoProduzido = itens.Count > 0 && itens.All(i => i.QuantidadeProduzida >= i.QuantidadePlanejada)
        };
    }

    // ========================================================================
    // DIVERGÊNCIA OP × BOM
    // ========================================================================

    /// <summary>
    /// Compara os itens da OP (snapshot) com a BOM atual do Produto raiz.
    /// Retorna diferenças (BOM mudou desde a criação da OP).
    /// Usado principalmente em OPs Master; nas Filhas a comparação é direta com a quantidade da BOM.
    /// </summary>
    public async Task<OrdemProducaoDivergenciaDTO?> GetDivergencia(int id)
    {
        var op = await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.OrdemPai)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (op == null)
            return null;

        // Para filha: compara com BOM do Produto raiz da Master
        var produtoRaizId = op.OrdemPaiId == null
            ? op.ProdutoId
            : (await _context.OrdensProducao.Where(m => m.Id == op.OrdemPaiId).Select(m => m.ProdutoId).FirstAsync());

        var explosao = await ExplodirBom(produtoRaizId);
        var itensOp = await _context.OrdensProducaoItens
            .Include(i => i.Produto)
            .Where(i => i.OrdemProducaoId == id)
            .ToListAsync();

        var divergencias = new List<OrdemProducaoDivergenciaItemDTO>();

        foreach (var item in itensOp)
        {
            var qtdBom = explosao.TryGetValue(item.ProdutoId, out var q) ? q : 0m;

            if (qtdBom != item.QuantidadePlanejada)
            {
                divergencias.Add(new OrdemProducaoDivergenciaItemDTO
                {
                    ProdutoId = item.ProdutoId,
                    ProdutoCodigo = item.Produto.Codigo,
                    ProdutoDescricao = item.Produto.Descricao,
                    QuantidadeNaOp = item.QuantidadePlanejada,
                    QuantidadeNaBomAtual = qtdBom,
                    Diferenca = qtdBom - item.QuantidadePlanejada,
                    Observacao = qtdBom == 0
                        ? "Item removido da BOM"
                        : qtdBom > item.QuantidadePlanejada
                            ? "BOM aumentou — considerar OP complementar"
                            : "BOM diminuiu"
                });
            }
        }

        return new OrdemProducaoDivergenciaDTO
        {
            OrdemProducaoId = op.Id,
            Codigo = op.Codigo,
            TemDivergencia = divergencias.Count > 0,
            Divergencias = divergencias
        };
    }

    // ========================================================================
    // HISTÓRICO
    // ========================================================================

    public async Task<List<OrdemProducaoHistoricoResponseDTO>> GetHistorico(int opId)
    {
        return await _context.OrdensProducaoHistorico
            .Where(h => h.OrdemProducaoId == opId)
            .OrderByDescending(h => h.DataHora)
            .Select(h => new OrdemProducaoHistoricoResponseDTO
            {
                Id = h.Id,
                OrdemProducaoId = h.OrdemProducaoId,
                Evento = h.Evento,
                StatusAnterior = h.StatusAnterior,
                StatusNovo = h.StatusNovo,
                Justificativa = h.Justificativa,
                Detalhe = h.Detalhe,
                DataHora = h.DataHora
            })
            .ToListAsync();
    }

    // ========================================================================
    // PRIVATES
    // ========================================================================

    private static (bool Valida, string? Erro) TransicaoPermitida(StatusOrdemProducao atual, StatusOrdemProducao novo)
    {
        if (atual == StatusOrdemProducao.Concluida)
            return (false, "OP Concluida é terminal — não aceita nova transição");
        if (atual == StatusOrdemProducao.Cancelada)
            return (false, "OP Cancelada é terminal — não aceita nova transição");

        return (atual, novo) switch
        {
            (StatusOrdemProducao.Pendente, StatusOrdemProducao.Andamento) => (true, null),
            (StatusOrdemProducao.Pendente, StatusOrdemProducao.Cancelada) => (true, null),
            (StatusOrdemProducao.Andamento, StatusOrdemProducao.Pausada) => (true, null),
            (StatusOrdemProducao.Andamento, StatusOrdemProducao.Concluida) => (true, null),
            (StatusOrdemProducao.Andamento, StatusOrdemProducao.Cancelada) => (true, null),
            (StatusOrdemProducao.Pausada, StatusOrdemProducao.Andamento) => (true, null),
            (StatusOrdemProducao.Pausada, StatusOrdemProducao.Cancelada) => (true, null),
            _ => (false, $"Transição não permitida: {atual} → {novo}")
        };
    }

    private static EventoOrdemProducao MapearEvento(StatusOrdemProducao anterior, StatusOrdemProducao novo)
    {
        if (anterior == StatusOrdemProducao.Pausada && novo == StatusOrdemProducao.Andamento)
            return EventoOrdemProducao.Retomada;

        return novo switch
        {
            StatusOrdemProducao.Andamento => EventoOrdemProducao.Iniciada,
            StatusOrdemProducao.Pausada => EventoOrdemProducao.Pausada,
            StatusOrdemProducao.Concluida => EventoOrdemProducao.Concluida,
            StatusOrdemProducao.Cancelada => EventoOrdemProducao.Cancelada,
            _ => EventoOrdemProducao.Criada
        };
    }

    private async Task<string> GerarCodigoMaster()
    {
        var agora = DateTime.UtcNow;
        var prefixo = $"OP.{agora:yyyy}.{agora:MM}";

        var ultimo = await _context.OrdensProducao
            .Where(o => o.Codigo.StartsWith(prefixo) && o.OrdemPaiId == null && !o.Codigo.Contains("/"))
            .OrderByDescending(o => o.Codigo)
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

    private async Task<string> GerarCodigoFilha(string codigoMaster, int masterId)
    {
        var filhasExistentes = await _context.OrdensProducao
            .Where(o => o.OrdemPaiId == masterId)
            .ToListAsync();

        var proximoSeq = filhasExistentes.Count + 1;
        return $"{codigoMaster}/{proximoSeq:D4}";
    }

    private void RegistrarEvento(
        int opId,
        EventoOrdemProducao evento,
        StatusOrdemProducao? anterior,
        StatusOrdemProducao? novo,
        string? justificativa,
        string? detalhe)
    {
        _context.OrdensProducaoHistorico.Add(new OrdemProducaoHistorico
        {
            OrdemProducaoId = opId,
            Evento = evento,
            StatusAnterior = anterior,
            StatusNovo = novo,
            Justificativa = justificativa,
            Detalhe = detalhe,
            DataHora = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Explode a BOM de um produto em memória, consolidando quantidades.
    /// Retorna Dictionary &lt;ProdutoFilhoId, QuantidadeTotal&gt;.
    /// </summary>
    private async Task<Dictionary<int, decimal>> ExplodirBom(int produtoPaiId)
    {
        var todasEstruturas = await _context.EstruturasProdutos.ToListAsync();
        var estruturasPorPai = todasEstruturas
            .GroupBy(e => e.ProdutoPaiId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var consolidado = new Dictionary<int, decimal>();
        var visitados = new HashSet<int>();
        Descer(produtoPaiId, 1m, estruturasPorPai, consolidado, visitados);
        return consolidado;
    }

    private static void Descer(
        int produtoId,
        decimal multiplicador,
        Dictionary<int, List<EstruturaProduto>> estruturasPorPai,
        Dictionary<int, decimal> consolidado,
        HashSet<int> visitados)
    {
        if (!visitados.Add(produtoId)) return;

        if (!estruturasPorPai.TryGetValue(produtoId, out var filhos))
        {
            visitados.Remove(produtoId);
            return;
        }

        foreach (var filho in filhos)
        {
            var qtd = multiplicador * filho.Quantidade;
            var ehFolha = !estruturasPorPai.ContainsKey(filho.ProdutoFilhoId);

            if (ehFolha)
            {
                consolidado[filho.ProdutoFilhoId] =
                    consolidado.TryGetValue(filho.ProdutoFilhoId, out var atual) ? atual + qtd : qtd;
            }
            else
            {
                // Intermediário também conta (pode virar OP Filha)
                consolidado[filho.ProdutoFilhoId] =
                    consolidado.TryGetValue(filho.ProdutoFilhoId, out var atual) ? atual + qtd : qtd;
                Descer(filho.ProdutoFilhoId, qtd, estruturasPorPai, consolidado, visitados);
            }
        }

        visitados.Remove(produtoId);
    }

    // ========================================================================
    // CONVERSORES
    // ========================================================================

    private async Task<OrdemProducaoResponseDTO> ToResponseDTO(OrdemProducao op)
    {
        var itens = await _context.OrdensProducaoItens
            .Include(i => i.Produto)
            .Where(i => i.OrdemProducaoId == op.Id)
            .OrderBy(i => i.Produto.Codigo)
            .ToListAsync();

        // Se é Master, carrega resumo das filhas
        var filhas = new List<OrdemProducaoFilhaResumoDTO>();
        if (op.OrdemPaiId == null)
        {
            var filhasDb = await _context.OrdensProducao
                .Include(f => f.Produto)
                .Where(f => f.OrdemPaiId == op.Id)
                .OrderBy(f => f.Codigo)
                .ToListAsync();

            foreach (var f in filhasDb)
            {
                var itensF = await _context.OrdensProducaoItens
                    .Where(i => i.OrdemProducaoId == f.Id)
                    .ToListAsync();

                decimal planejada = itensF.Sum(i => i.QuantidadePlanejada);
                decimal produzida = itensF.Sum(i => i.QuantidadeProduzida);
                decimal percent = planejada > 0 ? Math.Round(produzida / planejada * 100m, 2) : 0m;

                filhas.Add(new OrdemProducaoFilhaResumoDTO
                {
                    Id = f.Id,
                    Codigo = f.Codigo,
                    ProdutoId = f.ProdutoId,
                    ProdutoCodigo = f.Produto.Codigo,
                    ProdutoDescricao = f.Produto.Descricao,
                    Status = f.Status,
                    PercentualConcluido = percent
                });
            }
        }

        return new OrdemProducaoResponseDTO
        {
            Id = op.Id,
            Codigo = op.Codigo,
            PedidoVendaId = op.PedidoVendaId,
            PedidoVendaCodigo = op.PedidoVenda?.Codigo ?? string.Empty,
            ClienteNome = op.PedidoVenda?.Cliente?.Pessoa?.Nome ?? string.Empty,
            ProdutoId = op.ProdutoId,
            ProdutoCodigo = op.Produto?.Codigo ?? string.Empty,
            ProdutoDescricao = op.Produto?.Descricao ?? string.Empty,
            OrdemPaiId = op.OrdemPaiId,
            OrdemPaiCodigo = op.OrdemPai?.Codigo,
            Status = op.Status,
            DataInicio = op.DataInicio,
            DataFim = op.DataFim,
            Observacoes = op.Observacoes,
            Itens = itens.Select(ToItemResponseDTO).ToList(),
            Filhas = filhas,
            CriadoEm = op.CriadoEm,
            ModificadoEm = op.ModificadoEm
        };
    }

    private static OrdemProducaoItemResponseDTO ToItemResponseDTO(OrdemProducaoItem i) => new()
    {
        Id = i.Id,
        OrdemProducaoId = i.OrdemProducaoId,
        ProdutoId = i.ProdutoId,
        ProdutoCodigo = i.Produto?.Codigo ?? string.Empty,
        ProdutoDescricao = i.Produto?.Descricao ?? string.Empty,
        ProdutoUnidade = i.Produto?.Unidade.ToString() ?? string.Empty,
        QuantidadePlanejada = i.QuantidadePlanejada,
        QuantidadeProduzida = i.QuantidadeProduzida,
        Observacao = i.Observacao,
        CriadoEm = i.CriadoEm,
        ModificadoEm = i.ModificadoEm
    };
}
