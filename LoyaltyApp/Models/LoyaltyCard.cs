using System.ComponentModel.DataAnnotations;

namespace LoyaltyApp.Models
{
    public class LoyaltyCard
    {
        public int Id { get; set; }

        [Required]
        public string CardNumber { get; set; }

        public DateTime IssueDate { get; set; }

        // Внешний ключ для связи с клиентом
        public int ClientId { get; set; }

        // Навигационные свойства
        public virtual Client Client { get; set; }
        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }
}