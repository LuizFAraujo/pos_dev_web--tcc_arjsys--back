using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data.Configurations.Admin;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("Admin_Funcionarios");

        builder.HasOne(f => f.Pessoa)
               .WithMany()
               .HasForeignKey(f => f.PessoaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.Usuario).IsUnique();
        builder.Property(f => f.Usuario).HasMaxLength(50);
        builder.Property(f => f.SenhaHash).HasMaxLength(200);
        builder.Property(f => f.Cargo).HasMaxLength(100);
        builder.Property(f => f.Setor).HasMaxLength(100);
    }
}