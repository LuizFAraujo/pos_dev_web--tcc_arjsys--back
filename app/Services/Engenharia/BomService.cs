using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

/// <summary>
/// Serviço de BOM (Bill of Materials) — estrutura de produtos pai-filho.
/// Gerencia relações entre produtos com validação de ciclo recursivo e posição automática.
/// Fornece também a "explosão" da BOM: todos os itens folha consolidados com quantidades totais.
/// </summary>
public class BomService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista produtos que possuem estrutura (são pai de pelo menos um filho).
    /// Suporta paginação opcional.
    /// </summary>
    public async Task<(List<ProdutoResponseDTO> Itens, int Total)> GetProdutosComEstrutura(int pagina = 1, int tamanhoPorPagina = 0)
    {
        var query = _context.Produtos
            .Where(p => _context.EstruturasProdutos.Any(e => e.ProdutoPaiId == p.Id));

        var total = await query.CountAsync();

        query = query.OrderBy(p => p.Codigo);

        if (tamanhoPorPagina > 0)
            query = query.Skip((pagina - 1) * tamanhoPorPagina).Take(tamanhoPorPagina);

        var itens = await query
            .Select(p => new ProdutoResponseDTO
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Descricao = p.Descricao,
                DescricaoCompleta = p.DescricaoCompleta,
                Unidade = p.Unidade,
                Tipo = p.Tipo,
                Peso = p.Peso,
                Ativo = p.Ativo,
                CriadoEm = p.CriadoEm,
                ModificadoEm = p.ModificadoEm
            })
            .ToListAsync();

        return (itens, total);
    }

    /// <summary>
    /// Lista os filhos diretos de um produto pai, ordenados por posição.
    /// </summary>
    public async Task<List<EstruturaProdutoResponseDTO>> GetByProdutoId(int produtoPaiId)
    {
        return await _context.EstruturasProdutos
            .Where(e => e.ProdutoPaiId == produtoPaiId)
            .Include(e => e.ProdutoFilho)
            .OrderBy(e => e.Posicao)
            .ThenBy(e => e.ProdutoFilho.Codigo)
            .ThenBy(e => e.ProdutoFilho.Descricao)
            .Select(e => ToResponseDTO(e))
            .ToListAsync();
    }

    /// <summary>
    /// Busca um registro de estrutura por ID.
    /// </summary>
    public async Task<EstruturaProdutoResponseDTO?> GetById(int id)
    {
        var item = await _context.EstruturasProdutos
            .Include(e => e.ProdutoFilho)
            .FirstOrDefaultAsync(e => e.Id == id);

        return item == null ? null : ToResponseDTO(item);
    }

    /// <summary>
    /// Cria uma relação pai-filho na estrutura.
    /// Valida: auto-referência, duplicidade e ciclo recursivo.
    /// Se posição não informada, calcula automaticamente (próximo múltiplo de 10).
    /// </summary>
    public async Task<(EstruturaProdutoResponseDTO? Item, string? Erro)> Create(EstruturaProdutoCreateDTO dto)
    {
        if (dto.ProdutoPaiId == dto.ProdutoFilhoId)
            return (null, "Um produto não pode ser filho dele mesmo");

        var existeDuplicado = await _context.EstruturasProdutos
            .AnyAsync(e => e.ProdutoPaiId == dto.ProdutoPaiId && e.ProdutoFilhoId == dto.ProdutoFilhoId);

        if (existeDuplicado)
            return (null, "Este item já existe na estrutura deste produto");

        var temCiclo = await VerificarCiclo(dto.ProdutoPaiId, dto.ProdutoFilhoId);

        if (temCiclo)
            return (null, "Inclusão geraria referência circular na estrutura");

        var posicao = dto.Posicao > 0
            ? dto.Posicao
            : await CalcularProximaPosicao(dto.ProdutoPaiId);

        var item = new EstruturaProduto
        {
            ProdutoPaiId = dto.ProdutoPaiId,
            ProdutoFilhoId = dto.ProdutoFilhoId,
            Quantidade = dto.Quantidade,
            Posicao = posicao,
            Observacao = dto.Observacao,
            CriadoEm = DateTime.UtcNow
        };

        _context.EstruturasProdutos.Add(item);
        await _context.SaveChangesAsync();

        await _context.Entry(item).Reference(e => e.ProdutoFilho).LoadAsync();

        return (ToResponseDTO(item), null);
    }

    /// <summary>
    /// Atualiza uma relação existente.
    /// Se o filho mudou, revalida duplicidade e ciclo recursivo.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int id, EstruturaProdutoCreateDTO dto)
    {
        var existente = await _context.EstruturasProdutos.FindAsync(id);

        if (existente == null)
            return (false, "Item não encontrado");

        if (dto.ProdutoFilhoId != existente.ProdutoFilhoId)
        {
            if (dto.ProdutoPaiId == dto.ProdutoFilhoId)
                return (false, "Um produto não pode ser filho dele mesmo");

            var existeDuplicado = await _context.EstruturasProdutos
                .AnyAsync(e => e.ProdutoPaiId == existente.ProdutoPaiId
                            && e.ProdutoFilhoId == dto.ProdutoFilhoId
                            && e.Id != id);

            if (existeDuplicado)
                return (false, "Este item já existe na estrutura deste produto");

            var temCiclo = await VerificarCiclo(existente.ProdutoPaiId, dto.ProdutoFilhoId);

            if (temCiclo)
                return (false, "Alteração geraria referência circular na estrutura");

            existente.ProdutoFilhoId = dto.ProdutoFilhoId;
        }

        existente.Quantidade = dto.Quantidade;
        existente.Posicao = dto.Posicao;
        existente.Observacao = dto.Observacao;
        existente.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Remove um único registro de estrutura por ID.
    /// </summary>
    public async Task<bool> Delete(int id)
    {
        var item = await _context.EstruturasProdutos.FindAsync(id);

        if (item == null)
            return false;

        _context.EstruturasProdutos.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se adicionar produtoFilhoId como filho de produtoPaiId criaria um ciclo.
    /// </summary>
    private async Task<bool> VerificarCiclo(int produtoPaiId, int produtoFilhoId)
    {
        var visitados = new HashSet<int>();
        return await VerificarCicloRecursivo(produtoPaiId, produtoFilhoId, visitados);
    }

    /// <summary>
    /// Percorre recursivamente a árvore de filhos verificando se algum aponta de volta pro pai original.
    /// </summary>
    private async Task<bool> VerificarCicloRecursivo(int produtoPaiOriginal, int filhoAtual, HashSet<int> visitados)
    {
        if (filhoAtual == produtoPaiOriginal)
            return true;

        if (!visitados.Add(filhoAtual))
            return false;

        var filhosDoFilho = await _context.EstruturasProdutos
            .Where(e => e.ProdutoPaiId == filhoAtual)
            .Select(e => e.ProdutoFilhoId)
            .ToListAsync();

        foreach (var neto in filhosDoFilho)
        {
            if (await VerificarCicloRecursivo(produtoPaiOriginal, neto, visitados))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Calcula a próxima posição disponível para um filho (próximo múltiplo de 10).
    /// </summary>
    private async Task<int> CalcularProximaPosicao(int produtoPaiId)
    {
        var ultimaPosicao = await _context.EstruturasProdutos
            .Where(e => e.ProdutoPaiId == produtoPaiId)
            .MaxAsync(e => (int?)e.Posicao) ?? 0;

        return ((ultimaPosicao / 10) + 1) * 10;
    }

    /// <summary>
    /// Lista plana de todas as relações pai-filho com dados dos produtos.
    /// Suporta paginação opcional.
    /// </summary>
    public async Task<(List<EstruturaProdutoFlatDTO> Itens, int Total)> GetAllFlat(int pagina = 1, int tamanhoPorPagina = 0)
    {
        var query = _context.EstruturasProdutos
            .Include(e => e.ProdutoPai)
            .Include(e => e.ProdutoFilho)
            .OrderBy(e => e.ProdutoPai.Codigo)
            .ThenBy(e => e.Posicao)
            .ThenBy(e => e.ProdutoFilho.Codigo);

        var total = await query.CountAsync();

        var itensQuery = query.Select(e => new EstruturaProdutoFlatDTO
        {
            Id = e.Id,
            ProdutoPaiId = e.ProdutoPaiId,
            ProdutoPaiCodigo = e.ProdutoPai.Codigo,
            ProdutoPaiDescricao = e.ProdutoPai.Descricao,
            ProdutoFilhoId = e.ProdutoFilhoId,
            ProdutoFilhoCodigo = e.ProdutoFilho.Codigo,
            ProdutoFilhoDescricao = e.ProdutoFilho.Descricao,
            ProdutoFilhoUnidade = e.ProdutoFilho.Unidade,
            Quantidade = e.Quantidade,
            Posicao = e.Posicao,
            Observacao = e.Observacao
        });

        if (tamanhoPorPagina > 0)
            itensQuery = (IOrderedQueryable<EstruturaProdutoFlatDTO>)itensQuery.Skip((pagina - 1) * tamanhoPorPagina).Take(tamanhoPorPagina);

        var itens = await itensQuery.ToListAsync();

        return (itens, total);
    }

    /// <summary>
    /// Retorna a explosão consolidada da BOM de um produto.
    /// Percorre recursivamente todos os níveis da estrutura, descendo até os itens folha
    /// (que não têm filhos), multiplicando as quantidades pelo caminho e somando ocorrências
    /// do mesmo produto em níveis/ramos diferentes. Retorna 1 linha por produto folha com
    /// a quantidade total consolidada.
    /// Ex: se Parafuso M8 aparece em 5 subconjuntos com quantidades diferentes, a resposta
    /// contém 1 única linha com a soma total.
    /// Retorna null se o produto não existir ou não tiver estrutura.
    /// </summary>
    public async Task<BomExplosaoResponseDTO?> GetExplosao(int produtoPaiId)
    {
        var produto = await _context.Produtos.FindAsync(produtoPaiId);

        if (produto == null)
            return null;

        var temBom = await _context.EstruturasProdutos.AnyAsync(e => e.ProdutoPaiId == produtoPaiId);
        if (!temBom)
            return null;

        // Carrega toda a estrutura de uma vez (evita N queries recursivas)
        var todasEstruturas = await _context.EstruturasProdutos
            .Include(e => e.ProdutoFilho)
            .ToListAsync();

        var estruturasPorPai = todasEstruturas
            .GroupBy(e => e.ProdutoPaiId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Consolidado: ProdutoId -> {Produto, QuantidadeTotal}
        var consolidado = new Dictionary<int, (Produto Produto, decimal Qtd)>();
        var visitados = new HashSet<int>();

        ExplodirRecursivo(produtoPaiId, 1m, estruturasPorPai, consolidado, visitados);

        var itens = consolidado.Values
            .Select(v => new BomExplosaoItemDTO
            {
                ProdutoId = v.Produto.Id,
                Codigo = v.Produto.Codigo,
                Descricao = v.Produto.Descricao,
                Unidade = v.Produto.Unidade.ToString(),
                Tipo = v.Produto.Tipo.ToString(),
                QuantidadeTotal = v.Qtd
            })
            .OrderBy(i => i.Codigo)
            .ToList();

        return new BomExplosaoResponseDTO
        {
            ProdutoPaiId = produto.Id,
            ProdutoPaiCodigo = produto.Codigo,
            ProdutoPaiDescricao = produto.Descricao,
            Itens = itens,
            TotalItens = itens.Count
        };
    }

    /// <summary>
    /// Desce recursivamente pela estrutura.
    /// Itens folha (sem filhos) são acumulados no consolidado.
    /// Itens intermediários (com filhos) só têm sua estrutura descida — não aparecem no resultado.
    /// </summary>
    private static void ExplodirRecursivo(
        int produtoId,
        decimal multiplicador,
        Dictionary<int, List<EstruturaProduto>> estruturasPorPai,
        Dictionary<int, (Produto Produto, decimal Qtd)> consolidado,
        HashSet<int> visitados)
    {
        // Proteção contra ciclos em runtime (não deveria acontecer, mas por segurança)
        if (!visitados.Add(produtoId))
            return;

        if (!estruturasPorPai.TryGetValue(produtoId, out var filhos))
        {
            visitados.Remove(produtoId);
            return;
        }

        foreach (var filho in filhos)
        {
            var qtdCaminho = multiplicador * filho.Quantidade;
            var ehFolha = !estruturasPorPai.ContainsKey(filho.ProdutoFilhoId);

            if (ehFolha)
            {
                if (consolidado.TryGetValue(filho.ProdutoFilhoId, out var atual))
                    consolidado[filho.ProdutoFilhoId] = (atual.Produto, atual.Qtd + qtdCaminho);
                else
                    consolidado[filho.ProdutoFilhoId] = (filho.ProdutoFilho, qtdCaminho);
            }
            else
            {
                ExplodirRecursivo(filho.ProdutoFilhoId, qtdCaminho, estruturasPorPai, consolidado, visitados);
            }
        }

        visitados.Remove(produtoId);
    }

    private static EstruturaProdutoResponseDTO ToResponseDTO(EstruturaProduto e) => new()
    {
        Id = e.Id,
        ProdutoPaiId = e.ProdutoPaiId,
        ProdutoFilhoId = e.ProdutoFilhoId,
        ProdutoFilhoCodigo = e.ProdutoFilho.Codigo,
        ProdutoFilhoDescricao = e.ProdutoFilho.Descricao,
        Quantidade = e.Quantidade,
        Posicao = e.Posicao,
        Observacao = e.Observacao,
        CriadoEm = e.CriadoEm,
        ModificadoEm = e.ModificadoEm
    };

    /// <summary>
    /// Remove todos os filhos diretos de uma estrutura (nível 2).
    /// Não afeta estruturas internas dos filhos (nível 3+).
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> DeleteEstrutura(int produtoPaiId)
    {
        var produtoExiste = await _context.Produtos.AnyAsync(p => p.Id == produtoPaiId);

        if (!produtoExiste)
            return (false, null);

        var filhosDiretos = await _context.EstruturasProdutos
            .Where(e => e.ProdutoPaiId == produtoPaiId)
            .ToListAsync();

        if (filhosDiretos.Count == 0)
            return (false, "Estrutura não possui filhos para remover");

        _context.EstruturasProdutos.RemoveRange(filhosDiretos);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}
