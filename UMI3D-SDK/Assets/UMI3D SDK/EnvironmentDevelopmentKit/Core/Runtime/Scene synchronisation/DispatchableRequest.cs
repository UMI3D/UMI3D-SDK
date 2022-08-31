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
using System.Collections.Generic;

namespace umi3d.edk
{
    /// <summary>
    /// A dispatchable request is any request from the server the clients that does not concern a UMI3D entity.
    /// </summary>
    public abstract class DispatchableRequest
    {
        /// <summary>
        /// List of users to which this request should be send.
        /// </summary>
        public HashSet<UMI3DUser> users;
        /// <summary>
        /// Reliable requests are request where receiving is ensuring.
        /// </summary>
        /// Note that requests that are not reliable are lighter.
        public bool reliable;

        public DispatchableRequest(bool reliable, HashSet<UMI3DUser> users)
        {
            this.users = users ?? (UMI3DServer.Exists ? UMI3DServer.Instance.UserSetWhenHasJoined() : new HashSet<UMI3DUser>());
            this.reliable = reliable;
        }

        /// <summary>
        /// Convert the request to a byte array.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToBytes();

        /// <summary>
        /// Convert the request to a BSON as a byte array.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToBson();

        public void Dispatch()
        {
            UMI3DServer.Dispatch(this);
        }
    }
}