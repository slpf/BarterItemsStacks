namespace BarterItemsStacks.Web.Models;

public sealed class Suggestion
{
    public string TemplateId { get; }
    public string Name { get; }
    public string Category { get; }
    public bool InConfig { get; }

    public Suggestion(string templateId, string name, string category, bool inConfig)
    {
        TemplateId = templateId;
        Name = name;
        Category = category;
        InConfig = inConfig;
    }
}
