using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("cities")]
    public class Cities
    {
        [Key]
        [Column("city_id")]
        [Required]
        public int Id { get; set; }

        [Column("city_name")]
        [Display(Name = "Город")]
        [MaxLength(30)]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string CityName { get; set; }

        public List<Customers> Customers { get; set; } = new List<Customers>();
        public List<PickupPoints> PickupPoints { get; set; } = new List<PickupPoints>();
    }
}