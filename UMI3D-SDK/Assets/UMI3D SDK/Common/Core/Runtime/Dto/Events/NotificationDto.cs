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

namespace umi3d.common
{
    /// <summary>
    /// Notification to sent from the environment to a user.
    /// </summary>
    public class NotificationDto : AbstractEntityDto, IEntity
    {
        public enum NotificationPriority { Low, Medium, High }

        public NotificationPriority priority;
        public string title;
        public string content;
        public string[] callback;
        /// <summary>
        /// a notification with a duration of 0 should stay visible untile a DeleteEntityDto is received
        /// </summary>
        /// <seealso cref="DeleteEntityDto"/>
        public float duration;
        public ResourceDto icon2D;
        public ResourceDto icon3D;

        public NotificationDto() : base() { }
    }

    /// <summary>
    /// Notification conserning a UMI3D scene object.
    /// </summary>
    public class NotificationOnObjectDto : NotificationDto
    {
        public ulong objectId;

        public NotificationOnObjectDto() : base() { }
    }
}