using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<LoyaltyCard> LoyaltyCards { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

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

            // спец.хрень для постгреса, шобы правильно работать с enum.
            // вот эта строка создаст в бд спец.тип 'transaction_type'
            modelBuilder.HasPostgresEnum<TransactionType>();
        }
    }
}