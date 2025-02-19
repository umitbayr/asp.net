using Microsoft.EntityFrameworkCore;

namespace Login.Entities
{
    // DbContext sınıfından türeyen bir veritabanı bağlamı (context) oluşturuyoruz.
    public class AppDbContext : DbContext
    {
        // Bağımlılık enjeksiyonu (Dependency Injection) ile DbContextOptions parametresini alarak 
        // üst sınıf (DbContext) yapıcısına (base) gönderiyoruz.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanındaki UserAccounts tablosunu temsil eden DbSet<UserAccount> özelliği.
        // Bu özellik, UserAccount modeline karşılık gelen bir veritabanı tablosu oluşturulmasını sağlar.
        public DbSet<UserAccount> UserAccounts { get; set; }

        // Fluent API kullanarak model yapılandırmaları yapmak için OnModelCreating metodunu override ediyoruz.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Üst sınıftaki (DbContext) OnModelCreating metodunu çağırarak temel yapılandırmaların yapılmasını sağlıyoruz.
            base.OnModelCreating(modelBuilder);

           
        }
    }
}
