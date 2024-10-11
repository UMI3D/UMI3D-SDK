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
    public interface INotificationHub
    {
        #region Subscribe

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        /// <param name="publishersFilter">Only the notifications that pass this filter test can be sent to this 'subscriber'.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe(
            Object subscriber,
            string id,
            INotificationFilter publishersFilter,
            Action<Notification> action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="publishersFilter">Only the notifications that pass this filter test can be sent to this 'subscriber'.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe<T>(
            Object subscriber,
            INotificationFilter publishersFilter,
            Action<Notification> action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        /// <param name="publishersFilter">Only the notifications that pass this filter test can be sent to this 'subscriber'.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe(
            Object subscriber,
            string id,
            INotificationFilter publishersFilter,
            Action action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="publishersFilter">Only the notifications that pass this filter test can be sent to this 'subscriber'.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe<T>(
            Object subscriber,
            INotificationFilter publishersFilter,
            Action action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe(
            Object subscriber,
            string id,
            Action<Notification> action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe<T>(
            Object subscriber,
            Action<Notification> action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe(
            Object subscriber,
            string id,
            Action action
        );

        /// <summary>
        /// Add an entry to notify the <paramref name="subscriber"/> by calling the 
        /// <paramref name="action"/> when the publisher send a notification.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="action">The action perform to notify.</param>
        public void Subscribe<T>(
            Object subscriber,
            Action action
        );

        #endregion

        #region Unsubscibe

        /// <summary>
        /// Remove all entries concerning a specific <paramref name="subscriber"/>.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null.</param>
        public void Unsubscribe(Object subscriber);

        /// <summary>
        /// Remove matching entries concerning a specific <paramref name="subscriber"/> and <paramref name="id"/>.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        public void Unsubscribe(Object subscriber, string id);

        /// <summary>
        /// Remove matching entries concerning a specific <paramref name="subscriber"/> and id: <typeparamref name="T"/>.
        /// </summary>
        /// <param name="subscriber">The object waiting for notification. Must not be null. If subscriber is static then user typeof().FullName.</param>
        public void Unsubscribe<T>(Object subscriber);

        #endregion

        #region Notify

        /// <summary>
        /// Send a notification to all the concerning subscribers. Return the number of observers that have been notified.
        /// </summary>
        /// <param name="publisher">The object sending the notification. Must not be null. Ideally must correspond to object.GetType().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        /// <param name="subscribersFilter">The notifications that pass this filter test can be sent to the subscribers.</param>
        /// <param name="info">Additional information.</param>
        /// <returns></returns>
        public int Notify(
            Object publisher,
            string id,
            INotificationFilter subscribersFilter,
            Dictionary<string, Object> info = null
        );

        /// <summary>
        /// Send a notification to all the concerning subscribers. Return the number of observers that have been notified.
        /// </summary>
        /// <param name="publisher">The object sending the notification. Must not be null. Ideally must correspond to object.GetType().FullName.</param>
        /// <param name="subscribersFilter">The notifications that pass this filter test can be sent to the subscribers.</param>
        /// <param name="info">Additional information.</param>
        /// <returns></returns>
        public int Notify<T>(
            Object publisher,
            INotificationFilter subscribersFilter,
            Dictionary<string, Object> info = null
        );

        /// <summary>
        /// Send a notification to all the concerning subscribers. Return the number of observers that have been notified.
        /// </summary>
        /// <param name="publisher">The object sending the notification. Must not be null. Ideally must correspond to object.GetType().FullName.</param>
        /// <param name="id">Id of the notification.</param>
        /// <param name="info">Additional information.</param>
        /// <returns></returns>
        int Notify(
            Object publisher,
            string id,
            Dictionary<string, Object> info = null
        );

        /// <summary>
        /// Send a notification to all the concerning subscribers. Return the number of observers that have been notified.
        /// </summary>
        /// <param name="publisher">The object sending the notification. Must not be null. Ideally must correspond to object.GetType().FullName.</param>
        /// <param name="info">Additional information.</param>
        /// <returns></returns>
        int Notify<T>(
            Object publisher,
            Dictionary<string, Object> info = null
        );

        #endregion

        #region GetNotifier

        /// <summary>
        /// Get a <see cref="Notifier"/>. Use that to optimize notification sending.
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="id"></param>
        /// <param name="subscribersFilter"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public Notifier GetNotifier(
            Object publisher,
            string id,
            INotificationFilter subscribersFilter = null,
            Dictionary<string, Object> info = null
        );

        /// <summary>
        /// Get a <see cref="Notifier"/>. Use that to optimize notification sending.
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="subscribersFilter"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public Notifier GetNotifier<T>(
            Object publisher,
            INotificationFilter subscribersFilter = null,
            Dictionary<string, Object> info = null
        );

        #endregion
    }
}
