using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.BlockBehaviors
{
    class BlockBehaviorConvertToResinLog : BlockBehavior
    {
        public WorldInteraction[] scrapeLogInteractions = null;
        private string[] variantTypes = { "wood" };
        private string[] variantValues = { "pine", "acacia" };

        private bool beginScrape = false;

        public BlockBehaviorConvertToResinLog(Block block) : base(block)
        {

        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (block.CodeWithVariants(variantTypes, variantValues).Equals(block.Code))
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
                    HotKeyCode = "sneak",
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
            if (block.CodeWithVariants(variantTypes, variantValues).Equals(block.Code))
            {
                if (!FindResin(world, selection.Position))
                    return scrapeLogInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (block.CodeWithVariants(variantTypes, variantValues).Equals(block.Code))
            {
                if (blockSel == null || byPlayer.InventoryManager.ActiveTool != EnumTool.Knife)
                    return false;

                if (byPlayer.Entity.Controls.Sneak || byPlayer.Entity.Controls.Sprint)
                {
                    if (blockSel.Face != BlockFacing.UP || blockSel.Face != BlockFacing.DOWN)
                    {
                        if (!FindResin(world, blockSel.Position))
                        {
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
                return false;

            AnimateKnife(byPlayer, secondsUsed);

            if(secondsUsed > 1)
            {
                string woodType = block.FirstCodePart(2);
                string woodOrientation = block.FirstCodePart(3);

                Block resinBlock = world.GetBlock(new AssetLocation("ancienttools", "directionalresin-resinharvested-" + woodType + "-" + woodOrientation + "-" + GetFacingDirection(blockSel.Face)));
                world.BlockAccessor.SetBlock(resinBlock.Id, blockSel.Position);

                world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);
            }

            handling = EnumHandling.PreventSubsequent;
            return true;
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

                block = world.BlockAccessor.GetBlock(pos.X, pos.Y, pos.Z, BlockLayersAccess.SolidBlocks);
                string treeFellingGroupCode = block.Attributes?["treeFellingGroupCode"].AsString();

                if (block.FirstCodePart() == "log" || block.FirstCodePart() == "directionalresin")
                {
                    if (block.FirstCodePart(1) == "resin" || block.FirstCodePart(1) == "resinharvested")
                        return true;
                }

                for (int i = 0; i < Vec3i.DirectAndIndirectNeighbours.Length; i++)
                {
                    Vec3i facing = Vec3i.DirectAndIndirectNeighbours[i];
                    BlockPos neibPos = new BlockPos(pos.X + facing.X, pos.Y + facing.Y, pos.Z + facing.Z);

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
        private void AnimateKnife(IPlayer player, float secondsUsed)
        {
            ModelTransform tf = new ModelTransform();
            tf.EnsureDefaultValues();

            tf.Translation.Set(0, 0, -Math.Min(0.6f, secondsUsed * 2));
            tf.Rotation.Y = Math.Min(20, secondsUsed * 90 * 2f);


            if (secondsUsed > 0.4f)
            {
                tf.Translation.X += (float)Math.Cos(secondsUsed * 15) / 10;
                tf.Translation.Z += (float)Math.Sin(secondsUsed * 5) / 30;
            }

            player.Entity.Controls.UsingHeldItemTransformBefore = tf;
        }
    }
}
