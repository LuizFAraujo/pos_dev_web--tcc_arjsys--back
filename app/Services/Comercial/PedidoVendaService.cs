using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço do Pedido de Venda.
/// Gerencia CRUD, transições de status com justificativa condicional e histórico de eventos.
/// </summary>
public class PedidoVendaService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista todos os PVs. Suporta paginação opcional.
    /// </summary>
    public async Task<List<PedidoVendaResponseDTO>> GetAll(int pagina = 0, int tamanho = 0)
    {
        var query = _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .OrderByDescending(p => p.Data);

        List<PedidoVenda> pedidos;

        if (pagina > 0 && tamanho > 0)
            pedidos = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            pedidos = await query.ToListAsync();

        var resultado = new List<PedidoVendaResponseDTO>();

        foreach (var p in pedidos)
        {
            var itens = await _context.PedidosVendaItens
                .Where(i => i.PedidoVendaId == p.Id)
                .OrderBy(i => i.Id)
                .ToListAsync();

            resultado.Add(ToResponseDTO(p, itens));
        }

        return resultado;
    }

    /// <summary>
    /// Busca PV por ID, incluindo itens.
    /// </summary>
    public async Task<PedidoVendaResponseDTO?> GetById(int id)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return null;

        var itens = await _context.PedidosVendaItens
            .Where(i => i.PedidoVendaId == id)
            .OrderBy(i => i.Id)
            .ToListAsync();

        return ToResponseDTO(pedido, itens);
    }

    /// <summary>
    /// Cria Pedido de Venda. Status inicial é definido pelo tipo:
    /// Normal → EmAndamento, VendaFutura → Aguardando.
    /// Registra evento Criado no histórico.
    /// </summary>
    public async Task<(PedidoVendaResponseDTO? Item, string? Erro)> Create(PedidoVendaCreateDTO dto)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == dto.ClienteId);

        if (cliente == null)
            return (null, "Cliente não encontrado");

        var statusInicial = dto.Tipo == TipoPedidoVenda.VendaFutura
            ? StatusPedidoVenda.Aguardando
            : StatusPedidoVenda.EmAndamento;

        var codigo = await GerarCodigo();
        var agora = DateTime.UtcNow;

        var pedido = new PedidoVenda
        {
            Codigo = codigo,
            ClienteId = dto.ClienteId,
            Tipo = dto.Tipo,
            Status = statusInicial,
            Data = dto.Data ?? agora,
            DataEntrega = dto.DataEntrega,
            Observacoes = dto.Observacoes,
            CriadoEm = agora
        };

        _context.PedidosVenda.Add(pedido);
        await _context.SaveChangesAsync();

        RegistrarEvento(pedido.Id, EventoPedidoVenda.Criado, null, statusInicial, null);
        await _context.SaveChangesAsync();

        pedido.Cliente = cliente;
        return (ToResponseDTO(pedido, []), null);
    }

    /// <summary>
    /// Atualiza dados cadastrais do PV. Permitido apenas em Aguardando ou EmAndamento.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int id, PedidoVendaCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, null);

        if (pedido.Status != StatusPedidoVenda.Aguardando && pedido.Status != StatusPedidoVenda.EmAndamento)
            return (false, "Só é possível editar pedidos com status Aguardando ou Em Andamento");

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);

        if (cliente == null)
            return (false, "Cliente não encontrado");

        pedido.ClienteId = dto.ClienteId;
        pedido.Tipo = dto.Tipo;
        pedido.Data = dto.Data ?? pedido.Data;
        pedido.DataEntrega = dto.DataEntrega;
        pedido.Observacoes = dto.Observacoes;
        pedido.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Altera status do PV com validação de transição e justificativa.
    /// Justificativa é obrigatória em: pausar, cancelar, retroceder (qualquer) e reabrir (sair do Cancelado).
    /// Registra evento no histórico com statusAnterior, statusNovo e justificativa.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> AlterarStatus(int id, StatusPedidoVendaDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, null);

        var statusAnterior = pedido.Status;
        var novoStatus = dto.NovoStatus;

        if (statusAnterior == novoStatus)
            return (false, $"O pedido já está com status {novoStatus}");

        // Justificativa: obrigatória em pausar, cancelar, reabrir e retroceder
        var exigeJustificativa = ExigeJustificativa(statusAnterior, novoStatus);
        var justificativa = dto.Justificativa?.Trim();

        if (exigeJustificativa && string.IsNullOrWhiteSpace(justificativa))
            return (false, "Justificativa é obrigatória para esta transição de status");

        pedido.Status = novoStatus;
        pedido.ModificadoEm = DateTime.UtcNow;

        var evento = MapearEvento(statusAnterior, novoStatus);
        RegistrarEvento(id, evento, statusAnterior, novoStatus, justificativa);

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Exclui PV. Permitido apenas em Aguardando e sem NS vinculado.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, null);

        if (pedido.Status != StatusPedidoVenda.Aguardando)
            return (false, "Só é possível excluir pedidos com status Aguardando");

        var temNS = await _context.NumerosSerie.AnyAsync(n => n.PedidoVendaId == id);

        if (temNS)
            return (false, "Não é possível excluir pedido com Número de Série vinculado");

        var itens = await _context.PedidosVendaItens.Where(i => i.PedidoVendaId == id).ToListAsync();
        _context.PedidosVendaItens.RemoveRange(itens);

        var historico = await _context.PedidoVendaHistorico.Where(h => h.PedidoVendaId == id).ToListAsync();
        _context.PedidoVendaHistorico.RemoveRange(historico);

        _context.PedidosVenda.Remove(pedido);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Retorna o histórico de eventos de um PV, ordenado do mais recente pro mais antigo.
    /// </summary>
    public async Task<List<PedidoVendaHistoricoResponseDTO>> GetHistorico(int pedidoVendaId)
    {
        return await _context.PedidoVendaHistorico
            .Where(h => h.PedidoVendaId == pedidoVendaId)
            .OrderByDescending(h => h.DataHora)
            .Select(h => new PedidoVendaHistoricoResponseDTO
            {
                Id = h.Id,
                PedidoVendaId = h.PedidoVendaId,
                Evento = h.Evento,
                StatusAnterior = h.StatusAnterior,
                StatusNovo = h.StatusNovo,
                Justificativa = h.Justificativa,
                DataHora = h.DataHora
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gera o próximo código no formato PV.AAAA.MM.NNNN.
    /// </summary>
    private async Task<string> GerarCodigo()
    {
        var agora = DateTime.UtcNow;
        var prefixo = $"PV.{agora:yyyy}.{agora:MM}";

        var ultimo = await _context.PedidosVenda
            .Where(p => p.Codigo.StartsWith(prefixo))
            .OrderByDescending(p => p.Codigo)
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

    /// <summary>
    /// Nível ordinal do status — usado para detectar retrocesso.
    /// Cancelado é terminal (nível -1); qualquer saída do Cancelado é reabertura.
    /// Pausado divide o nível 1 com EmAndamento (transição lateral, não retrocesso).
    /// </summary>
    private static int NivelStatus(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.Cancelado => -1,
        StatusPedidoVenda.Aguardando => 0,
        StatusPedidoVenda.EmAndamento => 1,
        StatusPedidoVenda.Pausado => 1,
        StatusPedidoVenda.Concluido => 2,
        StatusPedidoVenda.AguardandoEntrega => 3,
        StatusPedidoVenda.Entregue => 4,
        _ => 0
    };

    /// <summary>
    /// Determina se a transição exige justificativa obrigatória.
    /// Regra: pausar, cancelar, reabrir (saída do Cancelado) e retroceder (nível menor).
    /// </summary>
    private static bool ExigeJustificativa(StatusPedidoVenda atual, StatusPedidoVenda novo)
    {
        if (novo == StatusPedidoVenda.Pausado) return true;
        if (novo == StatusPedidoVenda.Cancelado) return true;
        if (atual == StatusPedidoVenda.Cancelado) return true; // reabertura
        return NivelStatus(novo) < NivelStatus(atual);          // retrocesso
    }

    /// <summary>
    /// Mapeia a transição de status para o evento correspondente no histórico.
    /// </summary>
    private static EventoPedidoVenda MapearEvento(StatusPedidoVenda anterior, StatusPedidoVenda novo)
    {
        if (anterior == StatusPedidoVenda.Cancelado)
            return EventoPedidoVenda.Reaberto;

        return novo switch
        {
            StatusPedidoVenda.EmAndamento when anterior == StatusPedidoVenda.Aguardando => EventoPedidoVenda.Aprovado,
            StatusPedidoVenda.EmAndamento when anterior == StatusPedidoVenda.Pausado => EventoPedidoVenda.Retomado,
            StatusPedidoVenda.EmAndamento => EventoPedidoVenda.Retomado,
            StatusPedidoVenda.Pausado => EventoPedidoVenda.Pausado,
            StatusPedidoVenda.Cancelado => EventoPedidoVenda.Cancelado,
            StatusPedidoVenda.Concluido => EventoPedidoVenda.Concluido,
            StatusPedidoVenda.AguardandoEntrega => EventoPedidoVenda.AguardandoEntrega,
            StatusPedidoVenda.Entregue => EventoPedidoVenda.Entregue,
            StatusPedidoVenda.Aguardando => EventoPedidoVenda.Criado,
            _ => EventoPedidoVenda.Criado
        };
    }

    /// <summary>
    /// Registra um evento no histórico do PV.
    /// </summary>
    private void RegistrarEvento(
        int pedidoVendaId,
        EventoPedidoVenda evento,
        StatusPedidoVenda? statusAnterior,
        StatusPedidoVenda? statusNovo,
        string? justificativa)
    {
        _context.PedidoVendaHistorico.Add(new PedidoVendaHistorico
        {
            PedidoVendaId = pedidoVendaId,
            Evento = evento,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            Justificativa = justificativa,
            DataHora = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Converte entidade PV + itens em DTO de resposta.
    /// </summary>
    private static PedidoVendaResponseDTO ToResponseDTO(PedidoVenda p, List<PedidoVendaItem> itens) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        ClienteId = p.ClienteId,
        ClienteNome = p.Cliente?.Pessoa?.Nome ?? string.Empty,
        Tipo = p.Tipo,
        Status = p.Status,
        Data = p.Data,
        DataEntrega = p.DataEntrega,
        Observacoes = p.Observacoes,
        Itens = itens.Select(i => new PedidoVendaItemResponseDTO
        {
            Id = i.Id,
            PedidoVendaId = i.PedidoVendaId,
            Quantidade = i.Quantidade,
            Descricao = i.Descricao,
            Observacao = i.Observacao,
            CriadoEm = i.CriadoEm,
            ModificadoEm = i.ModificadoEm
        }).ToList(),
        TotalItens = itens.Count,
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}
