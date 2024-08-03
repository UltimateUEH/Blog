using App.Data;
using App.Models;
using App.Models.Blog;
using App.Models.Product;
using AppMVCWeb.Models.Blog;
using AppMVCWeb.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVCWeb.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-management/[action]")]
    public class DbManagementController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public DbManagementController(AppDbContext dbContext, UserManager<AppUser> userManager , RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult DeleteDb()
        {
            return View();
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpPost]
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> ConfirmDeleteDb()
        {
            var success = await _dbContext.Database.EnsureDeletedAsync();

            StatusMessage = success ? "Xóa database thành công" : "Có lỗi khi xóa database";

            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> MigrateDb()
        {
            await _dbContext.Database.MigrateAsync();

            StatusMessage = "Cập nhật database thành công";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedDataAsync()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();

            foreach (var r in roleNames)
            {
                var roleName = r.GetRawConstantValue().ToString();
                var roleFound = await _roleManager.FindByNameAsync(roleName);

                if (roleFound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var userAdmin = await _userManager.FindByEmailAsync("admin@gmail.com");

            if (userAdmin == null)
            {
                userAdmin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(userAdmin, "admin123");
                await _userManager.AddToRoleAsync(userAdmin, RoleName.Administrator);
                await _signInManager.SignInAsync(userAdmin, false);

                return RedirectToAction(nameof(SeedDataAsync));
            }
            else
            {
                var user = await _userManager.GetUserAsync(this.User);

                if (user == null)
                {
                    return this.Forbid();
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Any(r => r == RoleName.Administrator))
                {
                    return this.Forbid();
                }
            }

            SeedPostCategory();
            SeedProductCategory();

            StatusMessage = "Seed dữ liệu thành công";

            return RedirectToAction(nameof(Index));
        }

        private void SeedPostCategory()
        {
            Randomizer.Seed = new Random(8675309);

            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Description.Contains("[fakeData]")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("[fakeData]")));

            _dbContext.SaveChanges();

            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"Chuyên mục {cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
                var cate11 = fakerCategory.Generate();
                var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
                var cate21 = fakerCategory.Generate();
                    var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new List<Category>
            {
                cate1, cate11, cate12, cate2, cate21, cate211
            };

            _dbContext.Categories.AddRange(categories);

            
            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2024, 05, 07), new DateTime(2024, 07, 11)));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, fk => $"Bài {bv++} " + fk.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostCategory
                {
                    Post = post,
                    Category = categories[rCateIndex.Next(5)]
                });
            }

            _dbContext.AddRange(posts);
            _dbContext.AddRange(postCategories);

            _dbContext.SaveChanges();
        }
        
        private void SeedProductCategory()
        {
            Randomizer.Seed = new Random(8675309);

            _dbContext.CategoryProducts.RemoveRange(_dbContext.CategoryProducts.Where(c => c.Description.Contains("[fakeData]")));
            _dbContext.Products.RemoveRange(_dbContext.Products.Where(p => p.Content.Contains("[fakeData]")));

            _dbContext.SaveChanges();

            var fakerCategory = new Faker<CategoryProduct>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"Nhóm sản phẩm {cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
                var cate11 = fakerCategory.Generate();
                var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
                var cate21 = fakerCategory.Generate();
                    var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new List<CategoryProduct>
            {
                cate1, cate11, cate12, cate2, cate21, cate211
            };

            _dbContext.CategoryProducts.AddRange(categories);

            
            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerProduct = new Faker<ProductModel>();
            fakerProduct.RuleFor(p => p.AuthorId, user.Id);
            fakerProduct.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2024, 05, 07), new DateTime(2024, 07, 11)));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, f => $"Sản phẩm {bv++} " + f.Commerce.ProductName());
            fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500, 1000, 0)));

            List<ProductModel> products = new List<ProductModel>();
            List<ProductCategoryProduct> productCategories = new List<ProductCategoryProduct>();

            for (int i = 0; i < 40; i++)
            {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);
                productCategories.Add(new ProductCategoryProduct
                {
                    Product = product,
                    Category = categories[rCateIndex.Next(5)]
                });
            }

            _dbContext.AddRange(products);
            _dbContext.AddRange(productCategories);

            _dbContext.SaveChanges();
        }
    }
}
