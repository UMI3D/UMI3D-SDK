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

        /// <summary>
        /// Priority level of the notification.
        /// </summary>
        public NotificationPriority priority { get; set; }
        /// <summary>
        /// Notification title. A title is short and contain the main information on a notification.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Content of the notification. Could be a longer text than the title.
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// Answers to the notification as a string array.
        /// </summary>
        public string[] callback { get; set; }
        /// <summary>
        /// a notification with a duration of 0 should stay visible untile a DeleteEntityDto is received
        /// </summary>
        /// <seealso cref="DeleteEntityDto"/>
        public float duration { get; set; }
        /// <summary>
        /// Icon associated to a notification as a 2D resource.
        /// </summary>
        public ResourceDto icon2D { get; set; }
        /// <summary>
        /// Icon associated to a notification as a 3D resource.
        /// </summary>
        public ResourceDto icon3D { get; set; }
    }
}