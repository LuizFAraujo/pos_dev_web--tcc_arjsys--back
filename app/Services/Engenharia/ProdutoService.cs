using System.Diagnostics;
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


    // =============================================
    // RESOLUÇÃO DE PATH (método compartilhado)
    // =============================================

    /// <summary>
    /// Resolve o path completo da pasta de documentos de um produto.
    /// Busca path alternativo ativo por prefixo, senão usa path raiz.
    /// Retorna (path, null) se resolveu, (null, erro) se não conseguiu.
    /// </summary>
    private async Task<(string? Path, string? Erro)> ResolverPathProduto(string codigoProduto)
    {
        var prefixoCodigo = codigoProduto.Split('.').FirstOrDefault();

        if (string.IsNullOrEmpty(prefixoCodigo))
            return (null, "Código do produto não possui prefixo válido");

        // Busca configurações globais
        var configs = await _context.ConfiguracoesEngenharia.ToListAsync();
        var pathRaiz = configs.FirstOrDefault(c => c.Chave == "PathRaizDocumentos")?.Valor?.TrimEnd('\\', '/');
        var controlarPorPrefixoRaiz = configs.FirstOrDefault(c => c.Chave == "ControlarPorPrefixoRaiz")?.Valor == "true";

        // Busca path alternativo ativo para este prefixo
        var pathAlternativo = await _context.PathDocumentos
            .Include(p => p.GrupoProduto)
            .FirstOrDefaultAsync(p => p.Ativo && p.GrupoProduto.Codigo == prefixoCodigo);

        string? pathBase = null;
        bool controlarPorPrefixo = false;

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
            return (null, "Nenhum path de documentos configurado para este produto");

        // Monta caminho da pasta do produto
        string pastaProduto;

        if (controlarPorPrefixo)
            pastaProduto = Path.Combine(pathBase, prefixoCodigo, codigoProduto);
        else
            pastaProduto = Path.Combine(pathBase, codigoProduto);

        return (pastaProduto, null);
    }


    // =============================================
    // ABRIR PASTA
    // =============================================

    /// <summary>
    /// Abre a pasta de documentos do produto no Windows Explorer.
    /// Verifica se o produto existe, se tem pasta, e se o diretório existe no filesystem.
    /// </summary>
    public async Task<(AbrirPastaResultDTO? Item, string? Erro)> AbrirPasta(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return (null, "Produto não encontrado");

        if (!produto.TemPasta)
            return (null, "Este produto não possui pasta de documentos");

        var (path, erroPath) = await ResolverPathProduto(produto.Codigo);

        if (erroPath != null)
            return (null, erroPath);

        if (!Directory.Exists(path))
            return (null, $"Pasta não encontrada: {path}");

        Process.Start("explorer.exe", path!);

        return (new AbrirPastaResultDTO { Path = path!, Aberto = true }, null);
    }


    // =============================================
    // LISTAR EXTENSÕES
    // =============================================

    /// <summary>
    /// Lista as extensões de documentos encontrados na pasta do produto.
    /// Retorna apenas arquivos cujo nome corresponde ao código do produto.
    /// </summary>
    public async Task<(ExtensoesDocumentoResultDTO? Item, string? Erro)> ListarExtensoes(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return (null, "Produto não encontrado");

        if (!produto.TemPasta)
            return (null, "Este produto não possui pasta de documentos");

        var (path, erroPath) = await ResolverPathProduto(produto.Codigo);

        if (erroPath != null)
            return (null, erroPath);

        if (!Directory.Exists(path))
            return (null, $"Pasta não encontrada: {path}");

        var arquivos = Directory.GetFiles(path!, $"{produto.Codigo}.*");
        var extensoes = arquivos
            .Select(a => Path.GetExtension(a).TrimStart('.').ToLower())
            .Where(e => !string.IsNullOrEmpty(e))
            .OrderBy(e => e)
            .ToList();

        return (new ExtensoesDocumentoResultDTO { Path = path!, Extensoes = extensoes }, null);
    }


    // =============================================
    // ABRIR DOCUMENTO
    // =============================================

    /// <summary>
    /// Abre o documento do produto com o programa padrão do Windows.
    /// Se extensão informada, abre o arquivo com essa extensão.
    /// Se não informada, abre o primeiro arquivo encontrado.
    /// </summary>
    public async Task<(AbrirDocumentoResultDTO? Item, string? Erro)> AbrirDocumento(int id, string? extensao)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return (null, "Produto não encontrado");

        if (!produto.TemDocumento)
            return (null, "Este produto não possui documento cadastrado");

        var (path, erroPath) = await ResolverPathProduto(produto.Codigo);

        if (erroPath != null)
            return (null, erroPath);

        if (!Directory.Exists(path))
            return (null, $"Pasta não encontrada: {path}");

        string? arquivoPath;

        if (!string.IsNullOrWhiteSpace(extensao))
        {
            // Extensão específica
            arquivoPath = Path.Combine(path!, $"{produto.Codigo}.{extensao.TrimStart('.')}");

            if (!File.Exists(arquivoPath))
                return (null, $"Arquivo não encontrado: {produto.Codigo}.{extensao.TrimStart('.')}");
        }
        else
        {
            // Primeiro arquivo encontrado
            var arquivos = Directory.GetFiles(path!, $"{produto.Codigo}.*");

            if (arquivos.Length == 0)
                return (null, "Nenhum arquivo encontrado na pasta do produto");

            arquivoPath = arquivos[0];
        }

        var ext = Path.GetExtension(arquivoPath).TrimStart('.').ToLower();

        Process.Start(new ProcessStartInfo
        {
            FileName = arquivoPath,
            UseShellExecute = true
        });

        return (new AbrirDocumentoResultDTO
        {
            Path = arquivoPath,
            Extensao = ext,
            Aberto = true
        }, null);
    }


    // =============================================
    // VARREDURA DE DOCUMENTOS
    // =============================================

    /// <summary>
    /// Varre os produtos verificando se existe pasta e documento no path configurado.
    /// Suporta paginação por lotes (offset + limit) para varredura progressiva.
    /// Quando sem offset/limit, varre tudo (comportamento legado).
    /// </summary>
    public async Task<VarreduraDocumentosResultDTO> VarrerDocumentos(string? prefixo, int? offset = null, int? limit = null)
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

        // Total antes de paginar (para barra de progresso do frontend)
        var totalGeral = await query.CountAsync();

        // Aplica paginação por lote se informado
        List<Produto> produtos;

        if (offset.HasValue && limit.HasValue)
            produtos = await query.OrderBy(p => p.Codigo).Skip(offset.Value).Take(limit.Value).ToListAsync();
        else
            produtos = await query.ToListAsync();

        var resultado = new VarreduraDocumentosResultDTO
        {
            TotalGeral = totalGeral,
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
    /// Usado internamente pela varredura em lote (recebe dados pré-carregados).
    /// </summary>
    private static (bool TemPasta, bool TemDocumento) VerificarDocumento(
        string codigoProduto,
        List<PathDocumentos> pathsAlternativos,
        string? pathRaiz,
        bool controlarPorPrefixoRaiz)
    {
        var prefixoCodigo = codigoProduto.Split('.').FirstOrDefault();

        if (string.IsNullOrEmpty(prefixoCodigo))
            return (false, false);

        string? pathBase = null;
        bool controlarPorPrefixo = false;

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

        string pastaProduto;

        if (controlarPorPrefixo)
            pastaProduto = Path.Combine(pathBase, prefixoCodigo, codigoProduto);
        else
            pastaProduto = Path.Combine(pathBase, codigoProduto);

        if (!Directory.Exists(pastaProduto))
            return (false, false);

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