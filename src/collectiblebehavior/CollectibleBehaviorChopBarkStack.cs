using AncientTools.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.CollectibleBehaviors
{
    public class CollectibleBehaviorChopBarkStack: CollectibleBehavior
    {
        private WorldInteraction[] ChopBarkInteractions { get; set; } = null;

        public CollectibleBehaviorChopBarkStack(CollectibleObject collObj) : base(collObj)
        {
            
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            WorldInteraction chopBarkInteraction = new WorldInteraction
            {
                ActionLangCode = "ancienttools:itemhelp-chopbark",
                MouseButton = EnumMouseButton.Left
            };

            ChopBarkInteractions = ObjectCacheUtil.GetOrCreate(api, "chopBarkInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    chopBarkInteraction
                };
            });
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            return base.GetHeldInteractionHelp(inSlot, ref handling).Append(ChopBarkInteractions);
        }
        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
            if (byPlayer == null || blockSel == null) return;

            if (byEntity.World.BlockAccessor.GetBlock(blockSel.Position).GetType() != typeof(BlockGroundStorage))
            {
                base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);

                return;
            }
            else
            {
                BlockEntityGroundStorage groundStorageEntity = byEntity.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityGroundStorage;

                if (groundStorageEntity.Inventory.Empty)
                {
                    base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
                    return;
                }

                AssetLocation collectibleCode = groundStorageEntity.Inventory.FirstNonEmptySlot.Itemstack.Collectible.Code;

                if (collectibleCode.Domain == "ancienttools" && collectibleCode.FirstCodePart() == "bark")
                {
                    if (groundStorageEntity.Inventory.FirstNonEmptySlot.StackSize <= 64 / (groundStorageEntity.Inventory.FirstNonEmptySlot.StackSize / 4))
                    {
                        byEntity.Api.World.BlockAccessor.BreakBlock(blockSel.Position, byPlayer);

                        handling = EnumHandling.Handled;
                        handHandling = EnumHandHandling.Handled;
                    }

                    ItemStack barkStack = groundStorageEntity.Inventory.FirstNonEmptySlot.TakeOut(64 / (groundStorageEntity.Inventory.FirstNonEmptySlot.StackSize / 4));
                    ItemStack barkChunkStack = new ItemStack(byEntity.World.GetItem(new AssetLocation("ancienttools", "barkchunk-" + collectibleCode.Path.Split('-')[1])));

                    for (int i = 0; i < barkStack.StackSize; i++)
                    {
                        Vec3d velocityVector = new Vec3d(byEntity.World.Rand.Next(-1, 2) * byEntity.World.Rand.NextDouble() / 10, 0.5 * (byEntity.World.Rand.NextDouble() / 5), byEntity.World.Rand.Next(-1, 2) * (byEntity.World.Rand.NextDouble() / 10));
                        byEntity.World.SpawnItemEntity(barkChunkStack.Clone(), blockSel.Position, velocityVector);
                    }

                    groundStorageEntity.MarkDirty(true);

                    handling = EnumHandling.PreventDefault;
                    handHandling = EnumHandHandling.PreventDefaultAction;
                }
            }
        }
        public override bool OnHeldAttackStep(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
        {
            return base.OnHeldAttackStep(secondsPassed, slot, byEntity, blockSelection, entitySel, ref handling);
        }
        public override bool OnHeldAttackCancel(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handling)
        {
            handling = EnumHandling.Handled;
            return true;
        }
        public override void OnHeldAttackStop(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
        {
            handling = EnumHandling.Handled;
            base.OnHeldAttackStop(secondsPassed, slot, byEntity, blockSelection, entitySel, ref handling);
        }
    }
}
