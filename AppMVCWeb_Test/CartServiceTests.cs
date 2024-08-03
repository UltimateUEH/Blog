using AppMVCWeb.Areas.Product.Models;
using AppMVCWeb.Areas.Product.Services;
using AppMVCWeb.Models.Product;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;

namespace AppMVCWeb_Test
{
    public class CartServiceTests
    {
        [Fact]
        public void SaveCartSession_ShouldSaveCartToSession()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();

            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var cartService = new CartService(mockHttpContextAccessor.Object);
            var cartItems = new List<CartItem>
            {
                new CartItem
                {
                    Product = new ProductModel
                    {
                        ProductId = 1,
                        Title = "Sample Product",
                        Price = 100
                    },
                    Quantity = 2
                }
            };

            var cartItemsJson = JsonConvert.SerializeObject(cartItems);
            var cartItemsBytes = System.Text.Encoding.UTF8.GetBytes(cartItemsJson);

            // Act
            cartService.SaveCartSession(cartItems);

            // Assert
            mockSession.Verify(s => s.Set(It.IsAny<string>(), It.Is<byte[]>(b => b.SequenceEqual(cartItemsBytes))), Times.Once);
        }
    }
}
