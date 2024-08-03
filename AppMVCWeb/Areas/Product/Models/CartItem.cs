using AppMVCWeb.Models.Product;

namespace AppMVCWeb.Areas.Product.Models
{
    public class CartItem
    {
        public ProductModel Product { get; set; }

        public int Quantity { get; set; }
    }
}
