/*
 * Author: Riwaj Shrestha
 * Id: 8890002
 */
namespace PetStore.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
