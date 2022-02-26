using AncientTools.BlockEntities;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockSalve: Block
    {
        WorldInteraction[] interactions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            interactions = ObjectCacheUtil.GetOrCreate(api, "finishedSalveInteractions", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:blockhelp-take-salveportion",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            if(api.World.BlockAccessor.GetBlockEntity(pos) is BEFinishedSalve salveEntity)
            {
                StringBuilder infoString = new StringBuilder();

                infoString.Append("\n");
                infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-filled-remaining", salveEntity.SalveSlot.StackSize));

                return infoString.ToString();
            }

            return string.Empty;
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions;
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if(api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BEFinishedSalve salveEntity)
            {
                if(!salveEntity.SalveSlot.Empty)
                {
                    if(!byPlayer.Entity.Controls.Sneak)
                    {
                        salveEntity.GiveObject(byPlayer, salveEntity.SalveSlot);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
