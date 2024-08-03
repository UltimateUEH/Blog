using AppMVCWeb.Models.Blog;
using System.ComponentModel.DataAnnotations;

namespace AppMVCWeb.Areas.Blog.Models
{
    public class CreateProductModel : Post
    {
        [Display(Name = "Chuyên mục")]
        public int[] CategoryIds { get; set; }
    }
}
