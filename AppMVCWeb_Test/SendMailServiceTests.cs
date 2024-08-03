using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AppMVCWeb_Test
{
    public class SendMailServiceTests
    {
        [Fact]
        public async Task SendSmsAsync_ShouldCreateSmsFile()
        {
            // Arrange
            var mockMailSettings = new Mock<IOptions<MailSettings>>();
            var mockLogger = new Mock<ILogger<SendMailService>>();

            var mailSettings = new MailSettings
            {
                Mail = "test@example.com",
                DisplayName = "Test",
                Password = "password",
                Host = "smtp.example.com",
                Port = 587
            };

            mockMailSettings.Setup(ms => ms.Value).Returns(mailSettings);

            var sendMailService = new SendMailService(mockMailSettings.Object, mockLogger.Object);
            var number = "1234567890";
            var message = "Test message";

            // Act
            await sendMailService.SendSmsAsync(number, message);

            // Assert
            var files = Directory.GetFiles("SMSSave", $"{number}-*.txt");
            files.Should().NotBeEmpty();
        }
    }
}
