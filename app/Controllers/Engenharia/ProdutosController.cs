using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.DTOs.Shared;
using Api_ArjSys_Tcc.Services.Engenharia;
using Api_ArjSys_Tcc.Services.Shared.Thumbnail;
using Api_ArjSys_Tcc.Helpers;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
[Tags("Engenharia - Produtos")]
public class ProdutosController(ProdutoService service, ThumbnailService thumbnailService) : ControllerBase
{
    private readonly ProdutoService _service = service;
    private readonly ThumbnailService _thumbnailService = thumbnailService;

    // Descobre o content-type pela extensão do arquivo (pdf, jpg, png, etc).
    // Faz parte do ASP.NET, sem pacote extra. Estático: criar uma vez basta.
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    /// <summary>Listar todos os produtos (cache pra autocomplete cliente-side).</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProdutoResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
    }

    /// <summary>
    /// Busca paginada de produtos com filtros, ordenação e busca textual server-side.
    /// </summary>
    [HttpPost("buscar")]
    public async Task<ActionResult<PaginadoResponse<ProdutoResponseDTO>>> Buscar([FromBody] BuscaRequest req)
    {
        var resposta = await _service.Buscar(req);
        return Ok(resposta);
    }

    /// <summary>Buscar produto por ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDTO>> GetById(int id)
    {
        var produto = await _service.GetById(id);

        if (produto == null)
            return NotFound();

        return produto;
    }

    /// <summary>Criar novo produto</summary>
    [HttpPost]
    public async Task<ActionResult<ProdutoResponseDTO>> Create(ProdutoCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    /// <summary>Atualizar produto existente</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProdutoCreateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }

    /// <summary>Deletar produto (impedido se possui BOM)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (sucesso, erro) = await _service.Delete(id);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Executar varredura de documentos. Suporta filtro por prefixo e paginação por lotes (offset + limit).
    /// </summary>
    [HttpPost("varredura-documentos")]
    public async Task<ActionResult<VarreduraDocumentosResultDTO>> VarrerDocumentos(
        [FromQuery] string? prefixo,
        [FromQuery] int? offset,
        [FromQuery] int? limit)
    {
        if ((offset.HasValue && !limit.HasValue) || (!offset.HasValue && limit.HasValue))
            return BadRequest(new { erro = "offset e limit devem ser informados juntos" });

        var resultado = await _service.VarrerDocumentos(prefixo, offset, limit);
        return Ok(resultado);
    }

	/// <summary>
    /// Abrir pasta de documentos do produto no Windows Explorer.
    /// Se request vem de localhost, executa Process.Start. Se vem da rede, só retorna o path.
    /// </summary>
    [HttpPost("{id:int}/abrir-pasta")]
    public async Task<ActionResult<AbrirPastaResultDTO>> AbrirPasta(int id)
    {
        var isLocal = RequestHelper.IsLocalRequest(HttpContext);
        var (resultado, erro) = await _service.AbrirPasta(id, isLocal);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(resultado);
    }

    /// <summary>Listar extensões de documentos encontrados na pasta do produto</summary>
    [HttpGet("{id:int}/extensoes-documento")]
    public async Task<ActionResult<ExtensoesDocumentoResultDTO>> ListarExtensoes(int id)
    {
        var (resultado, erro) = await _service.ListarExtensoes(id);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(resultado);
    }

	/// <summary>
    /// Abrir documento do produto com o programa padrão do Windows.
    /// Se extensão informada, abre essa específica; senão, abre o primeiro encontrado.
    /// Se request vem de localhost, executa Process.Start. Se vem da rede, só retorna o path.
    /// </summary>
    [HttpPost("{id:int}/abrir-documento")]
    public async Task<ActionResult<AbrirDocumentoResultDTO>> AbrirDocumento(int id, [FromQuery] string? extensao)
    {
        var isLocal = RequestHelper.IsLocalRequest(HttpContext);
        var (resultado, erro) = await _service.AbrirDocumento(id, extensao, isLocal);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(resultado);
    }


    /// <summary>
    /// Retorna a miniatura (webp) da primeira página do PDF do produto.
    /// Renderiza on-demand e cacheia em disco. GET puro e idempotente, sem efeitos colaterais.
    /// Suporta ETag e If-None-Match (304).
    /// </summary>
    [HttpGet("{id:int}/thumbnail")]
    public async Task<IActionResult> Thumbnail(int id, [FromQuery] int w = 320)
    {
        w = Math.Clamp(w, 80, 800);

        // Parte específica de produto: resolve o caminho do PDF do produto.
        var (caminho, erro) = await _service.ResolverCaminhoDocumento(id, "pdf");

        if (erro != null)
            return NotFound(new { erro });

        // Parte genérica: gera (ou recupera do cache) a miniatura webp.
        var (filePath, etag, erroThumb) = _thumbnailService.Gerar(caminho!, w);

        if (erroThumb != null)
            return UnprocessableEntity(new { erro = erroThumb });

        if (Request.Headers.IfNoneMatch == etag)
            return StatusCode((int)HttpStatusCode.NotModified);

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "private, max-age=86400";

        return PhysicalFile(filePath!, "image/webp");
    }


    /// <summary>
    /// Entrega o documento ORIGINAL do produto para exibição e interação no
    /// navegador (por exemplo, abrir o PDF embutido no visualizador nativo).
    /// GET puro e idempotente, sem efeitos colaterais (diferente do
    /// abrir-documento, que abre o arquivo no servidor). Envia inline e com
    /// suporte a range, necessário para o visualizador de PDF do navegador
    /// navegar entre páginas e dar zoom.
    /// </summary>
    [HttpGet("{id:int}/documento")]
    public async Task<IActionResult> Documento(int id, [FromQuery] string ext = "pdf")
    {
        var (caminho, erro) = await _service.ResolverCaminhoDocumento(id, ext);

        if (erro != null)
            return NotFound(new { erro });

        if (!_contentTypeProvider.TryGetContentType(caminho!, out var contentType))
            contentType = "application/octet-stream";

        return PhysicalFile(caminho!, contentType, enableRangeProcessing: true);
    }


}