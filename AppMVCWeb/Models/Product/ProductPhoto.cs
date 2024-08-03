using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVCWeb.Models.Product
{
    [Table("ProductPhoto")]
    public class ProductPhoto
    {
        [Key]
        public int Id { get; set; }

        // /contents/Products/...
        public string FileName { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}