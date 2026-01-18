using BarterItemsStacks.Web.Models;
using BarterItemsStacks.Web.Config;
using SPTarkov.Server.Core.Servers;

namespace BarterItemsStacks.Web.Services;

public sealed class ItemsDbIndex
{
    private readonly List<ItemSearchEntry> _index;
    private readonly Dictionary<string, ItemSearchEntry> _byId;

    public ItemsDbIndex(
        DatabaseServer databaseServer,
        string otherCategoryName,
        IReadOnlyDictionary<string, string> localeLocalized,
        IReadOnlyDictionary<string, string> localeEn)
    {
        var itemsDb = databaseServer.GetTables().Templates.Items;

        _index = new List<ItemSearchEntry>(itemsDb.Count);

        foreach (var kvp in itemsDb)
        {
            var tplId = kvp.Key.ToString();
            var tpl = kvp.Value;

            var isParentNode = string.Equals(tpl.Type, "Node", StringComparison.OrdinalIgnoreCase);

            var parent = tpl.Parent.ToString();
            
            parent = string.IsNullOrWhiteSpace(parent) || parent.Equals("null", StringComparison.OrdinalIgnoreCase) 
                ? string.Empty : parent;
            
            string displayName;
            string englishName;
            string category;

            if (isParentNode)
            {
                displayName = ParentNames.TryGet(tplId, out var pn) ? pn : "";
                englishName = displayName;
                category = "Parent";
            }
            else
            {
                displayName = GetName(localeLocalized, tplId, tpl.Properties?.Name);
                englishName = GetName(localeEn, tplId, tpl.Properties?.Name);
                category = CategoriesNames.Get(parent, otherCategoryName);
            }

            _index.Add(new ItemSearchEntry(tplId, displayName, englishName, parent, category));
        }

        _byId = _index.ToDictionary(x => x.TemplateId, x => x, StringComparer.Ordinal);
    }

    public List<Suggestion> Search(string query, HashSet<string> inConfig, int limit)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length == 0)
        {
            return new List<Suggestion>();
        }
        
        var matches = new List<(ItemSearchEntry entry, string sortKey)>(limit * 2);
        
        foreach (var x in _index)
        {
            if (x.TemplateId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (!string.IsNullOrWhiteSpace(x.DisplayName) &&
                 x.DisplayName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrWhiteSpace(x.EnglishName) &&
                 x.EnglishName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var sortKey = string.IsNullOrWhiteSpace(x.DisplayName) ? x.TemplateId : x.DisplayName;
                matches.Add((x, sortKey));
                
                if (matches.Count > limit * 10) break;
            }
        }

        return matches
            .OrderBy(m => m.sortKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(m => m.entry.TemplateId, StringComparer.Ordinal)
            .Take(limit)
            .Select(m => new Suggestion(
                m.entry.TemplateId,
                m.entry.DisplayName,
                m.entry.Category,
                inConfig.Contains(m.entry.TemplateId)))
            .ToList();
    }
    
    private sealed record ItemSearchEntry(
        string TemplateId,
        string DisplayName,
        string EnglishName,
        string Parent,
        string Category
    );
    
    public bool TryGet(string tplId, out ItemDbEntry entry)
    {
        if (_byId.TryGetValue(tplId, out var x))
        {
            entry = new ItemDbEntry(x.TemplateId, x.DisplayName, x.Parent, x.Category);
            return true;
        }

        entry = default!;
        return false;
    }
    
    private static string GetName(IReadOnlyDictionary<string, string> dict, string tplId, string? fallback)
    {
        if (dict.TryGetValue($"{tplId} Name", out var n) && !string.IsNullOrWhiteSpace(n) &&
            !string.Equals(n, "null", StringComparison.OrdinalIgnoreCase))
        {
            return n;
        }
        
        if (string.IsNullOrWhiteSpace(fallback) || string.Equals(fallback, "null", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return fallback;
    }
}