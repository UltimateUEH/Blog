using App.Models.Product;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;

namespace App.Areas.Blog.Controllers
{
    [Area("Product")]
    [Authorize(Roles = RoleName.Administrator)]
    [Route("admin/categoryproduct/category/[action]/{id?}")]
    public class CategoryProductController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Product/Category
        public async Task<IActionResult> Index()
        {
            var query = (from c in _context.CategoryProducts
                         select c)
                         .Include(c => c.ParentCategory)
                         .Include(c => c.CategoryChildren);

            var categoryProducts = (await query.ToListAsync())
                                .Where(c => c.ParentCategory == null)
                                .ToList();

            return View(categoryProducts);
        }

        // GET: Admin/Product/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoryProduct == null)
            {
                return NotFound();
            }

            return View(categoryProduct);
        }

        // GET: Admin/Product/Category/Create
        public async Task<IActionResult> CreateAsync()
        {
            var listCategory = await _context.CategoryProducts.ToListAsync();
            listCategory.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategoryProducts(), "Id", "Title", -1);
            return View();
        }

        public static List<CategoryProduct> FlattenCategoryProductHierarchy(List<CategoryProduct> categoryProducts)
        {
            var flattenedCategoryProducts = new List<CategoryProduct>();

            void Flatten(List<CategoryProduct> children, int level)
            {
                foreach (var child in children)
                {
                    var flattenedChild = new CategoryProduct
                    {
                        Id = child.Id,
                        Title = new string(' ', level * 2) + child.Title,
                        ParentCategoryId = child.ParentCategoryId
                    };

                    flattenedCategoryProducts.Add(flattenedChild);
                    if (child.CategoryChildren != null && child.CategoryChildren.Any())
                    {
                        Flatten(child.CategoryChildren.ToList(), level + 1);
                    }
                }
            }

            Flatten(categoryProducts, 0);
            return flattenedCategoryProducts;
        }

        private async Task<IEnumerable<CategoryProduct>> GetItemsSelectCategoryProducts()
        {
            var allCategoryProducts = await _context.CategoryProducts
                                              .Include(c => c.CategoryChildren)
                                              .ToListAsync();

            List<CategoryProduct> resultItems = new List<CategoryProduct>() {
                new CategoryProduct() {
                    Id = -1,
                    Title = "Không có danh mục cha"
                }
            };

            var rootCategoryProducts = allCategoryProducts.Where(c => c.ParentCategory == null).ToList();
            var flattenedCategoryProducts = FlattenCategoryProductHierarchy(rootCategoryProducts);

            resultItems.AddRange(flattenedCategoryProducts);
            return resultItems;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParentCategoryId,Title,Description,Slug")] CategoryProduct categoryProduct)
        {
            if (ModelState.IsValid)
            {
                if (categoryProduct.ParentCategoryId.Value == -1)
                {
                    categoryProduct.ParentCategoryId = null;
                }

                _context.Add(categoryProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentId);
            var listCategoryProduct = await _context.CategoryProducts.ToListAsync();
            listCategoryProduct.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategoryProducts(), "Id", "Title", categoryProduct.ParentCategoryId);
            return View(categoryProduct);
        }

        // GET: Admin/Product/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            ViewData["ParentCategoryId"] = new SelectList(await GetItemsSelectCategoryProducts(), "Id", "Title", categoryProduct.ParentCategoryId);

            return View(categoryProduct);
        }

        // POST: Admin/Product/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Description,Slug")] CategoryProduct categoryProduct)
        {
            if (id != categoryProduct.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (categoryProduct.ParentCategoryId == categoryProduct.Id)
            {
                ModelState.AddModelError("ParentCategoryId", "Danh mục cha không thể là chính nó");
                canUpdate = false;
            }

            if (canUpdate && (categoryProduct.ParentCategoryId != null))
            {
                var childCates = (from c in _context.CategoryProducts
                                  where c.ParentCategoryId == categoryProduct.Id
                                  select c)
                                  .Include(c => c.CategoryChildren)
                                  .ToList();

                // Func check id
                Func<List<CategoryProduct>, bool> checkCateIds = null;
                checkCateIds = (cates) =>
                {
                    foreach (var cate in cates)
                    {
                        if (cate.Id == categoryProduct.ParentCategoryId)
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
                    if (categoryProduct.ParentCategoryId == -1)
                    {
                        categoryProduct.ParentCategoryId = null;
                    }

                    var dtc = _context.CategoryProducts.AsNoTracking().FirstOrDefault(c => c.Id == id);
                    _context.Entry(dtc).State = EntityState.Detached;

                    _context.Update(categoryProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(categoryProduct.Id))
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

            var listCategoryProduct = await _context.CategoryProducts.ToListAsync();
            listCategoryProduct.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            ViewData["ParentCategoryId"] = new SelectList(listCategoryProduct, "Id", "Title", categoryProduct.ParentCategoryId);
            return View(categoryProduct);
        }

        // GET: Admin/Product/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoryProduct == null)
            {
                return NotFound();
            }

            return View(categoryProduct);
        }

        // POST: Admin/Product/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryProduct = await _context.CategoryProducts.Include(c => c.CategoryChildren).FirstOrDefaultAsync(c => c.Id == id);

            if (categoryProduct == null)
            {
                return NotFound();
            }

            foreach (var cCategoryProduct in categoryProduct.CategoryChildren)
            {
                cCategoryProduct.ParentCategoryId = categoryProduct.ParentCategoryId;
            }

            _context.CategoryProducts.Remove(categoryProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
