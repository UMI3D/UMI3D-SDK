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

namespace inetum.unityUtils.observation
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
        /// Initializes a new instance of the Notification class.<br/>
        /// Logs an error if the id is null or empty, or if the publisher is null.<br/>
        /// <br/>
        /// <example>
        /// Given an id, publisher, and info when constructing a Notification then create a notification.<br/>
        /// <code>
        /// Notification notification = new(id, publisher, info);
        /// // notification.ID == id
        /// // notification.Publisher == publisher
        /// // notification.Info == info
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="id">The unique identifier for the notification.</param>
        /// <param name="publisher">The publisher of the notification.</param>
        /// <param name="info">Additional information related to the notification.</param>
        public Notification(string id, object publisher, Dictionary<string, object> info) : this()
        {
            if (string.IsNullOrEmpty(id))
            {
                UnityEngine.Debug.LogError($"[Notification.Notification] Error: create a new notification with id null or empty.");
            }

            if (publisher == null)
            {
                string message = $"[Notification.Notification] Error: create a new notification with a null publisher.\n" +
                    $"Having a null publisher is a bad practice because it increase complexity while debugging.";
                UnityEngine.Debug.LogError(message);
            }

            ID = id;
            Publisher = publisher;
            Info = info;
        }

        /// <summary>
        /// Tries to retrieve information associated with a given key from the notification.<br/>
        /// If the key is null or not found, it logs an error (if logError is true) and returns false.<br/>
        /// <br/>
        /// <example>
        /// Given a notification and a key, when the key is found in the notification's info, then return true and the associated info.<br/>
        /// <code>
        /// Notification notification = new Notification("id", this, new() { { "key", "value" } });
        /// bool result = notification.TryGetInfo("key", out object info); // result = true, info = "value"
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="key">The key to search for in the notification's info.</param>
        /// <param name="info">The output parameter that will hold the associated info if the key is found.</param>
        /// <param name="logError">Optional parameter to log errors if the key is not found. Default is true.</param>
        /// <returns>True if the key is found and info is retrieved; otherwise, false.</returns>
        public bool TryGetInfo(string key, out Object info, bool logError = true)
        {
            // Key cannot be null.
            if (key == null)
            {
                info = null;
                if (logError)
                {
                    UnityEngine.Debug.LogError($"[Notification.TryGetInfo] Error: key is null for notification '{ID}'.");
                }
                return false;
            }

            string errorMessage;
            if (Info == null)
            {
                info = null;
                if (logError)
                {
                    errorMessage = $"[Notification.TryGetInfo] Error: key '{key}' not found for notification '{ID}'.\n" +
                        $"Reason: info is null.";
                    UnityEngine.Debug.LogError(errorMessage);
                }
                return false;
            }
            else if (Info.Count == 0)
            {
                info = null;
                if (logError)
                {
                    errorMessage = $"[Notification.TryGetInfo] Error: key '{key}' not found for notification '{ID}'.\n" +
                        $"Reason: info is empty.";
                    UnityEngine.Debug.LogError(errorMessage);
                }
                return false;
            }
            else if (!Info.TryGetValue(key, out info))
            {
                info = null;
                if (logError)
                {
                    errorMessage = $"[Notification.TryGetInfo] Error: key '{key}' not found for notification '{ID}'.\n" +
                        $"Reason: info does not contain '{key}'.";
                    UnityEngine.Debug.LogError(errorMessage);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to retrieve information of a specified type associated with a given key from the notification.<br/>
        /// If the key is null or not found or if the type does not match, it logs an error (if logError is true) and returns false.<br/>
        /// If the value is null and the type is a reference type, it logs a warning and returns true.<br/>
        /// <br/>
        /// <example>
        /// Given a notification and a key, when the key is found in the notification's info and the type matches, then return true and the associated info.<br/>
        /// <code>
        /// Notification notification = new Notification("id", this, new() { { "key", "value" } });
        /// bool result = notification.TryGetInfoT("key", out string info); // result = true, info = "value"
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T">The type of the information to retrieve.</typeparam>
        /// <param name="key">The key to search for in the notification's info.</param>
        /// <param name="info">The output parameter that will hold the associated info if the key is found and the type matches.</param>
        /// <param name="logError">Optional parameter to log errors if the key is not found or the type does not match. Default is true.</param>
        /// <returns>True if the key is found and info is retrieved; otherwise, false.</returns>
        public bool TryGetInfoT<T>(string key, out T info, bool logError = true)
        {
            if (!TryGetInfo(key, out object infoObject, logError))
            {
                info = default;
                return false;
            }

            if (infoObject == null)
            {
                info = default;
                string error;
                Type type = typeof(T);
                
                if (!IsNullable(type) && type.IsValueType || type.IsEnum)
                {
                    if (logError)
                    {
                        error =
                        $"[Notification.TryGetInfoT] Error: notification '{ID}' does not contain key '{key}' of type {typeof(T)}.\n" +
                        $"Type of the object is Unknown because the value is null.";
                        UnityEngine.Debug.LogError(error);
                    }
                    return false;
                } else
                {
                    if (logError)
                    {
                        error =
                        $"[Notification.TryGetInfoT] Warning: notification '{ID}' has a null value for key '{key}'.\n" +
                        $"The initial type of a null value is Unknown but you are trying to cast it in '{typeof(T)}'.";
                        UnityEngine.Debug.LogWarning(error);
                    }
                    return true;
                }
            }

            // Try to cast the information.
            if (infoObject is not T infoT)
            {
                info = default;
                if (logError)
                {
                    string error = 
                    $"[Notification.TryGetInfoT] Error: notification '{ID}' does not contain key '{key}' of type {typeof(T)}.\n" +
                    $"Type of the object is {infoObject.GetType()}.";
                    UnityEngine.Debug.LogError(error);
                }
                return false;
            }

            info = infoT;
            return true;
        }

        bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Logs an error message indicating that a notification does not contain a specified key.
        /// </summary>
        /// <param name="subscriber">The subscriber name.</param>
        /// <param name="infoKey">The key that is missing in the notification's info</param>
        public void LogError(string subscriber, string infoKey, string message = null)
        {
            string error = "";
            if (subscriber == null)
            {
                error += "[NULL]";
            } else if (subscriber == "")
            {
                error += "[EMPTY]";
            } else
            {
                error += $"[{subscriber}]";
            }

            error += $" notification: '{ID}' does not contain key: ";

            if (infoKey == null)
            {
                error += "'NULL'.";
            }
            else if (infoKey == "")
            {
                error += "'EMPTY'.";
            }
            else
            {
                error += $"'{infoKey}'.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                error += "\n" + message;
            }

            UnityEngine.Debug.LogError(error);
        }
    }
}
