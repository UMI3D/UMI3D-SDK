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
using System.Threading.Tasks;
using UnityEngine;

namespace inetum.unityUtils
{
    public struct SubGlobal 
    {
        public string mainKey;

        public SubGlobal(string mainKey)
        {
            this.mainKey = mainKey;
        }

        /// <summary>
        /// Try to get the variable stored with the key <see cref="mainKey"/>/<paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="variable"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public bool TryGet<T>(string key, out T variable, bool debug = true)
            => Global.TryGet($"{mainKey}/{key}", out variable, debug);

        /// <summary>
        /// Try to get the variable stored with the key typeof(<typeparamref name="T"/>).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T variable, bool debug = true)
            => TryGet($"{mainKey}/{typeof(T).FullName}", out variable, debug);

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
        public async Task<bool> TryGetAsync<T>(string key, int numberOfRetry, Action<T> getVariable, bool debug = true)
            => await Global.TryGetAsync($"{mainKey}/{key}", numberOfRetry, getVariable, debug);

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
        public async Task<bool> TryGetAsync<T>(int numberOfRetry, Action<T> getVariable, bool debug = true)
            => await TryGetAsync($"{mainKey}/{typeof(T).FullName}", numberOfRetry, getVariable, debug);

        /// <summary>
        /// Get the variable stored with this key in an asynchronous way.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key)
        => await Global.GetAsync<T>($"{mainKey}/{key}");

        /// <summary>
        /// Get the variable stored with the key typeof(<typeparamref name="T"/>).FullName in an asynchronous way.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetAsync<T>()
            => await GetAsync<T>($"{mainKey}/{typeof(T).FullName}");

        /// <summary>
        /// Store a variable with this key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="variable"></param>
        public void Add<T>(string key, T variable)
            => Global.Add($"{mainKey}/{key}", variable);

        /// <summary>
        /// Store a variable with the key typeof(<typeparamref name="T"/>).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        public void Add<T>(T variable)
            => Add($"{mainKey}/{typeof(T).FullName}", variable);

        /// <summary>
        /// Try to remove a variable stored with this key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
            => Global.Remove($"{mainKey}/{key}");

        /// <summary>
        /// Try to remove a variable stored with the key typeof(<typeparamref name="T"/>).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public bool Remove<T>()
            => Remove($"{mainKey}/{typeof(T).FullName}");
    }
}