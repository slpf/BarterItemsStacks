using BarterItemsStacks.Web.Models;
using Microsoft.AspNetCore.Components;

namespace BarterItemsStacks.Web.Components;

public partial class ItemRow : ComponentBase
{
    [Parameter, EditorRequired] public ConfigItemRow Item { get; set; } = default!;
    [Parameter] public bool Highlight { get; set; }
    
    [Parameter] public Func<string, string>? ItemImageSrc { get; set; }
    
    [Parameter] public EventCallback<string> OnDelete { get; set; }
    
    private string SafeImageSrc(string tplId) => ItemImageSrc?.Invoke(tplId) ?? "";
}