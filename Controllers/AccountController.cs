using System.Security.Claims;
using Login.Entities;
using Login.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;

namespace Login.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Veritabanı erişimi için dependency injection ile AppDbContext kullanılıyor.
        public AccountController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        
        // Kullanıcı kayıt sayfasını döndürür
        public IActionResult Kayit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Kayit(KayitViewModel model)
        {
            if (ModelState.IsValid) // Model doğrulama geçerli mi?
            {
                // Kullanıcı zaten var mı kontrol et
                var existingUser = _context.UserAccounts.FirstOrDefault(x => x.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Bu e-posta zaten kayıtlı.");
                    return View(model);
                }

                // 6 haneli rastgele doğrulama kodu oluştur
                var verificationCode = new Random().Next(100000, 999999).ToString();

                // Kullanıcıyı oluştur
                var user = new UserAccount
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName,
                    Password = model.Password, // Güvenlik için şifre HASH'lenmeli
                    EmailVerificationCode = verificationCode,
                    IsEmailConfirmed = false // Başlangıçta doğrulanmamış olacak
                };

                _context.UserAccounts.Add(user);
                _context.SaveChanges();

                // Kullanıcıya doğrulama kodu e-posta ile gönder
                SendVerificationEmail(user.Email, verificationCode);

                // Kullanıcıyı doğrulama sayfasına yönlendir
                return RedirectToAction("EmailDogrulama", new { email = user.Email });
            }

            return View(model);
        }

        // Kullanıcı giriş sayfasını döndürür
        public IActionResult Giris()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (ModelState.IsValid) // Model doğrulama geçerli mi?
            {
                // Kullanıcıyı e-posta ve şifreyle sorgula
                var user = _context.UserAccounts.FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Şifre veya e-mail hatalı.");
                    return View(model);
                }

                // Eğer e-posta doğrulanmadıysa, girişe izin verme
                if (!user.IsEmailConfirmed)
                {
                    ModelState.AddModelError("", "E-posta adresinizi doğrulamadınız.");
                    return View(model);
                }

                // Kullanıcı oturum açma işlemi için kimlik bilgilerini oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim("FirstName", user.FirstName), // Kullanıcı adı
                    new Claim("LastName", user.LastName),   // Kullanıcı soyadı
                    new Claim(ClaimTypes.Role, "User"),     // Kullanıcı rolü
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(claimsIdentity);

                // Kullanıcıya oturum açtır
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Başarılı giriş sonrası yönlendirme
                return RedirectToAction("Dashboard", "Admin");
                // Admin sayfasına yönlendir
            }

            // Model geçersizse tekrar giriş formunu göster
            return View(model);
        }

        // Kullanıcı çıkış işlemi
        public IActionResult Cikis()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Giris");
        }

        [Authorize] // Yetkilendirilmiş kullanıcılar erişebilir
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }

        // Kullanıcıya e-posta doğrulama kodu gönderen metod
        private void SendVerificationEmail(string email, string verificationCode)
        {
            var fromAddress = new MailAddress("bayramumit16@gmail.com", "Panelia");
            var toAddress = new MailAddress(email);
            const string fromPassword = "mazc kxqy itxk fytm"; // Gmail için uygulama şifresi kullanılmalı
            const string subject = "E-posta Doğrulama Kodu";
            string body = $"Merhaba,\n\nE-posta adresinizi doğrulamak için kodunuz: {verificationCode}\n\nBu kodu ilgili ekrana girerek işlemi tamamlayabilirsiniz.";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        // Kullanıcı doğrulama kodunu girerek e-postasını doğrular
        [HttpPost]
        public IActionResult EmailDogrulama(EmailDogrulamaViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcıyı e-postasına göre bul
                var user = _context.UserAccounts.SingleOrDefault(x => x.Email == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                    return View(model);
                }

                // Eğer girilen doğrulama kodu doğruysa, hesabı doğrula
                if (user.EmailVerificationCode == model.VerificationCode)
                {
                    user.IsEmailConfirmed = true; // E-posta doğrulandı
                    user.EmailVerificationCode = ""; // Kod sıfırlandı
                    _context.SaveChanges();
                    return RedirectToAction("Giris"); // Giriş sayfasına yönlendir
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz doğrulama kodu.");
                    return View(model);
                }

            }

            return View(model);
        }

        // Doğrulama sayfasını döndürür, e-posta adresini model içine yerleştirir
        public IActionResult EmailDogrulama(string email)
        {
            var model = new EmailDogrulamaViewModel
            {
                Email = email // Kullanıcının e-posta adresi view'e aktarılır
            };

            return View(model); // EmailDogrulama sayfasını döndür
        }

        public IActionResult SifreBelirle(string token, string email)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == email && u.EmailVerificationCode == token);
            if (user == null)
            {
                return BadRequest("Geçersiz veya süresi dolmuş bağlantı.");
            }

            return View(new SifreBelirleViewModel { Email = email, Token = token });
        }



        [HttpPost]
        public IActionResult SifreBelirle(SifreBelirleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == model.Email && u.EmailVerificationCode == model.Token);
            if (user == null)
            {
                return BadRequest("Geçersiz veya süresi dolmuş bağlantı.");
            }

            user.Password = model.Password;
            user.EmailVerificationCode = ""; // Token artık geçersiz hale gelir
            user.IsEmailConfirmed = true;
            _context.SaveChanges();

            ViewBag.Message = "Şifreniz başarıyla oluşturuldu. Giriş yapabilirsiniz.";
            return RedirectToAction("Giris", "Account");
        }

        
    }
}

