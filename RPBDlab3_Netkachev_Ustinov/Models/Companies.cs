using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("companies")]
    public class Companies
    {
        [Key]
        [Column("company_id")]
        [Required]
        public int Id { get; set; }

        [Column("company_name")]
        [Display(Name = "Название компании")]
        [MaxLength(30, ErrorMessage = "Превышен лимит")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string CompanyName { get; set; }

        public List<Sellers> Sellers { get; set; } = new List<Sellers>();
    }
}