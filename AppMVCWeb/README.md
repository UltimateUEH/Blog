## Controller

    - Là một lớp kế thừa từ lớp Controller: Microsoft.AspNetCore.Mvc.Controller
    - Action trong controller là một phương thức public (không được static)
    - Action trả về bất kì kiểu dữ liệu nào, nhưng thường là IActionResult
    - Các dịch vụ inject vào controller thông qua constructor

## View

    - Là một file cshtml
    - View cho Action được luu tại: /Views/ControllerName/ActionName.cshtml
    - Thêm các thư mục chứa view vào thư mục Views:

    {0} - Action Name
    {1} - Controller Name
    {2} - Area Name

options.ViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);

## Truyền dũ liệu sang View

    - ViewBag: dynamic property
    - ViewData: dictionary
    - Model: object
    - TempData: dictionary

## Area

    - Là tên dùng để routing
    - Là cấu trúc thư mục chứa các controller, view, model, ...
    - Thiết lập Area cho các controller bằng``[Area("AreaName")]``
    - Tạo cấu trúc thư mục:

```
	dotnet aspnet-codegenerator area Product
```

## Route

    - app.MapControllerRoute
    - app.MapAreaControllerRoute
    - [AcceptVerbs("GET", "POST")]
    - [Route("pattern")]
    - [HttpGet], [HttpPost], [HttpPut], [HttpDelete]

## Url Generation

### UrlHelper:

    - Url.Action
    - Url.RouteUrl
    - Url.Content
    - Url.Link

```

Url.Action("PlanetInfo", "Planet", new { id = 1 }, Context.Request.Scheme);
Url.RouteUrl("default", new { controller = "First", action = "HelloView", id = 1, username = "Ultimate"})

```

### HtmlTagHelper: `<a> <button> <form> `

Use attribute:

```
asp-area="Area"
asp-action="Action"
asp-controller="Controller"
asp-route...="123"
asp-route="default"
```
