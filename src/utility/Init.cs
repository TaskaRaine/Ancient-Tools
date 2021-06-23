using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AncientTools.Utility
{
    class Init: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            ParticleColor.InitColours();
        }
        public override void Dispose()
        {
            base.Dispose();

            ParticleColor.Destroy();
        }
    }
}
