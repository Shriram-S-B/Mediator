# Mediator

A lightweight, dependency-injection-friendly mediator implementation for .NET that provides request/response messaging between components.

This package offers a simple alternative to MediatR for projects that need a minimal mediator abstraction without extra features.

## Key features

- Minimal, easy-to-understand API
- Works with Microsoft.Extensions.DependencyInjection
- Async request/response handlers
- CancellationToken support
- Clean exception propagation from handlers

## Why use this Mediator vs MediatR

- Smaller surface area and fewer dependencies — ideal when you only need the basic request/response pattern.
- Simple implementation that is easy to inspect, customize, and extend.
- If you require advanced features like pipelines, notifications, or rich behaviors, prefer MediatR. This package is designed for straightforward mediator scenarios.

## Installation

Add the project to your solution or reference the compiled assembly. Example (when packaged as a NuGet package):

```bash
dotnet add package Mediator
```

Register your handlers with Microsoft.Extensions.DependencyInjection (Transient/Scoped/Singleton as appropriate):

```csharp
var services = new ServiceCollection();
services.AddScoped<IRequestHandler<MyRequest, MyResponse>, MyRequestHandler>();
var provider = services.BuildServiceProvider();
var mediator = new Mediator.Mediator(provider);
```

## Core API - Syntaxes

Interfaces and important signatures used by the package:

- Request interface (marker with response type):

```csharp
public interface IRequest<TResponse> { }
```

- Request handler interface:

```csharp
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
```

- Mediator Send method (async):

```csharp
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
```

Concrete mediator exposes the same signature:

```csharp
var result = await mediator.Send<MyResponse>(myRequest);
```

## Usage examples

1) Basic request/handler example

```csharp
// Request
public class HelloRequest : IRequest<string>
{
    public string Name { get; set; }
}

// Handler
public class HelloHandler : IRequestHandler<HelloRequest, string>
{
    public Task<string> Handle(HelloRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Hello, {request.Name}!");
    }
}

// Register and send
var services = new ServiceCollection();
services.AddScoped<IRequestHandler<HelloRequest, string>, HelloHandler>();
var provider = services.BuildServiceProvider();
var mediator = new Mediator.Mediator(provider);

var response = await mediator.Send<string>(new HelloRequest { Name = "Alex" });
// response == "Hello, Alex!"
```

2) Request returning primitive types

```csharp
public class DoubleRequest : IRequest<int>
{
    public int Value { get; set; }
}

public class DoubleHandler : IRequestHandler<DoubleRequest, int>
{
    public Task<int> Handle(DoubleRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Value * 2);
    }
}
```

3) Complex response object

```csharp
public class PersonRequest : IRequest<PersonResponse> { public int Id { get; set; } }
public class PersonResponse { public string Name { get; set; } public int Age { get; set; } }

public class PersonHandler : IRequestHandler<PersonRequest, PersonResponse>
{
    public Task<PersonResponse> Handle(PersonRequest request, CancellationToken cancellationToken)
    {
        // fetch data, build response
        return Task.FromResult(new PersonResponse { Name = "Jane", Age = 28 });
    }
}
```

4) Cancellation support

The mediator forwards the CancellationToken to the handler. Handlers that call cancellable APIs (e.g., Task.Delay, HttpClient) should accept and pass the token.

```csharp
public class LongRunningHandler : IRequestHandler<MyRequest, string>
{
    public async Task<string> Handle(MyRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        return "done";
    }
}

// Usage
var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
await Assert.ThrowsAsync<TaskCanceledException>(() => mediator.Send<string>(new MyRequest(), cts.Token));
```

Note: Task.Delay throws TaskCanceledException when cancelled; tests should assert the concrete exception type or handle OperationCanceledException accordingly.

5) Exception propagation

When a handler throws an exception, the mediator will propagate the original exception (not a reflection wrapper). For best behavior, throw meaningful exceptions from handlers:

```csharp
public class FailingHandler : IRequestHandler<MyRequest, string>
{
    public Task<string> Handle(MyRequest request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Handler failed");
    }
}

// Sending will surface InvalidOperationException
await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send<string>(new MyRequest()));
```

## Registration patterns

- Register many handlers in a module or startup:

```csharp
services.AddScoped<IRequestHandler<ARequest, AResponse>, AHandler>();
services.AddScoped<IRequestHandler<BRequest, BResponse>, BHandler>();
```

- Use assembly scanning in your DI setup (custom helper) to register all handlers automatically.

## Best practices

- Keep handlers focused and single-responsibility.
- Avoid long-running synchronous work in handlers; use async APIs.
- Respect the CancellationToken in handlers to support cooperative cancellation.
- Throw meaningful exceptions and use domain-specific types for richer error handling.

## Extending the mediator

This implementation is intentionally small. If needed, add:

- Pipeline behaviors (pre/post processing)
- Notification/Publish/Subscribe support
- Request validation or retry behaviors

All extensions can be implemented externally and composed into the DI registration.

## Contributing

Contributions, bug reports and feature requests are welcome. Prefer small, focused pull requests.

---

This README provides a practical reference for using the Mediator package as a compact alternative to MediatR in .NET projects.