using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Codigo).HasMaxLength(50);
            entity.Property(p => p.Descricao).HasMaxLength(140);
            entity.Property(p => p.Unidade).HasConversion<string>().HasMaxLength(2);
            entity.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        });
    }
}