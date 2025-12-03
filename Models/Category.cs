using PetStore.Models;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Models
{
    public class Category
    {
        [Key] public int CategoryID { get; set; }

        [Required, StringLength(50)] public string CategoryName { get; set; }
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}