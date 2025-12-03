using PetStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetStore.Models
{
    public class OrderedItem
    {
        [Key] public int OrderedItemID { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; }

        [Required] public int Quantity { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")] public decimal PriceAtPurchase { get; set; }
    }
}