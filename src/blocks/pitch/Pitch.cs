using AncientTools.BlockEntities;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockPitch: Block
    {
        public WorldInteraction[] takePitchInteractions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ItemStack[] stick =
            {
                new ItemStack(api.World.GetItem(new AssetLocation("game", "stick")))
            };

            WorldInteraction pitchStickInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-take-pitch",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = stick
            };
            WorldInteraction pitchInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-take-pitch",
                MouseButton = EnumMouseButton.Right,
                RequireFreeHand = true
            };

            takePitchInteractions = ObjectCacheUtil.GetOrCreate(api, "takePitchInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    pitchInteraction,
                    pitchStickInteraction
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (api.World.BlockAccessor.GetBlockEntity(selection.Position) is BEFinishedPitch pitch)
            {
                if (pitch.PitchSlot.StackSize != 0)
                    return takePitchInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            if (api.World.BlockAccessor.GetBlockEntity(pos) is BEFinishedPitch pitch)
            {
                StringBuilder infoString = new StringBuilder();

                infoString.Append("\n");
                infoString.Append(Lang.Get("ancienttools:blockinfo-pitch-remaining", pitch.PitchSlot.StackSize));

                return infoString.ToString(); 
            }

            return string.Empty;
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEFinishedPitch pitchEntity)
            {
                pitchEntity.GiveObject(byPlayer, pitchEntity.PitchSlot);

                return true;
            }

            return false;
        }
    }
}
