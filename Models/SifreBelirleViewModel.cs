using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class SifreBelirleViewModel
    {

        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}

