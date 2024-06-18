using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("brands")]
    public class Brands
    {
        [Key]
        [Column("brand_id")]
        public int Id { get; set; }

        [Column("brand_name")]
        [Display(Name = "Название бренда")]
        [MaxLength(30, ErrorMessage = "Превышен лимит")]
        [Required(ErrorMessage = "Значение не может быть NULL")]

        public string BrandName { get; set; }
        public List<Products> Products { get; set; } = new List<Products>();
    }
}