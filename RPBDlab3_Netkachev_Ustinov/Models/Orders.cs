using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDlab3_Netkachev_Ustinov.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DeliveryDateValidationAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public DeliveryDateValidationAttribute(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName ?? throw new ArgumentNullException(nameof(startDatePropertyName));
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateValue = GetStartDateValue(validationContext);
            var deliveryDateValue = (DateTime)value;

            if (deliveryDateValue < startDateValue)
            {
                return new ValidationResult("Дата доставки не должна быть раньше даты заказа.");
            }

            return ValidationResult.Success;
        }

        private DateTime GetStartDateValue(ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);

            if (startDateProperty == null)
            {
                throw new InvalidOperationException($"Ошибка: {_startDatePropertyName} не найден.");
            }

            var startDateValue = startDateProperty.GetValue(validationContext.ObjectInstance);

            if (startDateValue == null || !(startDateValue is DateTime))
            {
                throw new InvalidOperationException($"Ошибка: {_startDatePropertyName} не является датой.");
            }

            return (DateTime)startDateValue;
        }
    }

    [Table("orders")]
    public class Orders
    {
        [Key]
        [Column("order_id")]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        [Column("customer_id")]
        [Display(Name = "Имя покупателя")]
        [Required]
        public int CustomerId { get; set; }


        [ForeignKey("Product")]
        [Column("product_id")]
        [Display(Name = "Название товара")]
        [Required]
        public int ProductId { get; set; }

        [ForeignKey("PickupPoint")]
        [Column("pickup_point_id")]
        [Display(Name = "Адрес пункта выдачи")]
        [Required]
        public int PickupPointId { get; set; }

        [ForeignKey("OrderStatus")]
        [Column("order_status_id")]
        [Display(Name = "Статус заказа")]
        [Required]
        public int OrderStatusId { get; set; }

        [Column("amount_of_products")]
        [Display(Name = "Количество товара")]
        [Range(1, 100, ErrorMessage = "Больше 100 нельзя приобрести за один заказ")]
        [Required]
        public int AmountOfProducts { get; set; }

        [Column("order_date")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата заказа")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public DateTime OrderDate { get; set; }

        [Column("delivery")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата доставки")]
        [DeliveryDateValidation("OrderDate")]
        [Required(ErrorMessage = "Значение не может быть NULL")]
        public DateTime DeliveryDate { get; set; }

        [Display(Name = "Имя покупателя")]
        public Customers? Customer { get; set; }
        [Display(Name = "Название товара")]
        public Products? Product { get; set; }
        [Display(Name = "Адрес пункта выдачи")]
        public PickupPoints? PickupPoint { get; set; }
        [Display(Name = "Статус заказа")]
        public OrderStatuses? OrderStatus { get; set; }
    }
}