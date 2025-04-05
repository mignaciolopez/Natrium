using Unity.Entities;
using Unity.NetCode;

namespace CEG.Shared
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
    public partial class MovementSystemGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PredictedSimulationSystemGroup))]
    public partial class AttackSystemGroup : ComponentSystemGroup { }
}