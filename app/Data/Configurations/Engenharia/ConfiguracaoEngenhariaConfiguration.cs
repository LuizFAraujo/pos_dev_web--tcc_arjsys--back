using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class ConfiguracaoEngenhariaConfiguration : IEntityTypeConfiguration<ConfiguracaoEngenharia>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoEngenharia> builder)
    {
        builder.ToTable("Engenharia_Configuracoes");

        builder.HasIndex(c => c.Chave).IsUnique();
        builder.Property(c => c.Chave).HasMaxLength(100);
        builder.Property(c => c.Valor).HasMaxLength(500);
        builder.Property(c => c.Descricao).HasMaxLength(300);
    }
}