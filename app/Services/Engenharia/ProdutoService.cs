using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class ProdutoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<ProdutoResponseDTO>> GetAll()
    {
        return await _context.Produtos
            .OrderBy(p => p.Codigo)
            .Select(p => ToResponseDTO(p))
            .ToListAsync();
    }

    public async Task<ProdutoResponseDTO?> GetById(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        return produto == null ? null : ToResponseDTO(produto);
    }

    public async Task<(ProdutoResponseDTO? Item, string? Erro)> Create(ProdutoCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo))
            return (null, "Código é obrigatório");

        var existe = await _context.Produtos.AnyAsync(p => p.Codigo == dto.Codigo);

        if (existe)
            return (null, "Já existe um produto com este código");

        var produto = new Produto
        {
            Codigo = dto.Codigo,
            Descricao = dto.Descricao,
            DescricaoCompleta = dto.DescricaoCompleta,
            Unidade = dto.Unidade,
            Tipo = dto.Tipo,
            Peso = dto.Peso,
            Ativo = dto.Ativo,
            TemDocumento = dto.TemDocumento,
            CriadoEm = DateTime.UtcNow
        };

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return (ToResponseDTO(produto), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, ProdutoCreateDTO dto)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return (false, "Produto não encontrado");

        var existe = await _context.Produtos.AnyAsync(p => p.Codigo == dto.Codigo && p.Id != id);

        if (existe)
            return (false, "Já existe um produto com este código");

        produto.Codigo = dto.Codigo;
        produto.Descricao = dto.Descricao;
        produto.DescricaoCompleta = dto.DescricaoCompleta;
        produto.Unidade = dto.Unidade;
        produto.Tipo = dto.Tipo;
        produto.Peso = dto.Peso;
        produto.Ativo = dto.Ativo;
        produto.TemDocumento = dto.TemDocumento;
        produto.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return (false, "Produto não encontrado");

        var temEstrutura = await _context.EstruturasProdutos
            .AnyAsync(e => e.ProdutoPaiId == id || e.ProdutoFilhoId == id);

        if (temEstrutura)
            return (false, "Produto possui estrutura (BOM) e não pode ser excluído");

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Varre os produtos verificando se existe pasta e documento no path configurado.
    /// Usa Engenharia_PathDocumentos para paths alternativos por prefixo.
    /// Se não há path alternativo ativo, usa PathRaizDocumentos + ControlarPorPrefixoRaiz.
    /// </summary>
    public async Task<VarreduraDocumentosResultDTO> VarrerDocumentos(string? prefixo)
    {
        // Busca configurações globais
        var configs = await _context.ConfiguracoesEngenharia.ToListAsync();
        var pathRaiz = configs.FirstOrDefault(c => c.Chave == "PathRaizDocumentos")?.Valor?.TrimEnd('\\', '/');
        var controlarPorPrefixoRaiz = configs.FirstOrDefault(c => c.Chave == "ControlarPorPrefixoRaiz")?.Valor == "true";

        // Busca paths alternativos ativos com dados do grupo
        var pathsAlternativos = await _context.PathDocumentos
            .Include(p => p.GrupoProduto)
            .Where(p => p.Ativo)
            .ToListAsync();

        // Busca produtos filtrados ou todos
        var query = _context.Produtos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(prefixo))
            query = query.Where(p => p.Codigo.StartsWith(prefixo));

        var produtos = await query.ToListAsync();

        var resultado = new VarreduraDocumentosResultDTO
        {
            TotalVerificados = produtos.Count
        };

        foreach (var produto in produtos)
        {
            var (temPasta, temDoc) = VerificarDocumento(
                produto.Codigo, pathsAlternativos, pathRaiz, controlarPorPrefixoRaiz);

            var mudou = produto.TemPasta != temPasta || produto.TemDocumento != temDoc;

            if (mudou)
            {
                produto.TemPasta = temPasta;
                produto.TemDocumento = temDoc;
                produto.ModificadoEm = DateTime.UtcNow;
                resultado.Atualizados++;
            }

            if (temPasta && temDoc)
                resultado.ComDocumento++;
            else if (temPasta && !temDoc)
                resultado.PastaVazia++;
            else
                resultado.SemPasta++;

            if (temPasta)
                resultado.ComPasta++;
        }

        await _context.SaveChangesAsync();
        return resultado;
    }

    /// <summary>
    /// Verifica se existe pasta e documento para um produto.
    /// 1. Extrai prefixo do código (antes do primeiro ponto)
    /// 2. Busca path alternativo ativo para o prefixo
    ///    - Se existe → usa Path do registro + respeita ControlarPorPrefixo
    ///    - Se não → usa PathRaizDocumentos + respeita ControlarPorPrefixoRaiz
    /// 3. Monta caminho e verifica existência de pasta e arquivo
    /// </summary>
    private static (bool TemPasta, bool TemDocumento) VerificarDocumento(
        string codigoProduto,
        List<PathDocumentos> pathsAlternativos,
        string? pathRaiz,
        bool controlarPorPrefixoRaiz)
    {
        // Extrai o prefixo (Coluna1) do código do produto — antes do primeiro ponto
        var prefixoCodigo = codigoProduto.Split('.').FirstOrDefault();

        if (string.IsNullOrEmpty(prefixoCodigo))
            return (false, false);

        string? pathBase = null;
        bool controlarPorPrefixo = false;

        // Busca path alternativo ativo para este prefixo
        var pathAlternativo = pathsAlternativos
            .FirstOrDefault(p => p.GrupoProduto.Codigo == prefixoCodigo);

        if (pathAlternativo != null)
        {
            pathBase = pathAlternativo.Path;
            controlarPorPrefixo = pathAlternativo.ControlarPorPrefixo;
        }
        else if (!string.IsNullOrWhiteSpace(pathRaiz))
        {
            pathBase = pathRaiz;
            controlarPorPrefixo = controlarPorPrefixoRaiz;
        }

        if (string.IsNullOrEmpty(pathBase))
            return (false, false);

        // Monta caminho da pasta do produto
        string pastaProduto;

        if (controlarPorPrefixo)
            pastaProduto = Path.Combine(pathBase, prefixoCodigo, codigoProduto);
        else
            pastaProduto = Path.Combine(pathBase, codigoProduto);

        // Verifica pasta
        if (!Directory.Exists(pastaProduto))
            return (false, false);

        // Pasta existe — verifica arquivo
        var arquivos = Directory.GetFiles(pastaProduto, $"{codigoProduto}.*");
        return (true, arquivos.Length > 0);
    }

    private static ProdutoResponseDTO ToResponseDTO(Produto p) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        Descricao = p.Descricao,
        DescricaoCompleta = p.DescricaoCompleta,
        Unidade = p.Unidade,
        Tipo = p.Tipo,
        Peso = p.Peso,
        Ativo = p.Ativo,
        TemPasta = p.TemPasta,
        TemDocumento = p.TemDocumento,
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}