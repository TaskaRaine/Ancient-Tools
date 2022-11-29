
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AncientTools.EntityRenderers
{
    class RegisterEntityRenderer: ModSystem
    {
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            api.RegisterEntityRendererClass("CartRenderer", typeof(CartRenderer));
        }
    }
}
