using AncientTools.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Entities.Tasks
{
    class AITaskFollowAttachedEntity : AiTaskBase
    {
        EntityMobileStorage mobileStorageEntity = null;

        public AITaskFollowAttachedEntity(EntityAgent entity) : base(entity)
        {
            if (entity is EntityMobileStorage)
                mobileStorageEntity = entity as EntityMobileStorage;
        }
        public override bool ShouldExecute()
        {
            if (mobileStorageEntity.AttachedEntity != null)
                return true;

            return false;
        }
        public override void StartExecute()
        {
            base.StartExecute();

            pathTraverser.WalkTowards(mobileStorageEntity.AttachedEntity.ServerPos.XYZ, 0.05f, 0.05f, OnGoalReached, OnStuck);
        }
        private void OnGoalReached()
        {

        }
        private void OnStuck()
        {

        }
        public override bool ContinueExecute(float dt)
        {
            if (mobileStorageEntity.AttachedEntity == null)
            {
                pathTraverser.Stop();

                return false;
            }

            if (mobileStorageEntity.AttachedEntity.Controls.TriesToMove)
            {
                pathTraverser.Retarget();

                pathTraverser.CurrentTarget.X = mobileStorageEntity.AttachedEntity.Pos.X + mobileStorageEntity.AttachedEntity.Controls.WalkVector.X * 20;
                pathTraverser.CurrentTarget.Y = mobileStorageEntity.AttachedEntity.Pos.Y + mobileStorageEntity.AttachedEntity.Controls.WalkVector.Y * 20;
                pathTraverser.CurrentTarget.Z = mobileStorageEntity.AttachedEntity.Pos.Z + mobileStorageEntity.AttachedEntity.Controls.WalkVector.Z * 20;
            }
            else
            {
                if (mobileStorageEntity.AttachedEntity.Pos.DistanceTo(mobileStorageEntity.Pos) <= 0.5)
                {
                    mobileStorageEntity.SetEntityPosition(mobileStorageEntity.AttachedEntity.ServerPos.BehindCopy(0.5).XYZ);

                    pathTraverser.Stop();
                }
            }

            return base.ContinueExecute(dt);
        }
    }
}
