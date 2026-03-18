using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Engenharia_Produtos");

        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.Property(p => p.Codigo).HasMaxLength(50);
        builder.Property(p => p.Descricao).HasMaxLength(140);
        builder.Property(p => p.Unidade).HasConversion<string>().HasMaxLength(2);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);

        // Ordem das colunas (aplicada na criação da tabela)
        builder.Property(p => p.Id).HasColumnOrder(0);
        builder.Property(p => p.Codigo).HasColumnOrder(1);
        builder.Property(p => p.Descricao).HasColumnOrder(2);
        builder.Property(p => p.DescricaoCompleta).HasColumnOrder(3);
        builder.Property(p => p.Unidade).HasColumnOrder(4);
        builder.Property(p => p.Tipo).HasColumnOrder(5);
        builder.Property(p => p.Peso).HasColumnOrder(6);
        builder.Property(p => p.Ativo).HasColumnOrder(7);
        builder.Property(p => p.TemPasta).HasColumnOrder(8);
        builder.Property(p => p.TemDocumento).HasColumnOrder(9);
        builder.Property(p => p.CriadoEm).HasColumnOrder(10);
        builder.Property(p => p.ModificadoEm).HasColumnOrder(11);
        builder.Property(p => p.CriadoPor).HasColumnOrder(12);
        builder.Property(p => p.ModificadoPor).HasColumnOrder(13);
    }
}