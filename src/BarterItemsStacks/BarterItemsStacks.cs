using System.Reflection;
using BarterItemsStack;
using BarterItemsStacks.Configs;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;
using SPTarkov.Server.Web;
using Path = System.IO.Path;

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

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 50000)]
public class BarterItemsStacks(ModHelper modHelper, DatabaseServer databaseServer, JsonUtil jsonUtil, ConfigReload configReload, DatabaseService databaseService, ISptLogger<BarterItemsStacks> logger) : IOnLoad
{
    public const string RofsRouter = "RemoveOneFromStack";
    public const string PotRouter = "PlantOneTripwire";
    private readonly record struct DefaultProps(int? StackMaxSize, int? MaxResource, int? MaxHpResource, int? MaxRepairResource, int? Height, int? Width, double? Weight, double? Price);
    private readonly Dictionary<string, DefaultProps> _defaultTemplates = new(StringComparer.Ordinal);
    private readonly Dictionary<string, XYZ> _defaultWeights = new(StringComparer.Ordinal);
    private readonly HashSet<string> _lastApplied = new(StringComparer.Ordinal);

    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        if (LoadItemsConfig(pathToMod))
        {
            logger.LogWithColor("[BarterItemsStacks] Config loaded.", LogTextColor.Green, LogBackgroundColor.Black);
        }

        LoadParamsConfig(pathToMod);

        configReload.Start(pathToMod, ItemsConfig.FileName, () => { return Task.FromResult(LoadItemsConfig(pathToMod)); });
        configReload.Start(pathToMod, ParamsConfig.FileName, () => { return Task.FromResult(LoadParamsConfig(pathToMod)); });

        BaseInteractionRequestDataConverter.RegisterModDataHandler(RofsRouter, jsonUtil.Deserialize<RemoveOneFromStack.RemoveOneFromStackModel>);
        BaseInteractionRequestDataConverter.RegisterModDataHandler(PotRouter, jsonUtil.Deserialize<PlantOneTripwire.PlantOneTripwireModel>);

        return Task.CompletedTask;
    }

    private bool LoadParamsConfig(string pathToMod)
    {
        try
        {
            var configPath = Path.Combine(pathToMod, ParamsConfig.FileName);

            if (!File.Exists(configPath))
            {
                DefaultConfigs.CreateParamsConfig(configPath);
            }

            var config = modHelper.GetJsonDataFromFile<ParamsConfig>(pathToMod, ParamsConfig.FileName);
            var mult = config.WeightMultiplier;

            var configuration = databaseServer.GetTables().Globals.Configuration;
            var stamina = configuration.Stamina;
            var inertia = configuration.Inertia;

            if (_defaultWeights.Count == 0)
            {
                _defaultWeights["BaseOverweight"] = stamina.BaseOverweightLimits;
                _defaultWeights["SprintOverweight"] = stamina.SprintOverweightLimits;
                _defaultWeights["WalkOverweight"] = stamina.WalkOverweightLimits;
                _defaultWeights["WalkSpeedOverweight"] = stamina.WalkSpeedOverweightLimits;
                _defaultWeights["Inertia"] = inertia.InertiaLimits;
            }

            stamina.BaseOverweightLimits = _defaultWeights["BaseOverweight"] with
            {
                X = _defaultWeights["BaseOverweight"].X * mult,
                Y = _defaultWeights["BaseOverweight"].Y * mult
            };

            stamina.SprintOverweightLimits = _defaultWeights["SprintOverweight"] with
            {
                X = _defaultWeights["SprintOverweight"].X * mult,
                Y = _defaultWeights["SprintOverweight"].Y * mult
            };

            stamina.WalkOverweightLimits = _defaultWeights["WalkOverweight"] with
            {
                X = _defaultWeights["WalkOverweight"].X * mult,
                Y = _defaultWeights["WalkOverweight"].Y * mult
            };

            stamina.WalkSpeedOverweightLimits = _defaultWeights["WalkSpeedOverweight"] with
            {
                X = _defaultWeights["WalkSpeedOverweight"].X * mult,
                Y = _defaultWeights["WalkSpeedOverweight"].Y * mult
            };

            configuration.Inertia.InertiaLimits = _defaultWeights["Inertia"] with
            {
                Y = _defaultWeights["Inertia"].Y * mult
            };

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[BarterItemsStacks] Loading Error >> {ex.Message}", LogTextColor.White, LogBackgroundColor.Red);
            return false;
        }
    }

    private bool LoadItemsConfig(string pathToMod)
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
                    
                    if (props != null && _defaultTemplates.TryGetValue(tplId, out var def))
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

            var configPath = Path.Combine(pathToMod, ItemsConfig.FileName);
            if (!File.Exists(configPath))
            {
                DefaultConfigs.CreateItemsConfig(configPath);
                logger.LogWithColor("[BarterItemsStacks] Default config generated.", LogTextColor.Green, LogBackgroundColor.Black);
            }
            
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
            if (!_defaultTemplates.ContainsKey(tplId))
            {
                _defaultTemplates[tplId] = new DefaultProps(
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
                var def = _defaultTemplates[tplId];
                props.Weight = (def.Weight ?? props.Weight) * weight;
                changed = true;
            }
            
            // Hot reload not working with handbook
            if (price > 0)
            {
                var def = _defaultTemplates[tplId];

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