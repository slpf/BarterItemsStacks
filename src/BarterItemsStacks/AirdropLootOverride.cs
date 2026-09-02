using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Services.InRaid;

namespace BarterItemsStacks;

[Injectable(InjectionType.Singleton)]
public sealed class StackGenerationPolicy
{
    private volatile MongoId[] _tplIds = [];

    public MongoId[] GetTplIds()
    {
        return _tplIds;
    }

    public void SetTplIds(IReadOnlySet<MongoId> tplIds)
    {
        _tplIds = tplIds.ToArray();
    }
}

[Injectable]
public sealed class AirdropRandomStackLimitsPatch : AbstractPatch
{
    private static StackGenerationPolicy _stackGenerationPolicy = default!;

    public AirdropRandomStackLimitsPatch(StackGenerationPolicy stackGenerationPolicy)
    {
        _stackGenerationPolicy = stackGenerationPolicy;
    }

    protected override MethodBase? GetTargetMethod()
    {
        return typeof(AirdropService).GetMethod(
            "GetAirdropLootConfigByType",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(SptAirdropTypeEnum?)],
            null
        );
    }

    [PatchPostfix]
    public static void Postfix(AirdropLootRequest __result)
    {
        Dictionary<MongoId, MinMax<int>> itemStackLimits = __result.ItemStackLimits is null
            ? []
            : new Dictionary<MongoId, MinMax<int>>(__result.ItemStackLimits);

        foreach (MongoId tplId in _stackGenerationPolicy.GetTplIds())
        {
            itemStackLimits[tplId] = new MinMax<int>(1, 1);
        }

        __result.ItemStackLimits = itemStackLimits;
    }
}
