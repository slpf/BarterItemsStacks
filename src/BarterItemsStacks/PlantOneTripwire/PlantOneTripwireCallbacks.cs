using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace BarterItemsStacks.PlantOneTripwire
{
    [Injectable]
    public class PlantOneTripwireCallbacks(PlantOneTripwireController controller)
    {
        public async ValueTask<ItemEventRouterResponse> HandlePlantOneTripwire(PmcData pmcData, PlantOneTripwireModel body, string sessionID)
        {
            return await controller.PlantOneTripwire(pmcData, body, sessionID);
        }
    }
}
