using System.ComponentModel.DataAnnotations;

namespace LoyaltyApp.Models
{
    public class LoyaltyCard
    {
        public int Id { get; set; }

        [Required]
        public string CardNumber { get; set; }

        public decimal DiscountPercent { get; set; }

        // внешний ключ для связи один-к-одному
        public int ClientId { get; set; }

        public virtual Client Client { get; set; }
    }
}