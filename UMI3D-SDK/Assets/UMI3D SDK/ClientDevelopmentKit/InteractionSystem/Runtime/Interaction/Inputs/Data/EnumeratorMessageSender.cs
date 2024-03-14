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
using System.Collections;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    [Serializable]
    public class EnumeratorMessageSender 
    {
        [Tooltip("Network message emission frame rate")]
        /// <summary>
        /// Network message emission frame rate.<br/>
        /// <br/>
        /// Warning: high values can cause network flood.
        /// </summary>
        public float networkFrameRate = 1;

        public Coroutine networkMessage;

        public Func<bool> canSend;
        public Action messageHandler;

        public IEnumerator NetworkMessageSender()
        {
            var wait = new WaitForSeconds(1f / networkFrameRate);

            while (canSend?.Invoke() ?? false)
            {
                messageHandler?.Invoke();
                yield return wait;
            }
        }
    }
}