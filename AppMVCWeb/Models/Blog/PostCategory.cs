using App.Models.Blog;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVCWeb.Models.Blog
{
    [Table("PostCategory")]
    public class PostCategory
    {
        public int PostId { set; get; }

        public int CategoryId { set; get; }

        [ForeignKey("PostId")]
        public Post Post { set; get; }

        [ForeignKey("CategoryId")]
        public Category Category { set; get; }
    }
}
