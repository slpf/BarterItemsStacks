using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace BarterItemsStacks.RemoveOneFromStack
{
    [Injectable]
    public class RemoveOneFromStackRouter(RemoveOneFromStackCallbacks callbacks) : ItemEventRouterDefinition
    {
        public override async ValueTask<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, MongoId sessionID, ItemEventRouterResponse output)
        {
            return url switch
            {
                BarterItemsStacks.RofsRouter => await callbacks.HandleRemoveOneFromStack(pmcData, body as RemoveOneFromStackModel, sessionID),
                _ => throw new Exception($"RemoveOneFromStackRouter being used when it cant handle route {url}")
            };
        }

        protected override List<HandledRoute> GetHandledRoutes()
        {
            return [new(BarterItemsStacks.RofsRouter, false)];
        }

        protected override ValueTask<ItemEventRouterResponse> HandleItemEventInternal(string url, PmcData pmcData, BaseInteractionRequestData body, MongoId sessionID, ItemEventRouterResponse output)
        {
            throw new NotImplementedException();
        }
    }
}
