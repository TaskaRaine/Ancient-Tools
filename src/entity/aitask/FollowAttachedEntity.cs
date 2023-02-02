using AncientTools.Utility;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace AncientTools.Entities.Tasks
{
    class AITaskFollowAttachedEntity : AiTaskBase
    {
        EntityMobileStorage mobileStorageEntity = null;

        private float FollowDistanceWalk { get; set; } = 0.5f;
        private float FollowDistanceSprint { get; set; } = 0.25f;
        private float FollowDistanceSneak { get; set; } = 0.6f;

        private float ParkedDistance { get; set; } = 1.0f;
        private Vec3d BehindVector { get; set; }

        public AITaskFollowAttachedEntity(EntityAgent entity) : base(entity)
        {
            if (entity is EntityMobileStorage)
                mobileStorageEntity = entity as EntityMobileStorage;
        }
        public override bool ShouldExecute()
        {
            if (mobileStorageEntity.AttachedEntity != null)
            {
                return true;
            }

            return false;
        }
        public override void StartExecute()
        {
            base.StartExecute();

            SetBehindVector(FollowDistanceWalk);
        }
        private void OnGoalReached()
        {
            if (!mobileStorageEntity.AttachedEntity.Controls.TriesToMove)
                SetBehindVector(ParkedDistance);
        }

        public override bool ContinueExecute(float dt)
        {
            if (mobileStorageEntity.AttachedEntity == null)
            {
                return false;
            }
            if (mobileStorageEntity.AttachedEntity.Controls.TriesToMove)
            {
                if (mobileStorageEntity.AttachedEntity.Controls.Sneak)
                    SetBehindVector(FollowDistanceSneak);
                else if (mobileStorageEntity.AttachedEntity.Controls.Sprint)
                    SetBehindVector(FollowDistanceSprint);
                else
                    SetBehindVector(FollowDistanceWalk);
            }

            return base.ContinueExecute(dt);
        }
        private void SetBehindVector(float distance)
        {
            BehindVector = mobileStorageEntity.AttachedEntity.Pos.BehindCopy(distance).XYZ;
            BehindVector.Y = mobileStorageEntity.AttachedEntity.Pos.Y;

            mobileStorageEntity.SetEntityPosition(BehindVector);
        }
    }
}
