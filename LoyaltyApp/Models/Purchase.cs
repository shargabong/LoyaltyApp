namespace LoyaltyApp.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime PurchaseDate { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    }
}