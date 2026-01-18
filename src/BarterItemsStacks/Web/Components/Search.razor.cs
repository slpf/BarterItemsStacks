using BarterItemsStacks.Web.Models;
using BarterItemsStacks.Web.Services;
using Microsoft.AspNetCore.Components;

namespace BarterItemsStacks.Web.Components;

public partial class Search : ComponentBase
{
    private const int DefaultDebounceMs = 500;
    private const int DefaultBlurDelayMs = 180;
    
    [Parameter] public string Placeholder { get; set; } = "Search by id or name...";
    [Parameter] public Func<string, Task<List<Suggestion>>>? SearchFunc { get; set; }
    [Parameter] public EventCallback<string> OnSelect { get; set; }
    [Parameter] public Func<string, string>? ItemImageSrc { get; set; }
    [Parameter] public int DebounceMs { get; set; } = DefaultDebounceMs;
    [Parameter] public int BlurDelayMs { get; set; } = DefaultBlurDelayMs;
    [Parameter] public int RefreshToken { get; set; }
    
    private string _searchText = "";
    private bool _hasFocus;
    private bool _showSuggestions;
    private int _lastRefreshToken;
    
    private List<Suggestion> _suggestions = new();
    
    private readonly Debouncer _debouncer = new();

    protected override async Task OnParametersSetAsync()
    {
        if (RefreshToken != _lastRefreshToken)
        {
            _lastRefreshToken = RefreshToken;
            
            if (_hasFocus && !string.IsNullOrWhiteSpace(_searchText))
            {
                await RefreshSuggestionsAsync(showAfter: _showSuggestions);
            }
        }
    }

    private Task OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? "";
        _showSuggestions = false;
        _debouncer.Debounce(DebounceMs, ApplySearchAsync);
        
        return Task.CompletedTask;
    }

    private async Task OnSearchFocus()
    {
        _hasFocus = true;
        
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _showSuggestions = false;
            return;
        }
        
        await RefreshSuggestionsAsync(showAfter: true);
    }

    private async Task OnSearchBlur()
    {
        await Task.Delay(BlurDelayMs);
        
        _hasFocus = false;
        _showSuggestions = false;
        
        await InvokeAsync(StateHasChanged);
    }

    private async Task ApplySearchAsync()
    {
        if (SearchFunc is null)
        {
            return;
        }

        var q = (_searchText ?? "").Trim();

        if (string.IsNullOrWhiteSpace(q))
        {
            _suggestions = new List<Suggestion>();
            _showSuggestions = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        _suggestions = await SearchFunc(q) ?? new List<Suggestion>();
        _showSuggestions = _hasFocus;

        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshSuggestionsAsync(bool showAfter)
    {
        if (SearchFunc is null)
            return;

        var q = (_searchText ?? "").Trim();

        if (string.IsNullOrWhiteSpace(q))
        {
            _suggestions = new List<Suggestion>();
            _showSuggestions = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        _suggestions = await SearchFunc(q) ?? new List<Suggestion>();
        _showSuggestions = showAfter && _hasFocus;

        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectSuggestionAsync(string tplId)
    {
        await OnSelect.InvokeAsync(tplId);
        
        _showSuggestions = false;
        _searchText = "";
        _suggestions = new List<Suggestion>();

        await InvokeAsync(StateHasChanged);
    }

    private string SafeImageSrc(string tplId) => ItemImageSrc?.Invoke(tplId) ?? "";
}