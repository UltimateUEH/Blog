using Microsoft.AspNetCore.Builder;
using System.Net;

namespace AppMVCWeb.ExtendMethods
{
    public static class AppExtends
    {
        public static void AddStaticCodePage(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(appError =>
            {
                appError.Run(async context =>
                {
                    var response = context.Response;
                    var code = response.StatusCode;

                    var content = @$"
                        <html>
                            <head>
                                <meta charset='utf-8' />
                                <title>Lỗi {code}</title>
                            </head>
                            <body>
                                <div style='font-size: 16px; color: red;'>
                                    <h1>Lỗi {code} - {(HttpStatusCode)code}</h1>
                                </div>                                
                                <p>Đã xảy ra lỗi khi xử lý yêu cầu.</p>
                            </body>
                        </html>
                        ";

                    await response.WriteAsync(content);
                });
            }); // Code 400 - 599
        }
    }
}
