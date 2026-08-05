namespace BarterItemsStacks.Web.Config;

public sealed class CategoryResolver
{
    private readonly Dictionary<string, string> _byTemplate;
    private readonly Dictionary<string, string> _byParent;
    private readonly List<string> _orderedNames;

    private CategoryResolver(
        Dictionary<string, string> byTemplate,
        Dictionary<string, string> byParent,
        List<string> orderedNames)
    {
        _byTemplate = byTemplate;
        _byParent = byParent;
        _orderedNames = orderedNames;
    }

    public IReadOnlyList<string> OrderedNames => _orderedNames;

    public static CategoryResolver Build(IReadOnlyList<CategoriesConfig.CategoryEntry> categories)
    {
        var byTemplate = new Dictionary<string, string>(StringComparer.Ordinal);
        var byParent = new Dictionary<string, string>(StringComparer.Ordinal);
        var orderedNames = new List<string>();

        foreach (var category in categories)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                continue;
            }

            orderedNames.Add(category.Name);

            foreach (var templateId in category.TemplateIds)
            {
                if (!string.IsNullOrWhiteSpace(templateId) && !byTemplate.ContainsKey(templateId))
                {
                    byTemplate[templateId] = category.Name;
                }
            }

            foreach (var parentId in category.ParentIds)
            {
                if (!string.IsNullOrWhiteSpace(parentId) && !byParent.ContainsKey(parentId))
                {
                    byParent[parentId] = category.Name;
                }
            }
        }

        return new CategoryResolver(byTemplate, byParent, orderedNames);
    }

    public string Resolve(string? templateId, string? parent, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(templateId) && _byTemplate.TryGetValue(templateId, out var byTemplate))
        {
            return byTemplate;
        }

        if (!string.IsNullOrWhiteSpace(parent) && _byParent.TryGetValue(parent, out var byParent))
        {
            return byParent;
        }

        return fallback;
    }
}
