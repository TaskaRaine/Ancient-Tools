using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntityBehaviors
{
    class BlockEntityBehaviorFireproofFuel : BEBehaviorTemperatureSensitive
    {
        private bool fedFireproofFuel = false;

        public BlockEntityBehaviorFireproofFuel(BlockEntity blockentity) : base(blockentity)
        {

        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            fedFireproofFuel = tree.GetBool("fireprooffuel");
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("fireprooffuel", fedFireproofFuel);
        }
        public void SetFedFireproofFuel(bool fireproof)
        {
            fedFireproofFuel = fireproof;
        }
        public bool GetFedFireproofFuel()
        {
            return fedFireproofFuel;
        }
    }
}
