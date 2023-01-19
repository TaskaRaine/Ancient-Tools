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

            mobileStorageEntity.SetEntityPosition(mobileStorageEntity.AttachedEntity.ServerPos.BehindCopy(FollowDistanceWalk).XYZ);
        }
        private void OnGoalReached()
        {
            if(!mobileStorageEntity.AttachedEntity.Controls.TriesToMove)
                mobileStorageEntity.SetEntityPosition(mobileStorageEntity.AttachedEntity.ServerPos.BehindCopy(ParkedDistance).XYZ);
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
                    mobileStorageEntity.SetEntityPosition(TaskaMath.Vec3Lerp(mobileStorageEntity.ServerPos.XYZ, mobileStorageEntity.AttachedEntity.ServerPos.BehindCopy(FollowDistanceSneak).XYZ, 0.5f));
                else if (mobileStorageEntity.AttachedEntity.Controls.Sprint)
                    mobileStorageEntity.SetEntityPosition(TaskaMath.Vec3Lerp(mobileStorageEntity.ServerPos.XYZ, mobileStorageEntity.AttachedEntity.ServerPos.BehindCopy(FollowDistanceSprint).XYZ, 0.5f));
                else
                    mobileStorageEntity.SetEntityPosition(TaskaMath.Vec3Lerp(mobileStorageEntity.ServerPos.XYZ, mobileStorageEntity.AttachedEntity.ServerPos.BehindCopy(FollowDistanceWalk).XYZ, 0.5f));
            }

            return base.ContinueExecute(dt);
        }
    }
}
