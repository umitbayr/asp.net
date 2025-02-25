using System.Net.Mail;
using System.Net;
using Login.Entities;
using Login.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetCoreGeneratedDocument;
using Microsoft.IdentityModel.Tokens;

namespace Login.Controllers
{
    public class AdminController : Controller
    {

        private readonly AppDbContext _context;

        // Veritabanı erişimi için dependency injection ile AppDbContext kullanılıyor.
        public AdminController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Listele()
        {
            return View(_context.UserAccounts.ToList());
        }

        public IActionResult Ekle()
        {
            return View();
        }
        public IActionResult RolListele()
        {
            return View(_context.UserAccounts.ToList());
        }

        [HttpPost]
        public IActionResult Ekle(EkleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var varmiUser = _context.UserAccounts.FirstOrDefault(x => x.Email == model.Email);
                if (varmiUser != null)
                {
                    ModelState.AddModelError("", "Bu e-posta zaten kayıtlı.");
                    return View(model);
                }
            }
            // Kullanıcıya özel şifre oluşturma tokeni oluştur
            string pwToken = Guid.NewGuid().ToString();

            // Kullanıcıyı oluştur
            var user = new UserAccount
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
                Password = null,
                Role = model.Role,
                EmailVerificationCode = pwToken
            };
            _context.UserAccounts.Add(user);
            _context.SaveChanges();

            // Kullanıcıya sifre belirlemesi için e-posta ile gönder
            gonderSifreBelirleme(user.Email, pwToken);

            ViewBag.Message = "Kullanıcı eklendi ve şifre belirleme linki gönderildi!";

            // Kullanıcıyı sifre olusturma sayfasına yönlendir
            return View();
        }

        private void gonderSifreBelirleme(string email, string token)
        {
            const string fromEmail = "bayramumit16@gmail.com";  // Gönderen e-posta adresi
            const string fromPassword = "mazc kxqy itxk fytm";  // Gönderen e-posta şifresi (Gmail için uygulama şifresi kullanılmalı)
            const string subject = "Şifre Oluşturma Linki";  // E-posta konusu

            // Şifre sıfırlama linki
            string sifreOlustur = $"https://localhost:7086/Account/SifreBelirle?token={token}&email={email}";

            // E-posta içeriği
            string body = $"Merhaba,\n\n" +
                          $"Şifrenizi oluşturmak için aşağıdaki bağlantıyı kullanabilirsiniz:\n" +
                          $"{sifreOlustur}\n\n" +
                          $"Bu bağlantının geçerliliği sınırlıdır.";

            // SMTP ayarları
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",  // SMTP sunucu adresi (Gmail örneği)
                Port = 587,  // SMTP portu
                EnableSsl = true,  // SSL kullanımı
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            // E-posta gönderimi
            using (var message = new MailMessage(fromEmail, email)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);  // E-postayı gönder
            }
        }

        public IActionResult Sil(int id)
        {
            var user = _context.UserAccounts.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.UserAccounts.Remove(user);
            _context.SaveChanges();

            TempData["Message"] = "Kullanıcı başarıyla silindi!";
            return RedirectToAction("Listele"); // Kullanıcı listesine geri dön
        }

        [HttpGet]
        public IActionResult Guncelle(int id)
        {
            var user = _context.UserAccounts.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            // 
            var model = new GuncelleViewModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                UserId = user.UserId
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Guncelle(GuncelleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _context.UserAccounts.Find(model.UserId);
            if (user == null)
            {
                return NotFound();
            }
            // update user
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.Email = model.Email;

            // veritabanına kaydet

            _context.UserAccounts.Update(user);
            _context.SaveChanges();
            TempData["Message"] = "Kullanıcı başarıyla güncellendi!";

            // Listele sayfasına yönlendir
            return RedirectToAction("Listele", "Admin");
        }


        [Route("Admin/RolGuncelle/{userId}")]
        [HttpGet]
        public IActionResult RolGuncelle(int userId, string role)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcının rolünü güncelle
            user.Role = role;
            _context.SaveChanges();

            TempData["Message"] = "Rol başarıyla güncellendi!";
            return RedirectToAction("RolListele");
        }
        [HttpGet]
        public IActionResult Arama(string search, int columnIndex)
        {
            // 1. Eğer arama metni boş veya null ise, tüm kullanıcıları listele
            if (string.IsNullOrEmpty(search))
            {
                // Arama metni yoksa, tüm kullanıcıları getir ve "_UserListPartial" adlı partial view'ı döndür
                return PartialView("_UserListPartial", _context.UserAccounts.ToList());
            }

            IQueryable<UserAccount> users = _context.UserAccounts;

            switch (columnIndex)
            {
                case 0: // Id sütunu
                    if (int.TryParse(search, out int userId))
                        users = users.Where(u => u.UserId == userId);
                    break;
                case 1: // firstname sütunu
                    users = users.Where(u => u.FirstName.Contains(search));
                    break;
                case 2:
                    users = users.Where(u => u.LastName.Contains(search));
                    break;
                case 3:
                    users = users.Where(u => u.Email.Contains(search));
                    break;
                case 4:
                    users = users.Where(u => u.UserName.Contains(search));
                    break;
                case 5:
                    bool dogrulanmisMi = search.ToLower() == "evet";
                    users = users.Where(u => u.IsEmailConfirmed==dogrulanmisMi);
                    break;
                case 6:
                    users = users.Where(u => u.Role.Contains(search));
                    break;
                default:
                    break;
            }

            return PartialView("_UserListPartial", users.ToList());
        }





    }
}

