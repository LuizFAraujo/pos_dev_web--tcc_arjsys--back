using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
[Tags("Engenharia - Produtos")]
public class ProdutosController(ProdutoService service) : ControllerBase
{
    private readonly ProdutoService _service = service;

    /// <summary>Listar todos os produtos</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProdutoResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
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

    /// <summary>Abrir pasta de documentos do produto no Windows Explorer</summary>
    [HttpPost("{id:int}/abrir-pasta")]
    public async Task<ActionResult<AbrirPastaResultDTO>> AbrirPasta(int id)
    {
        var (resultado, erro) = await _service.AbrirPasta(id);

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

    /// <summary>Abrir documento do produto com o programa padrão do Windows. Se extensão informada, abre essa específica; senão, abre o primeiro encontrado.</summary>
    [HttpPost("{id:int}/abrir-documento")]
    public async Task<ActionResult<AbrirDocumentoResultDTO>> AbrirDocumento(int id, [FromQuery] string? extensao)
    {
        var (resultado, erro) = await _service.AbrirDocumento(id, extensao);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(resultado);
    }
}