# Mediator

[![NuGet](https://img.shields.io/nuget/v/CustomMediator.svg)](https://www.nuget.org/packages/CustomMediator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com)

A **lightweight, fast mediator pattern implementation** for .NET that serves as a **zero-dependency alternative to MediatR**. Perfect for projects requiring minimal overhead and straightforward request/response messaging with dependency injection.

> **Mediator Pattern**: Encapsulates how a set of objects interact without them being tightly coupled. Routes requests to appropriate handlers through a central mediator component.

## ?? Why Choose This Mediator?

| Feature | This Package | MediatR |
|---------|:---:|:---:|
| **Core Mediator** | ? | ? |
| **Request/Response** | ? | ? |
| **DI Integration** | ? | ? |
| **CancellationToken Support** | ? | ? |
| **Package Size** | ? **~30 KB** | ?? **~500 KB** |
| **Zero Dependencies** | ? **Yes** | ? No |
| **Learning Curve** | ? **Very Easy** | ?? Moderate |
| **Pipelines/Behaviors** | ? | ? |
| **Notifications** | ? | ? |

**Choose this Mediator if you:**
- ? Need a simple, focused request/response pattern
- ? Want minimal dependencies and fast startup
- ? Prefer inspectable, straightforward code
- ? Don't need advanced features like pipelines or pub/sub

**Choose MediatR if you:**
- ? Need pipeline behaviors and cross-cutting concerns
- ? Require pub/sub notifications
- ? Building enterprise-scale messaging infrastructure

## ? Features

- **Lightweight** — No external dependencies, just ~2KB runtime
- **High Performance** — Zero-allocation dispatcher pattern
- **Fully Async** — Native `async`/`await` with `Task<T>`
- **CancellationToken Support** — Full cooperative cancellation
- **DI-First** — Seamless integration with `Microsoft.Extensions.DependencyInjection`
- **Clean Exceptions** — Handler exceptions propagate naturally without wrapping
- **.NET 8 Native** — Built for modern .NET with C# 12 support
- **Testable** — Simple interface-based design for easy mocking

## ?? Installation

### NuGet Package Manager
```bash
dotnet add package CustomMediator
```

### Package Manager Console
```powershell
Install-Package CustomMediator
```

### .NET CLI
```bash
dotnet package add CustomMediator
```

## ?? Quick Start

### 1. Define Your Request

```csharp
using Mediator;

public class GetProductRequest : IRequest<ProductResponse>
{
    public int ProductId { get; set; }
}

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 2. Create a Handler

```csharp
using Mediator;
using System.Threading;
using System.Threading.Tasks;

public class GetProductHandler : IRequestHandler<GetProductRequest, ProductResponse>
{
    private readonly IProductService _productService;

    public GetProductHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductResponse> Handle(
        GetProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductAsync(
            request.ProductId, 
            cancellationToken);

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}
```

### 3. Register in DI

```csharp
using Microsoft.Extensions.DependencyInjection;
using Mediator;
using Mediator.Interfaces;

var services = new ServiceCollection();

// Register mediator
services.AddScoped<IMediator>(sp => new Mediator(sp));

// Register handlers
services.AddScoped<IRequestHandler<GetProductRequest, ProductResponse>, GetProductHandler>();
services.AddScoped<IProductService, ProductService>();

var provider = services.BuildServiceProvider();
```

### 4. Send a Request

```csharp
var mediator = provider.GetRequiredService<IMediator>();

var response = await mediator.Send<ProductResponse>(
    new GetProductRequest { ProductId = 42 }
);

Console.WriteLine($"Product: {response.Name} - ${response.Price}");
```

## ?? Core API Reference

### Interfaces

#### `IRequest<TResponse>`
Marker interface that identifies a request type and its response type.

```csharp
public interface IRequest<TResponse> { }
```

#### `IRequestHandler<TRequest, TResponse>`
Processes a request and returns a response asynchronously.

```csharp
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
```

#### `IMediator`
Routes requests to their registered handlers.

```csharp
public interface IMediator
{
    Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    );
}
```

### Implementation

```csharp
public class Mediator : IMediator
{
    public Mediator(IServiceProvider serviceProvider) { }
    
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default) { }
}
```

## ?? Real-World Examples

### Example 1: Simple String Response

```csharp
// Request
public class GreetRequest : IRequest<string>
{
    public string Name { get; set; }
}

// Handler
public class GreetHandler : IRequestHandler<GreetRequest, string>
{
    public Task<string> Handle(GreetRequest request, CancellationToken cancellationToken)
    {
        var greeting = $"Hello, {request.Name}! Welcome to Mediator.";
        return Task.FromResult(greeting);
    }
}

// Usage
var result = await mediator.Send<string>(new GreetRequest { Name = "Alice" });
// Output: "Hello, Alice! Welcome to Mediator."
```

### Example 2: Complex Business Logic

```csharp
// Request & Response
public class CreateOrderRequest : IRequest<CreateOrderResponse>
{
    public int CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class CreateOrderResponse
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
}

// Handler with DI
public class CreateOrderHandler : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        ILogger<CreateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> Handle(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating order for customer {CustomerId}", 
            request.CustomerId);

        // Validate
        if (!request.Items.Any())
            throw new ArgumentException("Order must contain at least one item");

        // Create order
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Items = request.Items.ToList(),
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        var savedOrder = await _orderRepository.AddAsync(order, cancellationToken);

        // Process payment
        try
        {
            await _paymentService.ProcessPaymentAsync(
                savedOrder.Id,
                savedOrder.Total,
                cancellationToken);

            savedOrder.Status = OrderStatus.Confirmed;
            _logger.LogInformation("Order {OrderId} confirmed", savedOrder.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed for order {OrderId}", savedOrder.Id);
            savedOrder.Status = OrderStatus.Failed;
            throw;
        }

        await _orderRepository.UpdateAsync(savedOrder, cancellationToken);

        return new CreateOrderResponse
        {
            OrderId = savedOrder.Id,
            Total = savedOrder.Total,
            Status = savedOrder.Status
        };
    }
}
```

### Example 3: CancellationToken Handling

```csharp
public class FetchDataRequest : IRequest<DataResponse>
{
    public string Url { get; set; }
    public int TimeoutSeconds { get; set; }
}

