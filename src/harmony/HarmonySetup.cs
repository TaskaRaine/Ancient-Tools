using HarmonyLib;
using Vintagestory.API.Common;

namespace AncientTools
{
    class HarmonySetup : ModSystem
    {
        Harmony harmony = new Harmony("ca.taska.ancienttools");

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            harmony.PatchAll();
        }

        public override void Dispose()
        {
            base.Dispose();

            harmony.UnpatchAll("ca.taska.ancienttools");
        }
    }
}
