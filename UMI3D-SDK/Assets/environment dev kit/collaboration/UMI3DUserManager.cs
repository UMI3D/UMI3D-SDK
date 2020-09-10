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

using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;

namespace umi3d.edk.collaboration
{
    public class UMI3DUserManager
    {

        /// <summary>
        /// Contain the users connected to the scene.
        /// </summary>
        Dictionary<string, UMI3DCollaborationUser> users = new Dictionary<string, UMI3DCollaborationUser>();
        Dictionary<string, string> loginMap = new Dictionary<string, string>();

        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DCollaborationUser GetUser(string id)
        {
            return users.ContainsKey(id) ? users[id] : null;
        }

        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DCollaborationUser GetUserByToken(string authorization)
        {
            foreach (UMI3DCollaborationUser u in users.Values)
            {
                if (UMI3DNetworkingKeys.bearer + u.token == authorization)
                    return u;
            }

            return null;
        }

        /// <summary>
        /// Return all UMI3D users.
        /// </summary>
        public IEnumerable<UMI3DCollaborationUser> Users
        {
            get {
                return users.Values;
            }
        }

        public void Logout(UMI3DCollaborationUser user)
        {
            if (users.ContainsKey(user.Id()))
                users.Remove(user.Id());
            if (loginMap.ContainsKey(user.login))
                loginMap.Remove(user.login);
            user.SetStatus(StatusType.NONE);
            user.Logout();
        }

        public IEnumerator ConnectionClose(string id)
        {
            if (users.ContainsKey(id))
            {
                var user = users[id];
                user.SetStatus(StatusType.MISSING);
            }
            yield break;
        }


        public void CreateUser(string Login, UMI3DWebSocketConnection connection, Action<UMI3DCollaborationUser, bool> Callback)
        {
            UMI3DCollaborationUser user;
            bool reconnection = false;
            if (loginMap.ContainsKey(Login) && users.ContainsKey(loginMap[Login]))
            {
                user = users[loginMap[Login]];
                user.connection = connection;
                reconnection = true;
            }
            else
            {
                user = new UMI3DCollaborationUser(Login, connection);
                users.Add(user.Id(), user);
            }
            Callback.Invoke(user, reconnection);
        }

        /// <summary>
        /// Called when a status update is received by the real-time connection.
        /// </summary>
        public void OnStatusUpdate(string id, StatusType status)
        {
            if (users.ContainsKey(id))
            {
                users[id].SetStatus(status);
            }
        }
    }
}