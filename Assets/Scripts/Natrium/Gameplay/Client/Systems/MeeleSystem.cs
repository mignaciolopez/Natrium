using Unity.Collections;
using Unity.Entities;

namespace Natrium.Gameplay.Client.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class MeeleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);



            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
