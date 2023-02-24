using AncientTools.Inventory;
using AncientTools.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using static Vintagestory.API.Client.GuiDialogBlockEntity;

namespace AncientTools.Gui
{
    class GuiDialogMobileStorage : GuiDialogGeneric
    {
        public bool IsDuplicate { get; }

        public EntityMobileStorage StorageEntity { get; }
        public InventoryMobileStorage Inventory { get; }
        private int Columns { get; set; }
        private EnumPosFlag screenPos;

        protected virtual double FloatyDialogPosition => 0.75;
        protected virtual double FloatyDialogAlign => 0.75;
        public GuiDialogMobileStorage(string dialogTitle, EntityMobileStorage storageEntity, InventoryMobileStorage inventory, int cols, ICoreClientAPI capi) : base(dialogTitle, capi)
        {
            IsDuplicate = capi.World.Player.InventoryManager.Inventories.ContainsValue(inventory);
            if (IsDuplicate) return;

            StorageEntity = storageEntity;
            Inventory = inventory;
            Columns = cols;

            double elemToDlgPad = GuiStyle.ElementToDialogPadding;
            double pad = GuiElementItemSlotGrid.unscaledSlotPadding;
            int rows = (int)Math.Ceiling(inventory.StoredInventorySlotCount() / (float)cols);
            int visibleRows = Math.Min(rows, 7);

            // 1. The bounds of the slot grid itself. It is offseted by slot padding. It determines the size of the dialog, so we build the dialog from the bottom up
            ElementBounds slotGridBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, pad, pad, cols, visibleRows);

            // 1a.) Determine the full size of scrollable area, required to calculate scrollbar handle size
            ElementBounds fullGridBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 0, cols, rows);

            // 2. Around that is the 3 wide inset stroke
            ElementBounds insetBounds = slotGridBounds.ForkBoundingParent(6, 6, 6, 6);

            screenPos = GetFreePos("smallblockgui");

