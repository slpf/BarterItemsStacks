using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Reflection;
using System.Text.Json;

namespace SLPF.BarterItemsStacks;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.slpf.barteritemsstacks";
    public override string Name { get; init; } = "Barter Items Stacks";
    public override string Author { get; init; } = "SLPF";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "MIT";
}

public class BarterItemsConfig
{
    public Dictionary<string, int> BarterItems { get; set; } = new();
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class AfterDBLoadHook(
    ModHelper modHelper,
    DatabaseServer databaseServer,
    ISptLogger<AfterDBLoadHook> logger) : IOnLoad
{
    public Task OnLoad()
    {
        try
        {
            var itemsDb = databaseServer.GetTables().Templates.Items;
            var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            var barterItemsConfig = modHelper.GetJsonDataFromFile<BarterItemsConfig>(pathToMod, "items.jsonc");

            foreach (var item in barterItemsConfig.BarterItems)
            {
                if (itemsDb.TryGetValue(item.Key, out TemplateItem template))
                {
                    template.Properties.StackMaxSize = item.Value < 1 ? 1 : item.Value;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"BarterItemsConfig >> {ex.Message}", LogTextColor.White, LogBackgroundColor.Red);
        }

        return Task.CompletedTask;
    }
}