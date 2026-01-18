using BarterItemsStacks.Web.Config;
using BarterItemsStacks.Web.Models;

namespace BarterItemsStacks.Web.Services;

public static class ViewBuilder
{
    public static List<CategoryGroup> Build(IEnumerable<ConfigItemRow> items, string otherCategoryName)
    {
        var list = items.ToList();
        
        var byCategory = list
            .GroupBy(i => CategoriesNames.Get(i.Parent, otherCategoryName), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var result = new List<CategoryGroup>();
        
        foreach (var cat in CategoriesNames.Categories)
        {
            if (!byCategory.TryGetValue(cat.Name, out var catItems) || catItems.Count == 0)
                continue;

            result.Add(new CategoryGroup(
                cat.Name,
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