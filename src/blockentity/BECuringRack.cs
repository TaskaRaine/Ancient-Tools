using AncientTools.Blocks;
using AncientTools.Items;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntities
{
    class BECuringRack : BlockEntityDisplay
    {
        internal InventoryGeneric inventory;

        public override InventoryBase Inventory => inventory;

        public override string InventoryClassName => "curingrackcontainer";

        public ItemSlot HookItemslot1
        {
            get { return inventory[0]; }
        }
        public ItemSlot HookItemslot2
        {
            get { return inventory[1]; }
        }

        public ItemSlot MeatSlot(int slotIndex)
        {
            return inventory[1 + slotIndex];
        }

        private double previousHourChecked;
        private double thisHourChecked;

        private string facing;

        private ICoreAPI coreAPI;
        private long tickListener;

        public BECuringRack()
        {
            inventory = new InventoryGeneric(10, null, null);
        }
        public override void Initialize(ICoreAPI api)
        {
            meshes = new MeshData[10];
            facing = this.Block.LastCodePart(1);

            if (this.Block != null && this.Block.Attributes != null)
            {
                inventory.TransitionableSpeedMulByType = this.Block.Attributes["transitionrate"].AsObject<Dictionary<EnumTransitionType, float>>();
            }

            base.Initialize(api);

            //-- Check the meat every in-game hour to advance the curing process --//
            if (api.Side == EnumAppSide.Server)
            {
                coreAPI = api;
                tickListener = api.World.RegisterGameTickListener(HourlyMeatCheck, (int)(3600000 / api.World.Calendar.SpeedOfTime));

                HourlyMeatCheck(0);

                this.MarkDirty(false);
            }
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            previousHourChecked = tree.GetDouble("previoushourchecked", worldForResolving.Calendar.TotalHours);
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            if (previousHourChecked != 0)
            {
                tree.SetDouble("previoushourchecked", previousHourChecked);
            }
            else
            {
                tree.SetDouble("previoushourchecked", Api.World.Calendar.TotalHours);
                previousHourChecked = Api.World.Calendar.TotalHours;
            }
        }
        public override void OnBlockBroken(IPlayer player)
        {
            base.OnBlockBroken(player);

            if (Api.Side == EnumAppSide.Server)
                Api.World.UnregisterGameTickListener(tickListener);
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            if (Api.Side == EnumAppSide.Server)
                Api.World.UnregisterGameTickListener(tickListener);
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            if(Api.Side == EnumAppSide.Server)
                Api.World.UnregisterGameTickListener(tickListener);
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if(facing == "ns")
            {
                PositionMeshesNS(mesher);
            }
            else if(facing == "ew")
            {
                PositionMeshesEW(mesher);
            }
            
            return false;
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            dsc.AppendLine(Lang.Get("ancienttools:blockinfo-curingrack-meat-perish",
                Math.Round(GetCurrentPerishRate(), 2)));
        }
        public void OnInteract(IPlayer byPlayer, int selectionBoxIndex)
        {
            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if(!activeSlot.Empty)
            {
                if(activeSlot.Itemstack.Collectible.Code.Domain == "ancienttools")
                {
                    if(activeSlot.Itemstack.Collectible.FirstCodePart() == "curinghook")
                    {
                        if(selectionBoxIndex == 1 && HookItemslot1.Empty)
                        {
                            InsertObject(activeSlot, HookItemslot1, activeSlot.Itemstack.Item, 1);
                            this.updateMesh(0);
                        }

                        if(selectionBoxIndex == 0 && HookItemslot2.Empty)
                        {
                            InsertObject(activeSlot, HookItemslot2, activeSlot.Itemstack.Item, 1);
                            this.updateMesh(1);
                        }
                    }
                    else if(activeSlot.Itemstack.Collectible is ItemSaltedMeat && activeSlot.Itemstack.Collectible.FirstCodePart(2) == "raw")
                    {
                        if (selectionBoxIndex == 0 && !HookItemslot2.Empty)
                        {
                            if (MeatSlot(5).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(5), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(5).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(5).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(5).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(6);
                            }
                            else if (MeatSlot(6).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(6), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(6).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(6).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(6).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(7);
                            }
                            else if (MeatSlot(7).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(7), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(7).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(7).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(7).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());
                                
                                this.updateMesh(8);
                            }
                            else if (MeatSlot(8).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(8), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(8).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(8).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(8).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(9);
                            }
                        }

                        if (selectionBoxIndex == 1 && !HookItemslot1.Empty)
                        {
                            if (MeatSlot(1).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(1), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(1).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(1).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(1).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(2);
                            }
                            else if (MeatSlot(2).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(2), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(2).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(2).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(2).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(3);
                            }
                            else if (MeatSlot(3).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(3), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(3).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(3).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(3).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(4);
                            }
                            else if (MeatSlot(4).Empty)
                            {
                                InsertObject(activeSlot, MeatSlot(4), activeSlot.Itemstack.Item, 1);

                                if (!MeatSlot(4).Itemstack.Attributes.HasAttribute("curinghoursremaining"))
                                    MeatSlot(4).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(4).Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());

                                this.updateMesh(5);
                            }
                        }
                    }
                }
            }
            else
            {
                if(selectionBoxIndex == 0 && !HookItemslot2.Empty)
                {
                    if (!HookEmpty(HookItemslot2))
                    {
                        GiveMeat(byPlayer, HookItemslot2);
                    }
                    else
                    {
                        GiveRackItem(byPlayer, HookItemslot2);
                    }
                }
                else if(selectionBoxIndex == 1 && !HookItemslot1.Empty)
                {
                    if (!HookEmpty(HookItemslot1))
                    {
                        GiveMeat(byPlayer, HookItemslot1);
                    }
                    else
                    {
                        GiveRackItem(byPlayer, HookItemslot1);
                    }
                }
            }

            this.MarkDirty();
        }
        public float GetCurrentPerishRate()
        {
            if(inventory.TransitionableSpeedMulByType != null)
                return inventory.TransitionableSpeedMulByType[EnumTransitionType.Perish];

            return 1.0f;
        }
        private void HourlyMeatCheck(float deltaTime)
        {
            thisHourChecked = coreAPI.World.Calendar.TotalHours;

            for(int i = 1; i < 9; i++)
            {
                if(!MeatSlot(i).Empty)
                {
                    Item meatItem = MeatSlot(i).Itemstack.Item;

                    if (meatItem == null)
                        return;

                    if (meatItem.FirstCodePart() == "saltedmeat" && meatItem.LastCodePart() == "raw")
                    {
                        double timeRemaining = MeatSlot(i).Itemstack.Attributes.GetDouble("curinghoursremaining", 480);
                        
                        if (timeRemaining <= 0)
                        {
                            string meatType = MeatSlot(i).Itemstack.Item.FirstCodePart(1) + "-cured";

                            MeatSlot(i).Itemstack = new ItemStack(coreAPI.World.GetItem(new AssetLocation("game", meatType)));

                            this.MarkDirty(true);
                        }
                        else
                        {
                            MeatSlot(i).Itemstack.Attributes.SetDouble("curinghoursremaining", timeRemaining - (thisHourChecked - previousHourChecked));
                        }
                    }

                    MeatSlot(i).MarkDirty();
                }
            }

            previousHourChecked = thisHourChecked;
        }
        private void PositionMeshesNS(ITerrainMeshPool mesher)
        {
            Vec3f meatTranslation = new Vec3f(0, 0.55f, 0);

            if (!HookItemslot1.Empty)
            {
                mesher.AddMeshData(meshes[0].Clone()
                    .Translate(new Vec3f(0.171875f, 0, 0)));
            }

            if (!HookItemslot2.Empty)
            {
                mesher.AddMeshData(meshes[1].Clone()
                    .Translate(new Vec3f(-0.171875f, 0, 0)));
            }

            if (!MeatSlot(1).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(1).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[2].Clone()
                    .Translate(meatTranslation + offset)
                    .Rotate(new Vec3f(0.671875f, 0.4375f, 0.5f), 0, 1.5708f, 1.5708f)
                    .Scale(new Vec3f(0.671875f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(2).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(2).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[3].Clone()
                    .Translate(new Vec3f(0, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.671875f, 0.4375f, 0.5f), 0, 3.1415f, 1.5708f)
                    .Scale(new Vec3f(0.671875f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(3).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(3).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[4].Clone()
                    .Translate(new Vec3f(0, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.671875f, 0.4375f, 0.5f), 0, 4.7123f, 1.5708f)
                    .Scale(new Vec3f(0.671875f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(4).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(4).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[5].Clone()
                    .Translate(new Vec3f(0, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.671875f, 0.4375f, 0.5f), 0, 6.283f, 1.5708f)
                    .Scale(new Vec3f(0.671875f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(5).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(5).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[6].Clone()
                    .Translate(new Vec3f(-0.35f, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.328125f, 0.4375f, 0.5f), 0, 1.5708f, 1.5708f)
                    .Scale(new Vec3f(0.328125f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(6).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(6).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[7].Clone()
                    .Translate(new Vec3f(-0.35f, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.328125f, 0.4375f, 0.5f), 0, 3.1415f, 1.5708f)
                    .Scale(new Vec3f(0.328125f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(7).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(7).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[8].Clone()
                    .Translate(new Vec3f(-0.35f, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.328125f, 0.4375f, 0.5f), 0, 4.7123f, 1.5708f)
                    .Scale(new Vec3f(0.328125f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }

            if (!MeatSlot(8).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(8).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[9].Clone()
                    .Translate(new Vec3f(-0.35f, 0.55f, 0) + offset)
                    .Rotate(new Vec3f(0.328125f, 0.4375f, 0.5f), 0, 6.283f, 1.5708f)
                    .Scale(new Vec3f(0.328125f, 0.4375f, 0.5f), 0.8f, 0.8f, 0.8f));
            }
        }
        private void PositionMeshesEW(ITerrainMeshPool mesher)
        {
            Vec3f meatTranslationHook1 = new Vec3f(0, 0.40f, 0.171875f);
            Vec3f meatTranslationHook2 = new Vec3f(0, 0.40f, -0.171875f);

            if (!HookItemslot1.Empty)
            {
                mesher.AddMeshData(meshes[0].Clone()
                    .Rotate(new Vec3f(0.5f, 0.603125f, 0.5f),  0.0f, 1.57079633f, 0.0f)
                    .Translate(new Vec3f(0, 0, 0.171875f)));
            }

            if (!HookItemslot2.Empty)
            {
                mesher.AddMeshData(meshes[1].Clone()
                    .Rotate(new Vec3f(0.5f, 0.603125f, 0.5f), 0.0f, 1.57079633f, 0.0f)
                    .Translate(new Vec3f(0, 0, -0.171875f)));
            }

            if (!MeatSlot(1).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(1).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.X = 0.075f;
                }

                mesher.AddMeshData(meshes[2].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 1.5708f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook1 + offset));
            }

            if (!MeatSlot(2).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(2).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = -0.075f;
                }

                mesher.AddMeshData(meshes[3].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 3.1415f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook1 + offset));
            }

            if (!MeatSlot(3).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(3).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.X = -0.075f;
                }

                mesher.AddMeshData(meshes[4].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 4.7124f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook1 + offset));
            }

            if (!MeatSlot(4).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(4).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[5].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 6.2832f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook1 + offset));
            }

            if (!MeatSlot(5).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(5).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.X = 0.075f;
                }

                mesher.AddMeshData(meshes[6].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 1.5708f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook2 + offset));
            }

            if (!MeatSlot(6).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(6).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = -0.075f;
                }

                mesher.AddMeshData(meshes[7].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 3.1415f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook2 + offset));
            }

            if (!MeatSlot(7).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(7).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.X = -0.075f;
                }

                mesher.AddMeshData(meshes[8].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 4.7124f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook2 + offset));
            }

            if (!MeatSlot(8).Empty)
            {
                Vec3f offset = Vec3f.Zero;

                if (MeatSlot(8).Itemstack.Item.LastCodePart(1) == "redmeat")
                {
                    offset.Z = 0.075f;
                }

                mesher.AddMeshData(meshes[9].Clone()
                    .Rotate(new Vec3f(0.5f, -0.1f, 0.5f), 0, 6.2832f, 1.5708f)
                    .Scale(new Vec3f(0.5f, -0.1f, 0.5f), 0.8f, 0.8f, 0.8f)
                    .Translate(meatTranslationHook2 + offset));
            }
        }
        private void InsertObject(ItemSlot playerActiveSlot, ItemSlot inventorySlot, Item item, int takeQuantity)
        {
            playerActiveSlot.TryPutInto(Api.World, inventorySlot, takeQuantity);
            MarkDirty(true);
        }
        private void GiveRackItem(IPlayer byPlayer, ItemSlot itemslot)
        {
            ItemStack item = itemslot.Itemstack;

            byPlayer.Entity.TryGiveItemStack(item);

            itemslot.TakeOutWhole();
            itemslot.MarkDirty();

            this.MarkDirty(true);
        }
        private void GiveMeat(IPlayer byPlayer, ItemSlot itemslot)
        {
            if(itemslot == HookItemslot1)
            {
                for (int i = 4; i > 0; i--)
                {
                    if (!MeatSlot(i).Empty)
                    {
                        GiveRackItem(byPlayer, MeatSlot(i));
                        break;
                    }
                }
            }
            else
            {
                for (int i = 8; i > 4; i--)
                {
                    if (!MeatSlot(i).Empty)
                    {
                        GiveRackItem(byPlayer, MeatSlot(i));
                        break;
                    }
                }
            }
        }
        private bool HookFull(ItemSlot hookSlot)
        {
            if (hookSlot == HookItemslot1)
            {
                if (!MeatSlot(1).Empty && !MeatSlot(2).Empty && !MeatSlot(3).Empty && !MeatSlot(4).Empty)
                    return true;

                return false;
            }
            else
            {
                if (!MeatSlot(5).Empty && !MeatSlot(6).Empty && !MeatSlot(7).Empty && !MeatSlot(8).Empty)
                    return true;

                return false;
            }
        }
        private bool HookEmpty(ItemSlot hookSlot)
        {
            if (hookSlot == HookItemslot1)
            {
                if (MeatSlot(1).Empty && MeatSlot(2).Empty && MeatSlot(3).Empty && MeatSlot(4).Empty)
                    return true;

                return false;
            }
            else
            {
                if (MeatSlot(5).Empty && MeatSlot(6).Empty && MeatSlot(7).Empty && MeatSlot(8).Empty)
                    return true;

                return false;
            }
        }
    }
}
