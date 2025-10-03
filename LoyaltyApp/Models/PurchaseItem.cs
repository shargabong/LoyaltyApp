namespace LoyaltyApp.Models
{
    public class PurchaseItem
    {
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public virtual Purchase Purchase { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}
