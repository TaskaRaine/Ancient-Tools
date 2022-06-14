using AncientTools.Blocks;
using AncientTools.Items;
using AncientTools.Utility;
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
    enum RackFacing { NorthSouth, EastWest };

    class BECuringRack : DisplayInventory
    {
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

        private RackFacing facing;

        private ICoreAPI coreAPI;
        private long tickListener;

        public BECuringRack()
        {
            InventorySize = 10;
            InitializeInventory();
        }
        public override void Initialize(ICoreAPI api)
        {
            facing = Block.CodeWithVariant("rotation", "ns").Equals(Block.Code) ? RackFacing.NorthSouth : RackFacing.EastWest;

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
            }

            UpdateMeshes();
            MarkDirty(true);
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            previousHourChecked = tree.GetDouble("previoushourchecked", worldForResolving.Calendar.TotalHours);

            if (Api == null || Api.Side == EnumAppSide.Server)
                return;
            else
                UpdateMeshes();
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
            PositionMeshes(mesher, facing);
            
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
                if (activeSlot.Itemstack.Collectible.Code.BeginsWith("ancienttools", "curinghook"))
                {
                    if(selectionBoxIndex == 1 && HookItemslot1.Empty)
                    {
                        InsertObject(activeSlot, HookItemslot1, activeSlot.Itemstack.Item, 1);
                        this.UpdateMesh(0);
                    }

                    if(selectionBoxIndex == 0 && HookItemslot2.Empty)
                    {
                        InsertObject(activeSlot, HookItemslot2, activeSlot.Itemstack.Item, 1);
                        this.UpdateMesh(1);
                    }
                }
                else
                {
                    if (activeSlot.Itemstack.Collectible.Attributes == null || !activeSlot.Itemstack.Collectible.Attributes["onCuringRackProps"].Exists)
                        return;
                    
                    if (selectionBoxIndex == 0 && !HookItemslot2.Empty)
                    {
                        if (MeatSlot(5).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(5), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(5).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(5).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(5).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(5).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(6);
                        }
                        else if (MeatSlot(6).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(6), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(6).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(6).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(6).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(6).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(7);
                        }
                        else if (MeatSlot(7).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(7), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(7).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(7).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(7).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(7).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());
                                
                            this.UpdateMesh(8);
                        }
                        else if (MeatSlot(8).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(8), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(8).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(8).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(8).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(8).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(9);
                        }
                    }

                    if (selectionBoxIndex == 1 && !HookItemslot1.Empty)
                    {
                        if (MeatSlot(1).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(1), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(1).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(1).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(1).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(1).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(2);
                        }
                        else if (MeatSlot(2).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(2), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(2).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(2).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(2).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(2).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(3);
                        }
                        else if (MeatSlot(3).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(3), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(3).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(3).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(3).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(3).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(4);
                        }
                        else if (MeatSlot(4).Empty)
                        {
                            InsertObject(activeSlot, MeatSlot(4), activeSlot.Itemstack.Item, 1);

                            if (!MeatSlot(4).Itemstack.Attributes.HasAttribute("curinghoursremaining") && MeatSlot(4).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"].Exists)
                                MeatSlot(4).Itemstack.Attributes.SetDouble("curinghoursremaining", MeatSlot(4).Itemstack.ItemAttributes["onCuringRackProps"]["conversionProps"]["totalCuringHours"].AsDouble());

                            this.UpdateMesh(5);
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
                    CollectibleObject meatItem = MeatSlot(i).Itemstack.Collectible;

                    if (meatItem == null || meatItem.Attributes == null)
                        return;

                    if (meatItem.Attributes["onCuringRackProps"]["conversionProps"].Exists)
                    {
                        double timeRemaining = MeatSlot(i).Itemstack.Attributes.GetDouble("curinghoursremaining", 480);
                        
                        if (timeRemaining - (thisHourChecked - previousHourChecked) <= 0)
                        {
                            switch(meatItem.Attributes["onCuringRackProps"]["conversionProps"]["convertedItemstackType"].AsString())
                            {
                                case "item":
                                    {
                                        AssetLocation itemLocation = new AssetLocation(meatItem.Attributes["onCuringRackProps"]["conversionProps"]["convertIntoItemstack"].ToString());

                                        MeatSlot(i).Itemstack = new ItemStack(coreAPI.World.GetItem(itemLocation));

                                        break;
                                    }
                                case "block":
                                    {
                                        AssetLocation itemLocation = new AssetLocation(meatItem.Attributes["onCuringRackProps"]["conversionProps"]["convertIntoItemstack"].ToString());

                                        MeatSlot(i).Itemstack = new ItemStack(coreAPI.World.GetBlock(itemLocation));

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }

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
        private void PositionMeshes(ITerrainMeshPool mesher, RackFacing facing)
        {
            Vec3f defaultOrigin = new Vec3f(0.5f, 0.0f, 0.5f);

            Vec3f translationHook1;
            Vec3f translationHook2;

            Vec3f resourceTranslationHook1;
            Vec3f resourceTranslationHook2;

            Vec3f resourceOriginHook1;
            Vec3f resourceOriginHook2;

            Vec3f attributeTranslation = Vec3f.Zero;
            Vec3f attributeRotateAround = new Vec3f(0.0f, 90.0f, 0.0f);
            Vec3f attributeRotation = Vec3f.Zero;
            Vec3f attributeScale = new Vec3f(1.0f, 1.0f, 1.0f);

            float attributeVerticalOffset = 0.0f;
            float attributeHorizontalOffset = 0.0f;
            float attributeDepthOffset = 0.0f;

            Vec3f[] attributeManualTranslation = new Vec3f[] { Vec3f.Zero, Vec3f.Zero, Vec3f.Zero, Vec3f.Zero };

            Vec3f attributeExtraRotation = Vec3f.Zero;

            Vec3f[] attributeExtraManualTranslation = new Vec3f[] { Vec3f.Zero, Vec3f.Zero, Vec3f.Zero, Vec3f.Zero };

            if (facing == RackFacing.NorthSouth)
            {
                translationHook1 = new Vec3f(0.171875f, 0, 0);
                translationHook2 = new Vec3f(-0.171875f, 0, 0);
                resourceTranslationHook1 = new Vec3f(0, 0.55f, 0);
                resourceTranslationHook2 = new Vec3f(-0.35f, 0.55f, 0);
                resourceOriginHook1 = new Vec3f(0.671875f, 0.4375f, 0.5f);
                resourceOriginHook2 = new Vec3f(0.328125f, 0.4375f, 0.5f);
            }
            else
            {
                translationHook1 = new Vec3f(0, 0, 0.171875f);
                translationHook2 = new Vec3f(0, 0, -0.171875f);
                resourceTranslationHook1 = new Vec3f(-0.175f, 0.55f, 0.175f);
                resourceTranslationHook2 = new Vec3f(-0.175f, 0.55f, -0.175f);
                resourceOriginHook1 = new Vec3f(0.5f, 0.4375f, 0.671875f);
                resourceOriginHook2 = new Vec3f(0.5f, 0.4375f, 0.328125f);
            }

            if (!HookItemslot1.Empty)
            {
                switch(facing)
                {
                    case RackFacing.NorthSouth:
                        mesher.AddMeshData(meshes[0].Clone()
                        .Translate(translationHook1));
                        break;
                    case RackFacing.EastWest:
                        mesher.AddMeshData(meshes[0].Clone()
                        .Translate(translationHook1)
                        .Rotate(resourceOriginHook1, 0.0f, 90 * GameMath.DEG2RAD, 0.0f));
                        break;
                }
            }

            if (!HookItemslot2.Empty)
            {
                switch (facing)
                {
                    case RackFacing.NorthSouth:
                        mesher.AddMeshData(meshes[1].Clone()
                        .Translate(translationHook2));
                        break;
                    case RackFacing.EastWest:
                        mesher.AddMeshData(meshes[1].Clone()
                        .Translate(translationHook2)
                        .Rotate(resourceOriginHook2, 0.0f, 90 * GameMath.DEG2RAD, 0.0f));
                        break;
                }
            }

            if (!MeatSlot(1).Empty)
            {
                GetItemAttributes(MeatSlot(1), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[0].IsZero)
                        attributeManualTranslation[0] = attributeExtraManualTranslation[0];

                    attributeManualTranslation[0] = attributeExtraManualTranslation[0];
                }

                Vec3f offsets = new Vec3f(-attributeHorizontalOffset, attributeVerticalOffset, -attributeDepthOffset);

                mesher.AddMeshData(meshes[2].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 1) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 1) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 1) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook1 + offsets + attributeTranslation + attributeManualTranslation[0]));
            }

            if (!MeatSlot(2).Empty)
            {
                GetItemAttributes(MeatSlot(2), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if(!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[1].IsZero)
                        attributeManualTranslation[1] = attributeExtraManualTranslation[1];
                }

                Vec3f offsets = new Vec3f(-attributeDepthOffset, attributeVerticalOffset, attributeHorizontalOffset);

                mesher.AddMeshData(meshes[3].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 2) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 2) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 2) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook1 + offsets + attributeTranslation + attributeManualTranslation[1]));
            }

            if (!MeatSlot(3).Empty)
            {
                GetItemAttributes(MeatSlot(3), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[2].IsZero)
                        attributeManualTranslation[2] = attributeExtraManualTranslation[2];
                }

                Vec3f offsets = new Vec3f(attributeHorizontalOffset, attributeVerticalOffset, attributeDepthOffset);

                mesher.AddMeshData(meshes[4].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 3) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 3) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 3) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook1 + offsets + attributeTranslation + attributeManualTranslation[2]));
            }

            if (!MeatSlot(4).Empty)
            {
                GetItemAttributes(MeatSlot(4), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[3].IsZero)
                        attributeManualTranslation[3] = attributeExtraManualTranslation[3];
                }

                Vec3f offsets = new Vec3f(attributeDepthOffset, attributeVerticalOffset, -attributeHorizontalOffset);

                mesher.AddMeshData(meshes[5].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 4) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 4) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 4) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook1 + offsets + attributeTranslation + attributeManualTranslation[3]));
            }

            if (!MeatSlot(5).Empty)
            {
                GetItemAttributes(MeatSlot(5), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[0].IsZero)
                        attributeManualTranslation[0] = attributeExtraManualTranslation[0];
                }

                Vec3f offsets = new Vec3f(-attributeHorizontalOffset, attributeVerticalOffset, -attributeDepthOffset);

                mesher.AddMeshData(meshes[6].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 1) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 1) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 1) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook2 + offsets + attributeTranslation + attributeManualTranslation[0]));
            }

            if (!MeatSlot(6).Empty)
            {
                GetItemAttributes(MeatSlot(6), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[1].IsZero)
                        attributeManualTranslation[1] = attributeExtraManualTranslation[1];
                }

                Vec3f offsets = new Vec3f(-attributeDepthOffset, attributeVerticalOffset, attributeHorizontalOffset);

                mesher.AddMeshData(meshes[7].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 2) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 2) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 2) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook2 + offsets + attributeTranslation + attributeManualTranslation[1]));
            }

            if (!MeatSlot(7).Empty)
            {
                GetItemAttributes(MeatSlot(7), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[2].IsZero)
                        attributeManualTranslation[2] = attributeExtraManualTranslation[2];
                }

                Vec3f offsets = new Vec3f(attributeHorizontalOffset, attributeVerticalOffset, attributeDepthOffset);

                mesher.AddMeshData(meshes[8].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 3) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 3) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 3) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook2 + offsets + attributeTranslation + attributeManualTranslation[2]));
                }

            if (!MeatSlot(8).Empty)
            {
                GetItemAttributes(MeatSlot(8), ref attributeTranslation, ref attributeRotateAround, ref attributeRotation, ref attributeScale, ref attributeVerticalOffset, ref attributeHorizontalOffset, ref attributeDepthOffset, ref attributeManualTranslation, ref attributeExtraRotation, ref attributeExtraManualTranslation);

                if (facing == RackFacing.EastWest)
                {
                    if (!attributeExtraRotation.IsZero)
                        attributeRotation = attributeExtraRotation;

                    if (!attributeExtraManualTranslation[3].IsZero)
                        attributeManualTranslation[3] = attributeExtraManualTranslation[3];
                }

                Vec3f offsets = new Vec3f(attributeDepthOffset, attributeVerticalOffset, -attributeHorizontalOffset);

                mesher.AddMeshData(meshes[9].Clone()
                    .Scale(defaultOrigin, attributeScale.X, attributeScale.Y, attributeScale.Z)
                    .Rotate(defaultOrigin, ((attributeRotateAround.X * 90 * 4) + attributeRotation.X) * GameMath.DEG2RAD, ((attributeRotateAround.Y * 90 * 4) + attributeRotation.Y) * GameMath.DEG2RAD, ((attributeRotateAround.Z * 90 * 4) + attributeRotation.Z) * GameMath.DEG2RAD)
                    .Translate(resourceTranslationHook2 + offsets + attributeTranslation + attributeManualTranslation[3]));
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
        private void GetItemAttributes(ItemSlot slot, ref Vec3f translation, ref Vec3f rotateAround, ref Vec3f rotation, ref Vec3f scale, ref float verticalOffset, ref float horizontalOffset, ref float depthOffset, ref Vec3f[] manualTranslation, ref Vec3f extraRotation, ref Vec3f[] extraManualTranslation)
        {
            JsonObject translationProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["translation"];
            JsonObject rotateAroundProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["rotateAround"];
            JsonObject rotationProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["rotation"];
            JsonObject scaleProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["scale"];
            JsonObject verticalProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["verticalOffset"];
            JsonObject horizontalProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["horizontalOffset"];
            JsonObject depthProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["depthOffset"];
            JsonObject manualTranslationProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["manualTranslation"];
            JsonObject extraRotationProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["extraRotation"];
            JsonObject extraManualTranslationProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["extraManualTranslation"];

            if (translationProperties != null)
            {
                translation.X = translationProperties["x"].Exists ? translationProperties["x"].AsFloat() : 0;
                translation.Y = translationProperties["y"].Exists ? translationProperties["y"].AsFloat() : 0;
                translation.Z = translationProperties["z"].Exists ? translationProperties["z"].AsFloat() : 0;
            }

            if (rotateAroundProperties != null)
            {
                rotateAround.X = rotateAroundProperties["x"].Exists ? rotateAroundProperties["x"].AsFloat() : 0;
                rotateAround.Y = rotateAroundProperties["y"].Exists ? rotateAroundProperties["y"].AsFloat() : 90;
                rotateAround.Z = rotateAroundProperties["z"].Exists ? rotateAroundProperties["z"].AsFloat() : 0;
            }

            if(rotationProperties != null)
            {
                rotation.X = rotationProperties["x"].Exists ? rotationProperties["x"].AsFloat() : 0;
                rotation.Y = rotationProperties["y"].Exists ? rotationProperties["y"].AsFloat() : 0;
                rotation.Z = rotationProperties["z"].Exists ? rotationProperties["z"].AsFloat() : 0;
            }

            if(scaleProperties != null)
            {
                scale.X = scaleProperties["x"].Exists ? scaleProperties["x"].AsFloat() : 1;
                scale.Y = scaleProperties["y"].Exists ? scaleProperties["y"].AsFloat() : 1;
                scale.Z = scaleProperties["z"].Exists ? scaleProperties["z"].AsFloat() : 1;
            }

            if(verticalProperties != null)
            {
                verticalOffset = verticalProperties.Exists ? verticalProperties.AsFloat() : 0.0f;
            }

            if(horizontalProperties != null)
            {
                horizontalOffset = horizontalProperties.Exists ? horizontalProperties.AsFloat() : 0.0f;
            }

            if(depthProperties != null)
            {
                depthOffset = depthProperties.Exists ? depthProperties.AsFloat() : 0.0f;
            }

            if(manualTranslationProperties != null)
            {
                manualTranslation[0].X = manualTranslationProperties["0"]["x"].AsFloat();
                manualTranslation[0].Y = manualTranslationProperties["0"]["y"].AsFloat();
                manualTranslation[0].Z = manualTranslationProperties["0"]["z"].AsFloat();

                manualTranslation[1].X = manualTranslationProperties["1"]["x"].AsFloat();
                manualTranslation[1].Y = manualTranslationProperties["1"]["y"].AsFloat();
                manualTranslation[1].Z = manualTranslationProperties["1"]["z"].AsFloat();

                manualTranslation[2].X = manualTranslationProperties["2"]["x"].AsFloat();
                manualTranslation[2].Y = manualTranslationProperties["2"]["y"].AsFloat();
                manualTranslation[2].Z = manualTranslationProperties["2"]["z"].AsFloat();

                manualTranslation[3].X = manualTranslationProperties["3"]["x"].AsFloat();
                manualTranslation[3].Y = manualTranslationProperties["3"]["y"].AsFloat();
                manualTranslation[3].Z = manualTranslationProperties["3"]["z"].AsFloat();
            }

            if(extraRotationProperties != null)
            {
                extraRotation.X = extraRotationProperties["x"].Exists ? extraRotationProperties["x"].AsFloat() : 0;
                extraRotation.Y = extraRotationProperties["y"].Exists ? extraRotationProperties["y"].AsFloat() : 0;
                extraRotation.Z = extraRotationProperties["z"].Exists ? extraRotationProperties["z"].AsFloat() : 0;
            }

            if(extraManualTranslationProperties != null)
            {
                extraManualTranslation[0].X = extraManualTranslationProperties["0"]["x"].AsFloat();
                extraManualTranslation[0].Y = extraManualTranslationProperties["0"]["y"].AsFloat();
                extraManualTranslation[0].Z = extraManualTranslationProperties["0"]["z"].AsFloat();

                extraManualTranslation[1].X = extraManualTranslationProperties["1"]["x"].AsFloat();
                extraManualTranslation[1].Y = extraManualTranslationProperties["1"]["y"].AsFloat();
                extraManualTranslation[1].Z = extraManualTranslationProperties["1"]["z"].AsFloat();

                extraManualTranslation[2].X = extraManualTranslationProperties["2"]["x"].AsFloat();
                extraManualTranslation[2].Y = extraManualTranslationProperties["2"]["y"].AsFloat();
                extraManualTranslation[2].Z = extraManualTranslationProperties["2"]["z"].AsFloat();

                extraManualTranslation[3].X = extraManualTranslationProperties["3"]["x"].AsFloat();
                extraManualTranslation[3].Y = extraManualTranslationProperties["3"]["y"].AsFloat();
                extraManualTranslation[3].Z = extraManualTranslationProperties["3"]["z"].AsFloat();
            }
        }
        private float GetAttributeScale(ItemSlot slot, string direction)
        {
            float scaleAttribute = 1;

            JsonObject scaleProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["scale"];

            if(scaleProperties != null)
                scaleAttribute = scaleProperties[direction].Exists ? scaleProperties[direction].AsFloat() : 1;

            return scaleAttribute;
        }
        private Vec3f GetAttributeRotateAround(ItemSlot slot)
        {
            Vec3f rotateAroundAttribute = new Vec3f(0, 90, 0);

            JsonObject rotateAroundProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["rotateAround"];

            if(rotateAroundProperties != null)
            {
                rotateAroundAttribute.X = rotateAroundProperties["x"].Exists ? rotateAroundProperties["x"].AsFloat() : rotateAroundAttribute.X;
                rotateAroundAttribute.Y = rotateAroundProperties["y"].Exists ? rotateAroundProperties["y"].AsFloat() : rotateAroundAttribute.Y;
                rotateAroundAttribute.Z = rotateAroundProperties["z"].Exists ? rotateAroundProperties["z"].AsFloat() : rotateAroundAttribute.Z;
            }

            return rotateAroundAttribute;
        }
        private Vec3f GetManualTranslation(int index, ItemSlot slot)
        {
            Vec3f manualTranslationAttribute = Vec3f.Zero;

            JsonObject manualTranslationProperties = slot.Itemstack.Collectible.Attributes["onCuringRackProps"]["transform"]["manualTranslation"][index.ToString()];

            if(manualTranslationProperties != null)
            {
                manualTranslationAttribute.X = manualTranslationProperties["x"].Exists ? manualTranslationProperties["x"].AsFloat() : 0;
                manualTranslationAttribute.Y = manualTranslationProperties["y"].Exists ? manualTranslationProperties["y"].AsFloat() : 0;
                manualTranslationAttribute.Z = manualTranslationProperties["z"].Exists ? manualTranslationProperties["z"].AsFloat() : 0;
            }

            return manualTranslationAttribute;
        }
        public override void UpdateMeshes()
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                UpdateMesh(i);
            }
        }
        public void UpdateMesh(int meshIndex)
        {
            if (Api == null || Api.Side == EnumAppSide.Server) return;
            if (Inventory[meshIndex].Empty)
            {
                meshes[meshIndex] = null;
                return;
            }

            MeshData mesh = GenMesh(Inventory[meshIndex].Itemstack);

            meshes[meshIndex] = mesh;
            MarkDirty(true);
        }
    }
}
