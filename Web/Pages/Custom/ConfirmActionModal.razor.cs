using BlazorBootstrap;
using Microsoft.AspNetCore.Components;

namespace Web.Pages.Custom;

public partial class ConfirmActionModal : ComponentBase
{
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public object? Entity { get; set; }
    [Parameter] public string Name { get; set; }
    [Parameter] public string ActionText { get; set; }
    [Parameter] public Func<Task>? Action { get; set; }
    [Parameter] public EventCallback OnEntityChanged { get; set; }

    private async Task HandleValidSubmit()
    {
        if (Entity == null || Action == null) return;

        await Action.Invoke();
        await OnEntityChanged.InvokeAsync(Entity);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}