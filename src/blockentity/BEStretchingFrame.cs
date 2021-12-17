
using AncientTools.Utility;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntity
{
    class BEStretchingFrame : DisplayInventory
    {
        private float skinningTime;
        private float skinningStartTime = -1;

        public BEStretchingFrame()
        {
            InventorySize = 1;
            InitializeInventory();
        }

        public ItemSlot HideSlot
        {
            get { return inventory[0]; }
        }

        public MeshData HideMesh
        {
            get { return meshes[0]; }
            protected set { meshes[0] = value; }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            skinningTime = api.World.Config.GetFloat("SkinningTime", 4.0f);

            HideSlot.MaxSlotStackSize = 1;

            UpdateMeshes();
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (worldForResolving.Side == EnumAppSide.Client && Api != null)
            {
                UpdateMeshes();
            }

            base.FromTreeAttributes(tree, worldForResolving);
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (!HideSlot.Empty)
            {
                this.AddMesh(mesher, HideMesh, GetShapeRotation());
            }

            return false;
        }
        public override void UpdateMeshes()
        {
            if (Api.Side != EnumAppSide.Client)
                return;

            if (!HideSlot.Empty)
            {
                string hideType = HideSlot.Itemstack.Item.FirstCodePart(1);
                string hideSize = HideSlot.Itemstack.Item.LastCodePart();

                string resourcePath = "ancienttools:shapes/block/stretchingframe/resourceshapes/" + hideType + "hide_" + hideSize;

                currentObject = HideSlot.Itemstack.Item;
                HideMesh = GenMesh(Api as ICoreClientAPI, resourcePath);
            }
        }
        /// <summary>
        /// Try to place a hide from the provided slot onto the stretching frame.
        /// </summary>
        /// <param name="playerSlot">The slot from which the item will be pulled.</param>
        /// <returns>A boolean representing whether the hide was placed successfully.</returns>
        public bool TryPlaceHide(ItemSlot playerSlot)
        {
            if(HideSlot.Empty)
            {
                playerSlot.TryPutInto(Api.World, HideSlot);
                MarkDirty(true);

                UpdateMeshes();
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try to give the player the hide currently held withing the stretching frame.
        /// </summary>
        /// <param name="player">The player to give the hide to.</param>
        /// <returns>A boolean representing whether the hide was taken by the player.</returns>
        public bool TryGiveHide(IPlayer player)
        {
            if(!HideSlot.Empty)
            {
                if(player.InventoryManager.TryGiveItemstack(HideSlot.TakeOut(1)))
                {
                    HideSlot.TakeOut(1);
                    MarkDirty(true);

                    UpdateMeshes();
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Begin the hide scraping process. Play a sound and set the timer to the current elapsed game time.
        /// </summary>
        /// <param name="player">The player that begins to scrape a hide.</param>
        /// <returns>A boolean that, if the process should continue, will return true.</returns>
        public bool BeginPrepareHide(IPlayer player)
        {
            if(skinningStartTime < 0)
            {
                skinningStartTime = Api.World.ElapsedMilliseconds;
                Api.World.PlaySoundAt(new AssetLocation("game", "sounds/player/scrape"), player.Entity, (player as EntityPlayer)?.Player, false, 12);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Keep the skinning process going until the current world time exceeds the skinning start time + amount of time required to skin.
        /// </summary>
        /// <param name="player">The player skinning.</param>
        /// <returns>True, unless the process is finished and should be stopped.</returns>
        public bool PrepareHide(IPlayer player)
        {
            if (skinningStartTime == -1)
                return false;

            if (Api.World.ElapsedMilliseconds < skinningStartTime + skinningTime * 1000)
            {
                AnimateSkinning(player, (Api.World.ElapsedMilliseconds - skinningStartTime) * 0.001f);

                return true;
            }
            else
            {
                FinishPreparing(player);
                return false;
            }
        }
        /// <summary>
        /// Convert the hide and update the frame mesh.
        /// </summary>
        /// <param name="player">The player skinning.</param>
        public void FinishPreparing(IPlayer player)
        {
            string hideSize = HideSlot.Itemstack.Item.LastCodePart();

            HideSlot.TakeOutWhole();

            HideSlot.Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation("game", "hide-scraped-" + hideSize)));
            HideSlot.MarkDirty();

            player.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(Api.World, player.Entity, player.InventoryManager.ActiveHotbarSlot, 1);

            Reset();
            UpdateMeshes();
            MarkDirty(true);
        }
        public void Reset()
        {
            skinningStartTime = -1;

            MarkDirty(true);
        }
        private Vec3f GetShapeRotation()
        {
            switch (Block.LastCodePart())
            {
                case "north":
                    return new Vec3f(0.0f, GameMath.PI, 0.0f);
                case "south":
                    return new Vec3f(0.0f, 0.0f, 0.0f);
                case "east":
                    return new Vec3f(0.0f, GameMath.PIHALF, 0.0f);
                case "west":
                    return new Vec3f(0.0f, GameMath.PI + GameMath.PIHALF, 0.0f);
                default: return new Vec3f(0.0f, 0.0f, 0.0f);

            }
        }
        private void AnimateSkinning(IPlayer player, float secondsUsed)
        {
            if (Api.World.Side == EnumAppSide.Client)
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
}
