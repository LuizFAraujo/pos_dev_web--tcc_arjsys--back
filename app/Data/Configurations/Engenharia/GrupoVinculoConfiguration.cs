using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data.Configurations.Engenharia;

public class GrupoVinculoConfiguration : IEntityTypeConfiguration<GrupoVinculo>
{
    public void Configure(EntityTypeBuilder<GrupoVinculo> builder)
    {
        builder.HasIndex(v => new { v.GrupoPaiId, v.GrupoFilhoId }).IsUnique();

        builder.HasOne(v => v.GrupoPai)
               .WithMany()
               .HasForeignKey(v => v.GrupoPaiId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.GrupoFilho)
               .WithMany()
               .HasForeignKey(v => v.GrupoFilhoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}