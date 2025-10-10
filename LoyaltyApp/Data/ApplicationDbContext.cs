using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<LoyaltyCard> LoyaltyCards { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Host=localhost;Port=5432;Database=loyalty_db;Username=postgres;Password=satan";
            optionsBuilder.UseNpgsql(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<LoyaltyCard>().ToTable("LoyaltyCards");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Purchase>().ToTable("Purchases");
            modelBuilder.Entity<PurchaseItem>().ToTable("PurchaseItems");

            modelBuilder.Entity<PurchaseItem>()
                .HasKey(pi => new { pi.PurchaseId, pi.ProductId });

            modelBuilder.Entity<Client>()
                .HasOne(client => client.LoyaltyCard)      // У Клиента есть одна Карта
                .WithOne(card => card.Client)            // У Карты есть один Клиент
                .HasForeignKey<LoyaltyCard>(card => card.ClientId); // Связь идет через поле ClientId в таблице Карт
        }
    }
}