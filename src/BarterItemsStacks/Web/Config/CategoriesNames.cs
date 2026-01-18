using System.Collections.Immutable;

namespace BarterItemsStacks.Web.Config;

public static class CategoriesNames
{
    public static ImmutableArray<Category> Categories { get; } =
    [
        new("Cash", new[]
        {
            "543be5dd4bdc2deb348b4569"
        }),
        
        new("Barter items", new[]
        {
            "57864a3d24597754843f8721",
            "57864a66245977548f04a81f",
            "57864ada245977548638de91",
            "57864bb7245977548b3b66c2",
            "57864c322459775490116fbf",
            "57864c8c245977548867e7f1",
            "57864e4c24597754843f8723",
            "57864ee62459775490116fc1",
            "590c745b86f7743cc433c5f2",
            "5c99f98d86f7745c314214b3",
            "5d650c3e815116009f6201d2",
            "6759673c76e93d8eb20b2080"
        }),
        
        new("Food", new[]
        {
            "5448e8d04bdc2ddf718b4569"
        }),
        
        new("Drinks", new[]
        {
            "5448e8d64bdc2dce718b4568"
        }),
        
        new("Medication", new[]
        {
            "5448f39d4bdc2d0a728b4568",
            "5448f3a14bdc2d27728b4569",
            "5448f3ac4bdc2dce718b4569",
            "5448f3a64bdc2d60728b456a"
        }),
        
        new("Repair kits", new[]
        {
            "616eb7aea207f41933308f46"
        }),
        
        new("Info Items", new[]
        {
            "5448ecbe4bdc2d60728b4568"
        }),
        
        new("Ammo Packs", new[]
        {
            "543be5cb4bdc2deb348b4568"
        }),
        
        new("Special Items", new[]
        {
            "5447e0e74bdc2d3c308b4567",
            "5f4fbaaca5573a5ac31db429",
            "61605ddea09d851a0a0c1bbc",
            "6672e40ebb23210ae87d39eb",
            "66abb0743f4d8b145b1612c1",
            "62e9103049c018f425059f38"
        })
    ];
    
    private static readonly ImmutableDictionary<string, string> ParentToCategory =
        Categories
            .SelectMany(c => c.ParentIds.Select(p => (ParentId: p, Category: c.Name)))
            .GroupBy(x => x.ParentId, StringComparer.Ordinal)
            .ToImmutableDictionary(
                g => g.Key,
                g => g.First().Category,
                StringComparer.Ordinal
            );
    
    public static string Get(string? parentId, string fallback)
    {
        if (string.IsNullOrWhiteSpace(parentId))
            return fallback;

        return ParentToCategory.TryGetValue(parentId, out var category)
            ? category
            : fallback;
    }
    
    public sealed record Category(string Name, IReadOnlyList<string> ParentIds);
}