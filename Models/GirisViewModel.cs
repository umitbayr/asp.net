using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class GirisViewModel
    {
        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please Enter Valid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [DataType(DataType.Password)] // Bu alanın bir şifre olduğunu belirtir (UI için)
        public string Password { get; set; }

    }
}
