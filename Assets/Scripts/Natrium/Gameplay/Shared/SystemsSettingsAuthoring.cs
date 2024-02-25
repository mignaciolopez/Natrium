using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace Natrium.Gameplay.Shared
{
    [DisallowMultipleComponent]
    public class SystemsSettingsAuthoring : MonoBehaviour
    {
        #region ConnectionSettings
        [Header("Connection Settings")]
        public string FQDN = "localhost";
        public ushort Port = 7979;
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
                    FQDN = authoring.FQDN,
                    Port = authoring.Port,
                    #endregion ConnectionSettings
                });
            }
        }
    }

    public struct SystemsSettings : IComponentData
    {
        #region ConnectionSettings
        public FixedString32Bytes FQDN;
        public ushort Port;
        #endregion ConnectionSettings
    }
}
