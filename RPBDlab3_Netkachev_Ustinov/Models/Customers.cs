using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("customers")]
    public class Customers
    {
        [Key]
        [Column("customer_id")]
        [Required]
        public int Id { get; set; }

        [Column("customer_name")]
        [Display(Name = "ФИО покупателя")]
        [MaxLength(100, ErrorMessage = "Превышен лимит")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string CustomerName { get; set; }

        [Column("birth_date")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата рождения")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public DateTime BirthDate { get; set; }

        [ForeignKey("City")]
        [Display(Name = "Город")]
        [Column("city_id")]
        public int CityId { get; set; }
        [Display(Name = "Город")]
        public Cities? City { get; set; }

        public List<Orders> Orders { get; set; } = new List<Orders>();
    }
}