public class FetchDataHandler : IRequestHandler<FetchDataRequest, DataResponse>
{
    private readonly HttpClient _httpClient;

    public FetchDataHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DataResponse> Handle(
        FetchDataRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Pass cancellation token to all async operations
            var response = await _httpClient.GetStringAsync(request.Url, cancellationToken);
            
            return new DataResponse { Data = response, Success = true };
        }
        catch (OperationCanceledException)
        {
            return new DataResponse { Data = null, Success = false, Error = "Request cancelled" };
        }
        catch (HttpRequestException ex)
        {
            return new DataResponse { Data = null, Success = false, Error = ex.Message };
        }
    }
}

// Usage with timeout
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
try
{
    var result = await mediator.Send<DataResponse>(
        new FetchDataRequest { Url = "https://api.example.com/data", TimeoutSeconds = 10 },
        cts.Token
    );
}
catch (TaskCanceledException)
{
    Console.WriteLine("Request timed out");
}
```

### Example 4: Input Validation

```csharp
public class CreateUserRequest : IRequest<int>
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public int Age { get; set; }
}

public class CreateUserHandler : IRequestHandler<CreateUserRequest, int>
{
    private readonly IUserRepository _repository;
    private static readonly Regex EmailRegex = 
        new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Validate early
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request.Email));

        if (!EmailRegex.IsMatch(request.Email))
            throw new ArgumentException("Invalid email format", nameof(request.Email));

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required", nameof(request.FullName));

        if (request.Age < 18)
            throw new ArgumentException("User must be at least 18 years old", nameof(request.Age));

        // Check if email exists
        var existingUser = await _repository.FindByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException($"Email {request.Email} is already registered");

        // Create user
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            Age = request.Age,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(user, cancellationToken);
    }
}
```

### Example 5: Query Handler with Caching

```csharp
public class GetUserCacheRequest : IRequest<UserResponse>
{
    public int UserId { get; set; }
}

