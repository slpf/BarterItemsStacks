using System.Collections.Immutable;

namespace BarterItemsStacks.Web.Config;

public static class ParentNames
{
    private static readonly ImmutableDictionary<string, string> Map =
        new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "57864a66245977548f04a81f", "Electronics" },
                { "6759673c76e93d8eb20b2080", "Flyers"},
                { "57864e4c24597754843f8723", "Flammable materials"},
                { "590c745b86f7743cc433c5f2", "Others"},
                { "57864ee62459775490116fc1", "Batteries"},
                { "57864ada245977548638de91", "Building materials"},
                { "57864a3d24597754843f8721", "Jewelry"},
                { "57864c8c245977548867e7f1", "Medical supplies"},
                { "543be6674bdc2df1348b4569", "Food and Drinks"},
                { "5448e8d04bdc2ddf718b4569", "Food"},
                { "5448e8d64bdc2dce718b4568", "Drinks"},
                { "543be5cb4bdc2deb348b4568", "Ammo containers"},
                { "567849dd4bdc2d150f8b456e", "Maps"},
                { "5448f3a64bdc2d60728b456a", "Injectors"},
                { "5448f3ac4bdc2dce718b4569", "Injury treatment"},
                { "5448f3a14bdc2d27728b4569", "Drugs"},
                { "5448f39d4bdc2d0a728b4568", "Medkits"},
                { "543be5dd4bdc2deb348b4569", "Money"},
                { "5448ecbe4bdc2d60728b4568", "Info items"},
                { "5447e0e74bdc2d3c308b4567", "Special items"},
                { "66abb0743f4d8b145b1612c1", "Multitools"},
                { "616eb7aea207f41933308f46", "Repair kits"},
                { "57864bb7245977548b3b66c2", "Tools"}
            }
            .ToImmutableDictionary(StringComparer.Ordinal);

    public static bool TryGet(string tplId, out string name)
    {
        if (Map.TryGetValue(tplId, out name!))
        {
            name ??= string.Empty;
            return name.Length > 0;
        }

        name = string.Empty;
        return false;
    }
}