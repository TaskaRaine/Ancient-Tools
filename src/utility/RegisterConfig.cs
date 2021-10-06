using Vintagestory.API.Common;

namespace AncientTools.Utility
{
    class RegisterConfig: ModSystem
    {
        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);

            ModConfig.ReadConfig(api);
        }
    }
}
