using Microsoft.EntityFrameworkCore;
using SupernovaColheita.Api.Models;

namespace SupernovaColheita.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produtor> Produtores => Set<Produtor>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Oportunidade> Oportunidades => Set<Oportunidade>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RegistroIdempotencia> RegistrosIdempotencia =>
        Set<RegistroIdempotencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<RegistroIdempotencia>()
            .HasIndex(r => new { r.Chave, r.Rota })
            .IsUnique();

        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Produtor)
            .WithMany()
            .HasForeignKey(p => p.ProdutorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasConversion<double>();

        modelBuilder.Entity<Produto>()
            .Property(p => p.QuantidadeDisponivel)
            .HasConversion<double>();

        modelBuilder.Entity<Oportunidade>()
            .Property(o => o.ValorEstimado)
            .HasConversion<double>();
    }
}