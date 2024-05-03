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
using System.Threading.Tasks;
using UnityEngine;

namespace inetum.unityUtils
{
    public static class Global 
    {
        static umi3d.debug.UMI3DLogger logger = new(mainTag: nameof(Global));
        static Dictionary<string, object> variables = new();

        /// <summary>
        /// Try to get the variable stored with this <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="variable"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static bool TryGet<T>(string key, out T variable, bool debug = true)
        {
            if (!variables.ContainsKey(key))
            {
                if (debug)
                {
                    logger.Error(nameof(TryGet), $"No variable of type: {typeof(T).FullName} with key: {key}");
                }
                variable = default(T);
                return false;
            }

            if (variables[key] is not T)
            {
                if (debug)
                {
                    logger.Error(nameof(TryGet), $"Variable for key: {key} has type: {variables[key].GetType().FullName} but type: {typeof(T).FullName} is expected");
                }
                variable = default(T);
                return false;
            }

            variable = (T)variables[key];
            return true;
        }

        /// <summary>
        /// Try to get the variable stored with the key typeof(<typeparamref name="T"/>).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static bool TryGet<T>(out T variable, bool debug = true)
            => TryGet(typeof(T).FullName, out variable, debug);

        /// <summary>
        /// Try to get the variable stored with this <paramref name="key"/> in an asynchronous way.<br/>
        /// <br/>
        /// The method will try 1 time + <paramref name="numberOfRetry"/> times before giving up. If <paramref name="numberOfRetry"/> is 0 the there is only one try. If <paramref name="numberOfRetry"/> is inferior to 0 then there is an infinit number of try.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="numberOfRetry"></param>
        /// <param name="getVariable"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static async Task<bool> TryGetAsync<T>(string key, int numberOfRetry, Action<T> getVariable, bool debug = true)
        {
            T variable;
            while (!TryGet(key, out variable, false) && numberOfRetry != 0)
            {
                numberOfRetry--;
                await Task.Yield();
            }

            if (numberOfRetry == 0)
            {
                logger.Error(nameof(TryGetAsync), $"The Number of try has been exceeded. Could not get variable: {typeof(T).FullName} with key: {key}.");
                return false;
            }

            getVariable?.Invoke(variable);
            return true;
        }

        /// <summary>
        /// Try to get the variable stored with the key typeof(<typeparamref name="T"/>).FullName in an asynchronous way.<br/>
        /// <br/>
        /// The method will try 1 time + <paramref name="numberOfRetry"/> times before giving up. If <paramref name="numberOfRetry"/> is 0 the there is only one try. If <paramref name="numberOfRetry"/> is inferior to 0 then there is an infinit number of try.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="numberOfRetry"></param>
        /// <param name="getVariable"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static async Task<bool> TryGetAsync<T>(int numberOfRetry, Action<T> getVariable, bool debug = true)
        {
            T variable;
            while (!TryGet(out variable, false) && numberOfRetry != 0)
            {
                numberOfRetry--;
                await Task.Yield();
            }

            if (numberOfRetry == 0)
            {
                logger.Error(nameof(TryGetAsync), $"The Number of try has been exceeded. Could not get variable: {typeof(T).FullName}.");
                return false;
            }

            getVariable?.Invoke(variable);
            return true;
        }

        /// <summary>
        /// Get the variable stored with this key in an asynchronous way.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string key)
        {
            T variable;
            while (!TryGet(key, out variable, false))
            {
                await Task.Yield();
            }

            return variable;
        }

        /// <summary>
        /// Get the variable stored with the key typeof(<typeparamref name="T"/>).FullName in an asynchronous way.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>()
        {
            T variable;
            while (!TryGet(out variable, false))
            {
                await Task.Yield();
            }

            return variable;
        }

        /// <summary>
        /// Store a variable with this key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="variable"></param>
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

        /// <summary>
        /// Store a variable with the key typeof(<typeparamref name="T"/>).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        public static void Add<T>(T variable) 
            => Add(variable.GetType().FullName, variable);

        /// <summary>
        /// Try to remove a variable stored with this key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            if (!variables.ContainsKey(key))
            {
                return false;
            }

            variables.Remove(key);
            return true;
        }

        /// <summary>
        /// Try to remove a variable stored with the key typeof(<typeparamref name="T"/>).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static bool Remove<T>()
            => Remove(typeof(T).FullName);
    }
}