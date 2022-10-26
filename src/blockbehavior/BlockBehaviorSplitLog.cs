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
        private BlockPos BlockPosition { get; set; }
        private string LogHorizontal { get; set; } = string.Empty;
        private string LogVertical { get; set; } = string.Empty;
        private string LogStage { get; set; } = string.Empty;

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
                properties["wedgebox2"].AsObject<Cuboidf>(FallbackCuboid)
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
            if (blockSel.SelectionBoxIndex != 0)
            {
                ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if(block is BlockLog)
                {
                    if(activeSlot.Itemstack?.Collectible is ItemWedge)
                    {
                        DetermineLogRotation();

                        world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("ancienttools", "splitlog-0-" + block.Variant["wood"] + "-" + LogVertical + "-" + LogHorizontal)).Id, blockSel.Position);

                        if(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
                        {
                            splitLogEntity.OnInteract(byPlayer, blockSel.SelectionBoxIndex, blockSel.HitPosition);
                        }
                    }    
                    else
                    {
                        return false;
                    }
                }
                else if(block is BlockSplitLog)
                {
                    if (activeSlot.Itemstack?.Collectible is ItemWedge)
                    {
                        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
                        {
                            splitLogEntity.OnInteract(byPlayer, blockSel.SelectionBoxIndex, blockSel.HitPosition);
                        }
                    }
                    else if(activeSlot.Empty)
                    {
                        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
                        {
                            splitLogEntity.OnInteract(byPlayer, blockSel.SelectionBoxIndex, blockSel.HitPosition);
                        }
                    }
                }

                handling = EnumHandling.Handled;
                return true;
            }

            return false;
        }
        private void Event_AfterActiveSlotChanged(ActiveSlotChangeEventArgs obj)
        {
            ItemSlot activeHotbarSlot = capi.World.Player?.InventoryManager?.ActiveHotbarSlot;

            if(activeHotbarSlot?.Itemstack?.Collectible is ItemWedge || activeHotbarSlot.Empty)
            {
                block.SelectionBoxes = OriginalSelectionBoxes.Append(WedgeSelectionBoxes);
            }
            else
            {
                block.SelectionBoxes = OriginalSelectionBoxes;
            }
        }
        private void SetInitialLogStage()
        {
            LogStage = "0";
        }
        private void IncrementLogStage()
        {
            if(block.Variant.ContainsKey("stage"))
            {
                int nextStage = LogStage.ToInt() + 1;
                LogStage = nextStage.ToString();
            }
        }
        private void DetermineLogRotation()
        {
            if(block.Variant.ContainsKey("rotation"))
            {
                switch(block.Variant["rotation"])
                {
                    case "ud":
                        LogVertical = "none";
                        LogHorizontal = "north";
                        break;
                    case "ns":
                        LogVertical = "up";
                        LogHorizontal = "north";
                        break;
                    case "ew":
                        LogVertical = "up";
                        LogHorizontal = "east";
                        break;
                }

                return;
            }

            if(block.Variant.ContainsKey("verticalorientation"))
            {
                LogVertical = block.Variant["verticalorientation"];
                LogHorizontal = block.Variant["horizontalorientation"];
            }
        }

    }
}
