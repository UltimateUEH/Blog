using App.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace AppMVCWeb.Views.Shared.Components.CategoryProductSidebar
{
    [ViewComponent]
    public class CategoryProductSidebar : ViewComponent
    {
        public class CategoryProductSidebarData
        {
            public List<CategoryProduct> CategoryProducts { get; set; }

            public int Level { get; set; }

            public string CategorySlug { get; set; }
        }

        public IViewComponentResult Invoke(CategoryProductSidebarData data)
        {
            return View(data);
        }
    }
}
