using Microsoft.EntityFrameworkCore;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<PickupPoints> PickupPoints { get; set; }
        public DbSet<Companies> Companies { get; set; }
        public DbSet<Sellers> Sellers { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Brands> Brands { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<OrderStatuses> OrderStatuses { get; set; }
        public DatabaseContext()
        {
            Database.EnsureCreated(); // Создаст если нет БД
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=pashok08"); // ИЗМЕНИТЬ ПОДКЛЮЧЕНИЕ НА СВОЮ БАЗУ ДАННЫХ
        }

    }
}