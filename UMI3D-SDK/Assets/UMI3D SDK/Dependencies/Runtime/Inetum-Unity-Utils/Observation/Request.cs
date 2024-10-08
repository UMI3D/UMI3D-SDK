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
    /// The request made by a client to get some values from a supplier.
    /// </summary>
    public class Request 
    {
        /// <summary>
        /// Whether this request is associated with a <see cref="Supplier"/>.
        /// </summary>
        public bool IsAssociated => Supplier != null;

        /// <summary>
        /// Id of the request.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The publisher of the request.
        /// </summary>
        public Object Supplier { get; private set; }

        /// <summary>
        /// Additional information.<br/>
        /// <br/>
        /// key: Id of the information, Value: a func that return the additional information.
        /// </summary>
        public Dictionary<string, Func<Object>> Info { get; private set; }

        /// <summary>
        /// Set the info.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Func<Object> this[string id]
        {
            set
            {
                if (Info == null)
                {
                    Info = new();
                }
                Info[id] = value;
            }
        }

        Request() { }

        /// <summary>
        /// Request constructor. Initialize all the information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="supplier"></param>
        /// <param name="info"></param>
        public Request(string id, object supplier)
        {
            ID = id;
            Supplier = supplier;
        }

        /// <summary>
        /// Update the supplier identity and clear the information.
        /// </summary>
        /// <param name="supplier"></param>
        public void SetSupplier(Object supplier)
        {
            Supplier = supplier;
            Info.Clear();
        }

        /// <summary>
        /// Try to get supplier as <typeparamref name="T"/>.<br/>
        /// <br/>
        /// Return true if <see cref="Supplier"/> is of type <typeparamref name="T"/>, else false.<br/>
        /// If <see cref="Supplier"/> is of type <typeparamref name="T"/> then cast <see cref="Supplier"/> as <typeparamref name="T"/> in <paramref name="supplier"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public bool TryGetSupplier<T>(out T supplier, bool logError = true)
        {
            if (Supplier == null)
            {
                if (logError)
                {
                    UnityEngine.Debug.LogError($"Request: Supplier null.");
                }
                supplier = default;
                return false;
            }

            if (Supplier is not T supplierT)
            {
                if (logError)
                {
                    UnityEngine.Debug.LogError($"Request: Supplier is not of type {typeof(T).FullName}.\n" +
                                    $"Supplier is of type {Supplier.GetType().FullName}");
                }
                supplier = default;
                return false;
            }

            supplier = supplierT;
            return true;
        }

        /// <summary>
        /// Try to get the information stored with this <paramref name="key"/>.<br/>
        /// Return true if the information exist and no exception is raised, else false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool TryGetInfo(string key, out Object info, bool logError = true)
        {
            // If 'Info' is null then there is no additional information.
            if (Info == null)
            {
                if (logError)
                {
                    UnityEngine.Debug.LogError($"Request: '{ID}' does not contain info id: '{key}'.");
                }
                info = null;
                return false;
            }

            // Try to find the value corresponding to that 'key'.
            if (Info.TryGetValue(key, out Func<Object> infoFunc))
            {
                try
                {
                    info = infoFunc();
                    return true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[Request] TryGetInfo return an exception:\n " +
                            $"id: {ID}, supplier: {Supplier}");
                    UnityEngine.Debug.LogException(e);
                    info = default;
                    return false;
                }
            }
            else
            {
                info = default;
                return false;
            }
        }

        /// <summary>
        /// Try to get the information stored with this <paramref name="key"/>.<br/>
        /// Return true if the information exist and is of type <typeparamref name="T"/> and no exception is raised, else false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <param name="logError">Whether a log error will be display if no value is found.</param>
        /// <returns></returns>
        public bool TryGetInfoT<T>(string key, out T info, bool logError = true)
        {
            if (!TryGetInfo(key, out object infoObject, logError))
            {
                info = default;
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
                    string error = $"Request: '{ID}' does not contain info id: '{key}' of type {typeof(T)}.";
                    error += $"\nType of the object is {infoObject.GetType()}\n";
                    UnityEngine.Debug.LogError(error);
                }
                return false;
            }

            info = infoT;
            return true;
        }

        /// <summary>
        /// Try to get the information stored with this <paramref name="key"/>.<br/>
        /// Return true if the information exist and is of type <see cref="Nullable{T}"/> and no exception is raised, else false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <param name="logError">Whether a log error will be display if no value is found.</param>
        /// <returns></returns>
        public bool TryGetInfoNullableT<T>(string key, out Nullable<T> info, bool logError = true)
            where T : struct
        {
            if (!TryGetInfo(key, out object infoObject, logError))
            {
                info = default;
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
                    string error = $"Request: '{ID}' does not contain info id: '{key}' of type {typeof(T)}.";
                    error += $"\nType of the object is {infoObject.GetType()}";
                    UnityEngine.Debug.LogError(error);
                }
                return false;
            }

            info = infoT;
            return true;
        }
    }
}