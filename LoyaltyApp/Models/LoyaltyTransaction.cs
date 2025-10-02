namespace LoyaltyApp.Models
{
    public enum TransactionType
    {
        Accrual,  // Начисление
        Deduction // Списание
    }
    public class LoyaltyTransaction
    {
        public int Id { get; set; }

        public int Points { get; set; }

        public TransactionType Type { get; set; }

        public DateTime TransactionDate { get; set; }

        public int LoyaltyCardId { get; set; }

        public int? PurchaseId { get; set; }

        public virtual LoyaltyCard LoyaltyCard { get; set; }
        public virtual Purchase Purchase { get; set; }
    }
}