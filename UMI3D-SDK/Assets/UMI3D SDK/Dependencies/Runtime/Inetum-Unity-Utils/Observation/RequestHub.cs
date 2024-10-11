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
    public class RequestHub : IRequestHub
    {
        /// <summary>
        /// The default instance of <see cref="RequestHub"/>.
        /// </summary>
        public static RequestHub Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new();
                }
                return _default;
            }
        }
        static RequestHub _default;

        /// <summary>
        /// ID to requests.
        /// </summary>
        Dictionary<string, Request> _requests = new();
        /// <summary>
        /// Supplier to IDs.
        /// </summary>
        Dictionary<Object, HashSet<string>> _supplierToID = new();

        #region Subscribe

        public Request SubscribeAsSupplier(
            Object supplier,
            string id
        )
        {
            // Check if a request already exist for that 'id'.
            if (_requests.TryGetValue(id, out Request request))
            {
                // A request is already registered for that 'id'.

                // Update the request's supplier if necessary.
                if (request.Supplier != supplier)
                {
                    // Only one supplier.
                    request.SetSupplier(supplier);
                }
            }
            else
            {
                // If no request exist for that 'id' create a new one.
                request = new(id, supplier);
                _requests.Add(id, request);
            }

            // Check if this 'supplier' already supply.
            if (_supplierToID.TryGetValue(supplier, out HashSet<string> ids))
            {
                // Add the 'id' to the list of ids, if the list didn't contain this 'id' already.
                // This list is a set, that means there is no duplicate ids.
                ids.Add(id);
            }
            else
            {
                // If that 'subscriber' listen to no one, create a new association 'subscriber' -> ids.
                _supplierToID.Add(supplier, new HashSet<string>() { id });
            }

            return request;
        }

        public Request SubscribeAsSupplier<T>(Object supplier)
        {
            return SubscribeAsSupplier(supplier, typeof(T).FullName);
        }

        public Request SubscribeAsClient(
            Object client,
            string id
        )
        {
            // Check if a request already exist for that 'id'.
            if (!_requests.TryGetValue(id, out Request request))
            {
                // If no request exist for that 'id' create a new one.
                request = new(id, null);
                _requests.Add(id, request);
            }

            request.SubscribeAsClient(client);
            return request;
        }

        public Request SubscribeAsClient<T>(Object client)
        {
            return SubscribeAsClient(this, typeof(T).FullName);
        }

        #endregion

        #region Unsubscibe

        public void UnsubscribeAsSupplier(Object supplier)
        {
            // Check if that 'supplier' is supplying.
            if (!_supplierToID.TryGetValue(supplier, out HashSet<string> ids))
            {
                UnityEngine.Debug.LogWarning($"[RequestHub] Try to unsubscribe [{supplier}] that is not supplying.");
                return;
            }

            // Loop through all the supplier request id.
            foreach (string id in ids)
            {
                // Check if subscriptions exist for 'id'.
                if (!_requests.TryGetValue(id, out Request request))
                {
                    UnityEngine.Debug.LogError($"[{nameof(UnsubscribeAsSupplier)}] Try remove request for {supplier}. No request for {id}, that should not happen.");
                    continue;
                }

                request.SetSupplier(null);

                if (request.Clients.Count == 0)
                {
                    // Remove id from the requests.
                    _requests.Remove(id);
                }
            }

            // Clear the ids.
            ids.Clear();

            // Remove 'supplier' from '_supplierToID'.
            _supplierToID.Remove(supplier);
        }

        public void UnsubscribeAsSupplier(Object supplier, string id)
        {
            // Check if that 'supplier' is supplying.
            if (!_supplierToID.TryGetValue(supplier, out HashSet<string> ids))
            {
                UnityEngine.Debug.LogWarning($"[RequestHub] Try to unsubscribe [{supplier}] but supplier is not supplying.");
                return;
            }

            if (!ids.Contains(id))
            {
                UnityEngine.Debug.LogWarning($"[RequestHub] Try to unsubscribe [{supplier}] that is not supplying for id [{id}].");
                return;
            }

            // Remove the id.
            ids.Remove(id);

            // If there is not more ids then remove 'supplier' from '_supplierToID'.
            if (ids.Count == 0)
            {
                _supplierToID.Remove(supplier);
            }

            // Check if subscriptions exist for 'id'.
            if (!_requests.TryGetValue(id, out Request request))
            {
                UnityEngine.Debug.LogError($"[{nameof(UnsubscribeAsSupplier)}] Try remove request for {supplier}. No request for {id}, that should not happen.");
                return;
            }

            request.SetSupplier(null);

            if (request.Clients.Count == 0)
            {
                // Remove id from the requests.
                _requests.Remove(id);
            }
        }

        public void UnsubscribeAsSupplier<T>(Object supplier)
        {
            UnsubscribeAsSupplier(this, typeof(T).FullName);
        }

        #endregion
    }
}