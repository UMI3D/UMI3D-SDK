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
using System.Linq;

namespace inetum.unityUtils
{
    public class NotificationHub : INotificationHub
    {
        /// <summary>
        /// The default instance of <see cref="NotificationHub"/>.
        /// </summary>
        public static NotificationHub Default
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
        static NotificationHub _default;

        /// <summary>
        /// ID to subscriptions.
        /// </summary>
        Dictionary<string, List<Subscription>> _subscriptions = new();
        /// <summary>
        /// Subscriber to IDs.
        /// </summary>
        Dictionary<Object, HashSet<string>> _subscriberToID = new();

 
        public void Subscribe(
            Object subscriber, 
            string id, 
            INotificationFilter publishersFilter, 
            Action<Notification> action
        )
        {
            // Create a subscription entry.
            Subscription subscription = new()
            {
                subscriber = subscriber,
                publishersFilter = publishersFilter,
                action = action
            };

            // Check if subscriptions already exist for that 'id'.
            if (_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
            {
                // Add the subscription to the list of subscriptions for that 'id'.
                subscriptions.Add(subscription);
            }
            else
            {
                // If no subscriptions exist for that 'id' create a new association 'id' -> subscriptions.
                _subscriptions.Add(id, new List<Subscription>() { subscription });
            }

            // Check if this 'subscriber' already listen to notifications.
            if (_subscriberToID.TryGetValue(subscriber, out HashSet<string> ids))
            {
                // Add the 'id' to the list of listen ids, if the list didn't contain this 'id' already.
                // This list is a set, that means there is no duplicate ids.
                ids.Add(id);
            }
            else
            {
                // If that 'subscriber' listen to no one, create a new association 'subscriber' -> ids.
                _subscriberToID.Add(subscriber, new HashSet<string>() { id });
            }
        }

        public void Subscribe(
            Object subscriber, 
            string id,
            INotificationFilter publishersFilter, 
            Action action
        )
        {
            Subscribe(subscriber, id, publishersFilter, notification => action());
        }

        public void Subscribe(
            Object subscriber,
            string id,
            Action<Notification> action
        )
        {
            Subscribe(subscriber, id, null, action);
        }

        public void Subscribe(
            Object subscriber,
            string id,
            Action action
        )
        {
            Subscribe(subscriber, id, null, action);
        }

        public void Unsubscribe(Object subscriber)
        {
            string subscriberName = subscriber is string
                ? subscriber as string
                : subscriber.GetType().FullName;

            // Check if that 'subscriber' listen to any notification.
            if (!_subscriberToID.TryGetValue(subscriber, out HashSet<string> ids))
            {
                UnityEngine.Debug.LogWarning($"[NotificationHub] Try to unsubscribe {subscriberName} but subscriber has not subscribed yet.");
                // If subscriber is not listening to notification then return;
                return;
            }

            // Loop through all the ids that this 'subscriber' is listening to.
            foreach (string id in ids)
            {
                // Check if subscriptions exist for 'id'.
                if (!_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
                {
                    UnityEngine.Debug.LogError($"[{nameof(Unsubscribe)}] Try remove subscriptions for {subscriberName}. No subscription for {id}, that should not happen.");
                    continue;
                }

                // Remove all the subscriptions concerning 'subscriber'.
                subscriptions.RemoveAll(sub => sub.subscriber == subscriber);

                // Remove id from the subscriptions if there is no more subscribers.
                if (subscriptions.Count == 0)
                {
                    _subscriptions.Remove(id);
                }
            }

            // Clear the ids.
            ids.Clear();

            // Remove 'subscriber' from '_subscriberToID'.
            _subscriberToID.Remove(subscriber);
        }

        public void Unsubscribe(Object subscriber, string id)
        {
            string subscriberName = subscriber is string
                ? subscriber as string
                : subscriber.GetType().FullName;

            // Check if that 'subscriber' listen to any notification.
            if (!_subscriberToID.TryGetValue(subscriber, out HashSet<string> ids))
            {
                UnityEngine.Debug.LogWarning($"[NotificationHub] Try to unsubscribe {subscriberName} but subscriber has not subscribed yet.");
                // If subscriber is not listening to notifications then return;
                return;
            }

            // Check if 'subscriber' listen to 'id'.
            if (!ids.Contains(id))
            {
                UnityEngine.Debug.LogWarning($"[NotificationHub] Try to unsubscribe {subscriberName} with id {id} but subscriber has not subscribed to this id yet.");
                // If subscriber is not listening to 'id' then return;
                return;
            }

            // Remove this 'id' from the list of listen ids.
            ids.Remove(id);

            // If there is not more ids then remove 'subscriber' from '_subscriberToID'.
            if (ids.Count == 0)
            {
                _subscriberToID.Remove(subscriber);
            }

            // Check if subscriptions exist for 'id'.
            if (!_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
            {
                UnityEngine.Debug.LogError($"[{nameof(Unsubscribe)}] Try remove subscriptions for {subscriberName}. No subscription for {id}, that should not happen.");
                return;
            }

            // Remove all the subscriptions concerning 'subscriber'.
            subscriptions.RemoveAll(sub => sub.subscriber == subscriber);

            // Remove id from the subscriptions if there is no more subscribers.
            if (subscriptions.Count == 0)
            {
                _subscriptions.Remove(id);
            }
        }

        public int Notify(
            Object publisher,
            string id,
            INotificationFilter subscribersFilter,
            Dictionary<string, Object> info = null
        )
        {
            int observers = 0;

            // Check if there are subscription for that 'id'.
            if (!_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
            {
                string publisherName = publisher is string
                ? publisher as string
                : publisher.GetType().FullName;

                UnityEngine.Debug.LogWarning($"[NotificationHub] {publisherName} try to notify with id {id} but no one is listening.");
                return observers;
            }

            // Create the notification.
            Notification notification = new Notification(id, publisher, info);

            foreach (Subscription subscription in subscriptions)
            {
                // filter the notification by subscribers and publishers.
                if ((subscribersFilter == null || subscribersFilter.IsAccepted(subscription.subscriber))
                    && (subscription.publishersFilter == null || subscription.publishersFilter.IsAccepted(publisher)))
                {
                    try
                    {
                        subscription.action(notification);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[NotificationHub] Subscription action return an exception:\n " +
                            $"id: {id}, publisher: {publisher}, subscriber: {subscription.subscriber}");
                        UnityEngine.Debug.LogException(e);
                        continue;
                    }
                    observers++;
                }
            }

            return observers;
        }

        public int Notify(
            Object publisher, 
            string id, 
            Dictionary<string, Object> info = null
        )
        {
            return Notify(publisher, id, null, info);
        }

        /// <summary>
        /// Class representing a subscription to a notification.
        /// </summary>
        private class Subscription
        {
            /// <summary>
            /// The object that wait for a notification. If subscriber is static then user typeof().FullName.
            /// </summary>
            public Object subscriber;

            /// <summary>
            /// Only the notifications that pass this filter test can be sent to this <see cref="subscriber"/>.<br/>
            /// <br/>
            /// If null the <see cref="Subscriber"/> listen to everyone.
            /// </summary>
            public INotificationFilter publishersFilter;

            /// <summary>
            /// Action to execute when the notification is received.
            /// </summary>
            public Action<Notification> action;
        }
    }
}