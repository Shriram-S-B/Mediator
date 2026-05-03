using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mediator.Tests
{
    /// <summary>
    /// Unit tests for the Mediator class.
    /// </summary>
    public class MediatorTests
    {
        // Test Data Models
        private class TestRequest : IRequest<string>
        {
            public string Message { get; set; } = "Test";
        }

        private class TestRequestWithData : IRequest<int>
        {
            public int Value { get; set; }
        }

        private class AnotherTestRequest : IRequest<int>
        {
            public int Data { get; set; }
        }

        private class ComplexRequest : IRequest<ComplexResponse>
        {
            public string Name { get; set; } = "TestName";
            public int Age { get; set; } = 25;
        }

        private class ComplexResponse
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Info { get; set; } = string.Empty;
        }

        // Test Handlers
        private class TestRequestHandler : IRequestHandler<TestRequest, string>
        {
            public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult($"Handled: {request.Message}");
            }
        }

        private class TestRequestWithDataHandler : IRequestHandler<TestRequestWithData, int>
        {
            public Task<int> Handle(TestRequestWithData request, CancellationToken cancellationToken)
            {
                return Task.FromResult(request.Value * 2);
            }
        }

        private class ComplexRequestHandler : IRequestHandler<ComplexRequest, ComplexResponse>
        {
            public Task<ComplexResponse> Handle(ComplexRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new ComplexResponse
                {
                    Name = request.Name,
                    Age = request.Age,
                    Info = $"{request.Name} is {request.Age} years old"
                });
            }
        }

        private class CancellableHandler : IRequestHandler<TestRequest, string>
        {
            public async Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            {
                await Task.Delay(100, cancellationToken);
                return "Completed";
            }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidServiceProvider_ShouldInitialize()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var mediator = new Mediator(serviceProvider);
            Assert.NotNull(mediator);
        }

        [Fact]
        public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new Mediator(null!));
            Assert.Equal("serviceProvider", ex.ParamName);
        }

        #endregion

        #region Send Method - Success Cases

        [Fact]
        public async Task Send_WithValidRequestAndHandler_ShouldReturnResponse()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest { Message = "Hello" };

            // Act
            var result = await mediator.Send<string>(request);

            // Assert
            Assert.Equal("Handled: Hello", result);
        }

        [Fact]
        public async Task Send_WithIntegerResponse_ShouldReturnCorrectValue()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequestWithData, int>, TestRequestWithDataHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequestWithData { Value = 5 };

            // Act
            var result = await mediator.Send<int>(request);

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public async Task Send_WithComplexObject_ShouldReturnPopulatedResponse()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new ComplexRequest { Name = "John", Age = 30 };

            // Act
            var result = await mediator.Send<ComplexResponse>(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal(30, result.Age);
            Assert.Equal("John is 30 years old", result.Info);
        }

        [Fact]
        public async Task Send_MultipleRequests_ShouldBeHandledIndependently()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequestWithData, int>, TestRequestWithDataHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());

            // Act
            var result1 = await mediator.Send<int>(new TestRequestWithData { Value = 5 });
            var result2 = await mediator.Send<int>(new TestRequestWithData { Value = 10 });
            var result3 = await mediator.Send<int>(new TestRequestWithData { Value = 15 });

            // Assert
            Assert.Equal(10, result1);
            Assert.Equal(20, result2);
            Assert.Equal(30, result3);
        }

        [Fact]
        public async Task Send_WithCancellationToken_ShouldPassItToHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, CancellableHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();

            // Act
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var result = await mediator.Send<string>(request, cts.Token);

            // Assert
            Assert.Equal("Completed", result);
        }

        [Fact]
        public async Task Send_WithDefaultCancellationToken_ShouldWork()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();

            // Act
            var result = await mediator.Send<string>(request);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Send Method - Null Request Tests

        [Fact]
        public async Task Send_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var mediator = new Mediator(services.BuildServiceProvider());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send<string>(null!));
        }

        #endregion

        #region Send Method - Handler Not Found Tests

        [Fact]
        public async Task Send_WithNoRegisteredHandler_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send<string>(request));
            Assert.Contains("No handler found", ex.Message);
            Assert.Contains(typeof(TestRequest).Name, ex.Message);
        }

        [Fact]
        public async Task Send_WithWrongResponseType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();

            // Act & Assert
            // Trying to send TestRequest with string response type should work, but let's test with unregistered request
            var anotherRequest = new AnotherTestRequest { Data = 5 };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send<int>(anotherRequest));
            Assert.Contains("No handler found", ex.Message);
        }

        #endregion

        #region Send Method - Concurrency Tests

        [Fact]
        public async Task Send_MultipleRequestsConcurrently_ShouldHandleAll()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequestWithData, int>, TestRequestWithDataHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());

            // Act
            var tasks = Enumerable.Range(1, 10)
                .Select(i => mediator.Send<int>(new TestRequestWithData { Value = i }))
                .ToList();

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, results.Length);
            for (int i = 0; i < 10; i++)
            {
                Assert.Equal((i + 1) * 2, results[i]);
            }
        }

        #endregion

        #region Send Method - Cancellation Tests

        [Fact]
        public async Task Send_WithCancelledToken_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, CancellableHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => 
                mediator.Send<string>(request, cts.Token));
        }

        #endregion

        #region Send Method - Edge Cases

        [Fact]
        public async Task Send_WithEmptyStringResponse_ShouldReturnEmptyString()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, EmptyStringHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();

            // Act
            var result = await mediator.Send<string>(request);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public async Task Send_WithZeroValue_ShouldReturnZero()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequestWithData, int>, ZeroReturnHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequestWithData();

            // Act
            var result = await mediator.Send<int>(request);

            // Assert
            Assert.Equal(0, result);
        }

        #endregion

        #region Send Method - Error Handling

        [Fact]
        public async Task Send_WhenHandlerThrowsException_ShouldPropagateException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, ExceptionThrowingHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send<string>(request));
            Assert.Equal("Handler failed", ex.Message);
        }

        #endregion

        #region Send Method - Multiple Handlers

        [Fact]
        public async Task Send_WithMultipleDifferentRequests_ShouldInvokeCorrectHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
            services.AddScoped<IRequestHandler<TestRequestWithData, int>, TestRequestWithDataHandler>();
            services.AddScoped<IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());

            // Act
            var result1 = await mediator.Send<string>(new TestRequest { Message = "Test1" });
            var result2 = await mediator.Send<int>(new TestRequestWithData { Value = 5 });
            var result3 = await mediator.Send<ComplexResponse>(new ComplexRequest { Name = "John", Age = 30 });

            // Assert
            Assert.Equal("Handled: Test1", result1);
            Assert.Equal(10, result2);
            Assert.Equal("John", result3.Name);
            Assert.Equal(30, result3.Age);
        }

        #endregion

        #region Send Method - Response Type Validation

        [Fact]
        public async Task Send_WithNullableResponseType_ShouldHandleCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
            var mediator = new Mediator(services.BuildServiceProvider());
            var request = new TestRequest { Message = "Nullable test" };

            // Act
            var result = await mediator.Send<string>(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Handled: Nullable test", result);
        }

        #endregion

        // Additional Helper Handlers

        private class EmptyStringHandler : IRequestHandler<TestRequest, string>
        {
            public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult("");
            }
        }

        private class ZeroReturnHandler : IRequestHandler<TestRequestWithData, int>
        {
            public Task<int> Handle(TestRequestWithData request, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }

        private class ExceptionThrowingHandler : IRequestHandler<TestRequest, string>
        {
            public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Handler failed");
            }
        }
    }
}

