using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace App.Models.Product
{
    [Table("CategoryProduct")]
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }

        // Tiều đề Category
        [Required(ErrorMessage = "Phải có tên danh mục")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string Title { get; set; }

        // Nội dung, thông tin chi tiết về Category
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string Description { set; get; }

        //chuỗi Url
        [Required(ErrorMessage = "Phải tạo url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Url hiện thị")]
        public string Slug { set; get; }

        // Các Category con
        public ICollection<CategoryProduct> CategoryChildren { get; set; }

        // Category cha (FKey)
        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Danh mục cha")]
        public CategoryProduct ParentCategory { set; get; }

        public void ChildCategoryIds(ICollection<CategoryProduct> childCates, List<int> lists)
        {
            if (childCates == null)
            {
                childCates = this.CategoryChildren;
            }

            foreach (CategoryProduct category in childCates)
            {
                lists.Add(category.Id);
                ChildCategoryIds(category.CategoryChildren, lists);
            }
        }

        public List<CategoryProduct> ListParents()
        {
            List<CategoryProduct> list = new List<CategoryProduct>();

            var parent = this.ParentCategory;
            while (parent != null)
            {
                list.Add(parent);
                parent = parent.ParentCategory;
            }


            list.Reverse();
            return list;
        }
    }
}
