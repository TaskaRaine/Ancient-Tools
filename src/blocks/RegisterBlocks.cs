using Vintagestory.API.Common;

namespace AncientTools.Blocks
{
    class RegisterBlocks: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockMortar", typeof(BlockMortar));
            api.RegisterBlockClass("BlockCuringRack", typeof(BlockCuringRack));

            base.Start(api);
        }
    }
}
