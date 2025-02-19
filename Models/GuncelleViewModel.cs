using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class GuncelleViewModel
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "İsim alanı boş bırakılamaz.")] // Boş bırakılırsa hata mesajı döndürür
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please Enter Valid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı alanı boş bırakılamaz.")]
        public string UserName { get; set; }
    }
}
