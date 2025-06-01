using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace AncientTools.BlockBehaviors
{
    public class BlockBehaviorSealLogBarrelInfo : BlockBehavior
    {
        public WorldInteraction[] SealLogInteractions { get; set; } = null;

        public BlockBehaviorSealLogBarrelInfo(Block block) : base(block)
        {
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            List<ItemStack> sealingItems = new List<ItemStack>();

            foreach(CollectibleObject collectible in api.World.Collectibles)
            {
                if (collectible.Attributes == null) continue;

                if (collectible.Attributes["canSealPrimitiveBarrel"]?.AsBool() == true)
                    sealingItems.Add(new ItemStack(collectible));
            }

            WorldInteraction sealInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-seal-logbarrel",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = sealingItems.ToArray()
            };

            SealLogInteractions = ObjectCacheUtil.GetOrCreate(api, "sealPrimitiveBarrelInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    sealInteraction
                };
            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling).Append(SealLogInteractions.ToArray());
        }
    }
}
