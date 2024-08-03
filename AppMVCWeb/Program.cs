using App.Data;
using App.Models;
using App.Services;
using AppMVCWeb.Areas.Product.Services;
using AppMVCWeb.ExtendMethods;
using AppMVCWeb.Hubs;
using AppMVCWeb.Menu;
using AppMVCWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;

namespace AppMVCWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add session
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.Cookie.Name = "LoginStated";
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                // Add mail setting
                builder.Services.AddOptions();
                var mailSetting = builder.Configuration.GetSection("MailSettings");
                builder.Services.Configure<MailSettings>(mailSetting);
                builder.Services.AddTransient<IEmailSender, SendMailService>();

                // Add connection string
                builder.Services.AddDbContext<AppDbContext>(options 
                    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                    {
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                ));

                // Truy cập IdentityOptions
                builder.Services.Configure<IdentityOptions>(options => {
                    // Thiết lập về Password
                    options.Password.RequireDigit = false; // Không bắt phải có số
                    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
                    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                    // Cấu hình Lockout - khóa user
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
                    options.Lockout.AllowedForNewUsers = true;

                    // Cấu hình về User.
                    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    options.User.RequireUniqueEmail = true;  // Email là duy nhất

                    // Cấu hình đăng nhập.
                    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                    options.SignIn.RequireConfirmedAccount = true;

                });

                builder.Services.ConfigureApplicationCookie(options => {
                    options.LoginPath = "/login/";
                    options.LogoutPath = "/logout/";
                    options.AccessDeniedPath = "/khongduoctruycap.html";
                });

                builder.Services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        var googleConfig = builder.Configuration.GetSection("Authentication:Google");
                        options.ClientId = googleConfig["ClientId"];
                        options.ClientSecret = googleConfig["ClientSecret"];
                        options.CallbackPath = "/signin-google";
                    })
                    .AddFacebook(options =>
                    {
                        var facebookConfig = builder.Configuration.GetSection("Authentication:Facebook");
                        options.AppId = facebookConfig["AppId"];
                        options.AppSecret = facebookConfig["AppSecret"];
                        options.CallbackPath = "/signin-facebook";
                    })
                    //.AddMicrosoftAccount()
                    //.AddTwitter()
                    ;

                builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Services.AddRazorPages();
                builder.Services.AddSerilog();

                builder.Services.AddSingleton<PlanetService>();

                // Đăng ký Identity
                builder.Services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("ViewManageMenu", builder =>
                    {
                        builder.RequireAuthenticatedUser();
                        builder.RequireRole(RoleName.Administrator);
                    });
                });

                builder.Services.AddTransient<CartService>();
                builder.Services.AddSingleton(x => new PaypalClient(
                    builder.Configuration["Paypal:ClientId"],
                    builder.Configuration["Paypal:ClientSecret"],
                    builder.Configuration["Paypal:Mode"]
                ));

                builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
                builder.Services.AddTransient<AdminSidebarService>();

                builder.Services.AddSignalR();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles(); // wwwroot 

                // /contents/1.jpg => Uploads/1.jpg
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
                    RequestPath = "/contents"
                });

                app.UseRouting();

                app.UseSession();

                app.UseAuthentication(); // Xác thực danh tính
                app.UseAuthorization(); // Xác thực quyền truy cập
                app.AddStaticCodePage(); // Tuỳ biến Response lỗi: 400 - 599

                app.MapControllers();

                app.MapHub<ChatHub>("/chathub");
         
                app.MapControllerRoute( 
                    name: "default",
                    pattern: "/{controller=Home}/{action=Index}/{id?}" 
                );

                app.MapRazorPages();

                app.Run();
            }
            catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
