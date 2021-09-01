using AncientTools.BlockEntity;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorSalveIngredient: CollectibleBehavior
    {
        public CollectibleBehaviorSalveIngredient(CollectibleObject collObj) : base(collObj)
        {

        }
        //-- A behavior applied to salve ingredients. Tests to see if it is being interacted with a cooking pot. If so, that cooking pot is converted to a salve pot --//
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null)
                return;

            ICoreAPI api = byEntity.Api;

            if (byEntity is EntityPlayer player)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position);

                if (interactedBlock is BlockCookingContainer)
                {
                    api.World.BlockAccessor.SetBlock(api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "salvepot-empty")).Id, blockSel.Position);
                    api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                    handling = EnumHandling.PreventDefault;
                    handHandling = EnumHandHandling.PreventDefault;
                    return;
                }
            }

            handling = EnumHandling.PassThrough;
        }
    }
}
