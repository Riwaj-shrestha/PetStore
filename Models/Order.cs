using PetStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetStore.Models
{
    public class Order
    {
        [Key] public int OrderID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, Column(TypeName = "decimal(10,2)")] public decimal TotalAmount { get; set; }

        [Required] public string Status { get; set; } = "Pending";

        [Required] public string ShippingAddress { get; set; }

        public ICollection<OrderedItem> OrderedItems { get; set; }
    }
}