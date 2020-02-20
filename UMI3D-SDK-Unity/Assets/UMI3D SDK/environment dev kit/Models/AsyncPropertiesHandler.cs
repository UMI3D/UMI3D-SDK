/*
Copyright 2019 Gfi Informatique

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

    public class AsyncPropertiesHandler : IHasAsyncProperties
    {

        public AsyncPropertiesHandler()
        {
        }

        public delegate void EBroadcastUpdate();
        public delegate void EbroadcastUpdateForUSer(UMI3DUser user);

        /// <summary>
        /// Delegate function to be called if this Handler Have to broadcast global update
        /// </summary>
        public EBroadcastUpdate DelegateBroadcastUpdate;
        /// <summary>
        /// Delegate function to be called if this Handler Have to broadcast local update
        /// </summary>
        public EbroadcastUpdateForUSer DelegatebroadcastUpdateForUser;

        /// <summary>
        /// Indicates if there is some updates to broadcast.
        /// </summary>
        protected bool updated = false;

        /// <summary>
        /// Indicates the users for whom there is specific updates.
        /// </summary>
        protected List<UMI3DUser> updatedFor = new List<UMI3DUser>();

        /// <summary>
        /// Notify that there is some updates in the default properties to broadcast.
        /// </summary>
        public void NotifyUpdate()
        {
            updated = true;
        }

        /// <summary>
        /// Notify that there is some updates in the user's specific properties to broadcast.
        /// </summary>
        /// <param name="user">the user</param>
        public void NotifyUpdate(UMI3DUser u)
        {
            if (!updatedFor.Contains(u))
                updatedFor.Add(u);
        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        public void BroadcastUpdates()
        {
            if (updated || updatedFor.Count > 0)
                if (DelegateBroadcastUpdate != null) DelegateBroadcastUpdate();
            updated = false;
            updatedFor.Clear();
        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        /// <param name="user">the user</param>
        public void BroadcastUpdates(UMI3DUser user)
        {
            if (DelegatebroadcastUpdateForUser != null) DelegatebroadcastUpdateForUser(user);
        }
    }
}