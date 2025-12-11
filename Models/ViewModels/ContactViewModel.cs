using System.ComponentModel.DataAnnotations;

namespace PetStore.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message cannot be empty.")]
        [StringLength(1000, ErrorMessage = "Message is too long.")]
        public string Message { get; set; }
    }
}