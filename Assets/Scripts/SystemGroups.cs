using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
public partial class MovementSystemGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PredictedSimulationSystemGroup))]
public partial class AttackSystemGroup : ComponentSystemGroup { }