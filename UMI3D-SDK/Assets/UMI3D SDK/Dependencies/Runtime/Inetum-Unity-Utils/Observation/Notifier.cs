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
    public class Notifier
    {
        /// <summary>
        /// Additional information.
        /// </summary>
        public Dictionary<string, Object> info;
        /// <summary>
        /// The notification subscribers filter.
        /// </summary>
        public INotificationFilter subscribersFilter;

        /// <summary>
        /// The object sending the notification.
        /// </summary>
        Object publisher;
        /// <summary>
        /// Id of the notification.
        /// </summary>
        string id;
        /// <summary>
        /// The notification hub that handle the notification.
        /// </summary>
        INotificationHub notificationHub;

        public Notifier(
            Object publisher,
            string id,
            INotificationFilter subscribersFilter,
            Dictionary<string, object> info,
            INotificationHub notificationHub
        )
        {
            this.publisher = publisher;
            this.id = id;
            this.subscribersFilter = subscribersFilter;
            this.info = info;
            this.notificationHub = notificationHub;
        }

        /// <summary>
        /// Get or set the additional information. See <see cref="info"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Object this[string id]
        {
            get => info[id];
            set
            {
                if (info == null)
                {
                    info = new();
                }
                info[id] = value;
            }
        }

        /// <summary>
        /// Send a notification to all the concerning subscribers. Return the number of observers that have been notified.
        /// </summary>
        /// <returns></returns>
        public int Notify()
        {
            return notificationHub.Notify(
                publisher,
                id,
                subscribersFilter,
                info
            );
        }
    }
}
