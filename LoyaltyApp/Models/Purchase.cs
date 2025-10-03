namespace LoyaltyApp.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public DateTime PurchaseDate { get; set; }
        public string PaymentMethod { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}