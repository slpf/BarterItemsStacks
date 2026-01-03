using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace BarterItemsStacks.RemoveOneFromStack
{
    [Injectable]
    public class RemoveOneFromStackCallbacks(RemoveOneFromStackController controller)
    {
        public async ValueTask<ItemEventRouterResponse> HandleRemoveOneFromStack(PmcData pmcData, RemoveOneFromStackModel body, string sessionID)
        {
            return await controller.RemoveOneFromStack(pmcData, body, sessionID);
        }
    }
}
