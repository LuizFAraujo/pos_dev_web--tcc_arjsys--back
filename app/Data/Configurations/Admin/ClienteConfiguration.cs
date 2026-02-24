using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data.Configurations.Admin;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Admin_Clientes");

        builder.HasOne(c => c.Pessoa)
               .WithMany()
               .HasForeignKey(c => c.PessoaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.RazaoSocial).HasMaxLength(200);
        builder.Property(c => c.InscricaoEstadual).HasMaxLength(20);
        builder.Property(c => c.ContatoComercial).HasMaxLength(200);
    }
}