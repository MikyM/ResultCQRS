# ResultCQRS

[![Build Status](https://github.com/MikyM/ResultCQRS/actions/workflows/release.yml/badge.svg)](https://github.com/MikyM/ResultCQRS/actions)

Library featuring CQRS pattern and mediator implementations in conjuction with functional Results to achieve exception-safe operations.

Uses a functional [Result](https://github.com/Remora/Remora.Results) approach for failure-prone operations.

To utilize all features using Autofac is required. 

## Features

- `ICommand` and `IQuery` abstractions
- `ICommandHandler` and `IQueryHandler` abstractions
- `ICommandDispatcher` and `IQueryDispatcher` abstractions and default implementations
- Supports decorators and adapters via Autofac's methods (for decorators with Microsoft's DI you may consider something like Scrutor)

## Description

For both commands and queries, there are two return options - one that only returns a [Result](https://github.com/Remora/Remora.Results) and one that returns an additional entity/DTO contained within the [Result](https://github.com/Remora/Remora.Results).

Every handler must return a [Result](https://github.com/Remora/Remora.Results) struct which determines whether the operation succedeed or not, handlers may or may not return additional results contained within the Result struct.

## Installation

To register the library with the DI container use the `ContainerBuilder` or `IServiceCollection` extension methods provided by the library:

```csharp
builder.AddResultCQRS(assembliesToScan);
```

To register decorators (or adapters) use:
1. the attributes provided by [AttributeBasedRegistration](https://github.com/MikyM/AttributeBasedRegistration)
2. or directly through Autofac, Scrutor or other libraries providing DI decoration support.

If you're using Autofac (or built-in attribute based Autofac) - you can register multiple decorators and they'll be applied in the order that you register them - read more at [Autofac's docs regarding decorators and adapters](https://autofac.readthedocs.io/en/latest/advanced/adapters-decorators.html).

## Documentation

Documentation available at https://docs.result-cqrs.mikym.me/.

## Example usage

Library offers a simple error catching and logging within the provided default `Dispatcher` implementations.

A command without a concrete result:
```csharp
public record SimpleCommand : ICommand
{
    public bool IsSuccess { get; }
    
    public SimpleCommand(bool isSuccess = true)
        => IsSuccess = isSuccess;
}
```

And a handler that handles it:
```csharp
public class SimpleCommandHandler : ICommandHandler<SimpleCommand>
{
    public async Task<Result> HandleAsync(SimpleCommand command)
    {
        if (command.IsSuccess)
            return Result.FromSuccess();
            
        return new InvalidOperationError();
    }
}
```

A command with a concrete result:
```csharp
public record SimpleCommandWithConcreteResult : ICommand<int>
{
    public bool IsSuccess { get; }
    
    public SimpleCommandWithConcreteResult(bool isSuccess = true)
        => IsSuccess = isSuccess;
}
```

And a handler that handles it:
```csharp
public class SimpleCommandHandlerWithConcreteResult : ICommandHandler<SimpleCommandWithConcreteResult, int>
{
    public async Task<Result<int>> HandleAsync(SimpleCommandWithConcreteResult command)
    {
        if (command.IsSuccess)
            return 1;
            
        return new InvalidOperationError();
    }
}
```

Then in your service you can use:

```csharp
public class Service : IService
{
    private readonly ICommandDispatcher _commandDispatcher;

    public Service(ICommandDispatcher commandDispatcher)
        => _commandDispatcher = commandDispatcher;

    public async Task<Result> DoAsync()
    {
        var result = await _commandDispatcher.DispatchAsync(command)
            
        return new InvalidOperationError();
    }
}
```


Using queries is pretty much same.