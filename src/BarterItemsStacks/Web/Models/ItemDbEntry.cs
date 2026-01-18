namespace BarterItemsStacks.Web.Models;

public sealed class ItemDbEntry
{
    public string TemplateId { get; }
    public string Name { get; }
    public string Parent { get; }
    public string Category { get; }

    public ItemDbEntry(string templateId, string name, string parent, string category)
    {
        TemplateId = templateId;
        Name = name;
        Parent = parent;
        Category = category;
    }
}
