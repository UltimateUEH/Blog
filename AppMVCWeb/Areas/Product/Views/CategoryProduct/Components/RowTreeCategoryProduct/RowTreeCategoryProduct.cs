using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AppMVCWeb.Areas.Product.Views.Category.Components.RowTreeCategoryProduct
{

    [ViewComponent]
    public class RowTreeCategoryProduct : ViewComponent
    {
        public RowTreeCategoryProduct()
        {

        }
        // data là sữ liệu có cấu trúc
        // { 
        //    categories - danh sách các Category
        //    level - cấp của các Category 
        // }
        public IViewComponentResult Invoke(dynamic data)
        {
            return View(data);
        }
    }
}
