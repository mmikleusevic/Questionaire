using Microsoft.JSInterop;

namespace Web.Services;

public class JavaScriptService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new Lazy<Task<IJSObjectReference>>(() => 
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/customModule.js").AsTask());

    public async ValueTask InvokeVoidAsync(string identifier, params object[] args)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync(identifier, args);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}