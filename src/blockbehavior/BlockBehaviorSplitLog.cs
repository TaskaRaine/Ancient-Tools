using AncientTools.BlockEntities;
using AncientTools.BlockEntityBehaviors;
using AncientTools.Blocks;
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
        private Cuboidf[] MalletHitboxes { get; set; }

        ICoreClientAPI capi;

        public BlockBehaviorSplitLog(Block block) : base(block)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            if (block.CodeWithVariant("rotation", "ud") != block.Code)
                return;

            base.Initialize(properties);

            OriginalSelectionBoxes = block.SelectionBoxes;
            WedgeSelectionBoxes = new Cuboidf[] {
                properties["wedgeboxnorth"].AsObject<Cuboidf>(FallbackCuboid),
                properties["wedgeboxeast"].AsObject<Cuboidf>(FallbackCuboid),
                properties["wedgeboxsouth"].AsObject<Cuboidf>(FallbackCuboid),
                properties["wedgeboxwest"].AsObject<Cuboidf>(FallbackCuboid)
            };
            MalletHitboxes = new Cuboidf[] {
                properties["mallethitboxnorth"].AsObject<Cuboidf>(FallbackCuboid),
                properties["mallethitboxeast"].AsObject<Cuboidf>(FallbackCuboid),
                properties["mallethitboxsouth"].AsObject<Cuboidf>(FallbackCuboid),
                properties["mallethitboxwest"].AsObject<Cuboidf>(FallbackCuboid)
            };
        }
        public override void OnLoaded(ICoreAPI api)
        {
            if (block.CodeWithVariant("rotation", "ud") != block.Code)
                return;

            base.OnLoaded(api);

            if(api is ICoreClientAPI clientAPI)
            {
                capi = clientAPI;

                capi.Event.AfterActiveSlotChanged += Event_AfterActiveSlotChanged;
            }
        }
        private void Event_AfterActiveSlotChanged(ActiveSlotChangeEventArgs obj)
        {
            ItemSlot activeHotbarSlot = capi.World.Player?.InventoryManager?.ActiveHotbarSlot;

            if(activeHotbarSlot?.Itemstack?.Collectible is ItemWedge || activeHotbarSlot.Empty && capi.World.Config.GetBool("DisableWedgePickupWireframe", false) == false)
            {
                block.SelectionBoxes = OriginalSelectionBoxes.Append(WedgeSelectionBoxes);
            }
            else if(activeHotbarSlot?.Itemstack?.Collectible is ItemMallet || activeHotbarSlot?.Itemstack?.Collectible is ItemHammer)
            {
                block.SelectionBoxes = OriginalSelectionBoxes.Append(MalletHitboxes);
            }
            else
            {
                block.SelectionBoxes = OriginalSelectionBoxes;
            }
        }
    }
}
