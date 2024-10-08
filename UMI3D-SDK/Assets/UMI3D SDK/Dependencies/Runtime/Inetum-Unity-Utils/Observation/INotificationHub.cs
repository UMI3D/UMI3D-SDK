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

        #region Supply

        /// <summary>
        /// Create an association (<see cref="inetum.unityUtils.Request"/>, <paramref name="id"/>, <paramref name="supplier"/>.<br/>
        /// <br/>
        /// If no such association exist then create a <see cref="inetum.unityUtils.Request"/>.<br/>
        /// Else if this association with the same <paramref name="id"/> and <paramref name="supplier"/> exist change nothing.<br/>
        /// Else bind this <see cref="inetum.unityUtils.Request"/> with the new <paramref name="supplier"/> and clear the <see cref="inetum.unityUtils.Request"/>'s information.<br/>
        /// In each case return the <see cref="inetum.unityUtils.Request"/> associated.
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Request Supply(
            Object supplier,
            string id
        );

        /// <summary>
        /// Create an association (<see cref="inetum.unityUtils.Request"/>, id, <paramref name="supplier"/> where id is typeof(T).FullName<br/>
        /// <br/>
        /// If no such association exist then create a <see cref="inetum.unityUtils.Request"/>.<br/>
        /// Else if this association with the same id and <paramref name="supplier"/> exist change nothing.<br/>
        /// Else bind this <see cref="inetum.unityUtils.Request"/> with the new <paramref name="supplier"/> and clear the <see cref="inetum.unityUtils.Request"/>'s information.<br/>
        /// In each case return the <see cref="inetum.unityUtils.Request"/> associated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public Request Supply<T>(Object supplier);

        #endregion

        #region Withhold 

        /// <summary>
        /// Remove all the associations (<see cref="inetum.unityUtils.Request"/>, id, supplier) where this <paramref name="supplier"/> is present.
        /// </summary>
        /// <param name="supplier"></param>
        public void Withhold(Object supplier);

        /// <summary>
        /// Remove the association (<see cref="inetum.unityUtils.Request"/>, <paramref name="id"/>, <paramref name="supplier"/>).
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="id"></param>
        public void Withhold(
           Object supplier,
           string id
        );

        /// <summary>
        /// Remove the association (<see cref="inetum.unityUtils.Request"/>, id, <paramref name="supplier"/>) where id is typeof(T).FullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="supplier"></param>
        public void Withhold<T>(Object supplier);

        #endregion

        #region Request

        /// <summary>
        /// Try to request.<br/>
        /// <br/>
        /// Return true if an association (<see cref="inetum.unityUtils.Request"/>, <paramref name="id"/>) exist, else return false.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryRequest(
            Object client,
            string id,
            out Request request,
            UnityEngine.LogType? logType = UnityEngine.LogType.Error
        );

        /// <summary>
        /// Try to request.<br/>
        /// <br/>
        /// Return true if an association (<see cref="inetum.unityUtils.Request"/>, id) exist, else return false.<br/>
        /// id is typeof(<typeparamref name="T"/>).fullName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool TryRequest<T>(
            Object client,
            out Request request,
            UnityEngine.LogType? logType = UnityEngine.LogType.Error
        );

        #endregion
    }
}
