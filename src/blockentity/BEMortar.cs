using AncientTools.BlockEntityRenderer;
using AncientTools.Blocks;
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
        private bool isGrinding = false;
        private bool isRendered = false;
        private long grindTickListener = -1;

        internal InventoryGeneric inventory;

        private ILoadedSound ambientSound, stoneSound;

        private SimpleParticleProperties grindingParticles;

        private MortarPestleRenderer pestleRenderer;
        private Vec3f lookAtPlayerVector = Vec3f.Zero;
        private Vec3f initialAnimationRotation = new Vec3f(0.0f, 0.15f, 0.0f);

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
            if (grindTickListener != -1) Api.Event.UnregisterGameTickListener(grindTickListener);
        }
        public override void Initialize(ICoreAPI api)
        {
            //-- Holds mesh data of items inserted into the mortar. The resource mesh is NOT used as the resource appearance is not actually represented by the inserted resrouce. --//
            meshes = new MeshData[2];

            base.Initialize(api);

            if (api.Side == EnumAppSide.Client)
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

                //-- The renderer is responsible for drawing and animating the pestle while a pestle is in the mortar inventory --//
                pestleRenderer = new MortarPestleRenderer(api as ICoreClientAPI, Pos);
                (api as ICoreClientAPI).Event.RegisterRenderer(pestleRenderer, EnumRenderStage.Opaque, "mortar");

                if (!PestleSlot.Empty)
                {
                    pestleRenderer.UpdateMesh(meshes[1]);
                    pestleRenderer.SetPestleLookAtVector(lookAtPlayerVector.X, lookAtPlayerVector.Y, lookAtPlayerVector.Z);
                    pestleRenderer.ShouldRender = true;
                }
            }
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

            if(pestleRenderer != null)
                pestleRenderer.Dispose();
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            if (pestleRenderer != null)
                pestleRenderer.Dispose();
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            BlockMortar block = Api.World.BlockAccessor.GetBlock(Pos) as BlockMortar;

            if (!ResourceSlot.Empty)
            {
                PrepareMesh("ancienttools:shapes/block/mortar/resource_", ResourceSlot, block, mesher, tessThreadTesselator);
            }

            return false;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            lookAtPlayerVector = new Vec3f(tree.GetFloat("x"), tree.GetFloat("y"), tree.GetFloat("z"));
            isGrinding = tree.GetBool("isGrinding", false);
            isRendered = tree.GetBool("isRendered", false);

            if(pestleRenderer != null)
            {
                //-- If the isRendered value retreived from the server is true, then display the pestle --//
                if (isRendered)
                {
                    this.updateMesh(1);
                    pestleRenderer.UpdateMesh(meshes[1]);
                    pestleRenderer.SetPestleLookAtVector(lookAtPlayerVector.X, lookAtPlayerVector.Y, lookAtPlayerVector.Z);
                    pestleRenderer.ShouldRender = true;

                    //-- If the isGrinding value retreived from the server is true, then begin to animate the pestle. Start playing the repeating ambient grind sound and begin a tick listener that spawns particles --//
                    if (isGrinding)
                    {
                        pestleRenderer.ShouldAnimate = true;

                        if(!ambientSound.IsPlaying)
                            ambientSound.Start();

                        if (grindTickListener == -1)
                            grindTickListener = this.Api.Event.RegisterGameTickListener(Grind, 16);
                    }
                    else
                    {
                        pestleRenderer.ShouldAnimate = false;

                        if (ambientSound.IsPlaying)
                            ambientSound.Stop();

                        this.Api.Event.UnregisterGameTickListener(grindTickListener);
                        grindTickListener = -1;
                    }
                }
                else
                {
                    pestleRenderer.ShouldRender = false;
                }
            }
        }
        //-- Pestle rotation is saved so that things look as they were left proper on game load --//
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetFloat("x", lookAtPlayerVector.X + initialAnimationRotation.X);
            tree.SetFloat("y", lookAtPlayerVector.Y + initialAnimationRotation.Y);
            tree.SetFloat("z", lookAtPlayerVector.Z + initialAnimationRotation.Z);

            tree.SetBool("isRendered", !PestleSlot.Empty);
            tree.SetBool("isGrinding", isGrinding);
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

                        if (Api.Side == EnumAppSide.Client)
                        {
                            pestleRenderer.ShouldRender = false;
                        }
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
                                    InsertObject(activeSlot, PestleSlot, activeSlot.Itemstack.Item, 1);

                                    if (Api.Side == EnumAppSide.Client)
                                    {
                                        this.updateMesh(1);

                                        pestleRenderer.UpdateMesh(meshes[1]);
                                        pestleRenderer.ShouldRender = true;

                                        stoneSound.Start();
                                    }
                                }
                            }
                        }

                        if (ResourceSlot.Empty && !activeSlot.Empty)
                        {
                            if (activeSlot.Itemstack.Collectible.ItemClass == EnumItemClass.Item)
                            {
                                if (activeSlot.Itemstack.Item.GrindingProps != null)
                                {
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
            if (!byPlayer.Entity.Controls.Sneak || PestleSlot.Empty || ResourceSlot.Empty || !byPlayer.InventoryManager.ActiveHotbarSlot.Empty || ResourceSlot.Itemstack.Item.GrindingProps == null)
            {
                return false;
            }

            BeginGrind(byPlayer);
            PerformGrind(byPlayer);

            return true;
        }
        private void BeginGrind(IPlayer byPlayer)
        {
            if (grindStartTime < 0)
            {
                grindStartTime = Api.World.ElapsedMilliseconds;
                isGrinding = true;

                SetPestleLookAtVector(byPlayer);

                if (Api.Side == EnumAppSide.Client)
                {
                    ambientSound.Start();

                    pestleRenderer.ShouldAnimate = true;
                }

                MarkDirty(true);
            }
        }
        private void PerformGrind(IPlayer byPlayer)
        {
            if (Api.World.ElapsedMilliseconds < grindStartTime + GRIND_TIME_IN_SECONDS * 1000)
            {
                if (Api.Side == EnumAppSide.Client)
                    pestleRenderer.SetPestleLookAtVector(byPlayer);
            }
            else
            {
                FinishGrind(byPlayer);
                OnInteractStop();
            }

            if(isGrinding)
            {
                if (grindTickListener == -1)
                    grindTickListener = this.Api.Event.RegisterGameTickListener(Grind, 16);
            }
        }
        //-- Spawns grind particles every time the function is called --//
        private void Grind(float deltaTime)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                grindingParticles.Color = ResourceSlot.Itemstack.Collectible.GetRandomColor(capi, ResourceSlot.Itemstack);
                this.Api.World.SpawnParticles(grindingParticles);
            }
        }
        private void FinishGrind(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Server)
            {
                if(ResourceSlot.Itemstack.Item.GrindingProps != null)
                    GiveGroundItem(byPlayer);
            }

            SetPestleLookAtVector(byPlayer);
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
            grindStartTime = -1;
            isGrinding = false;

            if (Api.Side == EnumAppSide.Client)
            {
                //-- Immediately stop audio and animation when the interacting player stops using the pestle, or grind has completed --//
                StopAudio();
                pestleRenderer.ShouldAnimate = false;

                this.Api.Event.UnregisterGameTickListener(grindTickListener);
                grindTickListener = -1;
            }

            MarkDirty(true);
        }
        public void SetPestleLookAtVector(IPlayer byPlayer)
        {
            Vec3f normal = ((this.Pos.ToVec3f() + new Vec3f(0.5f, 0, 0.5f)) - byPlayer.Entity.Pos.XYZFloat);
            normal.Y = 0.325f;

            double pitch = Math.Asin(normal.Y);
            double yaw = Math.Atan2(normal.X, normal.Z);

            lookAtPlayerVector.Y = (float)yaw;
            lookAtPlayerVector.Z = (float)pitch;
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
            playerActiveSlot.TryPutInto(Api.World, inventorySlot, takeQuantity);

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

                LifeLength = 0.2f,
                addLifeLength = 0.4f,

                ShouldDieInLiquid = true,

                WithTerrainCollision = true,

                Color = 0,
                VertexFlags = 0,
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 255),

                ParticleModel = EnumParticleModel.Quad
            };
        }
    }
}
