using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Login.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    public class UserAccount
    {
        [Key]
        public int UserId { get; set; }  // userID -> UserId olarak değiştirildi

        [Required(ErrorMessage = "İsim alanı boş bırakılamaz.")]
        
        public string FirstName { get; set; }  // firstName -> FirstName olarak değiştirildi

        [Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
        
        public string LastName { get; set; }  // lastName -> LastName olarak değiştirildi

        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        
        public string Email { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı alanı boş bırakılamaz.")]
        
        public string UserName { get; set; }  // userName -> UserName olarak değiştirildi

        public string? Password { get; set; }  // nullable yapmak için `?` ekledik


        public string EmailVerificationCode { get; set; } // Doğrulama kodu
        public bool IsEmailConfirmed { get; set; } // Kullanıcı doğrulandı mı?

        // Kullanıcıya atanacak rol
        public string Role { get; set; } = "User"; // Varsayılan olarak "User"
    }
}
