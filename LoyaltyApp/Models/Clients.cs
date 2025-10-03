using System.ComponentModel.DataAnnotations;

namespace LoyaltyApp.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; }

        // "свойство для связи один-к-одному с картой"
        public virtual LoyaltyCard LoyaltyCard { get; set; }
        // у клиента может быть много покупок
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}