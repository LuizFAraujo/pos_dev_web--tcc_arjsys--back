using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class BomService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

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

    public async Task<EstruturaProdutoResponseDTO?> GetById(int id)
    {
        var item = await _context.EstruturasProdutos
            .Include(e => e.ProdutoFilho)
            .FirstOrDefaultAsync(e => e.Id == id);

        return item == null ? null : ToResponseDTO(item);
    }

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

    public async Task<bool> Delete(int id)
    {
        var item = await _context.EstruturasProdutos.FindAsync(id);

        if (item == null)
            return false;

        _context.EstruturasProdutos.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> VerificarCiclo(int produtoPaiId, int produtoFilhoId)
    {
        var visitados = new HashSet<int>();
        return await VerificarCicloRecursivo(produtoPaiId, produtoFilhoId, visitados);
    }

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

    private async Task<int> CalcularProximaPosicao(int produtoPaiId)
    {
        var ultimaPosicao = await _context.EstruturasProdutos
            .Where(e => e.ProdutoPaiId == produtoPaiId)
            .MaxAsync(e => (int?)e.Posicao) ?? 0;

        return ((ultimaPosicao / 10) + 1) * 10;
    }

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
}