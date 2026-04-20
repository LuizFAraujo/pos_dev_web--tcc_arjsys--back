using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Producao;
using Api_ArjSys_Tcc.Services.Producao;

namespace Api_ArjSys_Tcc.Controllers.Producao;

[ApiController]
[Route("api/producao/[controller]")]
[Tags("Produção - Ordens de Produção")]
public class OrdemProducaoController(OrdemProducaoService service) : ControllerBase
{
    private readonly OrdemProducaoService _service = service;

    /// <summary>
    /// Lista todas as OPs.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrdemProducaoResponseDTO>>> GetAll(int pagina = 0, int tamanho = 0)
    {
        return await _service.GetAll(pagina, tamanho);
    }

    /// <summary>
    /// Busca OP por ID (com itens e filhas se for Master).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdemProducaoResponseDTO>> GetById(int id)
    {
        var op = await _service.GetById(id);
        if (op == null)
            return NotFound();
        return op;
    }

    /// <summary>
    /// Lista OPs de um Pedido de Venda.
    /// </summary>
    [HttpGet("pedido/{pedidoVendaId:int}")]
    public async Task<ActionResult<List<OrdemProducaoResponseDTO>>> GetByPedido(int pedidoVendaId)
    {
        return await _service.GetByPedidoVenda(pedidoVendaId);
    }

    /// <summary>
    /// Status de produção consolidado de uma OP.
    /// </summary>
    [HttpGet("{id:int}/status-producao")]
    public async Task<ActionResult<OrdemProducaoStatusProducaoDTO>> GetStatusProducao(int id)
    {
        var status = await _service.GetStatusProducao(id);
        if (status == null)
            return NotFound();
        return status;
    }

    /// <summary>
    /// Comparação da OP (snapshot) com a BOM atual do Produto raiz.
    /// </summary>
    [HttpGet("{id:int}/divergencia-bom")]
    public async Task<ActionResult<OrdemProducaoDivergenciaDTO>> GetDivergencia(int id)
    {
        var div = await _service.GetDivergencia(id);
        if (div == null)
            return NotFound();
        return div;
    }

    /// <summary>
    /// Histórico de eventos da OP.
    /// </summary>
    [HttpGet("{id:int}/historico")]
    public async Task<ActionResult<List<OrdemProducaoHistoricoResponseDTO>>> GetHistorico(int id)
    {
        return await _service.GetHistorico(id);
    }

    /// <summary>
    /// Cria OP Master (PV + Produto raiz).
    /// </summary>
    [HttpPost("master")]
    public async Task<ActionResult<OrdemProducaoResponseDTO>> CriarMaster(OrdemProducaoMasterCreateDTO dto)
    {
        var (criada, erro) = await _service.CriarMaster(dto);
        if (erro != null)
            return BadRequest(new { erro });
        return Ok(criada);
    }

    /// <summary>
    /// Cria OP Filha (vinculada a uma Master, produto da BOM).
    /// </summary>
    [HttpPost("filha")]
    public async Task<ActionResult<OrdemProducaoResponseDTO>> CriarFilha(OrdemProducaoFilhaCreateDTO dto)
    {
        var (criada, erro) = await _service.CriarFilha(dto);
        if (erro != null)
            return BadRequest(new { erro });
        return Ok(criada);
    }

    /// <summary>
    /// Atualiza observações da OP.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OrdemProducaoUpdateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(id, dto);
        if (erro != null)
            return BadRequest(new { erro });
        if (!sucesso)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Altera status da OP. Justificativa obrigatória em Pausada e Cancelada.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, OrdemProducaoStatusDTO dto)
    {
        var (sucesso, erro) = await _service.AlterarStatus(id, dto);
        if (erro != null)
            return BadRequest(new { erro });
        if (!sucesso)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Aponta produção num item da OP. Auto-conclui se todos os itens atingirem o planejado.
    /// </summary>
    [HttpPatch("{id:int}/itens/{itemId:int}/apontar")]
    public async Task<IActionResult> Apontar(int id, int itemId, OrdemProducaoApontamentoDTO dto)
    {
        var (sucesso, erro) = await _service.Apontar(id, itemId, dto);
        if (erro != null)
            return BadRequest(new { erro });
        if (!sucesso)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Exclui OP. Permitido apenas em Pendente, sem apontamentos.
    /// </summary>
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
}
