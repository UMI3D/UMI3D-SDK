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

namespace inetum.unityUtils
{
    /// <summary>
    /// The notification send by the publisher to the subscribers.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Id of the notification.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The publisher of the notification.
        /// </summary>
        public Object Publisher { get; private set; }

        /// <summary>
        /// Additional information.<br/>
        /// <br/>
        /// key: Id of the information, Value: the additional information.
        /// </summary>
        public Dictionary<string, Object> Info { get; private set; }

        Notification() { }

        /// <summary>
        /// Notification constructor. Initialize all the information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="publisher"></param>
        /// <param name="info"></param>
        public Notification(string id, object publisher, Dictionary<string, object> info)
        {
            ID = id;
            Publisher = publisher;
            Info = info;
        }

        /// <summary>
        /// Try to get the information stored with this <paramref name="key"/>.<br/>
        /// Return true if the information exist, else false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool TryGetInfo(string key, out Object info, bool logError = true)
        {
            // If 'Info' is null then there is no additional information.
            if (Info == null)
            {
                info = null;
                if (logError)
                {
                    UnityEngine.Debug.LogError($"Notification: '{ID}' does not contain info id: '{key}'.");
                }
                return false;
            }

            // Try to find the value corresponding to that 'key'.
            return Info.TryGetValue(key, out info);
        }

        /// <summary>
        /// Try to get the information stored with this <paramref name="key"/>.<br/>
        /// Return true if the information exist and is of type <typeparamref name="T"/>, else false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <param name="logError">Whether a log error will be display if no value is found.</param>
        /// <returns></returns>
        public bool TryGetInfoT<T>(string key, out T info, bool logError = true)
        {
            if (!TryGetInfo(key, out object infoObject))
            {
                info = default;
                if (logError)
                {
                    UnityEngine.Debug.LogError($"Notification: '{ID}' does not contain info id: '{key}'.");
                }
                return false;
            }

            // Try to cast the information.
            if (infoObject is not T infoT)
            {
                info = default;
                if (infoObject == null)
                {
                    // If infoObject is not T but is null then return true.
                    return true;
                }

                if (logError)
                {
                    string error = $"Notification: '{ID}' does not contain info id: '{key}' of type {typeof(T)}.";
                    error += $"\nType of the object is {infoObject.GetType()}";
                    UnityEngine.Debug.LogError(error);
                }
                return false;
            }

            info = infoT;
            return true;
        }

        /// <summary>
        /// Try to get the information stored with this <paramref name="key"/>.<br/>
        /// Return true if the information exist and is of type <see cref="Nullable{T}"/>, else false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <param name="logError">Whether a log error will be display if no value is found.</param>
        /// <returns></returns>
        public bool TryGetInfoNullableT<T>(string key, out Nullable<T> info, bool logError = true)
            where T : struct
        {
            if (!TryGetInfo(key, out object infoObject))
            {
                info = default;
                if (logError)
                {
                    UnityEngine.Debug.LogError($"Notification: '{ID}' does not contain info id: '{key}'.");
                }
                return false;
            }

            // Try to cast the information.
            if (infoObject is not T infoT)
            {
                info = null;
                if (infoObject == null)
                {
                    // If infoObject is not T but is null then return true.
                    // No cast exist to Nullable<T>.
                    return true;
                }

                if (logError)
                {
                    string error = $"Notification: '{ID}' does not contain info id: '{key}' of type {typeof(T)}.";
                    error += $"\nType of the object is {infoObject.GetType()}";
                    UnityEngine.Debug.LogError(error);
                }
                return false;
            }

            info = infoT;
            return true;
        }

        /// <summary>
        /// Display a log error.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="infoKey"></param>
        public void LogError(string subscriber, string infoKey, string message = null)
        {
            string error = $"[{subscriber}] notification: '{ID}' does not contain info id: '{infoKey}'.";
            if (!string.IsNullOrEmpty(error))
            {
                error += "\n";
                error += message;
            }
            UnityEngine.Debug.LogError(error);
        }
    }
}
