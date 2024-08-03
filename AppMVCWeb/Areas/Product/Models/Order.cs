using System.ComponentModel.DataAnnotations;

namespace AppMVCWeb.Areas.Product.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(15)]
        public string CustomerPhone { get; set; }

        [Required]
        [StringLength(200)]
        public string CustomerAddress { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
