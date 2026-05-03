# Mediator

[![NuGet](https://img.shields.io/nuget/v/CustomMediator.svg)](https://www.nuget.org/packages/CustomMediator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com)

A lightweight, fast mediator pattern implementation for .NET that serves as a zero-dependency alternative to MediatR. Perfect for projects requiring minimal overhead and straightforward request/response messaging with dependency injection.

> Mediator Pattern: Encapsulates how a set of objects interact without them being tightly coupled. Routes requests to appropriate handlers through a central mediator component.

## Why Choose This Mediator?

| Feature | This Package | MediatR |
|---------|:---:|:---:|
| Core Mediator | Yes | Yes |
| Request/Response | Yes | Yes |
| DI Integration | Yes | Yes |
| CancellationToken Support | Yes | Yes |
| Package Size | Small (~30 KB) | Larger (~500 KB) |
| Zero Dependencies | Yes | No |
| Learning Curve | Very Easy | Moderate |
| Pipelines/Behaviors | No | Yes |
| Notifications | No | Yes |

Choose this Mediator if you:
- Need a simple, focused request/response pattern
- Want minimal dependencies and fast startup
- Prefer inspectable, straightforward code
- Don't need advanced features like pipelines or pub/sub

Choose MediatR if you:
- Need pipeline behaviors and cross-cutting concerns
- Require pub/sub notifications
- Are building enterprise-scale messaging infrastructure

## Features

- Lightweight — No external dependencies
- High Performance — Minimal dispatch overhead
- Fully Async — Native async/await with Task<T>
- CancellationToken Support — Full cooperative cancellation
- DI-First — Works with Microsoft.Extensions.DependencyInjection
- Clean Exceptions — Handler exceptions propagate naturally
- .NET 8 Native — Built for modern .NET with C# 12 support
- Testable — Simple interface-based design for easy mocking

## Installation

NuGet Package Manager:

```
dotnet add package CustomMediator
```

Package Manager Console:

```
Install-Package CustomMediator
```

.NET CLI:

```
dotnet package add CustomMediator
```

## Quick Start

1. Define your request and response types:

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

2. Create a handler:

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

    public async Task<ProductResponse> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductAsync(request.ProductId, cancellationToken);

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}
```

3. Register services and handlers in DI:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Mediator;
using Mediator.Interfaces;

var services = new ServiceCollection();

services.AddScoped<IMediator>(sp => new Mediator(sp));
services.AddScoped<IRequestHandler<GetProductRequest, ProductResponse>, GetProductHandler>();
services.AddScoped<IProductService, ProductService>();

var provider = services.BuildServiceProvider();
```

4. Send a request:

```csharp
var mediator = provider.GetRequiredService<IMediator>();
var response = await mediator.Send<ProductResponse>(new GetProductRequest { ProductId = 42 });
Console.WriteLine($"Product: {response.Name} - ${response.Price}");
```

## Core API Reference

Interfaces:

```csharp
public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
```

## Examples

1) Simple greeting:

```csharp
public class GreetRequest : IRequest<string> { public string Name { get; set; } }
public class GreetHandler : IRequestHandler<GreetRequest, string>
{
    public Task<string> Handle(GreetRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Hello, {request.Name}!");
    }
}
```

2) Long-running operation with cancellation:

```csharp
public class LongRunningRequest : IRequest<string> { public int Seconds { get; set; } }
public class LongRunningHandler : IRequestHandler<LongRunningRequest, string>
{
    public async Task<string> Handle(LongRunningRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(request.Seconds), cancellationToken);
        return "Done";
    }
}
```

3) Exception propagation:

```csharp
public class FailingRequest : IRequest<string> { }
public class FailingHandler : IRequestHandler<FailingRequest, string>
{
    public Task<string> Handle(FailingRequest request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Handler failed");
    }
}

// The mediator will propagate the InvalidOperationException to the caller.
```

## Registration Patterns

Register handlers individually or use a helper to scan assemblies and register all IRequestHandler implementations.

## Best Practices

- Keep handlers focused and single-responsibility.
- Respect CancellationToken in handlers.
- Throw specific exceptions for error cases.
- Use logging for important operations.
- Validate inputs early.

## Unit Testing

Handlers are easy to unit test by mocking dependencies and invoking Handle directly.

## Troubleshooting

- "No handler found" — ensure the handler is registered in DI.
- "TaskCanceledException" — ensure cancellation token timeouts are configured and handled.
- Null dependencies — register dependencies before registering handlers.

## Resources

- GitHub: https://github.com/Shriram-S-B/Mediator
- MediatR: https://github.com/jbogard/MediatR
- Microsoft DI docs: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection

## License

This project is licensed under the MIT License. See LICENSE for details.