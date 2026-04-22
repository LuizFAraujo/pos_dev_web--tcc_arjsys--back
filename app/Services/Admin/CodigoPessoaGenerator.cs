using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.Services.Admin;

/// <summary>
/// Geração de código humano único por Pessoa.
/// Prefixo conforme o tipo: CLI- (Cliente), FUN- (Funcionario), FOR- (Fornecedor).
/// Sequencial independente por tipo, padding 4 dígitos (expande naturalmente acima de 9999).
/// </summary>
public static class CodigoPessoaGenerator
{
    /// <summary>
    /// Gera o próximo código para o tipo informado baseado nos códigos existentes.
    /// </summary>
    public static async Task<string> GerarProximo(AppDbContext context, TipoPessoa tipo)
    {
        var prefixo = PrefixoPorTipo(tipo);
        var prefixoComHifen = $"{prefixo}-";

        var existentes = await context.Pessoas
            .Where(p => p.Codigo.StartsWith(prefixoComHifen))
            .Select(p => p.Codigo)
            .ToListAsync();

        var maxSeq = 0;
        foreach (var cod in existentes)
        {
            var partes = cod.Split('-');
            if (partes.Length == 2 && int.TryParse(partes[1], out var n) && n > maxSeq)
                maxSeq = n;
        }

        var proximo = maxSeq + 1;
        return $"{prefixo}-{proximo:D4}";
    }

    /// <summary>
    /// Prefixo de 3 letras por tipo.
    /// </summary>
    public static string PrefixoPorTipo(TipoPessoa tipo) => tipo switch
    {
        TipoPessoa.Cliente     => "CLI",
        TipoPessoa.Funcionario => "FUN",
        TipoPessoa.Fornecedor  => "FOR",
        _                      => "PES"
    };
}
