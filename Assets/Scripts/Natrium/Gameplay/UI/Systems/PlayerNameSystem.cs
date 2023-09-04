using Unity.Entities;

namespace Natrium.Gameplay.UI.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class PlayerNameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            
        }
    }
}