using System.ComponentModel.DataAnnotations;

namespace PetStore.Models
{
    public class CheckoutModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit phone number.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "ZIP code is required.")]
        [RegularExpression(@"^[A-Za-z]{2}\d{3}$", ErrorMessage = "Enter a valid ZIP code (e.g., AB123).")]
        public string Zip { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card Number is required.")]
        [CreditCard(ErrorMessage = "Enter a valid card number.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Enter a valid expiry date (MM/YY).")]
        public string Expiry { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Enter a valid CVV (3 or 4 digits).")]
        public string CVV { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }
}
