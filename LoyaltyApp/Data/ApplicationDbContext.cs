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
            string connectionString = "Host=localhost;Port=5432;Database=loyalty_db;Username=postgres;Password=zxc";

            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // определение уникальных индексов для таблицы Clients
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(c => c.PhoneNumber).IsUnique();
                entity.HasIndex(c => c.Email).IsUnique();
            });

            // определение уникального индекса для таблицы LoyaltyCards
            modelBuilder.Entity<LoyaltyCard>(entity =>
            {
                entity.HasIndex(lc => lc.CardNumber).IsUnique();
            });
            // настройка связи один-к-одному между client и loyaltycard
            modelBuilder.Entity<Client>()
                .HasOne(c => c.LoyaltyCard)
                .WithOne(lc => lc.Client)
                .HasForeignKey<LoyaltyCard>(lc => lc.ClientId);
            // настройка составного ключа для таблицы "Позиции Покупки"  
            modelBuilder.Entity<PurchaseItem>()
                .HasKey(pi => new {pi.PurchaseId, pi.ProductId});
            // настройка связи многие-ко-многим между Purchase и Product через PurchaseItem
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(pi => pi.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId);

            modelBuilder.Entity<PurchaseItem>()
                 .HasOne(pi => pi.Product)
                 .WithMany(p => p.PurchaseItems)
                 .HasForeignKey(pi => pi.ProductId);
        }
    }
}