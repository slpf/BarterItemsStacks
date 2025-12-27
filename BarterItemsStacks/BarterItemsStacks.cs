using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace BarterItemsStacks;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.slpf.barteritemsstacks";
    public override string Name { get; init; } = "Barter Items Stacks";
    public override string Author { get; init; } = "SLPF";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.2.5");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "MIT";
}

public class ItemsConfig
{
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
    private readonly record struct DefaultProps(int? StackMaxSize, int? MaxResource, int? MaxHpResource, int? MaxRepairResource);
    private readonly Dictionary<string, DefaultProps> _defaults = new(StringComparer.Ordinal);
    private readonly HashSet<string> _lastApplied = new(StringComparer.Ordinal);

    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        if (LoadConfig(pathToMod))
        {
            logger.LogWithColor("[BarterItemsStacks] Config loaded.", LogTextColor.Green, LogBackgroundColor.Black);
        }

        configReload.Start(pathToMod, ItemsConfig.FileName, () => { return Task.FromResult(LoadConfig(pathToMod)); });

        BaseInteractionRequestDataConverter.RegisterModDataHandler(RofsRouter, jsonUtil.Deserialize<RemoveOneFromStack.RemoveOneFromStackModel>);

        return Task.CompletedTask;
    }

    private bool LoadConfig(string pathToMod)
    {
        try
        {
            var itemsDb = databaseServer.GetTables().Templates.Items;

            foreach (var tpl in _lastApplied)
            {
                if (itemsDb.TryGetValue(tpl, out TemplateItem template))
                {
                    var prev = template.Properties;
                    if (prev != null && _defaults.TryGetValue(tpl, out var def))
                    {
                        prev.StackMaxSize = def.StackMaxSize;
                        prev.MaxResource = def.MaxResource;
                        prev.MaxHpResource = def.MaxHpResource;
                        prev.MaxRepairResource = def.MaxRepairResource;
                    }
                }
            }
            _lastApplied.Clear();

            var config = modHelper.GetJsonDataFromFile<ItemsConfig>(pathToMod, ItemsConfig.FileName);

            foreach (var item in config.Items)
            {
                if (!itemsDb.TryGetValue(item.Key, out var template))
                    continue;

                if (template.Type != "Node")
                {
                    ProcessTemplate(
                        itemId: item.Key,
                        itemRule: item.Value,
                        template: template);
                }
                else
                {
                    // We only support one level of nestedness, it's up to
                    // the user to give us correct immediate parent

                    var children = itemsDb.OfClass(x => x.Type != "Node", item.Key);
                    foreach (var child in children)
                    {
                        ProcessTemplate(
                            itemId: child.Id,
                            itemRule: item.Value,
                            template: child);
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

    void ProcessTemplate(MongoId itemId, ItemsConfig.ItemRule itemRule, TemplateItem template)
    {
        var parent = template.Parent;

        if (parent == BaseClasses.KEYCARD || parent == BaseClasses.KEY_MECHANICAL)
            return;

        var stack = itemRule.Stack;
        var resource = itemRule.Resource;

        var props = template.Properties;

        if (props != null)
        {
            if (!_defaults.ContainsKey(itemId))
            {
                _defaults[itemId] = new DefaultProps(
                    props.StackMaxSize,
                    props.MaxResource,
                    props.MaxHpResource,
                    props.MaxRepairResource
                );
            }

            var changed = false;

            if (stack > 0)
            {
                props.StackMaxSize = stack;
                changed = true;
            }

            if (resource > 0)
            {
                if (props.MaxResource.HasValue)
                {
                    props.MaxResource = resource;
                    changed = true;
                }
                else if (props.MaxHpResource.HasValue)
                {
                    props.MaxHpResource = resource == 1 ? 0 : resource;
                    changed = true;
                }
                else if (props.MaxRepairResource.HasValue)
                {
                    props.MaxRepairResource = resource;
                    changed = true;
                }
            }

            if (changed)
            {
                _lastApplied.Add(itemId);
            }
        }
    }
}