            if (visibleRows < rows)
            {
                // 2a. The scrollable bounds is also the clipping bounds. Needs it's parent to be set.
                ElementBounds clippingBounds = slotGridBounds.CopyOffsetedSibling();
                clippingBounds.fixedHeight -= 3; // Why?

                // 3. Around all that is the dialog centered to screen middle, with some extra spacing right for the scrollbar
                ElementBounds dialogBounds = insetBounds
                    .ForkBoundingParent(elemToDlgPad, elemToDlgPad + 30, elemToDlgPad + 20, elemToDlgPad)
                    .WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0)
                    .WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle)
                ;

                if (!capi.Settings.Bool["immersiveMouseMode"])
                {
                    dialogBounds.fixedOffsetY += (dialogBounds.fixedHeight + 10) * YOffsetMul(screenPos);
                }

                // 4. Right of the slot grid is the scrollbar
                ElementBounds scrollbarBounds = ElementStdBounds.VerticalScrollbar(insetBounds).WithParent(dialogBounds);

                SingleComposer = capi.Gui
                    .CreateCompo("mobilestorage", dialogBounds)
                    .AddShadedDialogBG(ElementBounds.Fill)
                    .AddDialogTitleBar(dialogTitle, CloseIconPressed)
                    .AddInset(insetBounds)
                    .AddVerticalScrollbar(OnNewScrollbarvalue, scrollbarBounds, "scrollbar")
                    .BeginClip(clippingBounds)
                    .AddItemSlotGridExcl(inventory, DoSendPacket, cols, new int[] { 0 }, slotGridBounds, "slotgrid")
                    .EndClip()
                    .Compose();

                SingleComposer.GetScrollbar("scrollbar").SetHeights(
                    (float)(slotGridBounds.fixedHeight),
                    (float)(fullGridBounds.fixedHeight + pad)
                );

            }
            else
            {
                // 3. Around all that is the dialog centered to screen middle, with some extra spacing right for the scrollbar
                ElementBounds dialogBounds = insetBounds
                    .ForkBoundingParent(elemToDlgPad, elemToDlgPad + 20, elemToDlgPad, elemToDlgPad)
                    .WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0)
                    .WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle)
                ;

                if (!capi.Settings.Bool["immersiveMouseMode"])
                {
                    dialogBounds.fixedOffsetY += (dialogBounds.fixedHeight + 10) * YOffsetMul(screenPos);
                }

                SingleComposer = capi.Gui
                    .CreateCompo("mobilestorage", dialogBounds)
                    .AddShadedDialogBG(ElementBounds.Fill)
                    .AddDialogTitleBar(dialogTitle, CloseIconPressed)
                    .AddInset(insetBounds)
                    .AddItemSlotGridExcl(inventory, DoSendPacket, cols, new int[] { 0 }, slotGridBounds, "slotgrid")
                    .Compose();
            }

            SingleComposer.UnfocusOwnElements();
        }
        public override void OnFinalizeFrame(float dt)
        {
            base.OnFinalizeFrame(dt);

            if (!IsInRangeOfStorage())
            {
                // Because we cant do it in here
                capi.Event.EnqueueMainThreadTask(() => TryClose(), "closedlg");
            }
        }
        private bool IsInRangeOfStorage()
        {
            Vec3d storagePos = StorageEntity.Pos.AsBlockPos.ToVec3d();
            Vec3d playerEye = capi.World.Player.Entity.Pos.XYZ.Add(capi.World.Player.Entity.LocalEyePos);

            return storagePos.DistanceTo(playerEye) <= capi.World.Player.WorldData.PickingRange + 0.5;
        }
        /// <summary>
        /// Render's the object in Orthographic mode.
        /// </summary>
        /// <param name="deltaTime">The time elapsed.</param>
        public override void OnRenderGUI(float deltaTime)
        {
            if (capi.Settings.Bool["immersiveMouseMode"])
            {
                Vec3d aboveHeadPos = new Vec3d(StorageEntity.Pos.X + 0.5, StorageEntity.Pos.Y + FloatyDialogPosition, StorageEntity.Pos.Z + 0.5);
                Vec3d pos = MatrixToolsd.Project(aboveHeadPos, capi.Render.PerspectiveProjectionMat, capi.Render.PerspectiveViewMat, capi.Render.FrameWidth, capi.Render.FrameHeight);

                // Z negative seems to indicate that the name tag is behind us \o/
                if (pos.Z < 0) return;

                SingleComposer.Bounds.Alignment = EnumDialogArea.None;
                SingleComposer.Bounds.fixedOffsetX = 0;
                SingleComposer.Bounds.fixedOffsetY = 0;
                SingleComposer.Bounds.absFixedX = pos.X - SingleComposer.Bounds.OuterWidth / 2;
                SingleComposer.Bounds.absFixedY = capi.Render.FrameHeight - pos.Y - SingleComposer.Bounds.OuterHeight * FloatyDialogAlign;
                SingleComposer.Bounds.absMarginX = 0;
                SingleComposer.Bounds.absMarginY = 0;
            }

            base.OnRenderGUI(deltaTime);
        }
        protected void DoSendPacket(object p)
        {
            //-- The entity handles this --//
            //capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }
        /// <summary>
        /// Called whenever the scrollbar or mouse wheel is used.
        /// </summary>
        /// <param name="value">The new value of the scrollbar.</param>
        protected void OnNewScrollbarvalue(float value)
        {
            ElementBounds bounds = SingleComposer.GetSlotGrid("slotgrid").Bounds;
            bounds.fixedY = 10 - GuiElementItemSlotGrid.unscaledSlotPadding - value;

            bounds.CalcWorldBounds();
        }
        /// <summary>
        /// Occurs whenever the X icon in the top right corner of the GUI (not the window) is pressed.
        /// </summary>
        protected void CloseIconPressed()
        {
            TryClose();
        }
        public override bool TryClose()
        {
            StorageEntity.SendCloseInventoryPacket();

            return base.TryClose();
        }
        /// <summary>
        /// Called whenver the GUI is opened.
        /// </summary>
        public override void OnGuiOpened()
        {
            if (Inventory != null)
            {
                Inventory.Open(capi.World.Player);
                capi.World.Player.InventoryManager.OpenInventory(Inventory);
            }

            if (capi.Gui.GetDialogPosition("mobilestorage") == null)
            {
                OccupyPos("smallblockgui", screenPos);
            }
        }

        /// <summary>
        /// Attempts to open this gui.
        /// </summary>
        /// <returns>Whether the attempt was successful.</returns>
        public override bool TryOpen()
        {
            if (IsDuplicate) return false;
            return base.TryOpen();
        }

        /// <summary>
        /// Called when the GUI is closed.
        /// </summary>
        public override void OnGuiClosed()
        {
            if (Inventory != null)
            {
                Inventory.Close(capi.World.Player);
                capi.World.Player.InventoryManager.CloseInventory(Inventory);
            }

            FreePos("smallblockgui", screenPos);
        }

        public override bool PrefersUngrabbedMouse => false;
        /// <summary>
        /// Reloads the values of the GUI.
        /// </summary>
        public void ReloadValues()
        {

        }


        public EnumPosFlag GetFreePos(string code)
        {
            var values = Enum.GetValues(typeof(EnumPosFlag));

            int flags = 0;
            posFlagDict().TryGetValue(code, out flags);

            foreach (EnumPosFlag flag in values)
            {
                if ((flags & (int)flag) > 0) continue;

                return flag;
            }

            return 0;
        }

        public void OccupyPos(string code, EnumPosFlag pos)
        {
            int flags = 0;
            posFlagDict().TryGetValue(code, out flags);
            posFlagDict()[code] = flags | (int)pos;
        }

        public void FreePos(string code, EnumPosFlag pos)
        {
            int flags = 0;
            posFlagDict().TryGetValue(code, out flags);
            posFlagDict()[code] = flags & ~(int)pos;
        }

        Dictionary<string, int> posFlagDict()
        {
            object valObj;
            capi.ObjectCache.TryGetValue("dialogCount", out valObj);
            Dictionary<string, int> val = valObj as Dictionary<string, int>;
            if (val == null) capi.ObjectCache["dialogCount"] = val = new Dictionary<string, int>();
            return val;
        }

        protected bool IsRight(EnumPosFlag flag)
        {
            return flag == EnumPosFlag.RightBot || flag == EnumPosFlag.RightMid || flag == EnumPosFlag.RightTop;
        }

        protected float YOffsetMul(EnumPosFlag flag)
        {
            if (flag == EnumPosFlag.RightTop || flag == EnumPosFlag.LeftTop) return -1;
            if (flag == EnumPosFlag.RightBot || flag == EnumPosFlag.LeftBot) return 1;
            return 0;
        }
    }
}
