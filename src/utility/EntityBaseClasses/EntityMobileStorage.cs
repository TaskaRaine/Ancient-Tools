using AncientTools.EntityRenderers;
using AncientTools.Gui;
using AncientTools.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace AncientTools.Utility
{
    abstract class EntityMobileStorage : EntityAgent
    {
        private const int ATTACHED_CHECK_IN_SECONDS = 15;
        protected float DroppedHeight { get; set; } = -0.2f;
        protected bool IsDropped { get; set; } = false;
        public abstract int StorageBlocksCount { get; protected set; }
        protected GuiDialogMobileStorage InvDialog { get; set; }

        public ICoreClientAPI Capi { get; set; }
        public ICoreServerAPI Sapi { get; set; }
        public InventoryMobileStorage MobileStorageInventory { get; set; }
        //public InventoryGeneric[] StorageBlockInventories { get; set; }

        public EntityAgent AttachedEntity { get; set; }
        public EntityPos EntityTransform { get; set; }
        public Vec3f LookAtVector { get; set; } = new Vec3f(0.0f, 0.0f, 0.0f);

        private long PreviousAttachedEntityCheckTime { get; set; } = 0;

        public EntityMobileStorage()
        {
            //AllowDespawn = false;
        }
        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            EntityTransform = this.SidedPos;
            Capi = api as ICoreClientAPI;
            Sapi = api as ICoreServerAPI;

            if (WatchedAttributes.HasAttribute("lookAtVectorX") && WatchedAttributes.HasAttribute("lookAtVectorY") && WatchedAttributes.HasAttribute("lookAtVectorZ"))
            {
                LookAtVector.X = WatchedAttributes.GetFloat("lookAtVectorX", DroppedHeight);
                LookAtVector.Y = WatchedAttributes.GetFloat("lookAtVectorY", 0);
                LookAtVector.Z = WatchedAttributes.GetFloat("lookAtVectorZ", 0);
            }

            UpdateStorageContentsFromTree();

            MobileStorageInventory.OnInventoryClosed += CloseClientDialog;
        }
        public override string GetInfoText()
        {
            StringBuilder infotext = new StringBuilder();

            /*
             * behavior.GetInfoText(infotext) does not actually modify infotext and, therefore, the information is never passed back.
             * Cannot be used to extract info from a behavior
            foreach (EntityBehavior behavior in SidedProperties.Behaviors)
            {
                behavior.GetInfoText(infotext);
            }
            */

            if (Api.Side == EnumAppSide.Client)
            {
                TreeAttribute healthTree = WatchedAttributes.GetTreeAttribute("health") as TreeAttribute;

                infotext.Append(Lang.Get("ancienttools:entityinfo-cart-health", Math.Round(healthTree.GetFloat("currenthealth"), 2), healthTree.GetFloat("maxhealth")));
            }

            return infotext.ToString();
        }
        public override void OnReceivedClientPacket(IServerPlayer player, int packetid, byte[] data)
        {
            if (packetid < 1000)
            {
                MobileStorageInventory.InvNetworkUtil.HandleClientPacket(player, packetid, data);

                UpdateTreesFromInventoryContents();
                return;
            }
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
            if(packetid == (int)AncientToolsNetworkPackets.MobileStorageSyncAttachedEntity)
            {
                PerformClientAttachEntity(data);
            }
            if(packetid == (int)AncientToolsNetworkPackets.MobileStorageCloseInventory)
            {
                InvDialog?.TryClose();
            }

            base.OnReceivedServerPacket(packetid, data);
        }
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if(AttachedEntity != null && Api.Side == EnumAppSide.Server)
            {
                if(Api.World.ElapsedMilliseconds / 1000 > PreviousAttachedEntityCheckTime + ATTACHED_CHECK_IN_SECONDS)
                {
                    SyncAttachedEntity(AttachedEntity.EntityId);

                    PreviousAttachedEntityCheckTime = Api.World.ElapsedMilliseconds / 1000;
                }
            }
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
        /// <summary>
        /// Sends the entity ID that the cart should attach to so that all can assign their AttachedEntity for accurate visual rotation. 
        /// </summary>
        /// <param name="entityID">The ID of the entity a cart is attached to. If -1, the AttachedEntity should be removed.</param>
        public void SyncAttachedEntity(long entityID)
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                TreeAttribute tree = new TreeAttribute();
                tree.SetLong("attachedID", entityID);
                tree.ToBytes(writer);
                data = ms.ToArray();
            }

            foreach (IServerPlayer eachPlayer in Sapi.Server.Players)
            {
                if (eachPlayer.ConnectionState == EnumClientState.Playing)
                    Sapi.Network.SendEntityPacket(eachPlayer, this.EntityId, (int)AncientToolsNetworkPackets.MobileStorageSyncAttachedEntity, data);
            }
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
            
            IsDropped = true;
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
        public void UpdateTreesFromInventoryContents()
        {
            if (MobileStorageInventory != null)
            {
                ITreeAttribute invtree = new TreeAttribute();
                MobileStorageInventory.ToTreeAttributes(invtree);
                WatchedAttributes["mobilestorageinventory"] = invtree;
            }
        }
        public void UpdateStorageContentsFromTree()
        {
            if (MobileStorageInventory == null || MobileStorageInventory.Empty)
                MobileStorageInventory = new InventoryMobileStorage(this, StorageBlocksCount, this.Code.FirstCodePart() + "/" + EntityId.ToString(), null, Api);

            if (WatchedAttributes.HasAttribute("mobilestorageinventory"))
            {
                MobileStorageInventory.FromTreeAttributes(WatchedAttributes.GetTreeAttribute("mobilestorageinventory"));
            }
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
                    writer.Write("CartInventory");
                    writer.Write(Lang.Get("ancienttools:gui-title-cart-inventory"));
                    writer.Write((byte)4);
                    writer.Write((byte)index);
                    TreeAttribute tree = new TreeAttribute();
                    MobileStorageInventory.ToTreeAttributes(tree);
                    tree.ToBytes(writer);
                    data = ms.ToArray();
                }

                ((ICoreServerAPI)Api).Network.SendEntityPacket(
                    serverPlayer,
                    this.EntityId,
                    (int)AncientToolsNetworkPackets.MobileStorageOpenInventory,
                    data
                );

                player.InventoryManager.OpenInventory(MobileStorageInventory);
            }
        }
        protected void PlayPlacementSound()
        {
            AssetLocation placementSoundLocation;

            //-- Type is also checked in in EntityCart before this method is called --//
            if (MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible is BlockGenericTypedContainer)
            {
                string type = MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Attributes?.GetString("type");

                placementSoundLocation = new AssetLocation(MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible.Attributes["mobileStorageProps"][type]["placementSound"].AsString() + ".ogg");
            }
            else
                placementSoundLocation = new AssetLocation(MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible.Attributes["mobileStorageProps"]["placementSound"].AsString() + ".ogg");

            (Capi.World).LoadSound(new SoundParams()
            {
                Location = placementSoundLocation,
                ShouldLoop = false,
                Position = Pos.XYZFloat,
                DisposeOnFinish = true,
                Volume = 1.0f,
                Range = 32
            }).Start();
        }
        protected void ServerCloseClientDialogs()
        {
            foreach(IServerPlayer eachPlayer in Sapi.World.AllOnlinePlayers)
            {
                if (eachPlayer.ConnectionState == EnumClientState.Playing)
                    ((ICoreServerAPI)Api).Network.SendEntityPacket(eachPlayer, this.EntityId, (int)AncientToolsNetworkPackets.MobileStorageCloseInventory);
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

            foreach (IServerPlayer eachPlayer in Sapi.World.AllOnlinePlayers)
            {
                if (eachPlayer.ConnectionState == EnumClientState.Playing)
                    Sapi.Network.SendEntityPacket(eachPlayer, this.EntityId, (int)AncientToolsNetworkPackets.MobileStorageRotationSync, data);
            }
        }
        private void PerformClientRotationSync(byte[] data)
        {
            TreeAttribute tree = TreeData(data);

            LookAtVector.X = tree.GetFloat("lookAtVectorX");
            LookAtVector.Y = tree.GetFloat("lookAtVectorY");
            LookAtVector.Z = tree.GetFloat("lookAtVectorZ");
        }
        private void PerformClientAttachEntity(byte[] data)
        {
            long id = TreeData(data).GetLong("attachedID");

            if (id == -1)
            {
                AttachedEntity = null; 
                return;
            }

            if (AttachedEntity == null)
                AttachedEntity = Api.World.GetEntityById(id) as EntityAgent;
        }
        private void OpenInventoryDialog(byte[] data)
        {
            AssetLocation openSoundLocation;

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

            MobileStorageInventory.FromTreeAttributes(tree);
            MobileStorageInventory.ResolveBlocksOrItems();

            InvDialog = new GuiDialogMobileStorage(dialogTitle, this, MobileStorageInventory, cols, Api as ICoreClientAPI);

            if (MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible is BlockGenericTypedContainer)
            {
                string type = MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Attributes?.GetString("type");

                openSoundLocation = new AssetLocation(MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible.Attributes["mobileStorageProps"][type]["openSound"].AsString() + ".ogg");
            }
            else
                openSoundLocation = new AssetLocation(MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible.Attributes["mobileStorageProps"]["openSound"].AsString() + ".ogg");
            
            (Capi.World).LoadSound(new SoundParams()
            {
                Location = openSoundLocation,
                ShouldLoop = false,
                Position = Pos.XYZFloat,
                DisposeOnFinish = true,
                Volume = 1.0f,
                Range = 32
            }).Start();

            InvDialog.TryOpen();
        }
        private void CloseClientDialog(IPlayer player)
        {
            var inv = InvDialog;
            InvDialog = null; // Weird handling because to prevent endless recursion
            if (inv?.IsOpened() == true) inv?.TryClose();
            inv?.Dispose();

            if(!MobileStorageInventory.Empty)
            {
                AssetLocation closeSoundLocation;

                if (MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible is BlockGenericTypedContainer)
                {
                    string type = MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Attributes?.GetString("type");

                    closeSoundLocation = new AssetLocation(MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible.Attributes["mobileStorageProps"][type]["closeSound"].AsString() + ".ogg");
                }
                else
                    closeSoundLocation = new AssetLocation(MobileStorageInventory.GetMobileStorageSlot(0).Itemstack.Collectible.Attributes["mobileStorageProps"]["closeSound"].AsString() + ".ogg");

                (Capi.World).LoadSound(new SoundParams()
                {
                    Location = closeSoundLocation,
                    ShouldLoop = false,
                    Position = Pos.XYZFloat,
                    DisposeOnFinish = true,
                    Volume = 1.0f,
                    Range = 32
                }).Start();
            }
        }
    }
}
