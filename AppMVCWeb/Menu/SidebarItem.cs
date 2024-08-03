using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AppMVCWeb.Menu
{
    public enum SidebarItemType
    {
        Divider = 0,
        Heading = 1,
        NavItem = 2
    }

    public class SidebarItem
    {
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public SidebarItemType Type { get; set; }
        public string Controller { get; set; }
        public string Action {  get; set; }
        public string Area { get; set; }
        public string AwesomeIcon { get; set; }
        public List<SidebarItem> Items { get; set; }
        public string CollapseId { get; set; }

        public string GetUrl(IUrlHelper urlHelper)
        {
            return urlHelper.Action(Action, Controller, new
            {
                area = Area,
            });
        }

        public string RenderHtml(IUrlHelper urlHelper)
        {
            StringBuilder html = new StringBuilder();

            if (Type == SidebarItemType.Divider)
            {
                html.Append("<hr class=\"sidebar-divider my-2\" />");
            }
            else if (Type == SidebarItemType.Heading)
            {
                html.Append(@$"<div class=""sidebar-heading"">
                                    {Title}
                                </div>");
            }
            else if (Type == SidebarItemType.NavItem)
            {
                if (Items == null)
                {
                    var url = GetUrl(urlHelper);
                    var icon = (AwesomeIcon != null) ? $"<i class=\"{AwesomeIcon}\"></i>" : null;

                    var cssClass = "nav-item";
                    if (IsActive)
                    {
                        cssClass += " active";
                    }

                    html.Append(@$" <li class=""{cssClass}"">
                                        <a class=""nav-link"" href=""{url}"">
                                            {icon}      
                                            <span>{Title}</span>
                                        </a>
                                    </li>");
                }
                else
                {
                    var cssClass = "nav-item";
                    if (IsActive)
                    {
                        cssClass += " active";
                    }

                    var collapseCss = "collapse";

                    if (IsActive)
                    {
                        collapseCss += " show";
                    }

                    string itemMenu = null;

                    foreach (var item in Items)
                    {
                        var urlItem = item.GetUrl(urlHelper);
                        var cssItem = "collapse-item";

                        if (item.IsActive)
                        {
                            cssItem += " active";
                        }

                        itemMenu += $"<a class=\"{cssItem}\" href=\"{urlItem}\">{item.Title}</a>";
                    }

                    var icon = (AwesomeIcon != null) ? $"<i class=\"{AwesomeIcon}\"></i>" : null;

                    html.Append(@$"<li class=""{cssClass}"">
                                        <a class=""nav-link collapsed"" href=""#"" 
                                        data-bs-toggle=""collapse"" data-bs-target=""#{CollapseId}"" 
                                        aria-expanded=""false"" aria-controls=""{CollapseId}"">
                                            {icon}
                                            <span>{Title}</span>
                                        </a>

                                        <div id=""{CollapseId}"" class=""{collapseCss}"" aria-labelledby=""headingTwo"" 
                                        data-bs-parent=""#accordionSidebar"">
                                            <div class=""bg-white py-2 collapse-inner rounded"">
                                                {itemMenu}
                                            </div>
                                        </div>
                                    </li>");
                }
            }


            return html.ToString();
        }
    }
}
