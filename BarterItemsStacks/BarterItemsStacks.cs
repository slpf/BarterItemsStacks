using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BarterItemsStacks;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.slpf.barteritemsstacks";
    public override string Name { get; init; } = "Barter Items Stacks";
    public override string Author { get; init; } = "SLPF";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.2.1");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "MIT";
}

public class ItemsConfig
{
    public const string KeycardParent = "5c164d2286f774194c5e69fa";
    public const string KeyParent = "5c99f98d86f7745c314214b3";

    public const string FileName = "config.jsonc";

    public Dictionary<string, ItemRule> Items { get; set; } = new();

    public sealed class ItemRule
    {
        [JsonInclude]
        private int? StackSize;

        [JsonInclude]
        private int? MaxResource;

        [JsonIgnore]
        public int Stack => Gt0(StackSize ?? 0);

        [JsonIgnore]
        public int Resource => Gt0(MaxResource ?? 0);

        private static int Gt0(int v) => v < 0 ? 0 : v;

        private static int Clamp(int v, int min, int max)
            => v < min ? min : (v > max ? max : v);
    }
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 50000)]
public class BarterItemsStacks(ModHelper modHelper, DatabaseServer databaseServer, JsonUtil jsonUtil, ConfigReload configReload, ISptLogger<BarterItemsStacks> logger) : IOnLoad
{
    public const string RofsRouter = "RemoveOneFromStack";

    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        LoadConfig(pathToMod);

        configReload.Start(pathToMod, ItemsConfig.FileName, () => { return Task.FromResult(LoadConfig(pathToMod)); });

        logger.LogWithColor("[BarterItemsStacks] Config loaded.", LogTextColor.Green, LogBackgroundColor.Black);

        BaseInteractionRequestDataConverter.RegisterModDataHandler(RofsRouter, jsonUtil.Deserialize<RemoveOneFromStack.RemoveOneFromStackModel>);

        return Task.CompletedTask;
    }

    private bool LoadConfig(string pathToMod)
    {
        try
        {
            var itemsDb = databaseServer.GetTables().Templates.Items;
            var config = modHelper.GetJsonDataFromFile<ItemsConfig>(pathToMod, ItemsConfig.FileName);

            foreach (var item in config.Items)
            {
                if (itemsDb.TryGetValue(item.Key, out TemplateItem template))
                {
                    var parent = template.Parent;

                    if (parent != ItemsConfig.KeycardParent && parent != ItemsConfig.KeyParent)
                    {
                        var stack = item.Value.Stack;
                        var resource = item.Value.Resource;

                        var props = template.Properties;

                        if (props != null)
                        {
                            if (stack > 0)
                            {
                                props.StackMaxSize = stack;
                            }

                            if (resource > 0)
                            {
                                if (props.MaxResource.HasValue)
                                {
                                    props.MaxResource = resource;
                                }
                                else if (props.MaxHpResource.HasValue)
                                {
                                    props.MaxHpResource = resource == 1 ? 0 : resource;
                                }
                                else if (props.MaxRepairResource.HasValue)
                                {
                                    props.MaxRepairResource = resource;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[BarterItemsStacks] Loading Error >> {ex.Message}", LogTextColor.White, LogBackgroundColor.Red);
            return false;
        }
    }
}