using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NicolaParo.BlazorMes.ManagerApp.Services
{
    public class BrowserInterop
    {
        private readonly IJSRuntime jsRuntime;
        private readonly NavigationManager navigationManager;

        public BrowserInterop(IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            this.jsRuntime = jsRuntime;
            this.navigationManager = navigationManager;
        }

        public ValueTask NavigateBackAsync()
        {
            return jsRuntime.InvokeVoidAsync("history.back");
        }
        public ValueTask NavigateToAsync(string uri, bool forceLoad = false, bool replace = false)
        {
            navigationManager.NavigateTo(uri, forceLoad, replace);
            return ValueTask.CompletedTask;
        }

    }
}
