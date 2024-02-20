using System;
using System.IO;
using Unity.Logging;
using Unity.Logging.Internal.Debug;
using Unity.Logging.Sinks;
using UnityEngine;

namespace Natrium.Shared
{
    public class Log : MonoBehaviour
    {
        #region Singleton
        private static Log _instance;
        public static Log Instance { private set { _instance = value; } get { return _instance; } }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            #endregion Singleton

            #region Logger

            var path = $"{Application.persistentDataPath}/Logs";
            var file = $"{DateTime.Now:yyyy.MM.dd - HH.mm.ss}.json";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fullPath = $"{path}/{file}";

            Unity.Logging.Log.Logger = new Unity.Logging.Logger(new LoggerConfig()
                .MinimumLevel.Set(_logLevel)
                .RedirectUnityLogs(false)
                .WriteTo.File(fullPath, minLevel: LogLevel.Verbose, formatter: LogFormatterJson.Formatter));
                //.WriteTo.StdOut(outputTemplate: "{Level} || {Timestamp} || {Message}"));

            SelfLog.SetMode(SelfLog.Mode.EnabledInUnityEngineDebugLogError);

            #endregion Logger
        }

        [SerializeField] private LogLevel _logLevel;
        private LogLevel _previousLogLevel;

        private void Update()
        {
            if (_previousLogLevel != _logLevel)
                OnLogLevelChanged();
        }

        private void OnLogLevelChanged()
        {
            Unity.Logging.Log.Logger.Config.MinimumLevel.Set(_logLevel);
            _previousLogLevel = _logLevel;
        }

        public static void Verbose(string message, UnityEngine.Object context = null)
        {
            if (Instance != null)
            {
                if (Instance._logLevel <= LogLevel.Verbose)
                {
                    Unity.Logging.Log.Verbose(message);
                    UnityEngine.Debug.Log($"<b><color=grey>[{LogLevel.Verbose}]</color></b> {message}", context);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"<b><color=grey>[{LogLevel.Verbose}]</color></b> {message}", context);
            }
        }

        public static void Debug(string message, UnityEngine.Object context = null)
        {
            if (Instance != null)
            {
                if (Instance._logLevel <= LogLevel.Debug)
                {
                    Unity.Logging.Log.Debug(message);
                    UnityEngine.Debug.Log($"<color=white>[{LogLevel.Debug}]</color> <color=white>{message}</color>", context);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"<color=white>[{LogLevel.Debug}]</color> <color=white>{message}</color>", context);
            }
        }

        public static void Info(string message, UnityEngine.Object context = null)
        {
            if (Instance != null)
            {
                if (Instance._logLevel <= LogLevel.Info)
                {
                    Unity.Logging.Log.Info(message);
                    UnityEngine.Debug.Log($"<color=cyan>[{LogLevel.Info}]</color> <color=white>{message}</color>", context);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"<color=cyan>[{LogLevel.Info}]</color> <color=white>{message}</color>", context);
            }
        }

        public static void Warning(string message, UnityEngine.Object context = null)
        {
            if (Instance != null)
            {
                if (Instance._logLevel <= LogLevel.Warning)
                {
                    Unity.Logging.Log.Warning(message);
                    UnityEngine.Debug.Log($"<color=yellow>[{LogLevel.Warning}]</color> <color=white>{message}</color>", context);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"<color=yellow>[{LogLevel.Warning}]</color> <color=white>{message}</color>", context);
            }
        }

        public static void Error(string message, UnityEngine.Object context = null)
        {
            if (Instance != null)
            {
                if (Instance._logLevel <= LogLevel.Error)
                {
                    Unity.Logging.Log.Error(message);
                    UnityEngine.Debug.Log($"<color=#AB1B1B>[{LogLevel.Error}]</color> <color=white>{message}</color>", context);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"<color=#AB1B1B>[{LogLevel.Error}]</color> <color=white>{message}</color>", context);
            }
        }

        public static void Fatal(string message, UnityEngine.Object context = null)
        {
            if (Instance != null)
            {
                if (Instance._logLevel <= LogLevel.Fatal)
                {
                    Unity.Logging.Log.Fatal(message);
                    UnityEngine.Debug.Log($"<color=red>[{LogLevel.Fatal}]</color> <color=white>{message}</color>", context);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"<color=red>[{LogLevel.Fatal}]</color> <color=white>{message}</color>", context);
            }
        }
    }
}
