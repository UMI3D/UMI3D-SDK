/*
Copyright 2019 - 2024 Inetum

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
using System.Collections.Generic;
using UnityEngine;

namespace inetum.unityUtils
{
    public static class Global 
    {
        static umi3d.debug.UMI3DLogger logger = new(mainTag: nameof(Global));
        static Dictionary<string, object> variables = new();

        public static bool TryGetVariable<T>(string key, out T variable, bool debug = true)
        {
            if (!variables.ContainsKey(key))
            {
                if (debug)
                {
                    logger.Error(nameof(TryGetVariable), $"No variable of type: {typeof(T).FullName} with key: {key}");
                }
                variable = default(T);
                return false;
            }

            if (variables[key] is not T)
            {
                if (debug)
                {
                    logger.Error(nameof(TryGetVariable), $"Variable for key: {key} has type: {variables[key].GetType().FullName} but type: {typeof(T).FullName} is expected");
                }
                variable = default(T);
                return false;
            }

            variable = (T)variables[key];
            return true;
        }

        public static bool Get<T>(out T variable, bool debug = true)
            => TryGetVariable(typeof(T).FullName, out variable, debug);

        public static void Add<T>(string key, T variable)
        {
            if (variables.ContainsKey(key))
            {
                variables[key] = variable;
            }
            else
            {
                variables.Add(key, variable);
            }
        }

        public static void Add<T>(T variable) 
            => Add(variable.GetType().FullName, variable);

        public static bool Remove(string key)
        {
            if (!variables.ContainsKey(key))
            {
                return false;
            }

            variables.Remove(key);
            return true;
        }

        public static void Remove<T>()
            => Remove(typeof(T).FullName);
    }
}