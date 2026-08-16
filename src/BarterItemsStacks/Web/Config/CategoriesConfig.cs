using System.Text.Json;

namespace BarterItemsStacks.Web.Config;

public class CategoriesConfig
{
    public const string FileName = "wc.json";

    public List<CategoryEntry> Categories { get; set; } = new();

    public sealed class CategoryEntry
    {
        public string Name { get; set; } = "";
        public List<string> ParentIds { get; set; } = new();
        public List<string> TemplateIds { get; set; } = new();
    }

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static CategoriesConfig Default()
    {
        return new CategoriesConfig
        {
            Categories = CategoriesNames.Categories
                .Select(c => new CategoryEntry
                {
                    Name = c.Name,
                    ParentIds = c.ParentIds.ToList(),
                    TemplateIds = new List<string>()
                })
                .ToList()
        };
    }

    public static string Serialize(CategoriesConfig config)
    {
        return JsonSerializer.Serialize(config, WriteOptions);
    }

    public static void EnsureExists(string pathToMod)
    {
        var path = Path.Combine(pathToMod, FileName);
        if (File.Exists(path))
        {
            return;
        }

        File.WriteAllText(path, Serialize(Default()));
    }
}
