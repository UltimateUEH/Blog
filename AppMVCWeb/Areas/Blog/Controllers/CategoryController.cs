using App.Models.Blog;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator)]
    [Route("admin/blog/category/[action]/{id?}")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Blog/Category
        public async Task<IActionResult> Index()
        {
            var query = (from c in _context.Categories
                         select c)
                         .Include(c => c.ParentCategory)
                         .Include(c => c.CategoryChildren);

            var categories = (await query.ToListAsync())
                                .Where(c => c.ParentCategory == null)
                                .ToList();

            return View(categories);
        }

        // GET: Admin/Blog/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Blog/Category/Create
        public async Task<IActionResult> CreateAsync()
        {
            // ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug");
            var listcategory = await _context.Categories.ToListAsync();
            listcategory.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategories(), "Id", "Title", -1);
            return View();
        }

        //private async Task<IEnumerable<Category>> GetItemsSelectCategories()
        //{

        //    var items = await _context.Categories
        //                        .Include(c => c.CategoryChildren)
        //                        .Where(c => c.ParentCategory == null)
        //                        .ToListAsync();

        //    List<Category> resultitems = new List<Category>() {
        //        new Category() {
        //            Id = -1,
        //            Title = "Không có danh mục cha"
        //        }
        //    };

        //    Action<List<Category>, int> _ChangeTitleCategory = null;
        //    Action<List<Category>, int> ChangeTitleCategory = (items, level) =>
        //    {
        //        string prefix = String.Concat(Enumerable.Repeat("", level));
        //        foreach (var item in items)
        //        {
        //            item.Title = prefix + item.Title;
        //            resultitems.Add(item);
        //            if ((item.CategoryChildren != null) && (item.CategoryChildren.Count > 0))
        //            {
        //                _ChangeTitleCategory(item.CategoryChildren.ToList(), level + 1);
        //            }
        //        }
        //    };

        //    _ChangeTitleCategory = ChangeTitleCategory;
        //    ChangeTitleCategory(items, 0);

        //    return resultitems;
        //}

        public static List<Category> FlattenCategoryHierarchy(List<Category> categories)
        {
            var flattenedCategories = new List<Category>();

            void Flatten(List<Category> children, int level)
            {
                foreach (var child in children)
                {
                    var flattenedChild = new Category
                    {
                        Id = child.Id,
                        Title = new string(' ', level * 2) + child.Title,
                        ParentCategoryId = child.ParentCategoryId
                    };
                    flattenedCategories.Add(flattenedChild);
                    if (child.CategoryChildren != null && child.CategoryChildren.Any())
                    {
                        Flatten(child.CategoryChildren.ToList(), level + 1);
                    }
                }
            }

            Flatten(categories, 0);
            return flattenedCategories;
        }

        private async Task<IEnumerable<Category>> GetItemsSelectCategories()
        {
            var allCategories = await _context.Categories
                                              .Include(c => c.CategoryChildren)
                                              .ToListAsync();

            List<Category> resultItems = new List<Category>() {
                new Category() {
                    Id = -1,
                    Title = "Không có danh mục cha"
                }
            };

            var rootCategories = allCategories.Where(c => c.ParentCategory == null).ToList();
            var flattenedCategories = FlattenCategoryHierarchy(rootCategories);

            resultItems.AddRange(flattenedCategories);
            return resultItems;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParentCategoryId,Title,Description,Slug")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId.Value == -1)
                {
                    category.ParentCategoryId = null;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentId);
            var listcategory = await _context.Categories.ToListAsync();
            listcategory.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategories(), "Id", "Title", category.ParentCategoryId);
            return View(category);
        }

        // GET: Admin/Blog/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategories(), "Id", "Title", category.ParentCategoryId);

            return View(category);
        }

        // POST: Admin/Blog/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Description,Slug")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError("ParentCategoryId", "Danh mục cha không thể là chính nó");
                canUpdate = false;
            }

            if (canUpdate && (category.ParentCategoryId != null))
            {
                var childCates = (from c in _context.Categories
                                  where c.ParentCategoryId == category.Id
                                  select c)
                                  .Include(c => c.CategoryChildren)
                                  .ToList();

                // Func check id
                Func<List<Category>, bool> checkCateIds = null;
                checkCateIds = (cates) =>
                {
                    foreach (var cate in cates)
                    {
                        if (cate.Id == category.ParentCategoryId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError("ParentCategoryId", "Danh mục cha không thể là danh mục con của nó");
                            return true;
                        }

                        if (cate.CategoryChildren != null)
                        {
                            return checkCateIds(cate.CategoryChildren.ToList());
                        }
                    }
                    return false;
                };
            }

            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    if (category.ParentCategoryId == -1)
                    {
                        category.ParentCategoryId = null;
                    }

                    var dtc = _context.Categories.AsNoTracking().FirstOrDefault(c => c.Id == id);
                    _context.Entry(dtc).State = EntityState.Detached;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var listcategory = await _context.Categories.ToListAsync();
            listcategory.Insert(0, new Category()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            ViewData["ParentCategoryId"] = new SelectList(listcategory, "Id", "Title", category.ParentCategoryId);
            return View(category);
        }

        // GET: Admin/Blog/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Blog/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.Include(c => c.CategoryChildren).FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            foreach (var cCategory in category.CategoryChildren)
            {
                cCategory.ParentCategoryId = category.ParentCategoryId;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
