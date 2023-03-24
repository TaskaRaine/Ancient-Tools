using AncientTools.Items;
using AncientTools.Utility;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AncientTools.BlockEntities
{
    class BESplitLog : DisplayInventory
    {
        private struct WedgeProperties
        {
            public EnumWedgeState WedgeState { get; set; }
            public EnumWedgeCardinalDirection CurrentCardinalPosition { get; set; }
            public Vec3d WedgePosition { get; set; }

            public WedgeProperties(EnumWedgeState state, EnumWedgeCardinalDirection direction, Vec3d position)
            {
                WedgeState = state;
                CurrentCardinalPosition = direction;
                WedgePosition = position;
            }
            public void RoundWedgePosition()
            {
                WedgePosition.X = Math.Round(WedgePosition.X * 8) / 8.0;
                WedgePosition.Y = Math.Round(WedgePosition.Y * 8) / 8.0;
                WedgePosition.Z = Math.Round(WedgePosition.Z * 8) / 8.0;
            }
        };

        public int WedgeCount { get; } = 4;
        public enum EnumWedgeState { Empty, Inserted, Smacked, Used }
        public enum EnumWedgeCardinalDirection { None, N, E, S, W }

        public bool SkipDefaultMesh { get; set; } = false;
        public string SplitBlockShapeLocation;
        public MeshData NewSplitLogMesh;

        public ItemSlot WedgeSlot(int index)
        {
            return GenericDisplayInventory[index];
        }
        public MeshData WedgeMesh(int index)
        {
            return Meshes[index];
        }
        public void SetWedgeMesh(int index, MeshData mesh)
        {
            Meshes[index] = mesh;
        }

        private WedgeProperties[] WedgeProps;

        public BESplitLog()
        {
            InventorySize = WedgeCount;
            InitializeInventory();
        }
        public override void InitializeInventory()
        {
            base.InitializeInventory();

            WedgeProps = new[] { 
                new WedgeProperties(EnumWedgeState.Empty, EnumWedgeCardinalDirection.None, Vec3d.Zero),
                new WedgeProperties(EnumWedgeState.Empty, EnumWedgeCardinalDirection.None, Vec3d.Zero),
                new WedgeProperties(EnumWedgeState.Empty, EnumWedgeCardinalDirection.None, Vec3d.Zero),
                new WedgeProperties(EnumWedgeState.Empty, EnumWedgeCardinalDirection.None, Vec3d.Zero),
            };
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            SetInventoryMaxSlotSize(1);
            UpdateMeshes();
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (SkipDefaultMesh == true)
            {
                AssetLocation location = new AssetLocation(SplitBlockShapeLocation + ".json");

                if(Capi.Assets.Exists(location))
                {
                    NewSplitLogMesh = this.GenMesh(Capi, SplitBlockShapeLocation, tessThreadTesselator.GetTexSource(this.Block));
                
                    if(NewSplitLogMesh != null)
                        this.AddMesh(mesher, NewSplitLogMesh);
                }
            }

            for (int i = 0; i < InventorySize; i++)
                if (!WedgeSlot(i).Empty)
                {
                    int alignmentRotation = 0;

                    if (i == 0 || i == 2)
                        alignmentRotation = 90;

                    this.AddMesh(mesher, WedgeMesh(i).Clone()
                        .Rotate(new Vec3f(0.5f, 0, 0.5f), 90 * GameMath.DEG2RAD, 0, alignmentRotation * GameMath.DEG2RAD)
                        .Translate(WedgeProps[i].WedgePosition.ToVec3f() - new Vec3f(0.5f, 0.0f, 0.5f)));
                }

            return SkipDefaultMesh;
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("skipdefaultmesh", SkipDefaultMesh);

            tree.SetDouble("wedge1posx", WedgeProps[0].WedgePosition.X);
            tree.SetDouble("wedge1posy", WedgeProps[0].WedgePosition.Y);
            tree.SetDouble("wedge1posz", WedgeProps[0].WedgePosition.Z);
            tree.SetInt("wedge1state", (int)WedgeProps[0].WedgeState);
            tree.SetInt("wedge1direction", (int)WedgeProps[0].CurrentCardinalPosition);

            tree.SetDouble("wedge2posx", WedgeProps[1].WedgePosition.X);
            tree.SetDouble("wedge2posy", WedgeProps[1].WedgePosition.Y);
            tree.SetDouble("wedge2posz", WedgeProps[1].WedgePosition.Z);
            tree.SetInt("wedge2state", (int)WedgeProps[1].WedgeState);
            tree.SetInt("wedge2direction", (int)WedgeProps[1].CurrentCardinalPosition);

            tree.SetDouble("wedge3posx", WedgeProps[2].WedgePosition.X);
            tree.SetDouble("wedge3posy", WedgeProps[2].WedgePosition.Y);
            tree.SetDouble("wedge3posz", WedgeProps[2].WedgePosition.Z);
            tree.SetInt("wedge3state", (int)WedgeProps[2].WedgeState);
            tree.SetInt("wedge3direction", (int)WedgeProps[2].CurrentCardinalPosition);

            tree.SetDouble("wedge4posx", WedgeProps[3].WedgePosition.X);
            tree.SetDouble("wedge4posy", WedgeProps[3].WedgePosition.Y);
            tree.SetDouble("wedge4posz", WedgeProps[3].WedgePosition.Z);
            tree.SetInt("wedge4state", (int)WedgeProps[3].WedgeState);
            tree.SetInt("wedge4direction", (int)WedgeProps[3].CurrentCardinalPosition);
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            SkipDefaultMesh = tree.GetBool("skipdefaultmesh");

            Vec3d wedge1Position = new Vec3d(tree.GetDouble("wedge1posx"), tree.GetDouble("wedge1posy"), tree.GetDouble("wedge1posz"));
            Vec3d wedge2Position = new Vec3d(tree.GetDouble("wedge2posx"), tree.GetDouble("wedge2posy"), tree.GetDouble("wedge2posz"));
            Vec3d wedge3Position = new Vec3d(tree.GetDouble("wedge3posx"), tree.GetDouble("wedge3posy"), tree.GetDouble("wedge3posz"));
            Vec3d wedge4Position = new Vec3d(tree.GetDouble("wedge4posx"), tree.GetDouble("wedge4posy"), tree.GetDouble("wedge4posz"));

            WedgeProps[0].WedgePosition = wedge1Position;
            WedgeProps[1].WedgePosition = wedge2Position;
            WedgeProps[2].WedgePosition = wedge3Position;
            WedgeProps[3].WedgePosition = wedge4Position;

            WedgeProps[0].WedgeState = (EnumWedgeState)tree.GetInt("wedge1state");
            WedgeProps[1].WedgeState = (EnumWedgeState)tree.GetInt("wedge2state");
            WedgeProps[2].WedgeState = (EnumWedgeState)tree.GetInt("wedge3state");
            WedgeProps[3].WedgeState = (EnumWedgeState)tree.GetInt("wedge4state");

            WedgeProps[0].CurrentCardinalPosition = (EnumWedgeCardinalDirection)tree.GetInt("wedge1direction");
            WedgeProps[1].CurrentCardinalPosition = (EnumWedgeCardinalDirection)tree.GetInt("wedge2direction");
            WedgeProps[2].CurrentCardinalPosition = (EnumWedgeCardinalDirection)tree.GetInt("wedge3direction");
            WedgeProps[3].CurrentCardinalPosition = (EnumWedgeCardinalDirection)tree.GetInt("wedge4direction");

            SplitBlockShapeLocation = GenerateSplitLogMeshFilenameFromWedgeDirection();

            if (worldForResolving.Side == EnumAppSide.Client && Api != null)
            {
                UpdateMeshes();
            }
        }
        public override void UpdateMeshes()
        {
            if (Api.Side != EnumAppSide.Client)
                return;

            for (int i = 0; i < InventorySize; i++)
            {
                if (WedgeSlot(i).Empty)
                    continue;

                CurrentObject = WedgeSlot(i).Itemstack.Collectible;
                MeshData mesh = GenMesh(GenericDisplayInventory[i].Itemstack);
                SetWedgeMesh(i, mesh);
            }

            MarkDirty(true);
        }
        public bool IsInventoryEmpty()
        {
            for (int i = 0; i < InventorySize; i++)
                if (!WedgeSlot(i).Empty)
                    return false;
            
            return true;
        }
        public bool IsInventoryFull()
        {
            for (int i = 0; i < InventorySize; i++)
                if (WedgeSlot(i).Empty)
                    return false;

            return true;
        }
        public bool HasUnsmackedWedges()
        {
            for (int i = 0; i < InventorySize; i++)
            {
                if (WedgeProps[i].WedgeState == EnumWedgeState.Inserted)
                    return true;
            }

            return false;
        }
        public bool InsertedExists()
        {
            for (int i = 0; i < InventorySize; i++)
            {
                if (WedgeSlot(i).Empty)
                    continue;

                if (WedgeProps[i].WedgeState == EnumWedgeState.Inserted)
                    return true;
            }

            return false;
        }
        public bool OnInteract(IPlayer byPlayer, int index, Vec3d hitPosition)
        {
            CollectibleObject activeCollectible = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible;

            if (activeCollectible == null)
            {
                if (!WedgeSlot(index).Empty)
                {
                    GiveObject(byPlayer, index);
                    return true;
                }
            }
            else
            {
                if (activeCollectible is ItemWedge && WedgeProps[index].WedgeState == EnumWedgeState.Empty)
                {
                    InsertWedge(byPlayer, index, hitPosition);
                    return true;
                }
            }

            return false;
        }
        public void GiveObject(IPlayer byPlayer, int index)
        {
            if(byPlayer.InventoryManager.TryGiveItemstack(WedgeSlot(index).TakeOutWhole()))
            {
                if (WedgeProps[index].WedgeState == EnumWedgeState.Inserted)
                    WedgeProps[index].WedgeState = EnumWedgeState.Empty;

                UpdateMeshes();

                WedgeSlot(index).MarkDirty();
                MarkDirty(true);
            }
        }
        public void InsertWedge(IPlayer byPlayer, int index, Vec3d hitPosition)
        {
            if(byPlayer.InventoryManager.ActiveHotbarSlot.TryPutInto(Api.World, WedgeSlot(index), 1) >= 1)
            {
                WedgeProps[index].WedgeState = EnumWedgeState.Inserted;
                WedgeProps[index].CurrentCardinalPosition = (EnumWedgeCardinalDirection)index + 1;
                WedgeProps[index].WedgePosition = hitPosition;
                WedgeProps[index].RoundWedgePosition();
                UpdateMeshes();

                WedgeSlot(index).MarkDirty();
                MarkDirty(true);
            }
        }
        public void SmackWedge(int index, IPlayer byPlayer)
        {
            if (byPlayer == null)
                return;

            if (WedgeProps[index].WedgeState == EnumWedgeState.Inserted)
            {
                WedgeProps[index].WedgePosition.Y = WedgeProps[index].WedgePosition.Y - 0.2;
                WedgeProps[index].WedgeState = EnumWedgeState.Smacked;

                WedgeSlot(index)?.Itemstack?.Collectible?.DamageItem(Api.World, byPlayer.Entity, WedgeSlot(index), 1);

                ProcessPlayerInteraction(byPlayer);

                SplitBlockShapeLocation = GenerateSplitLogMeshFilenameFromWedgeDirection();

                UpdateMeshes();
                MarkDirty(true);
            }
        }
        public int GetSmackedCount()
        {
            return WedgeCount;
        }
        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture)
        {
            Shape shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

            MeshData wholeMesh;

            capi.Tesselator.TesselateShape("mortar", shape, out wholeMesh, capi.Tesselator.GetTexSource(this.Block));

            return wholeMesh;
        }
        private void ProcessPlayerInteraction(IPlayer byPlayer)
        {
            int smackedWedges, usedWedges;

            DetermineWedgeStateCounts(out smackedWedges, out usedWedges);
            DropBeams(byPlayer, smackedWedges, usedWedges);
        }
        private void DetermineWedgeStateCounts(out int smackedWedges, out int usedWedges)
        {
            smackedWedges = 0;
            usedWedges = 0;

            foreach (WedgeProperties wedgeproperty in WedgeProps)
            {
                switch(wedgeproperty.WedgeState)
                {
                    case EnumWedgeState.Smacked:
                        smackedWedges++;
                        break;
                    case EnumWedgeState.Used:
                        usedWedges++;
                        break;
                    default: break;
                }
            }
        }
        private string GenerateSplitLogMeshFilenameFromWedgeDirection()
        {
            string filenamePart = "ancienttools:shapes/block/splitlogs/splitlog_";

            foreach (WedgeProperties wedgeproperty in WedgeProps)
            {
                switch(wedgeproperty.CurrentCardinalPosition)
                {
                    case EnumWedgeCardinalDirection.None:
                        break;
                    default:
                        if(wedgeproperty.WedgeState >= EnumWedgeState.Smacked)
                            filenamePart += Enum.GetName(typeof(EnumWedgeCardinalDirection), wedgeproperty.CurrentCardinalPosition).ToLower();
                        break;
                }
            }

            return filenamePart;
        }
        private void DropBeams(IPlayer byPlayer, int smackedCount, int usedCount)
        {
            int totalCount = smackedCount + usedCount;

            if (totalCount <= 1 || totalCount == 2 && SmackedWedgesAreParallel())
                return;

            SkipDefaultMesh = true;

            if(Api.Side == EnumAppSide.Server)
            {
                ItemStack beamStack = new ItemStack(Api.World.GetItem(new AssetLocation("ancienttools", "beam-" + Block.LastCodePart(1))));

                if (totalCount == 4)
                {
                    Api.World.SpawnItemEntity(beamStack.Clone(), Pos.ToVec3d());
                    Api.World.SpawnItemEntity(beamStack.Clone(), Pos.ToVec3d());

                    Api.World.BlockAccessor.BreakBlock(this.Pos, byPlayer, 0);
                    return;
                }
                else if (totalCount >= 2)
                {
                    Api.World.SpawnItemEntity(beamStack.Clone(), Pos.ToVec3d());

                    for (int i = 0; i < 4; i++)
                    {
                        if (WedgeProps[i].WedgeState == EnumWedgeState.Smacked)
                        {
                            WedgeProps[i].WedgeState = EnumWedgeState.Used;

                            if(!WedgeSlot(i).Empty)
                            {    
                                Api.World.SpawnItemEntity(WedgeSlot(i).TakeOutWhole(), Pos.ToVec3d());
                                WedgeSlot(i).MarkDirty();
                            }
                        }
                    }
                }
            }
        }
        private bool SmackedWedgesAreParallel()
        {
            if (WedgeProps[0].WedgeState == EnumWedgeState.Smacked && WedgeProps[2].WedgeState == EnumWedgeState.Smacked)
                return true;

            if (WedgeProps[1].WedgeState == EnumWedgeState.Smacked && WedgeProps[3].WedgeState == EnumWedgeState.Smacked)
                return true;

            return false;
        }
    }
}
