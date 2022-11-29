using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace AncientTools.Entities.Tasks
{
    class RegisterAITasks: ModSystem
    {
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            AiTaskRegistry.Register<AITaskFollowAttachedEntity>("followattached");
        }
    }
}
