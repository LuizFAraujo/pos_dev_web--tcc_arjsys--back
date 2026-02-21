using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class EstruturaProdutoConfiguration : IEntityTypeConfiguration<EstruturaProduto>
{
    public void Configure(EntityTypeBuilder<EstruturaProduto> builder)
    {
        builder.HasIndex(e => new { e.ProdutoPaiId, e.ProdutoFilhoId }).IsUnique();

        builder.HasOne(e => e.ProdutoPai)
               .WithMany()
               .HasForeignKey(e => e.ProdutoPaiId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProdutoFilho)
               .WithMany()
               .HasForeignKey(e => e.ProdutoFilhoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}