using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
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
    /// Se prefixo informado, filtra por produtos cujo código começa com o prefixo.
    /// Atualiza o campo TemDocumento de cada produto conforme encontra ou não o documento.
    /// </summary>
    public async Task<VarreduraDocumentosResultDTO> VarrerDocumentos(string? prefixo)
    {
        // Busca path raiz nas configurações
        var configRaiz = await _context.ConfiguracoesEngenharia
            .FirstOrDefaultAsync(c => c.Chave == "PathRaizDocumentos");

        var pathRaiz = configRaiz?.Valor?.TrimEnd('\\', '/');

        // Busca todos os grupos Coluna1 para paths customizados
        var gruposColuna1 = await _context.GruposProdutos
            .Where(g => g.Nivel == Models.Engenharia.Enums.NivelGrupo.Coluna1)
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
            var temDoc = VerificarDocumento(produto.Codigo, gruposColuna1, pathRaiz);
            var mudou = produto.TemDocumento != temDoc;

            if (mudou)
            {
                produto.TemDocumento = temDoc;
                produto.ModificadoEm = DateTime.UtcNow;
                resultado.Atualizados++;
            }

            if (temDoc)
                resultado.ComDocumento++;
            else
                resultado.SemDocumento++;
        }

        await _context.SaveChangesAsync();
        return resultado;
    }

    /// <summary>
    /// Verifica se existe pasta com nome do código e arquivo dentro com o mesmo nome.
    /// Busca primeiro no path customizado do grupo, depois no path raiz.
    /// </summary>
    private static bool VerificarDocumento(string codigoProduto, List<GrupoProduto> gruposColuna1, string? pathRaiz)
    {
        // Extrai o prefixo (Coluna1) do código do produto — antes do primeiro ponto
        var prefixoCodigo = codigoProduto.Split('.').FirstOrDefault();

        if (string.IsNullOrEmpty(prefixoCodigo))
            return false;

        // Encontra o grupo Coluna1 correspondente
        var grupo = gruposColuna1.FirstOrDefault(g => g.Codigo == prefixoCodigo);

        string? pathBase = null;

        // Tenta path customizado do grupo
        if (grupo != null && !string.IsNullOrWhiteSpace(grupo.PathDocumentos))
        {
            pathBase = grupo.PathDocumentos.TrimEnd('\\', '/');
        }
        // Senão usa path raiz + código do grupo
        else if (!string.IsNullOrWhiteSpace(pathRaiz) && grupo != null)
        {
            pathBase = Path.Combine(pathRaiz, grupo.Codigo);
        }

        if (string.IsNullOrEmpty(pathBase))
            return false;

        // Verifica se existe pasta com nome do código
        var pastaProduto = Path.Combine(pathBase, codigoProduto);

        if (!Directory.Exists(pastaProduto))
            return false;

        // Verifica se dentro da pasta tem arquivo com o nome do código (qualquer extensão)
        var arquivos = Directory.GetFiles(pastaProduto, $"{codigoProduto}.*");
        return arquivos.Length > 0;
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
        TemDocumento = p.TemDocumento,
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}