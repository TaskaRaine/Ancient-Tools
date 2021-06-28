using AncientTools.Blocks;
using AncientTools.Utility;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntity
{
    class BEMortar : BlockEntityDisplay
    {

        public override InventoryBase Inventory => inventory;
        public override string InventoryClassName => "mortarcontainer";

        private const float GRIND_TIME_IN_SECONDS = 4;

        private long grindStartTime = -1;

        internal InventoryGeneric inventory;

        private ILoadedSound ambientSound, stoneSound;

        private SimpleParticleProperties grindingParticles;
        private int particleColor;

        private Vec3f lookAtPlayerVector = new Vec3f(0.0f, 0.0f, 0.0f);
        private float[] animationPositions = { 0.050f, 0.050f, 0.075f, 0.1f, 0.125f, 0.150f, 0.125f, 0.1f, 0.070f, 0.050f, 0.050f };
        private Vec3f[] animationRotations = { 
            new Vec3f(0.0f, 0.15f, 0.0f), 
            new Vec3f(0.0f, 0.10f, 0.0f), 
            new Vec3f(0.0f, 0.05f, 0.0f), 
            new Vec3f(0f, 0.025f, 0f), 
            new Vec3f(0f, 0.0f, 0f), 
            new Vec3f(0f, 0.0f, 0f), 
            new Vec3f(0f, 0.0f, 0f), 
            new Vec3f(0f, 0.025f, 0f), 
            new Vec3f(0.0f, 0.05f, 0.0f), 
            new Vec3f(0.0f, 0.10f, 0.0f), 
            new Vec3f(0.0f, 0.15f, 0.0f) 
        };

        private int animPosition = 0;

        public ItemSlot ResourceSlot
        {
            get { return inventory[0]; }
        }
        public ItemSlot PestleSlot
        {
            get { return inventory[1]; }
        }

        public BEMortar()
        {
            inventory = new InventoryGeneric(2, null, null);
        }
        ~BEMortar()
        {
            if (ambientSound != null) ambientSound.Dispose();
            if (stoneSound != null) stoneSound.Dispose();
        }
        public override void Initialize(ICoreAPI api)
        {
            //-- Holds mesh data of items inserted into the mortar. The resource mesh is NOT used as the resource appearance is not actually represented by the inserted resrouce. --//
            meshes = new MeshData[2];

            if(api.Side == EnumAppSide.Client)
            {
                InitializeGindingParticles();

                ambientSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams()
                {
                    Location = new AssetLocation("ancienttools", "sounds/block/mortargrind.ogg"),
                    ShouldLoop = true,
                    Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1.0f
                });
                stoneSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams()
                {
                    Location = new AssetLocation("game", "sounds/block/loosestone2.ogg"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1.0f,
                    Range = 32
                });
            }

            base.Initialize(api);
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            if(ambientSound != null)
            {
                ambientSound.Stop();
                ambientSound.Dispose();
            }
            if(stoneSound != null)
            {
                stoneSound.Stop();
                stoneSound.Dispose();
            }
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            BlockMortar block = Api.World.BlockAccessor.GetBlock(Pos) as BlockMortar;

            if (!PestleSlot.Empty)
            {
                //-- Each time the block is set to dirty, animPostion is changed. This modifies which translate/rotate keyframe is used to transform the pestle mesh --// 
                if (PestleSlot.Itemstack.Item.FirstCodePart() == "pestle")
                {
                    mesher.AddMeshData(meshes[1].Clone()
                        .Translate(new Vec3f(0, animationPositions[animPosition], 0))
                        .Rotate(new Vec3f(0.5f, 0.0f, 0.5f), 
                        lookAtPlayerVector.X + animationRotations[animPosition].X, 
                        lookAtPlayerVector.Y + animationRotations[animPosition].Y, 
                        lookAtPlayerVector.Z + animationRotations[animPosition].Z));
                }
            }

            if (!ResourceSlot.Empty)
            {
                PrepareMesh("ancienttools:shapes/block/mortar/resource_", ResourceSlot, block, mesher, tessThreadTesselator);
            }

            return false;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            particleColor = tree.GetInt("particleColor");
            lookAtPlayerVector = new Vec3f(tree.GetFloat("x"), tree.GetFloat("y"), tree.GetFloat("z"));
        }
        //-- Particle color and pestle rotation is saved so that things look as they were left proper on game load --//
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetInt("particleColor", particleColor);
            tree.SetFloat("x", lookAtPlayerVector.X + animationRotations[0].X);
            tree.SetFloat("y", lookAtPlayerVector.Y + animationRotations[0].Y);
            tree.SetFloat("z", lookAtPlayerVector.Z + animationRotations[0].Z);
        }
        public void OnInteract(IPlayer byPlayer)
        {
            if (!byPlayer.Entity.Controls.Sneak)
            {
                ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                //-- If the player is standing and clicks the pestle with an empty hand, attempt to give them a pestle/resource that may be in the mortar --//
                if (activeSlot.Empty)
                {
                    if (!PestleSlot.Empty)
                    {
                        GiveObject(byPlayer, PestleSlot);
                        this.updateMesh(1);
                    }
                    else if (!ResourceSlot.Empty)
                    {
                        GiveObject(byPlayer, ResourceSlot);
                    }
                }
                else
                {
                    //-- Give the player the resource if the resource in hand matches the resource in the mortar --//
                    if (!ResourceSlot.Empty && ResourceSlot.Itemstack.Collectible.Code == activeSlot.Itemstack.Collectible.Code)
                    {
                        GiveObject(byPlayer, ResourceSlot);
                    }
                    else
                    {
                        if (PestleSlot.Empty && !activeSlot.Empty)
                        {
                            if (activeSlot.Itemstack.Collectible.ItemClass == EnumItemClass.Item)
                            {
                                if (activeSlot.Itemstack.Item.FirstCodePart() == "pestle")
                                {
                                    if(Api.Side == EnumAppSide.Client)
                                        stoneSound.Start();

                                    InsertObject(activeSlot, PestleSlot, activeSlot.Itemstack.Item, 1);
                                    this.updateMesh(1);
                                }
                            }
                        }

                        if (ResourceSlot.Empty && !activeSlot.Empty)
                        {
                            if (activeSlot.Itemstack.Collectible.ItemClass == EnumItemClass.Item)
                            {
                                if (activeSlot.Itemstack.Item.GrindingProps != null)
                                {
                                    if (Api.Side == EnumAppSide.Client)
                                        SetGrindingParticlesColor(activeSlot.Itemstack.Item.LastCodePart(), activeSlot.Itemstack.Item.FirstCodePart());

                                    InsertObject(activeSlot, ResourceSlot, activeSlot.Itemstack.Item, 1);
                                }
                            }
                        }
                    }
                }
            }
        }
        public bool OnSneakInteract(IPlayer byPlayer)
        {
            if (!byPlayer.Entity.Controls.Sneak || PestleSlot.Empty || ResourceSlot.Empty || !byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
            {
                OnInteractStop();

                return false;
            }

            BeginGrind();
            PerformGrind(byPlayer);

            return true;
        }
        private void BeginGrind()
        {
            if (grindStartTime < 0)
            {
                grindStartTime = Api.World.ElapsedMilliseconds;
            }
        }
        private void PerformGrind(IPlayer byPlayer)
        {
            if (Api.World.ElapsedMilliseconds < grindStartTime + GRIND_TIME_IN_SECONDS * 1000)
            {
                SetPestleLookAtVector(byPlayer);
                SetPestleAnimationFrame();

                ClientGrind();
            }
            else
            {
                FinishGrind(byPlayer);
            }

            MarkDirty(true);
        }
        //-- Sets the look at vector in such a way that the pestle appears to be held in hand anytime the mortar is being used --//
        private void SetPestleLookAtVector(IPlayer byPlayer)
        {
            Vec3f normal = ((this.Pos.ToVec3f() + new Vec3f(0.5f, 0, 0.5f)) - byPlayer.Entity.Pos.XYZFloat);
            normal.Y = 0.325f;

            double pitch = Math.Asin(normal.Y);
            double yaw = Math.Atan2(normal.X, normal.Z);

            lookAtPlayerVector.Y = (float)yaw;
            lookAtPlayerVector.Z = (float)pitch;
        }
        //-- Tick to the next animation frame any time the pestle is being used --//
        private void SetPestleAnimationFrame()
        {
            if (animPosition < animationPositions.Length - 1)
                animPosition++;
            else
                animPosition = 0;
        }
        private void ClientGrind()
        {
            if(Api.Side == EnumAppSide.Client)
            {
                this.Api.World.SpawnParticles(grindingParticles);

                if (!ambientSound.IsPlaying)
                    ambientSound.Start();
            }
        }
        private void FinishGrind(IPlayer byPlayer)
        {
            grindStartTime = -1;

            if(Api.Side == EnumAppSide.Server)
            {
                GiveGroundItem(byPlayer);
            }
            else if(Api.Side == EnumAppSide.Client)
            {
                StopAudio();
            }
        }
        //-- Try to give the player the contents of the mortar resource stack whenever the grind is finished. Puts resource on the ground if there is no room. --//
        private void GiveGroundItem(IPlayer byPlayer)
        {
            ItemStack groundItem = this.ResourceSlot.Itemstack.Collectible.GrindingProps.GroundStack.ResolvedItemstack.Clone();

            if (groundItem.StackSize == 0)
                groundItem.StackSize = 1;

            if (!byPlayer.InventoryManager.TryGiveItemstack(groundItem, true))
            {
                Api.World.SpawnItemEntity(groundItem, Pos.ToVec3d());
            }

            ResourceSlot.TakeOutWhole();
            ResourceSlot.MarkDirty();
        }
        private void StopAudio()
        {
            if (ambientSound.IsPlaying)
            {
                ambientSound.Stop();
            }
        }
        public void OnInteractStop()
        {
            animPosition = 0;
            grindStartTime = -1;

            if (Api.Side == EnumAppSide.Client)
            {
                if (ambientSound.IsPlaying)
                {
                    ambientSound.Stop();
                }
            }

            MarkDirty(true);
        }
        private void PrepareMesh(string shapeFolderLocation, ItemSlot inventorySlot, BlockMortar block, ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            string codePath = inventorySlot.Itemstack.Collectible.Code.Path;
            string resourcePath = shapeFolderLocation + inventorySlot.Itemstack.Collectible.Code.Domain + "_";

            foreach (char character in codePath)
            {
                if (character != '-')
                    resourcePath += character;
                else
                    resourcePath += '_';
            }

            //-- If no shape asset is found then a default mesh is used. --//
            if (Api.Assets.Exists(new AssetLocation(resourcePath + ".json")))
            {
                this.AddMesh(block, mesher, resourcePath, tessThreadTesselator.GetTexSource(block));
            }
            else
            {
                resourcePath = "ancienttools:shapes/block/mortar/resource_default";

                this.AddMesh(block, mesher, resourcePath, tessThreadTesselator.GetTexSource(block));
            }
        }
        private void AddMesh(BlockMortar block, ITerrainMeshPool mesher, string path, ITexPositionSource textureSource)
        {
            MeshData addMesh = block.GenMesh(Api as ICoreClientAPI, path, textureSource);
            mesher.AddMeshData(addMesh);
        }
        private void InsertObject(ItemSlot playerActiveSlot, ItemSlot inventorySlot, Item item, int takeQuantity)
        {
            inventorySlot.Itemstack = new ItemStack(item, takeQuantity);
            playerActiveSlot.TakeOut(takeQuantity);
            MarkDirty(true);
        }
        private void GiveObject(IPlayer byPlayer, ItemSlot inventorySlot)
        {
            byPlayer.InventoryManager.TryGiveItemstack(inventorySlot.TakeOutWhole());
            MarkDirty(true);
        }
        private void InitializeGindingParticles()
        {
            grindingParticles = new SimpleParticleProperties()
            {
                MinPos = new Vec3d(this.Pos.X + 0.25, this.Pos.Y + 0.3, this.Pos.Z + 0.25),
                AddPos = new Vec3d(0.5, 0.1, 0.5),

                GravityEffect = -0.02f,

                MinSize = 0.1f,
                MaxSize = 0.4f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.1f),

                MinQuantity = 1,
                AddQuantity = 2,

                LifeLength = 1.2f,
                addLifeLength = 1.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = particleColor,
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                ParticleModel = EnumParticleModel.Quad
            };
        }
        private void SetGrindingParticlesColor(string color1, string color2)
        {
            grindingParticles.Color = ParticleColor.GetColour(color1, color2);

            particleColor = grindingParticles.Color;
        }
    }
}
