using App.Models;
using AppMVCWeb.Areas.Product.Controllers;
using AppMVCWeb.Models.Product;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;

namespace AppMVCWeb_Test
{
    public class JsonResponse
    {
        public int Success { get; set; }
        public string Message { get; set; }
    }

    public class ProductManagementControllerTests
    {
        [Fact]
        public async Task DeletePhoto_ShouldReturnSuccess_WhenPhotoExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new AppDbContext(options);
            var mockUserManager = new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

            var photo = new ProductPhoto { Id = 1, FileName = "test.jpg" };
            context.ProductPhotos.Add(photo);
            await context.SaveChangesAsync();

            var controller = new ProductManagementController(context, mockUserManager.Object);

            // Mock file system interaction
            var filePath = Path.Combine("Uploads", "Products", photo.FileName);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            File.WriteAllText(filePath, "dummy content");

            // Act
            var result = await controller.DeletePhoto(1);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var json = JsonConvert.SerializeObject(jsonResult.Value);
            var data = JsonConvert.DeserializeObject<JsonResponse>(json);

            data.Should().NotBeNull();
            data.Success.Should().Be(1);

            // Clean up
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
