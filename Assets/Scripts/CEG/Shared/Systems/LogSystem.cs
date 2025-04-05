using CEG.Shared.Components;
using Unity.Entities;
using Unity.Collections;

namespace CEG.Shared.Systems
{
    public partial class LogSystem : SystemBase
    {
        private EntityCommandBuffer _ecb;

        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();

            //_ecb = new EntityCommandBuffer(WorldUpdateAllocator);
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            _ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            PerformLogging();

            _ecb.Playback(EntityManager);
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
