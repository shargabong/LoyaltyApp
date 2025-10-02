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

        // "один ко многим"
        public virtual ICollection<LoyaltyCard> LoyaltyCards { get; set; } = new List<LoyaltyCard>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}