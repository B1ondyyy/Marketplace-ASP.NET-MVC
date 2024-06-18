using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [Table("order_statuses")]
    public class OrderStatuses
    {
        [Key]
        [Column("status_id")]
        public int Id { get; set; }

        [Column("status_string")]
        [MaxLength(30, ErrorMessage = "Превышен лимит")]
        [Display(Name = "Статус заказа")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public string StatusString { get; set; }

        public List<Orders> Orders { get; set; } = new List<Orders>();
    }
}