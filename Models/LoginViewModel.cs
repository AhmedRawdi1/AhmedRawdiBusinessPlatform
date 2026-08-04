using System.ComponentModel.DataAnnotations;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "User code is required")]
        [Display(Name = "User Code")]
        public string UserCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
