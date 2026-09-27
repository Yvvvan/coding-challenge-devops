using CodingChallenge.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Component> Components => Set<Component>();
    public DbSet<OrderBoard> OrderBoards => Set<OrderBoard>();
    public DbSet<BoardComponent> BoardComponents => Set<BoardComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderBoard>(entity =>
        {
            entity.HasKey(ob => new { ob.OrderId, ob.BoardId });
            entity.HasOne(ob => ob.Order)
                .WithMany(o => o.OrderBoards)
                .HasForeignKey(ob => ob.OrderId);
            entity.HasOne(ob => ob.Board)
                .WithMany(b => b.OrderBoards)
                .HasForeignKey(ob => ob.BoardId);
        });

        modelBuilder.Entity<BoardComponent>(entity =>
        {
            entity.HasKey(bc => new { bc.BoardId, bc.ComponentId });
            entity.HasOne(bc => bc.Board)
                .WithMany(b => b.BoardComponents)
                .HasForeignKey(bc => bc.BoardId);
            entity.HasOne(bc => bc.Component)
                .WithMany(c => c.BoardComponents)
                .HasForeignKey(bc => bc.ComponentId);
        });

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();
    }
}
