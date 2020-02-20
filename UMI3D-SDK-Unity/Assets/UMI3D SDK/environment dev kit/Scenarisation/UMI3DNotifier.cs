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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;

namespace umi3d.edk
{
    public abstract class UMI3DNotifier 
    {
        /// <summary>
        /// Send a notification to a given user.
        /// </summary>
        /// <param name="user">User to notify</param>
        /// <param name="notification">Notification to send</param>
        public static void Notify(UMI3DUser user, NotificationDto notification)
        {
            user.Send(notification);
        }

        /// <summary>
        /// Send a notification to every users.
        /// </summary>
        /// <param name="notification">Notification to send</param>
        public static void NotifyAllUsers(NotificationDto notification)
        {
            foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
                Notify(user, notification);
        }

    }
}