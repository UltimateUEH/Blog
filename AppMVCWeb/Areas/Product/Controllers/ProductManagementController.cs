using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Areas.Identity.Models.UserViewModels;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using AppMVCWeb.Models.Product;
using AppMVCWeb.Areas.Product.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AppMVCWeb.Areas.Product.Controllers
{
    [Area("Product")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    [Route("admin/productmanagement/[action]/{id?}")]
    public class ProductManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductManagementController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Posts
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int itemPerPage)
        {
            var productsQuery = _context.Products.Include(p => p.Author)
                                            .OrderByDescending(p => p.DateUpdated);

            int totalProducts = await productsQuery.CountAsync();

            if (itemPerPage <= 0)
                itemPerPage = 10;
            int countPages = (int)Math.Ceiling((double)totalProducts / itemPerPage);

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

            ViewBag.PagingModel = pagingModel;
            ViewBag.totalProducts = totalProducts;

            ViewBag.postIndex = (currentPage - 1) * itemPerPage;

            var productsInPage = await productsQuery.Skip((currentPage - 1) * itemPerPage)
                                .Take(itemPerPage)
                                .Include(p => p.ProductCategoryProducts)
                                .ThenInclude(pc => pc.Category)
                                .ToListAsync();

            return View(productsInPage);
        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categoryProducts = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIds,Price")] CreateProductModel product)
        {
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categories, "Id", "Title");

            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError("Slug", "Url đã tồn tại. Vui lòng nhập Url khác");
                return View(product);
            }            

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);

                product.DateCreated = product.DateUpdated = DateTime.Now;
                product.AuthorId = user.Id;

                _context.Add(product);

                if (product.CategoryIds != null)
                {
                    foreach (var categoryId in product.CategoryIds)
                    {
                        _context.Add(new ProductCategoryProduct()
                        {
                            Product = product,
                            CategoryId = categoryId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Bạn vừa tạo sản phẩm: " + product.Title + " thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var post = await _context.Products.FindAsync(id);

            var product = await _context.Products
                .Include(p => p.ProductCategoryProducts)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var productEdit = new CreateProductModel()
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                Price = product.Price,
                CategoryIds = product.ProductCategoryProducts.Select(pc => pc.CategoryId).ToArray()
            };

            var categoryProducts = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");

            return View(productEdit);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoryIds,Price")] CreateProductModel product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var categoryProducts = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");

            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductId != id))
            {
                ModelState.AddModelError("Slug", "Url đã tồn tại. Vui lòng nhập Url khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate = await _context.Products
                        .Include(p => p.ProductCategoryProducts)
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (productUpdate == null)
                    {
                        return NotFound();
                    }

                    productUpdate.Title = product.Title;
                    productUpdate.Description = product.Description;
                    productUpdate.Content = product.Content;
                    productUpdate.Published = product.Published;
                    productUpdate.Slug = product.Slug;
                    productUpdate.Price = product.Price;
                    productUpdate.DateUpdated = DateTime.Now;
                    
                    // Update categories
                    if (product.CategoryIds == null)
                    {
                        product.CategoryIds = new int[] { };
                    }

                    var oldCategoryIds = productUpdate.ProductCategoryProducts.Select(pc => pc.CategoryId).ToArray();
                    var newCategoryIds = product.CategoryIds;

                    var removedCategoryIds = from productCate in productUpdate.ProductCategoryProducts
                                             where !newCategoryIds.Contains(productCate.CategoryId)
                                             select productCate;

                    _context.ProductCategoryProducts.RemoveRange(removedCategoryIds);

                    var addedCategoryIds = from cateId in newCategoryIds
                                           where !oldCategoryIds.Contains(cateId)
                                           select cateId;

                    foreach (var cateId in addedCategoryIds)
                    {
                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
                        {
                            ProductId = id,
                            CategoryId = cateId
                        });
                    }

                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                StatusMessage = "Bạn vừa cập nhật sản phẩm: " + product.Title + " thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName", product.AuthorId);
            return View(product);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            StatusMessage = "Bạn vừa xóa sản phẩm: " + product.Title + " thành công!";

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        public class UploadOneFile
        {
            [Required(ErrorMessage = "Chọn file")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Chỉ chấp nhận file ảnh jpg, jpeg, png, gif")]
            [Display(Name = "Chọn file ảnh")]
            public IFormFile FileUpload { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> UploadPhoto(int id)
        {
            var product = await _context.Products.Where(e => e.ProductId == id)
                                                .Include(p => p.Photos)
                                                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            ViewData["Product"] = product;

            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id, [Bind("FileUpload")]UploadOneFile file)
        {
            var product = await _context.Products.Where(e => e.ProductId == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            ViewData["Product"] = product;

            if (file?.FileUpload != null)
            {
                var photo = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileUpload.FileName);
                var path = Path.Combine("Uploads", "Products", photo);

                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.FileUpload.CopyToAsync(stream);
                }

                _context.Add(new ProductPhoto()
                {
                    ProductId = product.ProductId,
                    FileName = photo
                });
                
                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("FileUpload", "Vui lòng chọn một file để tải lên.");
                return View();
            }

            return View(file);
        }

        public async Task<IActionResult> ListPhotos(int id)
        {
            var product = await _context.Products.Where(e => e.ProductId == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefaultAsync();

            if (product == null)
            {
                return Json(
                    new
                    {
                        success = 0,
                        message = "Khong co san pham"
                    }
                );
            }

            var listPhotos = product.Photos.Select(photo => new
            {
                id = photo.Id,
                path = "/contents/Products/" + photo.FileName
            });

            return Json(
                new
                {
                    success = 1,
                    photos = listPhotos
                }
            );
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            try
            {
                var photo = await _context.ProductPhotos.FirstOrDefaultAsync(p => p.Id == id);

                if (photo == null)
                {
                    return Json(new { success = 0, message = "Ảnh không tồn tại" });
                }

                _context.ProductPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                var fileName = Path.Combine("Uploads", "Products", photo.FileName);
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                else
                {
                    return Json(new { success = 0, message = "Không thể tìm thấy file để xóa" });
                }

                return Json(new { success = 1, message = "Đã xóa ảnh thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = 0, message = "Lỗi khi xóa ảnh: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadPhotoApi(int id, [Bind("FileUpload")] UploadOneFile file)
        {
            var product = await _context.Products.Where(e => e.ProductId == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            if (file?.FileUpload != null)
            {
                var photo = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileUpload.FileName);
                var path = Path.Combine("Uploads", "Products", photo);

                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.FileUpload.CopyToAsync(stream);
                }

                _context.Add(new ProductPhoto()
                {
                    ProductId = product.ProductId,
                    FileName = photo
                });

                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("FileUpload", "Vui lòng chọn một file để tải lên.");
                return View();
            }

            return Ok();
        }
    }
}
