using AppMVCWeb.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace AppMVCWeb.Areas.Product.Models
{
    public class CreateProductModel : ProductModel
    {
        [Display(Name = "Chuyên mục")]
        public int[] CategoryIds { get; set; }
    }
}
