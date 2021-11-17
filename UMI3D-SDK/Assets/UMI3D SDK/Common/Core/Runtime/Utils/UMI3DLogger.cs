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

        Editor = 1<<29
    }

    public class UMI3DLogger : Singleton<UMI3DLogger>
    {

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

        static bool validLevel(DebugLevel level)
        {
            return Exists ? Instance._validLevel(level) : (level & LogLevel) != 0;
        }

        static bool validScope(DebugScope scope)
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
    }

}