using Unity.Entities;

namespace Natrium.Gameplay.Client.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class MeeleSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);



            ecb.Playback(EntityManager);
        }
    }
}
