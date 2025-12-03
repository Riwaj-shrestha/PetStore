using PetStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetStore.Models
{
    public class Product
    {
        [Key] public int ProductID { get; set; }

        [Required, StringLength(100)] public string ProductName { get; set; }

        public int CategoryID { get; set; }
        public Category Category { get; set; }

        [Required, StringLength(50)] public string Breed { get; set; }

        [Required, Range(0, 999)] public int AgeInMonths { get; set; }

        [Required, Column(TypeName = "decimal(6,2)")] public decimal WeightInKg { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")] public decimal Price { get; set; }

        public string Gender { get; set; }          // Male / Female
        public string Color { get; set; }
        [Required] public string HealthInfo { get; set; } // Vaccinated, Dewormed, etc.

        public string Description { get; set; }
        public string ImageUrl { get; set; }

        [Required] public int StockQuantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderedItem> OrderedItems { get; set; }
    }
}