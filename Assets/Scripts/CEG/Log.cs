using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Logging;
using Unity.Logging.Internal.Debug;
using Unity.Logging.Sinks;
using UnityEngine;

namespace CEG
{
    public class Log : MonoBehaviour
    {
        #region Singleton
        private static Log _instance;
        public static Log Instance
        {
            private set => _instance = value;
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
        [SerializeField] private LogLevel logLevel;
        private LogLevel _previousLogLevel;

        [Serializable]
        private struct LogFilter : IEquatable<LogFilter>
        {
            public string name;
            public bool isLogging;
            public Color color;

            public bool Equals(LogFilter other)
            {
                return name == other.name && isLogging == other.isLogging && color.Equals(other.color);
            }

            public override bool Equals(object obj)
            {
                return obj is LogFilter other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(name, isLogging, color);
            }
        }
        
        [SerializeField] private List<LogFilter> logFilterWorlds;
        [SerializeField] private List<LogFilter> logFilterClass;
        [SerializeField] private int stackFrameIndex = 3;
        
        private void AwakeLogger()
        {
            logFilterWorlds ??= new List<LogFilter>();
            logFilterClass ??= new List<LogFilter>();

            var path = $"{Application.persistentDataPath}/Logs";
            var file = $"{DateTime.Now:yyyy.MM.dd - HH.mm.ss}.json";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fullPath = $"{path}/{file}";

            Unity.Logging.Log.Logger = new Unity.Logging.Logger(new LoggerConfig()
                .MinimumLevel.Set(logLevel)
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
            if (_previousLogLevel != logLevel)
                OnLogLevelChanged();
        }

        private void OnLogLevelChanged()
        {
            Verbose($"OnLogLevelChanged: {logLevel}", this.name, gameObject);
            Unity.Logging.Log.Logger.Config.MinimumLevel.Set(logLevel);
            _previousLogLevel = logLevel;
        }

        private string ResolveClassName(string className, UnityEngine.Object context = null)
        {
            if (className.Length > 0) 
                return className;
         
            var stackClassName = "";
            
            if (context != null)
            {
                stackClassName = context.name;
            }
            else
            {
                var stackFrames = new StackTrace();
                var methodInfo = stackFrames.GetFrame(stackFrameIndex).GetMethod();
                if (methodInfo.ReflectedType != null) 
                    stackClassName = methodInfo.ReflectedType.Name;
            }

            return stackClassName;
        }

        private string ResolveWorldName()
        {
            var worldName = "Unknown";

            var stackFrames = new StackTrace();
            var methodInfo = stackFrames.GetFrame(stackFrameIndex).GetMethod();
            if (methodInfo.ReflectedType == null)
                return worldName;
            
            var fullNameSpace = methodInfo.ReflectedType.Namespace;

            if (fullNameSpace == null)
                return worldName;
            
            var nameSpaces = fullNameSpace.Split(".");
            foreach (var nameSpace in nameSpaces)
            {
                if (nameSpace == "Client")
                {
                    worldName = nameSpace;
                    break;
                }
                
                if (nameSpace == "Server")
                {
                    worldName = nameSpace;
                    break;
                }

                if (nameSpace == "Shared")
                {
                    worldName = nameSpace;

                    fullNameSpace = stackFrames.GetFrame(stackFrameIndex + 1).GetMethod().ReflectedType?.Namespace;
                    var names2 = fullNameSpace?.Split(".");
                    if (names2 == null)
                        continue;
                    
                    foreach (var name2 in names2)
                    {
                        if (name2 == "Client")
                        {
                            worldName = name2;
                            break;
                        }

                        if (name2 == "Server")
                        {
                            worldName = name2;
                            break;
                        }
                    }
                }
            }

            return worldName;
        }

        private LogFilter UpdateLogFilterWorlds(string worldName)
        {
            for (var i = 0; i < logFilterWorlds.Count; i++)
                if (logFilterWorlds[i].name == worldName)
                    return logFilterWorlds[i];

            var logFilter = new LogFilter
            {
                name = worldName,
                isLogging = true,
                color = new Color(UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f)),
            };

            logFilterWorlds.Add(logFilter);
            Debug($"UpdateLogFilterWorlds: Add [{worldName}]", this.name, gameObject);

            return logFilter;
        }

        private LogFilter UpdateLogFilterClass(string className)
        {
            for (int i = 0; i < logFilterClass.Count; i++)
                if (logFilterClass[i].name == className)
                    return logFilterClass[i];

            var logFilter = new LogFilter
            {
                name = className,
                isLogging = true,
                color = new Color(UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f), UnityEngine.Random.Range(.4f, 1f)),
            };

            logFilterClass.Add(logFilter);
            Debug($"UpdateLogFilterClass: Add [{className}]", this.name, gameObject);

            return logFilter;
        }

        private void LogInternal(LogLevel level, string message, string className = "", UnityEngine.Object context = null)
        {
            if (Instance.logLevel > level)
            {
                return;
            }

            var worldName = Instance.ResolveWorldName();
            var worldFilter = Instance.UpdateLogFilterWorlds(worldName);

            if (!Instance.logFilterWorlds[Instance.logFilterWorlds.IndexOf(worldFilter)].isLogging)
            {
                return;
            }
            
            var stackClassName = Instance.ResolveClassName(className);
            var classFilter = Instance.UpdateLogFilterClass(stackClassName);

            if (!Instance.logFilterClass[Instance.logFilterClass.IndexOf(classFilter)].isLogging)
            {
                return;
            }
            
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
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
