using App.Models;
using AppMVCWeb.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Areas.Identity.Models.UserViewModels;
using AppMVCWeb.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;

namespace AppMVCWeb.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    [Route("admin/blog/post/[action]/{id?}")]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Posts
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int itemPerPage)
        {
            var postsQuery = _context.Posts.Include(p => p.Author)
                                            .OrderByDescending(p => p.DateUpdated);

            int totalPosts = await postsQuery.CountAsync();

            if (itemPerPage <= 0)
                itemPerPage = 10;
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

            ViewBag.PagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.postIndex = (currentPage - 1) * itemPerPage;

            var postsInPage = await postsQuery.Skip((currentPage - 1) * itemPerPage)
                                .Take(itemPerPage)
                                .Include(p => p.PostCategories)
                                .ThenInclude(pc => pc.Category)
                                .ToListAsync();

            return View(postsInPage);
        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIds")] CreateProductModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Url đã tồn tại. Vui lòng nhập Url khác");
                return View(post);
            }            

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);

                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;

                _context.Add(post);

                if (post.CategoryIds != null)
                {
                    foreach (var categoryId in post.CategoryIds)
                    {
                        _context.Add(new PostCategory()
                        {
                            Post = post,
                            CategoryId = categoryId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Bạn vừa tạo bài viết: " + post.Title + " thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var post = await _context.Posts.FindAsync(id);

            var post = await _context.Posts
                .Include(p => p.PostCategories)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreateProductModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIds = post.PostCategories.Select(pc => pc.CategoryId).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View(postEdit);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIds")] CreateProductModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "Url đã tồn tại. Vui lòng nhập Url khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts
                        .Include(p => p.PostCategories)
                        .FirstOrDefaultAsync(p => p.PostId == id);

                    if (postUpdate == null)
                    {
                        return NotFound();
                    }

                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;
                    
                    // Update categories
                    if (post.CategoryIds == null)
                    {
                        post.CategoryIds = new int[] { };
                    }

                    var oldCategoryIds = postUpdate.PostCategories.Select(pc => pc.CategoryId).ToArray();
                    var newCategoryIds = post.CategoryIds;

                    var removedCategoryIds = from postCate in postUpdate.PostCategories
                                             where !newCategoryIds.Contains(postCate.CategoryId)
                                             select postCate;

                    _context.PostCategories.RemoveRange(removedCategoryIds);

                    var addedCategoryIds = from cateId in newCategoryIds
                                           where !oldCategoryIds.Contains(cateId)
                                           select cateId;

                    foreach (var cateId in addedCategoryIds)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            PostId = id,
                            CategoryId = cateId
                        });
                    }

                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                StatusMessage = "Bạn vừa cập nhật bài viết: " + post.Title + " thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName", post.AuthorId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            StatusMessage = "Bạn vừa xóa bài viết: " + post.Title + " thành công!";

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
