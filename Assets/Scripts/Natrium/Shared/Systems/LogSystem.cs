using Natrium.Shared.Components;
using Unity.Entities;
using Unity.Collections;

namespace Natrium.Shared.Systems
{
    public partial class LogSystem : SystemBase
    {
        private EntityCommandBuffer _ecb;

        protected override void OnCreate()
        {
            base.OnCreate();

            //_ecb = new EntityCommandBuffer(Allocator.Temp);
        }

        protected override void OnUpdate()
        {
            _ecb = new EntityCommandBuffer(Allocator.Temp);

            PerformLogging();

            _ecb.Playback(EntityManager);
            _ecb.Dispose();
        }

        private void PerformLogging()
        {
            foreach(var (log , e) in SystemAPI.Query<LogData>().WithEntityAccess())
            {
                switch(log.logLevel)
                {
                    case Unity.Logging.LogLevel.Verbose:
                        Log.Verbose($"[{log.WorldName}] {log.Message}", log.ClassName.ToString());
                        break;
                    case Unity.Logging.LogLevel.Debug:
                        Log.Debug($"[{log.WorldName}] {log.Message}", log.ClassName.ToString());
                        break;
                    case Unity.Logging.LogLevel.Info:
                        Log.Info($"[{log.WorldName}] {log.Message}", log.ClassName.ToString());
                        break;
                    case Unity.Logging.LogLevel.Warning:
                        Log.Warning($"[{log.WorldName}] {log.Message}", log.ClassName.ToString());
                        break;
                    case Unity.Logging.LogLevel.Error:
                        Log.Error($"[{log.WorldName}] {log.Message}", log.ClassName.ToString());
                        break;
                    case Unity.Logging.LogLevel.Fatal:
                        Log.Fatal($"[{log.WorldName}] {log.Message}", log.ClassName.ToString());
                        break;
                }

                _ecb.DestroyEntity(e);
            }
        }
    }
}
