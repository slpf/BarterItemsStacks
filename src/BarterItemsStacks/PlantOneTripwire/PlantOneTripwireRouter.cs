using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;

namespace BarterItemsStacks.PlantOneTripwire
{
    [Injectable(TypePriority = OnLoadOrder.Routers + 1)]
    public sealed class PlantOneTripwireRouter(PlantOneTripwireCallbacks callbacks)
        : ItemEventRouter([
            new ItemRouteAction<PlantOneTripwireModel>(
                BarterItemsStacks.PotRouter,
                (url, pmcData, body, sessionID, output, cancellationToken) =>
                    callbacks.HandlePlantOneTripwire(pmcData, body, sessionID, cancellationToken)
            )
        ]) { }
}
