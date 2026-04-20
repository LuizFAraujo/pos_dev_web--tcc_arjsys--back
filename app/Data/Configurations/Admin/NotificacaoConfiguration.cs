using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data.Configurations.Admin;

/// <summary>
/// Configuração EF da tabela Admin_Notificacoes.
/// </summary>
public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("Admin_Notificacoes");

        builder.Property(n => n.ModuloDestino).HasConversion<string>().HasMaxLength(30);
        builder.Property(n => n.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Titulo).HasMaxLength(100);
        builder.Property(n => n.Mensagem).HasMaxLength(500);
        builder.Property(n => n.OrigemTabela).HasMaxLength(100);

        // Índice composto para acelerar a listagem típica: "não lidas por módulo"
        builder.HasIndex(n => new { n.ModuloDestino, n.Lida });
    }
}
