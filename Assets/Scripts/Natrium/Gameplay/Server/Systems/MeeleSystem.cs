using Natrium.Gameplay.Shared.Components;
using Unity.Collections;
using Unity.Entities;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class MeeleSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<MeeleSystemExecute>();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
