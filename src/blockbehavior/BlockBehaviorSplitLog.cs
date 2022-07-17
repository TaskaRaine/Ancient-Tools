using AncientTools.BlockEntityBehaviors;
using AncientTools.Items;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.BlockBehaviors
{
    class BlockBehaviorSplitLog: BlockBehavior
    {
        private Cuboidf FallbackCuboid { get; } = new Cuboidf(0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f); 
        private Cuboidf[] OriginalSelectionBoxes { get; set; }
        private Cuboidf[] WedgeSelectionBoxes { get; set; }

        ICoreClientAPI capi;

        public BlockBehaviorSplitLog(Block block) : base(block)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            OriginalSelectionBoxes = block.SelectionBoxes;
            WedgeSelectionBoxes = new Cuboidf[] {
                properties["wedgebox1"].AsObject<Cuboidf>(FallbackCuboid),
                properties["wedgebox2"].AsObject<Cuboidf>(FallbackCuboid),
                properties["wedgebox3"].AsObject<Cuboidf>(FallbackCuboid),
                properties["wedgebox4"].AsObject<Cuboidf>(FallbackCuboid)
            };
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if(api is ICoreClientAPI clientAPI)
            {
                capi = clientAPI;

                capi.Event.AfterActiveSlotChanged += Event_AfterActiveSlotChanged;
            }
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {

            if (world.BlockAccessor.GetBlock(blockSel.Position) is BlockLog log)
            {
                if(capi.World.Player?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible is ItemWedge)
                {
                    world.BlockAccessor.SetBlock(world.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "splitlog-1-" + log.Variant["wood"] + "-none-west")).Id, blockSel.Position);
                    world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                }
                return true;
            }

            return false;
        }
        private void Event_AfterActiveSlotChanged(ActiveSlotChangeEventArgs obj)
        {
            if(capi.World.Player?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible is ItemWedge)
            {
                block.SelectionBoxes = OriginalSelectionBoxes.Append(WedgeSelectionBoxes);
            }
            else
            {
                block.SelectionBoxes = OriginalSelectionBoxes;
            }
        }
    }
}
