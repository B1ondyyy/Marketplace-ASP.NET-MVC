using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("sellers")]
    public class Sellers
    {
        [Key]
        [Column("seller_id")]
        public int Id { get; set; }

        [Column("seller_name")]
        [Display(Name = "ФИО продавца")]
        [MaxLength(100, ErrorMessage = "Превышен лимит")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string SellerName { get; set; }

        [ForeignKey("Company")]
        [Display(Name = "Компания")]
        [Column("company_id")]
        public int CompanyId { get; set; }
        [Display(Name = "Компания")]

        public Companies? Company { get; set; }
        public List<Products> Products { get; set; } = new List<Products>();
    }
}