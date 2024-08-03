using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text;

namespace AppMVCWeb.Menu
{
    public class AdminSidebarService
    {
        private readonly IUrlHelper _urlHelper;
        public List<SidebarItem> Items { get; set; } = new List<SidebarItem>();

        public AdminSidebarService(IUrlHelperFactory factory, IActionContextAccessor action)
        {
            _urlHelper = factory.GetUrlHelper(action.ActionContext);

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider} );
            Items.Add(new SidebarItem() { Type = SidebarItemType.Heading, Title = "Chung"} );

            Items.Add(new SidebarItem() { 
                Type = SidebarItemType.NavItem,
                Controller = "DbManagement",
                Action = "Index",
                Area = "Database",
                Title = "Cơ sở dữ liệu",
                AwesomeIcon = "fas fa-database"
            });
            
            Items.Add(new SidebarItem() { 
                Type = SidebarItemType.NavItem,
                Controller = "Contact",
                Action = "Index",
                Area = "Contact",
                Title = "Liên hệ",
                AwesomeIcon = "fas fa-address-card"
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });


            Items.Add(new SidebarItem() { 
                Type = SidebarItemType.NavItem,
                Title = "Phân quyền & Thành viên",
                AwesomeIcon = "fas fa-folder",
                CollapseId = "role",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem() { 
                        Type = SidebarItemType.NavItem,
                        Controller = "Role",
                        Action = "Index",
                        Area = "Identity",
                        Title = "Danh sách vai trò"
                    },
                    new SidebarItem() { 
                        Type = SidebarItemType.NavItem,
                        Controller = "Role",
                        Action = "Create",
                        Area = "Identity",
                        Title = "Tạo vai trò mới"
                    },
                    new SidebarItem() { 
                        Type = SidebarItemType.NavItem,
                        Controller = "User",
                        Action = "Index",
                        Area = "Identity",
                        Title = "Danh sách thành viên"
                    }
                }
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Bài viết",
                AwesomeIcon = "fas fa-folder",
                CollapseId = "blog",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Category",
                        Action = "Index",
                        Area = "Blog",
                        Title = "Danh sách chuyên mục"
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Category",
                        Action = "Create",
                        Area = "Blog",
                        Title = "Tạo chuyên mục"
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Post",
                        Action = "Index",
                        Area = "Blog",
                        Title = "Danh sách bài viết"
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Post",
                        Action = "Create",
                        Area = "Blog",
                        Title = "Tạo bài viết"
                    },
                }
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Sản phẩm",
                AwesomeIcon = "fas fa-folder",
                CollapseId = "product",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "CategoryProduct",
                        Action = "Index",
                        Area = "Product",
                        Title = "Danh sách chuyên mục"
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "CategoryProduct",
                        Action = "Create",
                        Area = "Product",
                        Title = "Tạo chuyên mục"
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "ProductManagement",
                        Action = "Index",
                        Area = "Product",
                        Title = "Danh sách sản phẩm"
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "ProductManagement",
                        Action = "Create",
                        Area = "Product",
                        Title = "Tạo sản phẩm"
                    },
                }
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Hóa đơn",
                AwesomeIcon = "fa-solid fa-money-bill-wave",
                Controller = "Orders",
                Action = "Index",
                Area = "Product",
            });           

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Thư mục",
                AwesomeIcon = "fas fa-folder",
                Controller = "FileManager",
                Action = "Index",
                Area = "Files",
            });
        }

        public string RenderHtml()
        {
            StringBuilder html = new StringBuilder();
            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(_urlHelper));
            }
            return html.ToString();
        }

        public void SetActive(string controller, string action, string area)
        {
            foreach (var item in Items)
            {
                if (item.Controller == controller && item.Action == action && item.Area == area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var subItem in item.Items)
                        {
                            if (subItem.Controller == controller && subItem.Action == action && subItem.Area == area)
                            {
                                subItem.IsActive = true;
                                item.IsActive = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
