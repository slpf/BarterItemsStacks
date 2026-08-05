using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;

namespace BarterItemsStacks.RemoveOneFromStack
{
    [Injectable(TypePriority = OnLoadOrder.Routers + 1)]
    public sealed class RemoveOneFromStackRouter(RemoveOneFromStackCallbacks callbacks)
        : ItemEventRouter([
            new ItemRouteAction<RemoveOneFromStackModel>(
                BarterItemsStacks.RofsRouter,
                (url, pmcData, body, sessionID, output, cancellationToken) =>
                    callbacks.HandleRemoveOneFromStack(pmcData, body, sessionID, cancellationToken)
            )
        ]) { }
}
