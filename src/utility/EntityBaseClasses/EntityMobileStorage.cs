using AncientTools.EntityRenderers;
using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace AncientTools.Utility
{
    abstract class EntityMobileStorage : EntityAgent
    {
        protected float DroppedHeight { get; set; } = -0.2f;
        protected abstract int StorageBlocksCount { get; set; }
        protected GuiDialogBlockEntityInventory InvDialog { get; set; }

        public ICoreClientAPI Capi { get; set; }
        public InventoryGeneric MobileStorageInventory { get; set; }
        public InventoryGeneric[] StorageBlockInventories { get; set; }

        public EntityAgent AttachedEntity { get; set; }
        public EntityPos EntityTransform { get; set; }
        public Vec3f LookAtVector { get; set; } = new Vec3f(0.0f, 0.0f, 0.0f);

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            EntityTransform = this.SidedPos;

            if (api.Side == EnumAppSide.Client)
                Capi = (ICoreClientAPI)api;

            if (WatchedAttributes.HasAttribute("lookAtVectorX") && WatchedAttributes.HasAttribute("lookAtVectorY") && WatchedAttributes.HasAttribute("lookAtVectorZ"))
            {
                LookAtVector.X = WatchedAttributes.GetFloat("lookAtVectorX", DroppedHeight);
                LookAtVector.Y = WatchedAttributes.GetFloat("lookAtVectorY", 0);
                LookAtVector.Z = WatchedAttributes.GetFloat("lookAtVectorZ", 0);
            }

            UpdateStorageContentsFromTree();

            foreach (ItemSlot slot in MobileStorageInventory)
            {
                slot.MaxSlotStackSize = 1;
            }
        }
        public override void ToBytes(BinaryWriter writer, bool forClient)
        {
            UpdateTreesFromInventoryContents();

            base.ToBytes(writer, forClient);
        }
        public override void FromBytes(BinaryReader reader, bool forClient)
        {
            base.FromBytes(reader, forClient);

            //UpdateStorageContentsFromTree();
        }
        public override void OnReceivedClientPacket(IServerPlayer player, int packetid, byte[] data)
        {
            if (packetid == (int)AncientToolsNetworkPackets.MobileStorageRotationSync)
            {
                PerformServerRotationSync(data);
            }

            base.OnReceivedClientPacket(player, packetid, data);
        }
        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if (packetid == (int)AncientToolsNetworkPackets.MobileStorageRotationSync)
            {
                PerformClientRotationSync(data);
            }
            if(packetid == (int)AncientToolsNetworkPackets.MobileStorageOpenInventory)
            {
                OpenInventoryDialog(data);
            }

            base.OnReceivedServerPacket(packetid, data);
        }
        public void SetEntityPosition(Vec3d position)
        {
            EntityTransform.SetPos(position);

            SyncPosition();
        }
        public void SyncPosition()
        {
            ServerPos.SetFrom(EntityTransform);
            Pos.SetFrom(ServerPos);
        }
        public void SetLookAtVector(Vec3f cartPos, float xPos, float yPos, float zPos)
        {
            SetLookAtVector(cartPos, new Vec3f(xPos, yPos, zPos));
        }
        public void SetLookAtVector(Vec3f cartPos, Vec3f lookAtPos)
        {
            SetLookAtVector(cartPos, lookAtPos, Vec3f.Zero);
        }
        public void SetLookAtVector(Vec3f cartPos, Vec3f attachedEntityPos, Vec3f attachedEntityEyePos)
        {
            Vec3f normal = ((attachedEntityPos + (attachedEntityEyePos / 4)) - cartPos).Normalize();
            //normal.Y = 0.325f;

            double pitch = Math.Asin(normal.Y);
            double yaw = Math.Atan2(-normal.X, -normal.Z);

            LookAtVector.X = (float)pitch;
            LookAtVector.Y = (float)yaw;
        }
        public void DropCart()
        {
            byte[] data;

            LookAtVector.X = DroppedHeight;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                TreeAttribute tree = new TreeAttribute();
                tree.SetFloat("lookAtVectorX", LookAtVector.X);
                tree.SetFloat("lookAtVectorY", LookAtVector.Y);
                tree.SetFloat("lookAtVectorZ", LookAtVector.Z);
                tree.ToBytes(writer);
                data = ms.ToArray();
            }

            Capi.Network.SendEntityPacket(this.EntityId, (int)AncientToolsNetworkPackets.MobileStorageRotationSync, data);
        }
        public Shape[] GetInventoryShapes()
        {
            Queue<Shape> inventoryShapes = new Queue<Shape>();

            foreach (ItemSlot inventoryStorage in MobileStorageInventory)
            {
                if (!inventoryStorage.Empty)
                    inventoryShapes.Enqueue(Api.Assets.TryGet(inventoryStorage.Itemstack.Block.Shape.Base) as Shape);
            }

            return inventoryShapes.ToArray();
        }
        public void CreateNewStorageBlockInventory(int index)
        {
            StorageBlockInventories[index] = new InventoryGeneric(MobileStorageInventory[index].Itemstack.Collectible.Attributes["mobileStorageProps"]["quantitySlots"].AsInt(), this.Code.FirstCodePart() + "/" + EntityId.ToString(), null, Api);
            
            StorageBlockInventories[index].OnInventoryClosed += CloseClientDialog;
        }

        protected TreeAttribute TreeData(byte[] data)
        {
            TreeAttribute tree = new TreeAttribute();

            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(ms);
                tree.FromBytes(reader);
            }

            return tree;
        }
        protected void UpdateTreesFromInventoryContents()
        {
            if (MobileStorageInventory != null)
            {
                ITreeAttribute invtree = new TreeAttribute();
                MobileStorageInventory.ToTreeAttributes(invtree);
                WatchedAttributes["mobilestorageinventory"] = invtree;
            }

            for (int i = 0; i < StorageBlocksCount; i++)
            {
                if (StorageBlockInventories[i] != null)
                {
                    ITreeAttribute invtree = new TreeAttribute();
                    StorageBlockInventories[i].ToTreeAttributes(invtree);
                    WatchedAttributes["storageblockinventories" + i] = invtree;
                }
            }
        }
        protected void UpdateStorageContentsFromTree()
        {
            if (MobileStorageInventory == null || MobileStorageInventory.Empty)
                GenerateBaseInventories();

            if (WatchedAttributes.HasAttribute("mobilestorageinventory"))
            {
                MobileStorageInventory.FromTreeAttributes(WatchedAttributes.GetTreeAttribute("mobilestorageinventory"));

                for (int i = 0; i < StorageBlocksCount; i++)
                {
                    if (WatchedAttributes.HasAttribute("storageblockinventories" + i))
                    {
                        CreateNewStorageBlockInventory(i);

                        StorageBlockInventories[i].FromTreeAttributes(WatchedAttributes.GetTreeAttribute("storageblockinventories" + i));
                    }
                }
            }
        }
        protected void GenerateBaseInventories()
        {
            MobileStorageInventory = new InventoryGeneric(StorageBlocksCount, this.Code.FirstCodePart() + "/" + EntityId.ToString(), null, Api);
            StorageBlockInventories = new InventoryGeneric[StorageBlocksCount];
        }
        protected void SendOpenInventoryPacket(EntityPlayer entityPlayer, int index)
        {
            IPlayer player = (IPlayer)entityPlayer.Player;

            if (Api.World is IServerWorldAccessor)
            {
                IServerPlayer serverPlayer = (IServerPlayer)player;

                byte[] data;

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write("EntityInventory");
                    writer.Write("Entity Inventory");
                    writer.Write((byte)4);
                    writer.Write((byte)index);
                    TreeAttribute tree = new TreeAttribute();
                    StorageBlockInventories[index].ToTreeAttributes(tree);
                    tree.ToBytes(writer);
                    data = ms.ToArray();
                }

                ((ICoreServerAPI)Api).Network.SendEntityPacket(
                    serverPlayer,
                    this.EntityId,
                    (int)AncientToolsNetworkPackets.MobileStorageOpenInventory,
                    data
                );

                player.InventoryManager.OpenInventory(StorageBlockInventories[index]);
            }
        }
        private void PerformServerRotationSync(byte[] data)
        {
            TreeAttribute tree = TreeData(data);

            LookAtVector.X = tree.GetFloat("lookAtVectorX");
            LookAtVector.Y = tree.GetFloat("lookAtVectorY");
            LookAtVector.Z = tree.GetFloat("lookAtVectorZ");

            WatchedAttributes.SetFloat("lookAtVectorX", LookAtVector.X);
            WatchedAttributes.SetFloat("lookAtVectorY", LookAtVector.Y);
            WatchedAttributes.SetFloat("lookAtVectorZ", LookAtVector.Z);

            ICoreServerAPI sapi = (ICoreServerAPI)Api;

            foreach (IServerPlayer eachPlayer in sapi.Server.Players)
            {
                if (eachPlayer.ConnectionState == EnumClientState.Playing)
                    sapi.Network.SendEntityPacket(eachPlayer, this.EntityId, (int)AncientToolsNetworkPackets.MobileStorageRotationSync, data);
            }
        }
        private void PerformClientRotationSync(byte[] data)
        {
            TreeAttribute tree = TreeData(data);

            LookAtVector.X = tree.GetFloat("lookAtVectorX");
            LookAtVector.Y = tree.GetFloat("lookAtVectorY");
            LookAtVector.Z = tree.GetFloat("lookAtVectorZ");
        }
        private void OpenInventoryDialog(byte[] data)
        {
            IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;
            if (InvDialog != null)
            {
                if (InvDialog?.IsOpened() == true) InvDialog.TryClose();
                InvDialog?.Dispose();
                InvDialog = null;
                return;
            }

            string dialogClassName;
            string dialogTitle;
            int cols;
            int index;
            TreeAttribute tree = new TreeAttribute();

            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(ms);
                dialogClassName = reader.ReadString();
                dialogTitle = reader.ReadString();
                cols = reader.ReadByte();
                index = reader.ReadByte();
                tree.FromBytes(reader);
            }

            StorageBlockInventories[index].FromTreeAttributes(tree);
            StorageBlockInventories[index].ResolveBlocksOrItems();

            InvDialog = new GuiDialogBlockEntityInventory(dialogTitle, StorageBlockInventories[index], Pos.AsBlockPos, cols, Api as ICoreClientAPI);

            /*
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string os = block.Attributes?["openSound"]?.AsString();
            string cs = block.Attributes?["closeSound"]?.AsString();
            AssetLocation opensound = os == null ? null : AssetLocation.Create(os, block.Code.Domain);
            AssetLocation closesound = cs == null ? null : AssetLocation.Create(cs, block.Code.Domain);

            invDialog.OpenSound = opensound ?? this.OpenSound;
            invDialog.CloseSound = closesound ?? this.CloseSound;
            */

            InvDialog.TryOpen();
        }
        private void CloseClientDialog(IPlayer player)
        {
            var inv = InvDialog;
            InvDialog = null; // Weird handling because to prevent endless recursion
            if (inv?.IsOpened() == true) inv?.TryClose();
            inv?.Dispose();
        }
    }
}
