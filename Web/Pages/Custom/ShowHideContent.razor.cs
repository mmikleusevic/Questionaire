using Microsoft.AspNetCore.Components;

namespace Web.Pages.Custom;

public partial class ShowHideContent : ComponentBase
{
    private string currentButtonText = "";
    private bool internalVisibleState;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string ButtonClass { get; set; } = "show-hide-button";
    [Parameter] public string ShowText { get; set; } = "Show Details";
    [Parameter] public string HideText { get; set; } = "Hide Details";

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter] public bool InitiallyHidden { get; set; } = true;
    [Parameter] public bool ApplyVisibilityCss { get; set; } = true;
    [Parameter] public string ContentAreaBaseClass { get; set; } = "show-hide-content-area";

    private string ContentContainerClass =>
        $"{ContentAreaBaseClass} {(ApplyVisibilityCss ? internalVisibleState ? "content-visible" : "content-hidden" : "")}"
            .Trim();

    protected override void OnInitialized()
    {
        internalVisibleState = IsVisibleChanged.HasDelegate ? IsVisible : !InitiallyHidden;
        SetButtonText();
    }

    protected override void OnParametersSet()
    {
        if (!IsVisibleChanged.HasDelegate || internalVisibleState == IsVisible) return;

        internalVisibleState = IsVisible;
        SetButtonText();
    }

    private async Task ToggleVisibility()
    {
        internalVisibleState = !internalVisibleState;
        SetButtonText();

        await IsVisibleChanged.InvokeAsync(internalVisibleState);
    }

    private void SetButtonText()
    {
        currentButtonText = internalVisibleState ? HideText : ShowText;
    }
}