using AncientTools.Utility;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AncientTools.Entities.Tasks
{
    class AITaskFollowAttachedEntity : AiTaskBase
    {
        EntityMobileStorage mobileStorageEntity = null;

        private float FollowDistance { get; set; } = 0.25f;

        private float ParkedDistance { get; set; } = 1.0f;
        private Vec3d BehindVector { get; set; }

        public AITaskFollowAttachedEntity(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig) : base(entity, taskConfig, aiConfig)
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

            SetBehindVector(ParkedDistance);
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
                SetBehindVector(FollowDistance);
            }

            return base.ContinueExecute(dt);
        }
        private void SetBehindVector(float distance)
        {
            BehindVector = mobileStorageEntity.AttachedEntity.Pos.BehindCopy(FollowDistance).XYZ;
            BehindVector.Y = mobileStorageEntity.AttachedEntity.Pos.Y;

            mobileStorageEntity.SetEntityPosition(BehindVector);
        }
    }
}
