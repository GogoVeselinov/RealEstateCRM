using System.ComponentModel.DataAnnotations;

namespace RealEstateCRM.Models.Identity
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Имейл адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Въведете валиден имейл адрес")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Паролата е задължителна")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да бъде между {2} и {1} символа")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }
    }
}
