/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Levels used for debugging purposes.
    /// </summary>
    [Flags]
    public enum DebugLevel
    {
        None = 0,
        Default = 1 << 0,
        Error = 1 << 1,
        Warning = 1 << 2,
        Exception = 1 << 3
    }

    /// <summary>
    /// Contains the available debug scope byte identifiers. They are used to define precisely the debugging scope.
    /// </summary>
    /// Debug scopes are objects of 32 bits, with a only one isolated bit per scope. This allows to use bitwise operators like "|" 
    /// to define multiscope debugging. <br/>
    /// Example : the common scope is "00000000_00000000_00000000_00000001" and the core scope is "00000000_00000000_00000000_00001000", 
    /// so the common.core scope is "00000000_00000000_00000000_00001001".
    [Flags]
    public enum DebugScope
    {
        None = 0,

        Common = 1 << 0,
        EDK = 1 << 1,
        CDK = 1 << 2,

        Core = 1 << 3,
        Interaction = 1 << 4,
        UserCapture = 1 << 5,
        Collaboration = 1 << 6,

        Networking = 1 << 7,
        Loading = 1 << 8,
        Animation = 1 << 9,

        Material = 1 << 10,
        Bytes = 1 << 11,
        User = 1 << 12,

        Connection = 1 << 13,
        Mumble = 1 << 14,

        Editor = 1 << 29,
        Other = 1 << 28
    }

    /// <summary>
    /// Helper class to use to log error related to UMI3D.
    /// </summary>
    public class UMI3DLogger : inetum.unityUtils.PersistentSingleBehaviour<UMI3DLogger>
    {
        #region logging
        [SerializeField]
        private DebugScope _logScope = DebugScope.None;
        [SerializeField]
        private DebugLevel _logLevel = DebugLevel.Default | DebugLevel.Warning | DebugLevel.Error;

        /// <summary>
        /// Path where the logs are written.
        /// </summary>
        public static string LogPath
        {
            get => Exists ? Instance.logPath : null;
            set
            {
                if (Exists)
                {
                    Instance.logPath = value;
                    Instance.logWritter?.Stop();

                    Instance.logWritter = string.IsNullOrEmpty(Instance.logPath) ? null : new ThreadWritter(Instance.logPath);
                }
            }
        }

        /// <summary>
        /// Path where the logs are written.
        /// </summary>
        [SerializeField]
        private string logPath;

        /// <summary>
        /// If true, logging is allowed.
        /// </summary>
        public static bool ShouldLog
        {
            get => Exists && Instance.log;
            set
            {
                if (Exists)
                {
                    Instance.log = value;
                }
            }
        }
        [SerializeField]
        private bool log;


        public static DebugScope LogScope
        {
            get => Exists ? Instance._logScope : (DebugScope)~0;
            set
            {
                if (Exists)
                    Instance._logScope = value;
            }
        }
        public static DebugLevel LogLevel
        {
            get => Exists ? Instance._logLevel : DebugLevel.Error;
            set
            {
                if (Exists)
                    Instance._logLevel = value;
            }
        }

        public static void Log(object o, DebugScope scope)
        {
            if (validLevel(DebugLevel.Default) && validScope(scope))
                if (Exists)
                    Instance._Log(o, scope);
                else
                    Debug.Log(GetTime() + o);
        }

        /// <summary>
        /// Debug log raising a Warning.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="scope"></param>
        public static void LogWarning(object o, DebugScope scope)
        {
            if (validLevel(DebugLevel.Warning) && validScope(scope))
                if (Exists)
                    Instance._LogWarning(o, scope);
                else
                    Debug.LogWarning(GetTime() + o);
        }

        /// <summary>
        /// Debug log raising an Error.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="scope"></param>
        public static void LogError(object o, DebugScope scope)
        {
            if (validLevel(DebugLevel.Error) && validScope(scope))
                if (Exists)
                    Instance._LogError(o, scope);
                else
                    Debug.LogError(GetTime() + o);
        }

        /// <summary>
        /// Debug log related to an Exception.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="scope"></param>
        public static void LogException(Exception o, DebugScope scope)
        {
            if (validLevel(DebugLevel.Error) && validScope(scope))
                if (Exists)
                    Instance._LogException(o, scope);
                else
                {
                    Debug.LogError("Exception " + GetTime());
                    Debug.LogException(o);
                }
        }

        private static bool validLevel(DebugLevel level)
        {
            return Exists ? Instance._validLevel(level) : (level & LogLevel) != 0;
        }

        private static bool validScope(DebugScope scope)
        {
            return Exists ? Instance._validFlag(scope) : (scope & LogScope) != 0;
        }

        private static string GetTime()
        {
            var now = DateTime.Now;

#if UNITY_EDITOR
            return string.Empty;
#else
            return "[" + now.ToString("hh:mm:ss:fffff") + "] ";
#endif
        }

        protected virtual void _Log(object o, DebugScope scope)
        {
            if (ShouldLog)
                logWritter?.Write(o.ToString());
            Debug.Log(GetTime() + o);
        }

        protected virtual void _LogWarning(object o, DebugScope scope)
        {
            if (ShouldLog)
                logWritter?.Write("Warning: " + o.ToString());
            Debug.LogWarning(GetTime() + o);
        }

        protected virtual void _LogError(object o, DebugScope scope)
        {
            if (ShouldLog)
                logWritter?.Write("Error: " + o.ToString());
            Debug.LogError(GetTime() + o);
        }

        protected virtual void _LogException(Exception o, DebugScope scope)
        {
            if (ShouldLog)
                logWritter?.Write("Exception: " + o.Message + "\n" + o.StackTrace);
            Debug.LogError(GetTime() + "Exception");
            Debug.LogException(o);
        }

        protected virtual bool _validFlag(DebugScope scope) { return (scope & LogScope) != 0; }
        protected virtual bool _validLevel(DebugLevel level) { return (level & LogLevel) != 0; }
        #endregion

        #region DebugInfo

        private readonly Dictionary<ILoggable, List<DebugInfo>> Loggables = new Dictionary<ILoggable, List<DebugInfo>>();

        public static void Register(ILoggable loggable) { if (Exists) Instance._Register(loggable); }
        public static void Unregister(ILoggable loggable) { if (Exists) Instance._Unregister(loggable); }

        protected virtual void _Register(ILoggable loggable)
        {
            Loggables[loggable] = loggable.GetInfos();
        }

        protected virtual void _Unregister(ILoggable loggable)
        {
            Loggables.Remove(loggable);
        }

        protected virtual string LogData(float time)
        {
            string data = $"Time : {time},{Environment.NewLine}";
            bool globalOk = false;
            foreach (KeyValuePair<ILoggable, List<DebugInfo>> loggable in Loggables)
            {
                bool localOk = false;
                string localdata = $"\"{loggable.Key.GetLogName()}\":{{{Environment.NewLine}";
                if (loggable.Value != null && loggable.Value.Count > 0)
                    loggable.Value.ForEach(info =>
                    {
                        (bool, string) i = info.GetData();
                        if (i.Item1)
                        {
                            localOk = true;
                            localdata += $"\"{info.name}\":\"{i.Item2}\",{Environment.NewLine}";
                        }
                    });
                localdata += $"}}";
                if (localOk)
                {
                    data += localdata;
                    globalOk = true;
                }
            }
            return globalOk ? data : null;
        }

        public static string LogInfoPath
        {
            get => Exists ? Instance.infoPath : null;
            set
            {
                if (Exists)
                {
                    Instance.infoPath = value;
                    Instance.infoWritter?.Stop();
                    Instance.infoWritter = string.IsNullOrEmpty(Instance.infoPath) ? null : new ThreadWritter(Instance.infoPath);
                }
            }
        }
        [SerializeField]
        private string infoPath;
        public static float LogInfoDelta
        {
            get => Exists ? Instance.logInfoDelta : 0;
            set
            {
                if (Exists)
                {
                    Instance.logInfoDelta = value;
                    Instance.wait = new WaitForSecondsRealtime(Instance.logInfoDelta);
                }
            }
        }
        [SerializeField]
        private float logInfoDelta;
        public static bool ShouldLogInfo
        {
            get => Exists && Instance.logInfo;
            set
            {
                if (Exists)
                {
                    Instance.logInfo = value;
                    if (value)
                    {
                        StartCoroutine(Instance.LogCoroutine());
                    }
                }
            }
        }
        [SerializeField]
        private bool logInfo;
        private WaitForSecondsRealtime wait;
        private bool running = false;
        protected virtual IEnumerator LogCoroutine()
        {
            if (running || !ShouldLogInfo) yield break;
            running = true;
            if (wait == null) wait = new WaitForSecondsRealtime(LogInfoDelta);
            WriteLogFile($"{DateTime.Now}");
            while (ShouldLogInfo)
            {
                string data = LogData(Time.unscaledTime);
                if (data != null)
                    WriteLogFile(data);
                yield return wait;
            }
            running = false;
        }

        protected void WriteLogFile(string data)
        {
            infoWritter?.Write(data);
        }

        private ThreadWritter infoWritter;
        private ThreadWritter logWritter;

        protected override void Awake()
        {
            base.Awake();

            infoWritter = string.IsNullOrEmpty(LogInfoPath) ? null : new ThreadWritter(LogInfoPath);
            logWritter = string.IsNullOrEmpty(LogPath) ? null : new ThreadWritter(LogPath);
            StartCoroutine(LogCoroutine());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            infoWritter?.Stop();
            logWritter?.Stop();
        }
        #endregion
    }


    public abstract class DebugInfo
    {
        public readonly bool isStatic;
        public readonly string name;

        protected DebugInfo(string name, bool isStatic)
        {
            this.isStatic = isStatic;
            this.name = name;
        }

        /// <summary>
        /// state if the value was updated since last GetData
        /// </summary>
        /// <returns></returns>
        public abstract bool Updated();

        /// <summary>
        /// return the data as a string if it was updated since last call;
        /// </summary>
        /// <returns></returns>
        public abstract (bool, string) GetData();


        /// <summary>
        /// return the data as a string;
        /// </summary>
        /// <returns></returns>
        public abstract string GetCurrentData();
    }

    public class DebugInfo<T> : DebugInfo
    {
        private T lastValue;
        private readonly Func<T> GetValue;
        private readonly Func<T, string> serializer;
        private bool updated = false;

        public DebugInfo(string name, T value, Func<T, string> serializer = null) : this(name, true, serializer)
        {
            lastValue = value;

        }

        public DebugInfo(string name, Func<T> GetValue, Func<T, string> serializer = null) : this(name, false, serializer)
        {
            this.GetValue = GetValue;
        }

        private DebugInfo(string name, bool isStatic, Func<T, string> serializer) : base(name, isStatic)
        {
            this.serializer = serializer ?? ((T o) => o?.ToString() ?? "Null");
            updated = true;
        }

        public T GetTData()
        {
            if (!isStatic)
                return this.GetValue.Invoke();
            return lastValue;
        }

        /// <inheritdoc/>
        public override (bool, string) GetData()
        {
            bool ok = Updated();
            if (ok) Debug.Log(ok);

            return (ok, GetIfUpdatedData());
        }

        private string GetIfUpdatedData()
        {
            if (updated)
            {
                updated = false;
                return serializer(lastValue);
            }
            return null;
        }

        /// <inheritdoc/>
        public override string GetCurrentData()
        {
            return serializer(GetTData());
        }

        /// <inheritdoc/>
        public override bool Updated()
        {
            if (updated)
            {
                lastValue = GetTData();
                return true;
            }
            if (!isStatic)
            {
                T value = GetTData();
                if (
                    (!value?.Equals(lastValue) ?? false)
                    || (!lastValue?.Equals(value) ?? false)
                    )
                {
                    lastValue = value;
                    updated = true;
                    return true;
                }
            }
            return false;

        }
    }

    public interface ILoggable
    {
        string GetLogName();
        List<DebugInfo> GetInfos();
    }
}