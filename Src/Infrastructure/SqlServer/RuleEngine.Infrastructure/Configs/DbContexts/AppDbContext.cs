using Microsoft.EntityFrameworkCore;
using RuleEngine.Domain.Orders;
using RuleEngine.Domain.Products;
using RuleEngine.Domain.Users;

namespace RuleEngine.Infrastructure.Configs.DbContexts;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    
}