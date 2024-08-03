using App.Models.Product;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVCWeb.Models.Product
{
    [Table("ProductCategoryProduct")]
    public class ProductCategoryProduct
    {
        public int ProductId { set; get; }

        public int CategoryId { set; get; }

        [ForeignKey("ProductId")]
        public ProductModel Product { set; get; }

        [ForeignKey("CategoryId")]
        public CategoryProduct Category { set; get; }
    }
}
