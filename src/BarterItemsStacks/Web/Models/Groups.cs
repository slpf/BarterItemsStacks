namespace BarterItemsStacks.Web.Models;

public sealed class CategoryGroup
{
    public string Name { get; }
    public List<ConfigItemRow> Items { get; }

    public CategoryGroup(string name, List<ConfigItemRow> items)
    {
        Name = name;
        Items = items;
    }

    public int TotalItems => Items.Count;
}