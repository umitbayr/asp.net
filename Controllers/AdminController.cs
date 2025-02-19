using System.Net.Mail;
using System.Net;
using Login.Entities;
using Login.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Guncelle(GuncelleViewModel model)
        {
            if(!ModelState.IsValid)
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

            // Listele sayfasına yönlendir
            return RedirectToAction("Listele","Admin");
        }
    }
}