public class GetUserCacheHandler : IRequestHandler<GetUserCacheRequest, UserResponse>
{
    private readonly IUserRepository _repository;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "user_";

    public GetUserCacheHandler(IUserRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<UserResponse> Handle(
        GetUserCacheRequest request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{request.UserId}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out UserResponse cachedUser))
            return cachedUser;

        // Get from repository
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User {request.UserId} not found");

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.FullName,
            Email = user.Email
        };

        // Cache for 1 hour
        _cache.Set(cacheKey, response, TimeSpan.FromHours(1));

        return response;
    }
}
```

## ??? Registration Patterns

### Basic Registration

```csharp
services.AddScoped<IMediator>(sp => new Mediator(sp));
services.AddScoped<IRequestHandler<MyRequest, MyResponse>, MyHandler>();
```

### Multiple Handlers

```csharp
// Option 1: Register each individually
services.AddScoped<IRequestHandler<GetUserRequest, UserResponse>, GetUserHandler>();
services.AddScoped<IRequestHandler<CreateUserRequest, UserResponse>, CreateUserHandler>();
services.AddScoped<IRequestHandler<DeleteUserRequest, bool>, DeleteUserHandler>();

// Option 2: Extension method for batch registration
public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddMediatorHandlers(
        this IServiceCollection services,
        params Type[] scanAssemblies)
    {
        services.AddScoped<IMediator>(sp => new Mediator(sp));

        foreach (var assembly in scanAssemblies)
        {
            var handlerType = typeof(IRequestHandler<,>);
            var handlers = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && 
                        i.GetGenericTypeDefinition() == handlerType));

            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                        i.GetGenericTypeDefinition() == handlerType);

                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, handler);
                }
            }
        }

        return services;
    }
}

// Usage
services.AddMediatorHandlers(typeof(Program).Assembly);
```

## ? Best Practices

### 1. Single Responsibility Principle

```csharp
// ? Good - Single responsibility
public class GetUserHandler : IRequestHandler<GetUserRequest, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetUserAsync(request.UserId, cancellationToken);
        return new UserResponse { Id = user.Id, Name = user.Name };
    }
}

// ? Bad - Multiple responsibilities
public class BadHandler : IRequestHandler<ComplexRequest, ComplexResponse>
{
    public async Task<ComplexResponse> Handle(ComplexRequest request, CancellationToken cancellationToken)
    {
        // Validation, database access, API calls, logging, caching, etc.
        // All mixed together!
    }
}
```

### 2. Always Use CancellationToken

```csharp
// ? Good - Token passed to async operations
public async Task<string> Handle(MyRequest request, CancellationToken cancellationToken)
{
    return await _httpClient.GetStringAsync(url, cancellationToken);
}

// ? Bad - Token ignored
public async Task<string> Handle(MyRequest request, CancellationToken cancellationToken)
{
    return await _httpClient.GetStringAsync(url); // Never passes token!
}
```

### 3. Meaningful Exceptions

```csharp
// ? Good - Specific, descriptive exceptions
if (!user.IsActive)
    throw new InvalidOperationException("User account is not active");

if (request.Age < 0)
    throw new ArgumentOutOfRangeException(nameof(request.Age), "Age cannot be negative");

// ? Bad - Generic, unhelpful exceptions
throw new Exception("Error");
throw new ApplicationException("Something went wrong");
```

### 4. Implement Comprehensive Logging

```csharp
public class OrderHandler : IRequestHandler<CreateOrderRequest, OrderResponse>
{
    private readonly ILogger<OrderHandler> _logger;

    public async Task<OrderResponse> Handle(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing order request for customer {CustomerId} with {ItemCount} items",
            request.CustomerId,
            request.Items.Count);

