using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.DTOs.Admin;

namespace Api_ArjSys_Tcc.Services.Admin;

/// <summary>
/// Serviço da Configuração da Empresa (singleton - Id = 1).
///
/// Usado por:
/// - Front (GET/PUT pra exibir e editar AnoFundacao)
/// - NumeroSerieService (lê AnoFundacao pra gerar código no formato II.MM.AA.NNNNN
///   e checa Configurado antes de permitir Create)
///
/// Regras:
/// - Update normal: permitido apenas até existir o primeiro NS no banco.
///   Após isso, retorna 400 e exige uso do admin-override.
/// - UpdateAdmin: permite alterar sempre. Reservado pra role Admin (autenticação
///   ainda não implementada - endpoint fica aberto por enquanto).
/// - NS já gerados NUNCA são reescritos quando AnoFundacao muda.
/// </summary>
public class ConfiguracaoEmpresaService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Retorna a configuração singleton (Id = 1).
    /// Se não existir, cria com AnoFundacao = ano atual e Configurado = false (fallback).
    /// </summary>
    public async Task<ConfiguracaoEmpresaResponseDTO> Get()
    {
        var config = await _context.ConfiguracaoEmpresa.FindAsync(1);

        if (config == null)
        {
            config = new ConfiguracaoEmpresa
            {
                Id = 1,
                AnoFundacao = DateTime.UtcNow.Year,
                Configurado = false,
                CriadoEm = DateTime.UtcNow
            };
            _context.ConfiguracaoEmpresa.Add(config);
            await _context.SaveChangesAsync();
        }

        return new ConfiguracaoEmpresaResponseDTO
        {
            Id = config.Id,
            AnoFundacao = config.AnoFundacao,
            Configurado = config.Configurado
        };
    }

    /// <summary>
    /// Update normal: usado pelo usuário comum.
    /// - Valida faixa do ano (1800 .. ano atual).
    /// - Bloqueia se já existe NS emitido (use admin-override pra esse caso).
    /// - Marca Configurado = true (NS pode ser emitido a partir daqui).
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(ConfiguracaoEmpresaUpdateDTO dto)
    {
        var (validacaoOk, validacaoErro) = ValidarAno(dto.AnoFundacao);
        if (!validacaoOk)
            return (false, validacaoErro);

        var jaTemNS = await _context.NumerosSerie.AnyAsync();
        if (jaTemNS)
            return (false, "Não é possível alterar o ano de fundação após emitir Números de Série. Solicite ao administrador.");

        return await PersistirAlteracao(dto.AnoFundacao);
    }

    /// <summary>
    /// Update admin-override: usado por role Admin (autenticação ainda não implementada).
    /// - Valida faixa do ano.
    /// - Permite alterar mesmo após NS existir.
    /// - NS já emitidos NÃO são reescritos - ficam com o código original.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> UpdateAdmin(ConfiguracaoEmpresaUpdateDTO dto)
    {
        var (validacaoOk, validacaoErro) = ValidarAno(dto.AnoFundacao);
        if (!validacaoOk)
            return (false, validacaoErro);

        return await PersistirAlteracao(dto.AnoFundacao);
    }

    /// <summary>
    /// Helper interno usado por NumeroSerieService.GetAnoFundacao.
    /// </summary>
    public async Task<int> GetAnoFundacao()
    {
        var config = await _context.ConfiguracaoEmpresa.FindAsync(1);
        return config?.AnoFundacao ?? DateTime.UtcNow.Year;
    }

    /// <summary>
    /// Helper interno usado por NumeroSerieService pra checar se NS pode ser emitido.
    /// </summary>
    public async Task<bool> IsConfigurado()
    {
        var config = await _context.ConfiguracaoEmpresa.FindAsync(1);
        return config?.Configurado ?? false;
    }

    private static (bool Ok, string? Erro) ValidarAno(int ano)
    {
        if (ano < 1800)
            return (false, "Ano de fundação inválido (mínimo 1800).");

        if (ano > DateTime.UtcNow.Year)
            return (false, "Ano de fundação não pode ser futuro.");

        return (true, null);
    }

    private async Task<(bool Sucesso, string? Erro)> PersistirAlteracao(int novoAno)
    {
        var config = await _context.ConfiguracaoEmpresa.FindAsync(1);

        if (config == null)
        {
            config = new ConfiguracaoEmpresa
            {
                Id = 1,
                AnoFundacao = novoAno,
                Configurado = true,
                CriadoEm = DateTime.UtcNow
            };
            _context.ConfiguracaoEmpresa.Add(config);
        }
        else
        {
            config.AnoFundacao = novoAno;
            config.Configurado = true;
            config.ModificadoEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }
}
