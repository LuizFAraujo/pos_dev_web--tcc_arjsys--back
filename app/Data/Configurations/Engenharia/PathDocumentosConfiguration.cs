using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class PathDocumentosConfiguration : IEntityTypeConfiguration<PathDocumentos>
{
    public void Configure(EntityTypeBuilder<PathDocumentos> builder)
    {
        builder.ToTable("Engenharia_PathDocumentos");

        // Um path por prefixo (GrupoProduto Coluna1)
        builder.HasIndex(p => p.GrupoProdutoId).IsUnique();

        builder.Property(p => p.Path).HasMaxLength(500);

        builder.HasOne(p => p.GrupoProduto)
               .WithMany()
               .HasForeignKey(p => p.GrupoProdutoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}