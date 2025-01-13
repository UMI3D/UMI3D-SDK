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
using System.Diagnostics;
using System.Linq;

namespace inetum.unityUtils.observation
{
    public class NotificationHub : INotificationHub
    {
        /// <summary>
        /// The default instance of <see cref="NotificationHub"/>.
        /// </summary>
        public static NotificationHub Default => _default.Value;
        /// <summary>
        /// A thread safe lazy initialisation of a NotificationHub.
        /// </summary>
        static readonly Lazy<NotificationHub> _default = new (() => new());

        static readonly object _lockObject = new object();

        /// <summary>
        /// ID to subscriptions.
        /// </summary>
        Dictionary<string, List<Subscription>> _subscriptions = new();
        /// <summary>
        /// An empty subscribers array to return when there are no subscriptions for the given ID.
        /// </summary>
        static readonly object[] emptySubscribers = new object[0];
        /// <summary>
        /// Retrieves the subscribers for a given ID.<br/>
        /// If the ID is null or empty, logs an error and returns an empty array of subscribers.<br/>
        /// If there are no subscriptions for the given ID, returns an empty array of subscribers.<br/>
        /// Otherwise, returns the list of subscribers associated with the given ID.<br/>
        /// <br/>
        /// <example>
        /// Given an ID with subscriptions when getting subscribers for the ID then return the list of subscribers.<br/>
        /// <code>
        /// NotificationHub.Default.Subscribe(subscriber1, id, (Callback)(() => { }));
        /// NotificationHub.Default.Subscribe(subscriber2, id, (Callback)(() => { }));
        /// IEnumerable&lt;object&gt; subscribers = NotificationHub.Default.GetSubscribersFor(id); 
        /// // subscribers contains subscriber1 and subscriber2.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="id">The ID for which to retrieve subscribers.</param>
        /// <returns>An IEnumerable of subscribers for the given ID.</returns>
        public IEnumerable<object> GetSubscribersFor(ID id)
        {
            if (string.IsNullOrEmpty(id))
            {
                UnityEngine.Debug.LogError($"[NotificationHub.GetSubscribersFor] Error: id is null or empty.");
                return emptySubscribers;
            }

            if (!_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
            {
                return emptySubscribers;
            }

            return subscriptions.Select(subscription => subscription.subscriber);
        }

        /// <summary>
        /// Subscriber to IDs.
        /// </summary>
        Dictionary<object, HashSet<string>> _subscriberToID = new();
        /// <summary>
        /// An empty ids array to return when there are no IDs associated with the given subscriber 
        /// </summary>
        static readonly string[] emptyIds = new string[0];
        /// <summary>
        /// Retrieves the IDs associated with a given subscriber.<br/>
        /// If the subscriber is null, logs an error and returns an empty list of IDs.<br/>
        /// If there are no IDs associated with the given subscriber, returns an empty list of IDs.<br/>
        /// Otherwise, returns the list of IDs associated with the given subscriber.<br/>
        /// <br/>
        /// <example>
        /// Given a subscriber with subscriptions when getting IDs for the subscriber then return the list of IDs.<br/>
        /// <code>
        /// NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() => { }));
        /// NotificationHub.Default.Subscribe(subscriber, id2, (Callback)(() => { }));
        /// IEnumerable&lt;string&gt; ids = NotificationHub.Default.GetIdsFor(subscriber);
        /// // ids contains id1 and id2.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="subscriber">The subscriber for which to retrieve IDs.</param>
        /// <returns>An IEnumerable of IDs associated with the given subscriber.</returns>
        public IEnumerable<string> GetIdsFor(object subscriber)
        {
            if (subscriber == null)
            {
                UnityEngine.Debug.LogError($"[NotificationHub.GetIdsFor] Error: subscriber is null.");
                return emptyIds;
            }

            if (!_subscriberToID.TryGetValue(subscriber, out HashSet<string> ids))
            {
                return emptyIds;
            }

            return ids;
        }

        /// <summary>
        /// Subscribes a subscriber to notifications identified by a specific ID.<br/>
        /// If the subscriber or ID is null or empty, an error is logged.<br/>
        /// If a subscription already exists for the subscriber and ID, it is replaced.<br/>
        /// Otherwise, a new subscription is created.<br/>
        /// <br/>
        /// <example>
        /// Given a subscriber and an ID, when subscribing, then a subscription is added.<br/>
        /// <code>
        /// NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }));
        /// NotificationHub.Default.Subscribe(subscriber, id, (Callback)((Notification notification) => { }));
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="subscriber">The object that wants to subscribe to notifications.</param>
        /// <param name="id">The identifier for the notifications.</param>
        /// <param name="action">The callback action to be invoked when a notification is received.</param>
        /// <param name="publishersFilter">Optional filter to specify which publishers' notifications to receive.</param>
        public void Subscribe(
            object subscriber,
            ID id,
            Callback action,
            INotificationFilter publishersFilter = null
        )
        {
            if (string.IsNullOrEmpty(id))
            {
                UnityEngine.Debug.LogError($"[NotificationHub.Subscribe] Error: id is null or empty.");
                return;
            }

            if (subscriber == null)
            {
                UnityEngine.Debug.LogError($"[NotificationHub.Subscribe] Error: subscriber is null for id '{id}'.");
                return;
            }

            // Create a subscription entry.
            Subscription subscription = new()
            {
                id = id,
                subscriber = subscriber,
                action = action,
                publishersFilter = publishersFilter
            };

            lock (_lockObject)
            {
                _Subscribe(subscription);
            }
        }

        void _Subscribe(Subscription subscription)
        {
            // Check if subscriptions already exist for that 'id'.
            if (_subscriptions.TryGetValue(subscription.id, out List<Subscription> subscriptions))
            {
                // If subscriptions already exist then check if a subscription already exist for that 'subscriber'.
                int index = subscriptions.FindIndex(sub => sub.subscriber == subscription.subscriber);
                if (index >= 0)
                {
                    // If a subscription already exist for that 'subscriber' then replace the previous one.
                    subscriptions[index] = subscription;
                } else
                {
                    // Else add this subscription.
                    subscriptions.Add(subscription);
                }
            }
            else
            {
                // If no subscriptions exist for that 'id' create a new association 'id' -> subscriptions.
                _subscriptions.Add(subscription.id, new List<Subscription>() { subscription });
            }

            // Check if this 'subscriber' already listen to notifications.
            if (_subscriberToID.TryGetValue(subscription.subscriber, out HashSet<string> ids))
            {
                // Add the 'id' to the list of listen ids, if the list didn't contain this 'id' already.
                // This list is a set, that means there is no duplicate ids.
                ids.Add(subscription.id);
            }
            else
            {
                // If that 'subscriber' listen to no one, create a new association 'subscriber' -> ids.
                _subscriberToID.Add(subscription.subscriber, new HashSet<string>() { subscription.id });
            }
        }

        /// <summary>
        /// Unsubscribes a subscriber from notifications identified by a specific ID or from all notifications if no ID is provided.<br/>
        /// If the subscriber is null, an error is logged.<br/>
        /// If the subscriber has no subscriptions, a warning is logged.<br/>
        /// <br/>
        /// <example>
        /// Given a subscriber and an ID, when unsubscribing, then the subscription is removed.<br/>
        /// <code>
        /// NotificationHub.Default.Unsubscribe(subscriber, id);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="subscriber">The object that wants to unsubscribe from notifications.</param>
        /// <param name="id">The identifier for the notifications. If null, unsubscribes from all notifications.</param>
        public void Unsubscribe(object subscriber, ID? id = null)
        {
            if (subscriber == null)
            {
                UnityEngine.Debug.LogError($"[NotificationHub.Unsubscribe] Error: subscriber is null.");
                return;
            }

            lock (_lockObject)
            {
                _Unsubscribe(subscriber, id);
            }
        }

        void _Unsubscribe(object subscriber, ID? id = null)
        {
            // Check if that 'subscriber' listen to any notifications.
            if (!_subscriberToID.TryGetValue(subscriber, out HashSet<string> ids))
            {
                string subscriberName = subscriber is string
                ? subscriber as string
                : subscriber.GetType().FullName;

                UnityEngine.Debug.LogWarning($"[NotificationHub.Unsubscribe] Warning: no subscription for '{subscriberName}'.");
                return;
            }

            if (!id.HasValue || id.Value == null)
            {
                // Remove all the subscription for that 'subscriber'.
                // Loop through all the ids that this 'subscriber' is listening to.
                foreach (string _id in ids)
                {
                    RemoveIdForSubscriber(_id, subscriber);
                }

                // Clear the ids.
                ids.Clear();

                _subscriberToID.Remove(subscriber);
            } else
            {
                if (!ids.Contains(id))
                {
                    string subscriberName = subscriber is string
                    ? subscriber as string
                    : subscriber.GetType().FullName;

                    UnityEngine.Debug.LogWarning($"[NotificationHub.Unsubscribe] Warning: no subscription for '{subscriberName}' with id '{id.Value}'");
                    return;
                }
                RemoveIdForSubscriber(id, subscriber);

                // Remove this 'id' from the list of listen ids.
                ids.Remove(id);

                // If there is not more ids then remove 'subscriber' from '_subscriberToID'.
                if (ids.Count == 0)
                {
                    _subscriberToID.Remove(subscriber);
                }
            }
        }

        bool RemoveIdForSubscriber(string id, object subscriber)
        {
            // Check if subscriptions exist for 'id'.
            if (!_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
            {
                string subscriberName = subscriber is string
                   ? subscriber as string
                   : subscriber.GetType().FullName;

                UnityEngine.Debug.LogError($"[NotificationHub] Error: no id '{id}' for subscriber '{subscriberName}'.");
                return false;
            }

            // Remove all the subscriptions concerning 'subscriber'.
            subscriptions.RemoveAll(sub => sub.subscriber == subscriber);

            // Remove id from the subscriptions if there is no more subscribers.
            if (subscriptions.Count == 0)
            {
                _subscriptions.Remove(id);
            }

            return true;
        }

        /// <summary>
        /// This method notifies subscribers about an event identified by an ID.<br/>
        /// It logs warnings and errors if the publisher or ID is null, or if there are no subscribers for the given ID.<br/>
        /// It also ensures thread safety while notifying subscribers.<br/>
        /// <br/>
        /// <example>
        /// Given a publisher and an ID, when notifying, then subscribers are notified if they exist.<br/>
        /// <code>
        /// void NotificationAction(Notification notification)
        /// {
        ///     if (!notification.TryGetInfoT("key", out string value))
        ///     {
        ///         return;
        ///     }
        ///     UnityEngine.Debug.Log($"Notification received with info. Key is 'key' and value is '{value}'.");
        /// }
        /// NotificationHub.Default.Subscribe(subscriber, id, (Callback)(NotificationAction));
        /// 
        /// Dictionary&lt;string, object> info = new Dictionary&lt;string, object> { { "key", "value" } };
        /// int notifiedCount = NotificationHub.Default.Notify(publisher, id, info);
        /// // print: Notification received with info. Key is 'key' and value is 'value'.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="publisher">The object that is publishing the notification.</param>
        /// <param name="id">The identifier for the notification event.</param>
        /// <param name="info">Optional additional information for the notification.</param>
        /// <param name="subscribersFilter">Optional filter to apply to subscribers.</param>
        /// <returns>The number of subscribers that were notified.</returns>
        public int Notify(
            object publisher,
            ID id,
            Dictionary<string, object> info = null,
            INotificationFilter subscribersFilter = null
        )
        {
            if (publisher == null)
            {
                UnityEngine.Debug.LogWarning($"[NotificationHub.Notify] Warning: publisher is null. A null publisher makes debugging difficult.");
            }

            if (string.IsNullOrEmpty(id))
            {
                string publisherName = publisher is string
                ? publisher as string
                : (publisher?.GetType().FullName ?? "Unknown");

                UnityEngine.Debug.LogError($"[NotificationHub.Notify] Error: id is null or empty when publisher '{publisherName}' try to notify.");
                return 0;
            }

            // Check if there are subscription for that 'id'.
            if (!_subscriptions.TryGetValue(id, out List<Subscription> subscriptions))
            {
                string publisherName = publisher is string
                ? publisher as string
                : (publisher?.GetType().FullName ?? "Unknown");

                UnityEngine.Debug.LogWarning($"[NotificationHub.Notify] Warning: '{publisherName}' try to notify with id '{id}' but no one is listening.");
                return 0;
            }

            List<Subscription> subscriptionsCopy;
            // To be thread safe.
            lock (_lockObject)
            {
                subscriptionsCopy = new List<Subscription>(subscriptions);
            }

            // Create the notification.
            Notification notification = new Notification(id, publisher, info);
            int observers = 0;
            for (int i = 0; i < subscriptionsCopy.Count; i++)
            {
                Subscription subscription = subscriptionsCopy[i];
                // filter the notification by subscribers and publishers.
                if (IsNotificationAccepted(subscribersFilter, subscription, publisher))
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

        bool IsNotificationAccepted(
            INotificationFilter subscribersFilter, 
            Subscription subscription, 
            object publisher
        )
        {
            bool canSendToSubscriber = subscribersFilter?.IsAccepted(subscription.subscriber) ?? true;
            bool canSubscriberReceiveFromSubscriber = subscription.publishersFilter?.IsAccepted(publisher) ?? true;

            return canSendToSubscriber && canSubscriberReceiveFromSubscriber;
        }

        public Notifier GetNotifier(
            object publisher,
            ID id,
            Dictionary<string, Object> info = null,
            INotificationFilter subscribersFilter = null
        )
        {
            return new Notifier(
                publisher,
                id,
                subscribersFilter,
                info,
                this
            );
        }

        /// <summary>
        /// Class representing a subscription to a notification.
        /// </summary>
        class Subscription
        {
            /// <summary>
            /// Id of the notification.
            /// </summary>
            public string id;

            /// <summary>
            /// The object that wait for a notification. If subscriber is static then user typeof().FullName.
            /// </summary>
            public object subscriber;

            /// <summary>
            /// Action to execute when the notification is received.
            /// </summary>
            public Action<Notification> action;

            /// <summary>
            /// Only the notifications that pass this filter test can be sent to this <see cref="subscriber"/>.<br/>
            /// <br/>
            /// If null the <see cref="Subscriber"/> listen to everyone.
            /// </summary>
            public INotificationFilter publishersFilter;
        }

        [Conditional("UNITY_EDITOR")]
        public void Clear()
        {
            _subscriberToID.Clear();
            _subscriptions.Clear();
        }
    }
}