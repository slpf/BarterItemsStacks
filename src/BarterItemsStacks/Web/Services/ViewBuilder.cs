using BarterItemsStacks.Web.Config;
using BarterItemsStacks.Web.Models;

namespace BarterItemsStacks.Web.Services;

public static class ViewBuilder
{
    public static List<CategoryGroup> Build(IEnumerable<ConfigItemRow> items, string otherCategoryName, CategoryResolver resolver)
    {
        var list = items.ToList();

        var byCategory = list
            .GroupBy(i => resolver.Resolve(i.TemplateId, i.Parent, otherCategoryName), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var result = new List<CategoryGroup>();

        foreach (var catName in resolver.OrderedNames)
        {
            if (!byCategory.TryGetValue(catName, out var catItems) || catItems.Count == 0)
                continue;

            result.Add(new CategoryGroup(
                catName,
                catItems
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.TemplateId, StringComparer.Ordinal)
                    .ToList()
            ));
        }
        
        if (byCategory.TryGetValue(otherCategoryName, out var otherItems) && otherItems.Count > 0)
        {
            result.Add(new CategoryGroup(
                otherCategoryName,
                otherItems
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.TemplateId, StringComparer.Ordinal)
                    .ToList()
            ));
        }

        return result;
    }
}