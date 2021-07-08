using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection.Tests
{
    public static class WebObjectActivatorTests
    {
        [Fact]
        public static void Constructor_WithNullParentServiceProvider_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => new WebObjectActivator(null));
        }

        [Fact]
        public static void GetService_WithResolvedService_GetsServiceFromParentServiceProvider()
        {
            // Arrange

            var expected = new SimplePublicTestType();

            var parentServiceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService(typeof(SimplePublicTestType)) == expected);

            using (var target = new WebObjectActivator(parentServiceProvider))
            {
                // Act

                var actual = target.GetService(typeof(SimplePublicTestType));

                // Assert

                object.ReferenceEquals(actual, expected).ShouldBeTrue();
            }
        }

        [Fact]
        public static void GetService_WithUnresolvedService_GetsService()
        {
            // Arrange

            var parentServiceProvider = Mock.Of<IServiceProvider>();

            using (var target = new WebObjectActivator(parentServiceProvider))
            {
                // Act

                var actual = target.GetService(typeof(SimplePublicTestType));

                // Assert

                actual.ShouldBeOfType<SimplePublicTestType>();
            }
        }

        [Fact]
        public static void GetService_WithUnresolvedServiceAndUnresolvedDependency_GetsService()
        {
            // Arrange

            var parentServiceProvider = Mock.Of<IServiceProvider>();

            using (var target = new WebObjectActivator(parentServiceProvider))
            {
                // Act

                var actual = target.GetService(typeof(DependentPublicTestType));

                // Assert

                actual.ShouldBeOfType<DependentPublicTestType>()
                    .Dependency.ShouldNotBeNull();
            }
        }

        [Theory]
        [InlineData(typeof(IServiceProvider))]
        [InlineData(typeof(IServiceScopeFactory))]
        public static void GetService_ForImplementedService_ReturnsSelf(Type serviceType)
        {
            // Arrange

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            using (var target = new WebObjectActivator(parentServiceProviderMock.Object))
            {
                // Act

                var actual = target.GetService(serviceType);

                // Assert

                actual.ShouldBeSameAs(target);

                parentServiceProviderMock
                    .Verify(
                        sp => sp.GetService(serviceType),
                        Times.Never());
            }
        }

        [Fact]
        public static void GetService_WithUnresolvedServiceAndResolvedDependency_GetsServiceWithResolvedDependency()
        {
            // Arrange

            var expectedDependency = new SimplePublicTestType();

            var parentServiceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService(typeof(SimplePublicTestType)) == expectedDependency);

            using (var target = new WebObjectActivator(parentServiceProvider))
            {
                // Act

                var actual = target.GetService(typeof(DependentPublicTestType));

                // Assert

                actual.ShouldBeOfType<DependentPublicTestType>()
                    .Dependency.ShouldBeSameAs(expectedDependency);
            }
        }

        [Fact]
        public static void GetService_WithDisposableUnresolvedService_InvokesServiceDisposeOnContainerDispose()
        {
            // Arrange

            var disposeInvoked = false;

            var parentServiceProviderMock = new Mock<IServiceProvider>();
            parentServiceProviderMock
                .SetupSequence(sp => sp.GetService(typeof(Action)))
                .Returns((object)(new Action(() => { })))
                .Returns((object)(new Action(() => disposeInvoked = true)));

            using (var target = new WebObjectActivator(parentServiceProviderMock.Object))
            {
                // Act

                var actual = target.GetService(typeof(DisposableTestTypeWithControls));

                // Assert

                actual.ShouldBeOfType<DisposableTestTypeWithControls>();
            }

            disposeInvoked.ShouldBeTrue();
        }

        [Fact]
        public static void GetService_WithDisposableUnresolvedServiceAndDisposedContainer_ThrowsObjectDisposedException()
        {
            // Arrange

            var disposeInvoked = false;
            var constructorTcs = new TaskCompletionSource<bool>();

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            var target = new WebObjectActivator(parentServiceProviderMock.Object);

            parentServiceProviderMock
                .SetupSequence(sp => sp.GetService(typeof(Action)))
                .Returns((object)(new Action(() =>
                {
                    target.Dispose();
                    constructorTcs.Task.Wait();
                })))
                .Returns((object)(new Action(() => disposeInvoked = true)));

            // Act

            var actualTask = Task.Run(() => target.GetService(typeof(DisposableTestTypeWithControls)));

            constructorTcs.SetResult(true);

            Should.Throw<ObjectDisposedException>(() => actualTask.GetAwaiter().GetResult());

            // Assert

            disposeInvoked.ShouldBeTrue();
        }

        [Fact]
        public static void GetService_WithDisposedContainer_ReturnsSelf()
        {
            // Arrange

            var parentServiceProviderMock = new Mock<IServiceProvider>();

            var target = new WebObjectActivator(parentServiceProviderMock.Object);
            target.Dispose();

            // Act

            Should.Throw<ObjectDisposedException>(() => target.GetService(typeof(SimplePublicTestType)));

            // Assert

            parentServiceProviderMock
                .Verify(
                    sp => sp.GetService(typeof(SimplePublicTestType)),
                    Times.Never());
        }

        [Fact]
        public static void GetService_OfSameUnresolvedType_ShouldNotReturnSameInstance()
        {
            // Arrange

            var parentServiceProvider = Mock.Of<IServiceProvider>();

            using (var target = new WebObjectActivator(parentServiceProvider))
            {
                // Act

                var first = target.GetService(typeof(SimplePublicTestType));
                var second = target.GetService(typeof(SimplePublicTestType));

                // Assert

                first.ShouldNotBeSameAs(second);
            }
        }

        [Fact]
        public static void GetService_WithDisposableUnresolvedService_InvokesServiceScopeDisposeOnContainerDispose()
        {
            // Arrange

            var disposeInvoked = false;

            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            scopeServiceProviderMock
                .SetupSequence(sp => sp.GetService(typeof(Action)))
                .Returns((object)(new Action(() => { })))
                .Returns((object)(new Action(() => disposeInvoked = true)));

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(scopeServiceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock
                .Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);

            var parentServiceProviderMock = new Mock<IServiceProvider>();
            parentServiceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            using (var container = new WebObjectActivator(parentServiceProviderMock.Object))
            {
                using (var target = container.CreateScope())
                {
                    // Act

                    var actual = target.ServiceProvider.GetService(typeof(DisposableTestTypeWithControls));

                    // Assert

                    actual.ShouldBeOfType<DisposableTestTypeWithControls>();
                }

                disposeInvoked.ShouldBeTrue();
            }
        }

        [Fact]
        public static void GetScope__CreatesChildContainer()
        {
            // Arrange

            var scopeServiceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(scopeServiceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock
                .Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);

            var parentServiceProviderMock = new Mock<IServiceProvider>();
            parentServiceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            using (var container = new WebObjectActivator(parentServiceProviderMock.Object))
            {
                using (var target = container.CreateScope())
                {
                    // Act

                    var actual = target.ServiceProvider;

                    // Assert

                    actual.ShouldBeOfType<WebObjectActivator>()
                        .ShouldNotBeSameAs(container);
                }
            }
        }

        [Fact]
        public static void GetScope_DisponsingScope_DisposesChildContainer()
        {
            // Arrange

            var scopeServiceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(scopeServiceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock
                .Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);

            var parentServiceProviderMock = new Mock<IServiceProvider>();
            parentServiceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);

            using (var container = new WebObjectActivator(parentServiceProviderMock.Object))
            {
                IServiceProvider childContainer;
                using (var target = container.CreateScope())
                {
                    childContainer = target.ServiceProvider;
                }

                // Act

                Should.Throw<ObjectDisposedException>(() => childContainer.GetService(null));
            }
        }
    }

    public class SimplePublicTestType
    {
    }

    public class DependentPublicTestType
    {
        public DependentPublicTestType(SimplePublicTestType dependency)
        {
            this.Dependency = dependency;
        }

        public SimplePublicTestType Dependency { get; }
    }

    public class DisposableTestType : IDisposable
    {
        public Action DisposeAction { get; set; }

        public void Dispose() => this.DisposeAction?.Invoke();
    }

    public class DisposableTestTypeWithControls : IDisposable
    {
        private readonly Action disposeAction;

        public DisposableTestTypeWithControls(Action constructorAction, Action disposeAction)
        {
            constructorAction?.Invoke();
            this.disposeAction = disposeAction;
        }

        public void Dispose() => this.disposeAction?.Invoke();
    }
}
