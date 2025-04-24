﻿using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.BlockBehaviors
{
    class BlockBehaviorConvertToResinLog : BlockBehavior
    {
        public WorldInteraction[] scrapeLogInteractions = null;
        private string variantType = "wood";
        private string[] variantValues = { "pine", "acacia" };

        private bool beginScrape = false;

        public BlockBehaviorConvertToResinLog(Block block) : base(block)
        {

        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            if(properties["variantType"].Exists)
                variantType = properties["variantType"].AsString();

            if (properties["variantValues"].Exists)
                variantValues = properties["variantValues"].AsArray<string>();
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            foreach (string variant in variantValues)
                if (block.CodeWithVariant(variantType, variant).Equals(block.Code))
                {
                    List<ItemStack> knives = new List<ItemStack>();

                    foreach(Item item in api.World.Items)
                    {
                        if (item == null)
                            continue;

                        if (item.Tool == EnumTool.Knife)
                            knives.Add(new ItemStack(item));
                    }

                    WorldInteraction scrapeInteraction = new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:blockhelp-scrape-log",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift",
                        Itemstacks = knives.ToArray()
                    };

                    scrapeLogInteractions = ObjectCacheUtil.GetOrCreate(api, "scrapeInteractions", () =>
                    {
                        return new WorldInteraction[]
                        {
                            scrapeInteraction
                        };
                    });
                }
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            foreach (string variant in variantValues)
                if (block.CodeWithVariant(variantType, variant).Equals(block.Code))
                {
                    if (!FindResin(world, selection.Position))
                        return scrapeLogInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
                }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            foreach(string variant in variantValues)
                if (block.CodeWithVariant(variantType, variant).Equals(block.Code))
                {
                    if (blockSel == null || byPlayer.InventoryManager.ActiveTool != EnumTool.Knife)
                        return false;

                    if (byPlayer.Entity.Controls.ShiftKey || byPlayer.Entity.Controls.CtrlKey)
                    {
                        if (blockSel.Face != BlockFacing.UP || blockSel.Face != BlockFacing.DOWN)
                        {
                            if (!FindResin(world, blockSel.Position))
                            {

                                if (world.Api.Side == EnumAppSide.Client)
                                    byPlayer.Entity.StartAnimation("knifecut");

                                world.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/knifecarve"), byPlayer.Entity.Pos.X, byPlayer.Entity.Pos.Y, byPlayer.Entity.Pos.Z, byPlayer, false);
                                handling = EnumHandling.PreventSubsequent;

                                beginScrape = true;
                                return true;
                            }
                        }
                    }
                }

            beginScrape = false;

            return false;
        }
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel == null || byPlayer.InventoryManager.ActiveTool != EnumTool.Knife || beginScrape == false)
            {
                return false;
            }

            //-- Without this early stop, the animation doesn't get stopped in time before the block is changed. Hopefully this is fine for servers. --//
            if (secondsUsed >= 0.9)
            {
                byPlayer.Entity.StopAnimation("knifecut");
                byPlayer.Entity.AnimManager.StopAnimation("knifecut");
            }

            if (secondsUsed > 1)
            { 
                //-- These further checks attempt to make sure that the animation is off before the event is handled. --//
                if(!byPlayer.Entity.SelfFpAnimManager.ActiveAnimationsByAnimCode.ContainsKey("knifecut-fp") && !byPlayer.Entity.SelfFpAnimManager.ActiveAnimationsByAnimCode.ContainsKey("knifecut"))
                {
                    handling = EnumHandling.Handled;
                    return false;
                }
            }

            handling = EnumHandling.PreventSubsequent;
            return true;
        }
        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if(secondsUsed > 1)
            {
                string woodType = block.FirstCodePart(2);
                string woodOrientation = block.FirstCodePart(3);

                Block resinBlock = world.GetBlock(new AssetLocation("ancienttools", "directionalresin-resinharvested-" + woodType + "-" + woodOrientation + "-" + GetFacingDirection(blockSel.Face)));
                world.BlockAccessor.SetBlock(resinBlock.Id, blockSel.Position);
                world.BlockAccessor.BreakDecor(blockSel.Position);

                world.BlockAccessor.MarkChunkDecorsModified(blockSel.Position);
                world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);

                if (world.Api.Side == EnumAppSide.Client)
                    byPlayer.Entity.StopAnimation("knifecut");
            }

            handling = EnumHandling.Handled;

            base.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel, ref handling);
        }
        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            byPlayer.Entity.StopAnimation("knifecut");
            beginScrape = false;

            handling = EnumHandling.Handled;

            return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, ref handling);
        }
        //-- Most code borrowed from vanilla axe felling logic --//
        private bool FindResin(IWorldAccessor world, BlockPos startPos)
        {
            Queue<Vec4i> queue = new Queue<Vec4i>();
            HashSet<BlockPos> checkedPositions = new HashSet<BlockPos>();
            Block startBlock = world.BlockAccessor.GetBlock(startPos, BlockLayersAccess.SolidBlocks);
            Block block;

            int spreadIndex = startBlock.Attributes?["treeFellingGroupSpreadIndex"].AsInt(0) ?? 0;

            queue.Enqueue(new Vec4i(startPos.X, startPos.Y, startPos.Z, spreadIndex));
            checkedPositions.Add(startPos);

            while (queue.Count > 0)
            {
                if (queue.Count > 50)
                    return false;

                Vec4i pos = queue.Dequeue();

                block = world.BlockAccessor.GetBlock(new BlockPos(pos.X, pos.Y, pos.Z, 0), BlockLayersAccess.SolidBlocks);
                string treeFellingGroupCode = block.Attributes?["treeFellingGroupCode"].AsString();

                if (block.FirstCodePart() == "log" || block.FirstCodePart() == "directionalresin")
                {
                    if (block.FirstCodePart(1) == "resin" || block.FirstCodePart(1) == "resinharvested")
                        return true;
                }

                for (int i = 0; i < Vec3i.DirectAndIndirectNeighbours.Length; i++)
                {
                    Vec3i facing = Vec3i.DirectAndIndirectNeighbours[i];
                    BlockPos neibPos = new BlockPos(pos.X + facing.X, pos.Y + facing.Y, pos.Z + facing.Z, 0);

                    if (checkedPositions.Contains(neibPos)) continue;

                    block = world.BlockAccessor.GetBlock(neibPos, BlockLayersAccess.SolidBlocks);

                    if (block.Code == null || block.Id == 0) continue;

                    string ngcode = block.Attributes?["treeFellingGroupCode"].AsString();

                    if (ngcode != treeFellingGroupCode) continue;

                    int nspreadIndex = block.Attributes?["treeFellingGroupSpreadIndex"].AsInt(0) ?? 0;

                    checkedPositions.Add(neibPos);
                    queue.Enqueue(new Vec4i(neibPos.X, neibPos.Y, neibPos.Z, nspreadIndex));
                }
            }

            return false;
        }
        private string GetFacingDirection(BlockFacing faceClicked)
        {
            if (faceClicked == BlockFacing.EAST)
            {
                return "east";
            }
            else if (faceClicked == BlockFacing.WEST)
            {
                return "west";
            }
            else if (faceClicked == BlockFacing.NORTH)
            {
                return "north";
            }
            else if (faceClicked == BlockFacing.SOUTH)
            {
                return "south";
            }

            return "north";
        }
    }
}
