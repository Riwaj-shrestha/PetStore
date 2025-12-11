using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PetStore.Models
{
    // ================================
    // CUSTOM VALIDATION: FUTURE EXPIRY
    // ================================
    public class FutureExpiryAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            string expiry = value.ToString();

            // Validate MM/YY format
            if (!Regex.IsMatch(expiry, @"^(0[1-9]|1[0-2])\/\d{2}$"))
            {
                ErrorMessage = "Invalid expiry format (MM/YY).";
                return false;
            }

            var parts = expiry.Split('/');
            int month = int.Parse(parts[0]);
            int year = int.Parse("20" + parts[1]); // Convert YY → 20YY

            DateTime expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            if (expiryDate < DateTime.Now.Date)
            {
                ErrorMessage = "Card expiry cannot be in the past.";
                return false;
            }

            return true;
        }
    }

    // ================================
    // CHECKOUT MODEL
    // ================================
    public class CheckoutModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can only contain letters and spaces.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$",
            ErrorMessage = "Enter a valid Canadian phone number (e.g., 123-456-7890).")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required.")]
        public string Province { get; set; } = string.Empty;

        // ZIP: Canadian postal code (A1A 1A1)
        [Required(ErrorMessage = "Postal Code is required.")]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d$",
            ErrorMessage = "Enter a valid Canadian postal code (e.g., K1A 0B1).")]
        public string Zip { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card Holder Name is required.")]
        [StringLength(100, ErrorMessage = "Card Holder Name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Card Holder Name can only contain letters and spaces.")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card Number is required.")]
        [CreditCard(ErrorMessage = "Enter a valid card number.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Enter a valid expiry date (MM/YY).")]
        [FutureExpiry(ErrorMessage = "Card expiry cannot be in the past.")]
        public string Expiry { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Enter a valid CVV (3 or 4 digits).")]
        public string CVV { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }
}
