namespace ServiceRegistrationGenerator.ExampleLibrary;

#pragma warning disable CA1822
public sealed class FooService
{
    public void Execute()
    {
    }
}

public interface IBarService
{
    public void Execute();
}

public sealed class BarService : IBarService
{
    public void Execute()
    {
    }
}

public interface IMixedService1
{
    public void Execute1();
}

public interface IMixedService2
{
    public void Execute2();
}

public sealed class MixedService : IMixedService1, IMixedService2
{
    public void Execute1()
    {
    }

    public void Execute2()
    {
    }
}

public sealed class DisposalService : IDisposable
{
    public void Execute()
    {
    }

    public void Dispose()
    {
    }
}

public interface IBazService
{
    public void Execute();
}

public sealed class DisposalBazService : IBazService, IDisposable
{
    public void Execute()
    {
    }

    public void Dispose()
    {
    }
}
