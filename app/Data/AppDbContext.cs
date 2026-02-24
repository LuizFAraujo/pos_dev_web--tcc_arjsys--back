using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Admin;

namespace Api_ArjSys_Tcc.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    
    // Engenharia
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<EstruturaProduto> EstruturasProdutos => Set<EstruturaProduto>();
    public DbSet<GrupoProduto> GruposProdutos => Set<GrupoProduto>();
    public DbSet<GrupoVinculo> GruposVinculos => Set<GrupoVinculo>();


    // Admin
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}