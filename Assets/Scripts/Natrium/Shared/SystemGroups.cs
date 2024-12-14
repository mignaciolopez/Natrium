using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Shared
{
    public partial class SharedSystemGroup : ComponentSystemGroup { }
    public partial class GameplaySystemGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CopyCommandBufferToInputSystemGroup))]
    public partial class MovementSystemGroup : ComponentSystemGroup { }
}