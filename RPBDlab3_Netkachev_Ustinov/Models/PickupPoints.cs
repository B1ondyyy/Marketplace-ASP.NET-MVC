using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("pickup_points")]
    public class PickupPoints
    {
        [Key]
        [Column("pickup_point_id")]
        public int Id { get; set; }

        [Column("pp_address")]
        [Display(Name = "Адрес пункта выдачи")]
        [MaxLength(30, ErrorMessage = "Превышен лимит")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string PickupPointAddress { get; set; }

        [ForeignKey("City")]
        [Column("city_id")]
        [Display(Name = "Город")]
        [Required]
        public int CityId { get; set; }

        [Display(Name = "Город")]
        public Cities? City { get; set; }

        public List<Orders> Orders { get; set; } = new List<Orders>();
    }
}