using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("products")]
    public class Products
    {
        [Key]
        [Column("product_id")]
        public int Id { get; set; }

        [Column("product_name")]
        [Display(Name = "Название товара")]
        [MaxLength(50, ErrorMessage = "Превышен лимит")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string ProductName { get; set; }

        [Column("weight")]
        [Display(Name = "Вес товара (Кг)")]
        [Range(1, 2000, ErrorMessage = "Товар весом более 2 тонн оформить нельзя")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public float Weight { get; set; }

        [Column("amount_in_stock")]
        [Display(Name = "Количество в наличии")]
        [Range(0, 15000, ErrorMessage = "Больше 15000 на склад один товар положить нельзя")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public int Amount { get; set; }

        [ForeignKey("Brand")]
        [Display(Name = "Бренд")]
        [Column("brand_id")]
        public int BrandId { get; set; }

        [ForeignKey("Seller")]
        [Display(Name = "Продавец")]
        [Column("seller_id")]
        public int SellerId { get; set; }

        [Display(Name = "Бренд")]
        public Brands? Brand { get; set; }
        [Display(Name = "Продавец")]

        public Sellers? Seller { get; set; }

        public List<Orders> Orders { get; set; } = new List<Orders>();


    }
}