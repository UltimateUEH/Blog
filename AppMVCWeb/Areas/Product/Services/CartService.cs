using AppMVCWeb.Areas.Product.Models;
using Newtonsoft.Json;

namespace AppMVCWeb.Areas.Product.Services
{
    public class CartService
    {
        public const string CARTKEY = "cart";

        private readonly IHttpContextAccessor _context;

        public CartService(IHttpContextAccessor context)
        {
            _context = context;
        }

        // Lấy cart từ Session (danh sách CartItem)
        public List<CartItem> GetCartItems()
        {
            var session = _context.HttpContext.Session;
            string jsonCart = session.GetString(CARTKEY);
            if (jsonCart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsonCart);
            }
            return new List<CartItem>();
        }

        // Xóa cart khỏi session
        public void ClearCart()
        {
            var session = _context.HttpContext.Session;
            session.Remove(CARTKEY);
        }

        // Lưu Cart (Danh sách CartItem) vào session
        public void SaveCartSession(List<CartItem> ls)
        {
            var session = _context.HttpContext.Session;
            string jsonCart = JsonConvert.SerializeObject(ls);
            session.SetString(CARTKEY, jsonCart);
            // Thêm log để kiểm tra
            Console.WriteLine($"Saved cart session: {jsonCart}");
        }
    }
}
