using System;
using System.Web;
using Moq;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection.Tests
{
    public static class SystemWebServiceProviderTests
    {
        [Fact]
        public static void Constructor_WithNullParentServiceProvider_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => new SystemWebServiceProvider(null));
        }

        [Theory]
        [InlineData(typeof(IServiceProvider))]
        public static void GetService_ForImplementedService_ReturnsSelf(Type serviceType)
        {
            // Arrange

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            var target = new SystemWebServiceProvider(parentServiceProviderMock.Object);

            // Act

            var actual = target.GetService(serviceType);

            // Assert

            actual.ShouldBeSameAs(target);

            parentServiceProviderMock
                .Verify(
                    sp => sp.GetService(serviceType),
                    Times.Never());
        }

        [Fact]
        public static void GetService_WithoutCurrentHttpContext_InvokesParentServiceProvider()
        {
            // Arrange

            HttpContext.Current = null;

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            var target = new SystemWebServiceProvider(parentServiceProviderMock.Object);

            // Act

            target.GetService(null);

            // Assert

            parentServiceProviderMock
                .Verify(
                    sp => sp.GetService(null),
                    Times.Once());
        }

        [Fact]
        public static void GetService_WithoutHttpContextServiceProvider_InvokesParentServiceProvider()
        {
            // Arrange

            HttpContext.Current = new HttpContext(new HttpRequest(null, "https://mysite.com/", null), new HttpResponse(null));
            HttpContext.Current.Items[HttpContextKeys.HttpContextServiceProviderKey] = null;

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            var target = new SystemWebServiceProvider(parentServiceProviderMock.Object);

            // Act

            target.GetService(null);

            // Assert

            parentServiceProviderMock
                .Verify(
                    sp => sp.GetService(null),
                    Times.Once());
        }

        [Fact]
        public static void GetService_WithHttpContextServiceProvider_InvokesHttpContextServiceProvider()
        {
            // Arrange

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            var httpContextServiceProviderMock = new Mock<IServiceProvider>();

            HttpContext.Current = new HttpContext(new HttpRequest(null, "https://mysite.com/", null), new HttpResponse(null));
            HttpContext.Current.Items[HttpContextKeys.HttpContextServiceProviderKey] = httpContextServiceProviderMock.Object;

            var target = new SystemWebServiceProvider(parentServiceProviderMock.Object);

            // Act

            target.GetService(null);

            // Assert

            httpContextServiceProviderMock
                .Verify(
                    sp => sp.GetService(null),
                    Times.Once());

            parentServiceProviderMock
                .Verify(
                    sp => sp.GetService(null),
                    Times.Never());
        }
    }
}
