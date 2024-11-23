using Natrium.Shared;
using Unity.Entities;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class MeeleSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            ecb.Playback(EntityManager);
        }
    }
}
