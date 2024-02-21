using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static Log Instance
        {
            private set { _instance = value; }
            get
            {
                if (_instance == null)
                {
                }

                return _instance; 
            }
        }

        private void AwakeSingleton()
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
        }
        #endregion Singleton

        #region Logger
        [SerializeField] private LogLevel _logLevel;
        private LogLevel _previousLogLevel;

        [Serializable]
        private struct LogFilter
        {
            public string name;
            public bool isLogging;
            public Color color;
        }
        [SerializeField] private List<LogFilter> _logFilterWorlds;
        [SerializeField] private List<LogFilter> _logFilterClass;
        [SerializeField] private int _stackFrameIndex = 3;
        private void AwakeLogger()
        {
            _logFilterWorlds ??= new List<LogFilter>();
            _logFilterClass ??= new List<LogFilter>();

            var path = $"{Application.persistentDataPath}/Logs";
            var file = $"{DateTime.Now:yyyy.MM.dd - HH.mm.ss}.json";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fullPath = $"{path}/{file}";

            Unity.Logging.Log.Logger = new Unity.Logging.Logger(new LoggerConfig()
                .MinimumLevel.Set(_logLevel)
                .RedirectUnityLogs(false)
                .WriteTo.File(fullPath, minLevel: LogLevel.Verbose, formatter: LogFormatterJson.Formatter));

            SelfLog.SetMode(SelfLog.Mode.EnabledInUnityEngineDebugLogError);

            Info("Log Started.", this.name, this);
        }
        #endregion Logger

        private void Awake()
        {
            AwakeSingleton();
            AwakeLogger();
        }

        private void OnDestroy()
        {
            _instance = null;
            Info("Log Ended.", this.name, this);
        }

        private void Update()
        {
            if (_previousLogLevel != _logLevel)
                OnLogLevelChanged();
        }

        private void OnLogLevelChanged()
        {
            Verbose($"OnLogLevelChanged: {_logLevel}", this.name, gameObject);
            Unity.Logging.Log.Logger.Config.MinimumLevel.Set(_logLevel);
            _previousLogLevel = _logLevel;
        }

        private string ResolveClassName(string className, UnityEngine.Object context = null)
        {
            string stackClassName = className;

            if (stackClassName.Length <= 0)
            {
                if (context != null)
                {
                    stackClassName = context.name;
                }
                else
                {
                    var stackFrames = new StackTrace();
                    var methodInfo = stackFrames.GetFrame(_stackFrameIndex).GetMethod();
                    stackClassName = methodInfo.ReflectedType.Name;
                }
            }

            return stackClassName;
        }

        private string ResolveWorldName()
        {
            string worldName = "Unknown";

            var stackFrames = new StackTrace();
            var methodInfo = stackFrames.GetFrame(_stackFrameIndex).GetMethod();
            var nameSpace = methodInfo.ReflectedType.Namespace;

            var names = nameSpace.Split(".");
            foreach (var name in names)
            {
                if (name == "Client")
                    return "Client";
                else if (name == "Server")
                    return "Server";

                if (name == "Shared")
                {
                    worldName = "Shared";

                    nameSpace = stackFrames.GetFrame(_stackFrameIndex + 1).GetMethod().ReflectedType.Namespace;
                    var names2 = nameSpace.Split(".");
                    foreach (var name2 in names2)
                    {
                        if (name2 == "Client")
                            return "Client";
                        else if (name2 == "Server")
                            return "Server";
                    }
                }
            }

            return worldName;
        }

        private LogFilter UpdateLogFilterWorlds(string worldName)
        {
            for (int i = 0; i < _logFilterWorlds.Count; i++)
                if (_logFilterWorlds[i].name == worldName)
                    return _logFilterWorlds[i];

            var logFilter = new LogFilter
            {
                name = worldName,
                isLogging = true,
                color = new Color(UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f)),
            };

            _logFilterWorlds.Add(logFilter);
            Debug($"UpdateLogFilterWorlds: Add [{worldName}]", this.name, gameObject);

            return logFilter;
        }

        private LogFilter UpdateLogFilterClass(string className)
        {
            for (int i = 0; i < _logFilterClass.Count; i++)
                if (_logFilterClass[i].name == className)
                    return _logFilterClass[i];

            var logFilter = new LogFilter
            {
                name = className,
                isLogging = true,
                color = new Color(UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f)),
            };

            _logFilterClass.Add(logFilter);
            Debug($"UpdateLogFilterClass: Add [{className}]", this.name, gameObject);

            return logFilter;
        }

        private void LogInternal(LogLevel level, string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance._logLevel <= level)
            {
                string worldName = Instance.ResolveWorldName();
                var worldFilter = Instance.UpdateLogFilterWorlds(worldName);

                if (Instance._logFilterWorlds[Instance._logFilterWorlds.IndexOf(worldFilter)].isLogging)
                {
                    string stackClassName = Instance.ResolveClassName(className);
                    var classFilter = Instance.UpdateLogFilterClass(stackClassName);

                    if (Instance._logFilterClass[Instance._logFilterClass.IndexOf(classFilter)].isLogging)
                    {
                        var hexColorWorld = ColorUtility.ToHtmlStringRGB(worldFilter.color);
                        var hexColorClass = ColorUtility.ToHtmlStringRGB(classFilter.color);
                        switch(level)
                        {
                            case LogLevel.Verbose:
                                UnityEngine.Debug.Log($"<b><color=grey>[{LogLevel.Verbose}]</color></b> <color=#{hexColorWorld}>[{worldName}]</color> <color=#{hexColorClass}>[{stackClassName}]</color> {message}", context);
                                Unity.Logging.Log.Verbose(message);
                                break;
                            case LogLevel.Debug:
                                UnityEngine.Debug.Log($"<b><color=white>[{LogLevel.Debug}]</color></b> <color=#{hexColorWorld}>[{worldName}]</color> <color=#{hexColorClass}>[{stackClassName}]</color> <color=white>{message}</color>", context);
                                Unity.Logging.Log.Debug(message);
                                break;
                            case LogLevel.Info:
                                UnityEngine.Debug.Log($"<b><color=cyan>[{LogLevel.Info}]</color></b> <color=#{hexColorWorld}>[{worldName}]</color> <color=#{hexColorClass}>[{stackClassName}]</color> <color=white>{message}</color>", context);
                                Unity.Logging.Log.Info(message);
                                break;
                            case LogLevel.Warning:
                                UnityEngine.Debug.LogWarning($"<b><color=yellow>[{LogLevel.Warning}]</color></b> <color=#{hexColorWorld}>[{worldName}]</color> <color=#{hexColorClass}>[{stackClassName}]</color> <color=white>{message}</color>", context);
                                Unity.Logging.Log.Warning(message);
                                break;
                            case LogLevel.Error:
                                UnityEngine.Debug.LogError($"<b><color=#AB1B1B>[{LogLevel.Error}]</color></b> <color=#{hexColorWorld}>[{worldName}]</color> <color=#{hexColorClass}>[{stackClassName}]</color> <color=white>{message}</color>", context);
                                Unity.Logging.Log.Error(message);
                                break;
                            case LogLevel.Fatal:
                                UnityEngine.Debug.LogAssertion($"<b><color=red>[{LogLevel.Fatal}]</color></b> <color=#{hexColorWorld}>[{worldName}]</color> <color=#{hexColorClass}>[{stackClassName}]</color> <b><color=white>{message}</color></b>", context);
                                Unity.Logging.Log.Fatal(message);
                                break;
                        }
                    }
                }
            }
        }

        public static void Verbose(string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance != null)
                Instance.LogInternal(LogLevel.Verbose, message, className, context);
            else
                UnityEngine.Debug.Log($"<b><color=grey>[{LogLevel.Verbose}]</color></b> {message}", context);
        }

        public static void Debug(string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance != null)
                Instance.LogInternal(LogLevel.Debug, message, className, context);
            else
                UnityEngine.Debug.Log($"<color=white>[{LogLevel.Debug}]</color> <color=white>{message}</color>", context);
        }

        public static void Info(string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance != null)
                Instance.LogInternal(LogLevel.Info, message, className, context);
            else
                UnityEngine.Debug.Log($"<color=cyan>[{LogLevel.Info}]</color> <color=white>{message}</color>", context);
        }

        public static void Warning(string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance != null)
                Instance.LogInternal(LogLevel.Warning, message, className, context);
            else
                UnityEngine.Debug.LogWarning($"<color=yellow>[{LogLevel.Warning}]</color> <color=white>{message}</color>", context);
        }

        public static void Error(string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance != null)
                Instance.LogInternal(LogLevel.Error, message, className, context);
            else
                UnityEngine.Debug.LogError($"<color=#AB1B1B>[{LogLevel.Error}]</color> <color=white>{message}</color>", context);
        }

        public static void Fatal(string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance != null)
                Instance.LogInternal(LogLevel.Fatal, message, className, context);
            else
                UnityEngine.Debug.LogAssertion($"<color=red>[{LogLevel.Fatal}]</color> <color=white>{message}</color>", context);
        }
    }
}
