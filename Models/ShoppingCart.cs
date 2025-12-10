using PetStore.Models;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Models
{
    public class ShoppingCart
    {
        [Key] public int CartID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItem> CartItems { get; set; }
    }
}