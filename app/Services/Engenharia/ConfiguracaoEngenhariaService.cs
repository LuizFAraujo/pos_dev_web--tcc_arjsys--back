using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class ConfiguracaoEngenhariaService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<ConfiguracaoEngenhariaResponseDTO>> GetAll()
    {
        return await _context.ConfiguracoesEngenharia
            .OrderBy(c => c.Chave)
            .Select(c => ToResponseDTO(c))
            .ToListAsync();
    }

    public async Task<ConfiguracaoEngenhariaResponseDTO?> GetByChave(string chave)
    {
        var config = await _context.ConfiguracoesEngenharia
            .FirstOrDefaultAsync(c => c.Chave == chave);

        return config == null ? null : ToResponseDTO(config);
    }

    public async Task<(ConfiguracaoEngenhariaResponseDTO? Item, string? Erro)> Create(ConfiguracaoEngenhariaCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Chave))
            return (null, "Chave é obrigatória");

        var existe = await _context.ConfiguracoesEngenharia.AnyAsync(c => c.Chave == dto.Chave);

        if (existe)
            return (null, "Já existe uma configuração com esta chave");

        var config = new ConfiguracaoEngenharia
        {
            Chave = dto.Chave,
            Valor = dto.Valor,
            Descricao = dto.Descricao,
            CriadoEm = DateTime.UtcNow
        };

        _context.ConfiguracoesEngenharia.Add(config);
        await _context.SaveChangesAsync();

        return (ToResponseDTO(config), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, ConfiguracaoEngenhariaCreateDTO dto)
    {
        var config = await _context.ConfiguracoesEngenharia.FindAsync(id);

        if (config == null)
            return (false, "Configuração não encontrada");

        config.Valor = dto.Valor;
        config.Descricao = dto.Descricao;
        config.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var config = await _context.ConfiguracoesEngenharia.FindAsync(id);

        if (config == null)
            return (false, "Configuração não encontrada");

        _context.ConfiguracoesEngenharia.Remove(config);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static ConfiguracaoEngenhariaResponseDTO ToResponseDTO(ConfiguracaoEngenharia c) => new()
    {
        Id = c.Id,
        Chave = c.Chave,
        Valor = c.Valor,
        Descricao = c.Descricao,
        CriadoEm = c.CriadoEm,
        ModificadoEm = c.ModificadoEm
    };
}