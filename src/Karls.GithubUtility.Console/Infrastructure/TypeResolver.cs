using Spectre.Console.Cli;

namespace Karls.GithubUtility.Console.Infrastructure;

// Code taken from https://github.com/spectreconsole/spectre.console/tree/main/examples/Cli/Injection
public sealed class TypeResolver : ITypeResolver, IDisposable {
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider) {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type) {
        if(type == null) {
            return null;
        }

        try {
            return _provider.GetService(type);
        } catch(Exception ex) {
            System.Console.WriteLine($"Failed to resolve type {type.Name}: {ex.Message}");
            return null;
        }
    }

    public void Dispose() {
        if(_provider is IDisposable disposable) {
            disposable.Dispose();
        }
    }
}
