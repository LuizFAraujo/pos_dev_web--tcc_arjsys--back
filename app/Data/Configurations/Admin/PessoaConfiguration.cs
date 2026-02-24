using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data.Configurations.Admin;

public class PessoaConfiguration : IEntityTypeConfiguration<Pessoa>
{
    public void Configure(EntityTypeBuilder<Pessoa> builder)
    {
        builder.ToTable("Admin_Pessoas");

        builder.Property(p => p.Nome).HasMaxLength(200);
        builder.Property(p => p.CpfCnpj).HasMaxLength(18);
        builder.Property(p => p.Telefone).HasMaxLength(20);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Endereco).HasMaxLength(300);
        builder.Property(p => p.Cidade).HasMaxLength(100);
        builder.Property(p => p.Estado).HasMaxLength(2);
        builder.Property(p => p.Cep).HasMaxLength(10);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
    }
}