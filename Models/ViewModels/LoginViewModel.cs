using System.ComponentModel.DataAnnotations;

namespace RealEstateCRM.Models.Identity
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email е задължителен")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Парола е задължителна")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }
    }
}
