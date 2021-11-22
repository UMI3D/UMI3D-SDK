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
using System.IO;
using UnityEngine;

namespace umi3d.common
{
    [Flags]
    public enum DebugLevel
    {
        None = 0,
        Default = 1 << 0,
        Error = 1 << 1,
        Warning = 1 << 2,
    }

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

        Editor = 1 << 29
    }

    public class UMI3DLogger : Singleton<UMI3DLogger>
    {
        #region logging
        [SerializeField]
        private DebugScope _logScope = DebugScope.None;
        [SerializeField]
        private DebugLevel _logLevel = DebugLevel.Default | DebugLevel.Warning | DebugLevel.Error;

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
            get => Exists ? Instance._logLevel : (DebugLevel)~0;
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
                    Debug.Log(o);
        }

        public static void LogWarning(object o, DebugScope scope)
        {
            if (validLevel(DebugLevel.Warning) && validScope(scope))
                if (Exists)
                    Instance._LogWarning(o, scope);
                else
                    Debug.LogWarning(o);
        }

        public static void LogError(object o, DebugScope scope)
        {
            if (validLevel(DebugLevel.Error) && validScope(scope))
                if (Exists)
                    Instance._LogError(o, scope);
                else
                    Debug.LogError(o);
        }

        private static bool validLevel(DebugLevel level)
        {
            return Exists ? Instance._validLevel(level) : (level & LogLevel) != 0;
        }

        private static bool validScope(DebugScope scope)
        {
            return Exists ? Instance._validFlag(scope) : (scope & LogScope) != 0;
        }

        protected virtual void _Log(object o, DebugScope scope)
        {
            Debug.Log(o);
        }

        protected virtual void _LogWarning(object o, DebugScope scope)
        {
            Debug.LogWarning(o);
        }

        protected virtual void _LogError(object o, DebugScope scope)
        {
            Debug.LogError(o);
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
            string data = Environment.NewLine;
            data += $"Time : {time}";
            foreach (var loggable in Loggables)
            {
                data += $"{loggable.Key.GetLogName()}{Environment.NewLine}";
                if (loggable.Value != null && loggable.Value.Count > 0)
                    loggable.Value.ForEach(info =>
                    {
                        if (!info.isStatic)
                            data += $"{info.name}:{info.getData()}";
                    });
            }
            return data;
        }

        public static string LogPath
        {
            get => Exists ? Instance.path : null;
            set { if (Exists) Instance.path = value; }
        }
        [SerializeField]
        string path;
        public static float LogDelta
        {
            get => Exists ? Instance.logDelta : 0;
            set { if (Exists)
                {
                    Instance.logDelta = value;
                    Instance.wait = new WaitForSecondsRealtime(Instance.logDelta);
                }
            }
        }
        [SerializeField]
        float logDelta;
        public static bool ShouldLog
        {
            get => Exists ? Instance.log : false;
            set { 
                if (Exists)
                {
                    Instance.log = value;
                    if (value)
                    {
                        Instance.StartCoroutine(Instance.LogCoroutine());
                    }
                }
            }
        }
        [SerializeField]
        bool log;
        WaitForSecondsRealtime wait;

        bool running = false;
        protected virtual IEnumerator LogCoroutine()
        {
            if (running || !ShouldLog) yield break;
            running = true;
            if (wait == null) wait = new WaitForSecondsRealtime(LogDelta);
            CreateLogFile($"{DateTime.Now}");
            while (ShouldLog)
            {
                var data = LogData(Time.unscaledTime);
                WriteLogFile(data);
                yield return wait;
            }
            running = false;
        }

        protected void CreateLogFile(string header) {
            if (!File.Exists(LogPath))
                using (StreamWriter sw = File.CreateText(LogPath))
                    sw.Write(Environment.NewLine + header);
            else
                using (StreamWriter sw = File.AppendText(LogPath))
                    sw.Write(Environment.NewLine + header);
        }

        protected void WriteLogFile(string data)
        {
            using (StreamWriter sw = File.AppendText(LogPath))
                sw.WriteLine(Environment.NewLine + data);
        }


        protected virtual void Start()
        {
            StartCoroutine(LogCoroutine());
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

        public abstract object getData();

    }

    public abstract class DebugInfo<T> : DebugInfo
    {
        private T lastValue;
        private readonly Func<T> GetValue;

        protected DebugInfo(string name, T value) : base(name, true)
        {
            lastValue = value;
        }

        protected DebugInfo(string name, Func<T> GetValue) : base(name, false)
        {
            this.GetValue = GetValue;
        }

        public T GetData()
        {
            if (!isStatic)
                lastValue = GetValue();
            return lastValue;
        }

        public override object getData()
        {
            return GetData();
        }
    }

    public interface ILoggable
    {
        string GetLogName();
        List<DebugInfo> GetInfos();
    }
}