using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace CEG.Gameplay.Shared
{
    [DisallowMultipleComponent]
    public class SystemsSettingsAuthoring : MonoBehaviour
    {
        #region ConnectionSettings
        public string fqdn = "localhost";
        public ushort port = 7979;
        #endregion ConnectionSettings
    }

    public class SystemsSettingsBaker : Baker<SystemsSettingsAuthoring>
    {
        public override void Bake(SystemsSettingsAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            {
                AddComponent(e, new SystemsSettings
                {
                    #region ConnectionSettings
                    Fqdn = authoring.fqdn,
                    Port = authoring.port,
                    #endregion ConnectionSettings
                });
            }
        }
    }

    public struct SystemsSettings : IComponentData
    {
        #region ConnectionSettings
        public FixedString32Bytes Fqdn;
        public ushort Port;
        #endregion ConnectionSettings
    }
}
