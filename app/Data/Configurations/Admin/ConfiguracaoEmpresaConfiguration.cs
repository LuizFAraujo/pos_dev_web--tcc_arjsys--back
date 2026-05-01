using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data.Configurations.Admin;

public class ConfiguracaoEmpresaConfiguration : IEntityTypeConfiguration<ConfiguracaoEmpresa>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoEmpresa> builder)
    {
        builder.ToTable("Admin_ConfiguracaoEmpresa");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.AnoFundacao).IsRequired();
        builder.Property(c => c.Configurado).IsRequired().HasDefaultValue(false);
    }
}
