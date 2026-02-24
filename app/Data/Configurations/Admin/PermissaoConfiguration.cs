using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data.Configurations.Admin;

public class PermissaoConfiguration : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> builder)
    {
        builder.ToTable("Admin_Permissoes");

        builder.HasIndex(p => new { p.FuncionarioId, p.Modulo }).IsUnique();

        builder.HasOne(p => p.Funcionario)
               .WithMany()
               .HasForeignKey(p => p.FuncionarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Modulo).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Nivel).HasConversion<string>().HasMaxLength(20);
    }
}