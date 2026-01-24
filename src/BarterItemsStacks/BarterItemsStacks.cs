using System.Reflection;
using System.Text.Json.Serialization;
using BarterItemsStack;
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
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;
using SPTarkov.Server.Web;

[assembly: AssemblyProduct(ModInfo.Name)]
[assembly: AssemblyTitle(ModInfo.Name)]
[assembly: AssemblyDescription(ModInfo.Description)]
[assembly: AssemblyCopyright(ModInfo.Copyright)]
[assembly: AssemblyVersion(ModInfo.Version)]
[assembly: AssemblyFileVersion(ModInfo.Version)]
[assembly: AssemblyInformationalVersion(ModInfo.Version)]

namespace BarterItemsStacks;

public record ModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = ModInfo.Guid;
    public override string Name { get; init; } = ModInfo.Name;
    public override string Author { get; init; } = ModInfo.Author;
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new(ModInfo.Version);
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = ModInfo.License;
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
        
        [JsonInclude]
        private int? ItemHeight;

        [JsonInclude]
        private int? ItemWidth;
        
        [JsonInclude]
        private double? WeightMultiplier;
        
        [JsonInclude]
        private double? PriceMultiplier;

        [JsonIgnore]
        public int Stack => Gt0(StackSize ?? 0);

        [JsonIgnore]
        public int Resource => Gt0(MaxResource ?? 0);
        
        [JsonIgnore]
        public int Height => Gt0(ItemHeight ?? 0);

        [JsonIgnore]
        public int Width => Gt0(ItemWidth ?? 0);
        
        [JsonIgnore]
        public double Weight => Gt0(WeightMultiplier ?? 0);
        
        [JsonIgnore]
        public double Price => Gt0(PriceMultiplier ?? 0);

        private static int Gt0(int v) => v < 0 ? 0 : v;
        
        private static double Gt0(double v) => v < 0 ? 0 : v;

        private static int Clamp(int v, int min, int max)
            => v < min ? min : (v > max ? max : v);
    }
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 50000)]
public class BarterItemsStacks(ModHelper modHelper, DatabaseServer databaseServer, JsonUtil jsonUtil, ConfigReload configReload, DatabaseService databaseService, ISptLogger<BarterItemsStacks> logger) : IOnLoad
{
    public const string RofsRouter = "RemoveOneFromStack";
    private readonly record struct DefaultProps(int? StackMaxSize, int? MaxResource, int? MaxHpResource, int? MaxRepairResource, int? Height, int? Width, double? Weight, double? Price);
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
            var handbook = databaseService.GetHandbook();
            
            foreach (var tplId in _lastApplied)
            {
                if (itemsDb.TryGetValue(tplId, out TemplateItem template))
                {
                    var props = template.Properties;
                    
                    if (props != null && _defaults.TryGetValue(tplId, out var def))
                    {
                        props.StackMaxSize = def.StackMaxSize;
                        props.MaxResource = def.MaxResource;
                        props.MaxHpResource = def.MaxHpResource;
                        props.MaxRepairResource = def.MaxRepairResource;
                        props.Height = def.Height;
                        props.Width = def.Width;
                        props.Weight = def.Weight;
                        
                        var handbookItem = handbook.Items.FirstOrDefault(x => x.Id == tplId);
                        if (handbookItem != null) handbookItem.Price = def.Price;
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
                    var handbookItem = handbook.Items.FirstOrDefault(x => x.Id == item.Key);
                    ProcessTemplate(item.Key, item.Value, template, handbookItem);
                }
                else
                {
                    // We only support one level of nestedness, it's up to
                    // the user to give us correct immediate parent
                    var children = itemsDb.OfClass(x => x.Type != "Node", item.Key);
                    foreach (var child in children)
                    {
                        var handbookItem = handbook.Items.FirstOrDefault(x => x.Id == child.Id);
                        ProcessTemplate(child.Id, item.Value, child, handbookItem);
                    }
                    
                    // Find all descendants considering all nesting levels
                    // var children = GetAllChildren(itemsDb, item.Key);
                    // foreach (var childId in children)
                    // {
                    //     if (itemsDb.TryGetValue(childId, out var child))
                    //     {
                    //         ProcessTemplate(childId, item.Value, child);
                    //     }
                    // }
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
    
    private HashSet<string> GetAllChildren(Dictionary<MongoId, TemplateItem> itemsDb, string parentId)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        queue.Enqueue(parentId);

        while (queue.Count > 0)
        {
            var currentParent = queue.Dequeue();
            
            var directChildren = itemsDb
                .Where(kvp => kvp.Value.Parent.ToString() == currentParent)
                .Select(kvp => kvp.Key.ToString())
                .ToList();

            foreach (var childId in directChildren)
            {
                if (itemsDb.TryGetValue(childId, out var childTemplate))
                {
                    if (childTemplate.Type == "Node")
                    {
                        queue.Enqueue(childId);
                    }
                    else
                    {
                        result.Add(childId);
                    }
                }
            }
        }

        return result;
    }

    void ProcessTemplate(MongoId tplId, ItemsConfig.ItemRule itemRule, TemplateItem template, HandbookItem? handbookItem)
    {
        var parent = template.Parent;

        if (parent == BaseClasses.KEYCARD || parent == BaseClasses.KEY_MECHANICAL)
            return;

        var stack = itemRule.Stack;
        var resource = itemRule.Resource;
        var height = itemRule.Height;
        var width = itemRule.Width;
        var weight = itemRule.Weight;
        var price = itemRule.Price;

        var props = template.Properties;

        if (props != null)
        {
            if (!_defaults.ContainsKey(tplId))
            {
                _defaults[tplId] = new DefaultProps(
                    props.StackMaxSize,
                    props.MaxResource,
                    props.MaxHpResource,
                    props.MaxRepairResource,
                    props.Height,
                    props.Width,
                    props.Weight,
                    handbookItem?.Price
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

            if (height > 0)
            {
                props.Height = height;
                changed = true;
            }

            if (width > 0)
            {
                props.Width = width;
                changed = true;
            }
            
            if (weight > 0)
            {
                var def = _defaults[tplId];
                props.Weight = (def.Weight ?? props.Weight) * weight;
                changed = true;
            }
            
            // Hot reload not working with handbook
            if (price > 0)
            {
                var def = _defaults[tplId];

                if (handbookItem != null) handbookItem.Price = def.Price * price;
                
                changed = true;
            }
            
            if (changed)
            {
                _lastApplied.Add(tplId);
            }
        }
    }
}