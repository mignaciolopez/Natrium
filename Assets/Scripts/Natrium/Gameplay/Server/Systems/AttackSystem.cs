using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Server.Components;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AttackSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<AttackSystemExecute>();
        }

        protected override void OnUpdate()
        {
            
        }
    }
}