        try
        {
            var result = await ProcessOrderAsync(request, cancellationToken);
            
            _logger.LogInformation(
                "Order processed successfully. OrderId: {OrderId}, Total: {Total}",
                result.OrderId,
                result.Total);
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Order processing failed for customer {CustomerId}",
                request.CustomerId);
            throw;
        }
    }
}
```

### 5. Validate Early, Process Late

```csharp
public Task<string> Handle(UserRequest request, CancellationToken cancellationToken)
{
    // Validate immediately
    ArgumentNullException.ThrowIfNull(request);
    
    if (string.IsNullOrWhiteSpace(request.Name))
        throw new ArgumentException("Name is required", nameof(request.Name));
    
    if (request.Age < 0)
        throw new ArgumentOutOfRangeException(nameof(request.Age));

    // Only then proceed with business logic
    return ProcessUserAsync(request, cancellationToken);
}
```

## ?? Unit Testing

Testing is simple since handlers depend on interfaces:

```csharp
[Fact]
public async Task GetUserHandler_WithValidUserId_ReturnsUser()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    mockRepository
        .Setup(r => r.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new User { Id = 42, Name = "John Doe", Email = "john@example.com" });

    var handler = new GetUserHandler(mockRepository.Object);
    var request = new GetUserRequest { UserId = 42 };

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(42, result.Id);
    Assert.Equal("John Doe", result.Name);
    Assert.Equal("john@example.com", result.Email);
    
    mockRepository.Verify(
        r => r.GetUserAsync(42, It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task GetUserHandler_WithInvalidUserId_ThrowsException()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    mockRepository
        .Setup(r => r.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((User)null);

    var handler = new GetUserHandler(mockRepository.Object);
    var request = new GetUserRequest { UserId = 999 };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => handler.Handle(request, CancellationToken.None));
}
```

## ?? Troubleshooting

### "No handler found for request type..."

**Problem:** Handler isn't registered in the DI container.

**Solution:**
```csharp
services.AddScoped<IRequestHandler<MyRequest, MyResponse>, MyHandler>();
```

### "Handler threw an exception"

**Problem:** Unhandled exception in handler logic.

**Solution:**
```csharp
try
{
    var result = await mediator.Send<MyResponse>(request);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Handler error: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation error: {ex.ParamName} - {ex.Message}");
}
```

### "Request timed out / TaskCanceledException"

**Problem:** CancellationToken exceeded timeout.

**Solution:**
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    var result = await mediator.Send<MyResponse>(request, cts.Token);
}
catch (TaskCanceledException)
{
    Console.WriteLine("Request was cancelled");
}
```

### Handler receives null dependency

**Problem:** Dependency isn't registered in DI container.

**Solution:**
```csharp
// Register dependencies BEFORE the handler
services.AddScoped<IMyService, MyService>();
services.AddScoped<IRequestHandler<MyRequest, MyResponse>, MyHandler>();
```

## ?? Resources

- **GitHub:** [Shriram-S-B/Mediator](https://github.com/Shriram-S-B/Mediator)
- **Mediator Pattern:** [Wikipedia](https://en.wikipedia.org/wiki/Mediator_pattern)
- **MediatR Alternative:** [MediatR GitHub](https://github.com/jbogard/MediatR)
- **DI Documentation:** [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection)

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Contributing

Contributions are welcome! Please:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

For major changes, please open an issue first to discuss what you would like to change.

## ?? Support

For questions, issues, or suggestions:
- ?? Open an [issue on GitHub](https://github.com/Shriram-S-B/Mediator/issues)
- ?? Contact via GitHub Discussions
- ?? Report bugs with reproduction steps

## ?? Changelog

### v1.0.2
- ? Initial NuGet release
- ? Full .NET 8 support
- ? Comprehensive test coverage
- ? Clean exception propagation

---

**Mediator** — A lightweight, dependency-free request/response dispatcher for .NET 8  
*Made with ?? as a fast, minimal alternative to MediatR*