using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVCWeb.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/post/{categorySlug?}")]
        public IActionResult Index(string categorySlug, [FromQuery(Name = "p")] int currentPage, int itemPerPage)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categorySlug = categorySlug;

            Category category = null;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                category = _context.Categories
                                    .Include(c => c.CategoryChildren)
                                    .FirstOrDefault(c => c.Slug == categorySlug);

                if (category == null)
                {
                    return NotFound("Không tìm thấy category");
                }

                EnsureAllChildrenLoaded(category.CategoryChildren.ToList());
            }

            var posts = _context.Posts.Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .OrderByDescending(p => p.DateUpdated)
                                    .AsQueryable();

            if (category != null)
            {
                var categoryIds = new List<int>();
                category.ChildCategoryIds(null, categoryIds);
                categoryIds.Add(category.Id);

                posts = posts.Where(p => p.PostCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }

            if (itemPerPage <= 0)
                itemPerPage = 10;

            int totalPosts = posts.Count();
            int countPages = (int)Math.Ceiling((double)totalPosts / itemPerPage);

            if (currentPage < 1)
                currentPage = 1;
            if (currentPage > countPages)
                currentPage = countPages;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new { p = pageNumber, itemPerPage = itemPerPage })
            };

            var postsInPage = posts.Skip(Math.Max((currentPage - 1) * itemPerPage, 0))
                                .Take(itemPerPage)
                                .ToList();

            ViewBag.PagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;
            ViewBag.category = category;

            return View(postsInPage.ToList());
        }

        [Route("/post/{postSlug}.html")]
        public IActionResult Details(string postSlug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var post =  _context.Posts.Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault(p => p.Slug == postSlug);

            if (post == null)
            {
                return NotFound("Không tìm thấy bài viết");
            }

            Category category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id))
                                    .Where(p => p.PostId != post.PostId)
                                    .OrderByDescending(p => p.DateUpdated)
                                    .Take(5);

            ViewBag.otherPosts = otherPosts;

            return View(post);
        }

        private List<Category> GetCategories()
        {
            var categories = _context.Categories
                                    .Include(c => c.CategoryChildren)
                                    .ToList();

            // Filter to only include top-level categories in the final list
            var topLevelCategories = categories.Where(c => c.ParentCategory == null).ToList();

            return topLevelCategories;
        }

        private void EnsureAllChildrenLoaded(List<Category> categories)
        {
            foreach (var category in categories)
            {
                _context.Entry(category).Collection(c => c.CategoryChildren).Load();
                EnsureAllChildrenLoaded(category.CategoryChildren.ToList());
            }
        }
    }
}
