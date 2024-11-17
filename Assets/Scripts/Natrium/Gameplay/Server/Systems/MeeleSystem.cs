using Natrium.Shared;
using Unity.Entities;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class MeeleSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            ecb.Playback(EntityManager);
        }
    }
}
