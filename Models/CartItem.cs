using PetStore.Models;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Models
{
    public class CartItem
    {
        [Key] public int CartItemID { get; set; }

        public int CartID { get; set; }
        public ShoppingCart Cart { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; }

        [Required, Range(1, 100)] public int Quantity { get; set; } = 1;
    }
}