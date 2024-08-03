using AppMVCWeb.Models.Blog;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AppMVCWeb_Test
{
    public class PostTests
    {
        [Fact]
        public void Title_ShouldHaveRequiredAttribute()
        {
            var titleProperty = typeof(Post).GetProperty("Title");
            var requiredAttribute = titleProperty.GetCustomAttributes(typeof(RequiredAttribute), false);
            requiredAttribute.Should().NotBeEmpty();
        }

        [Fact]
        public void Title_ShouldHaveStringLengthAttribute()
        {
            var titleProperty = typeof(Post).GetProperty("Title");
            var stringLengthAttribute = titleProperty.GetCustomAttributes(typeof(StringLengthAttribute), false);
            stringLengthAttribute.Should().NotBeEmpty();
        }
    }
}
