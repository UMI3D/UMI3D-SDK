/*
Copyright 2019 - 2025 Inetum

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

using inetum.unityUtils.observation;
using System;
using System.Diagnostics;
using UnityEngine;

namespace inetum.unityUtils.lifeCycle
{
    public class Quitting
    {
        public enum QuittingState
        {
            NotQuitting,
            WaitsForConfirmation,
            IsQuitting
        }
        public enum SubscriptionType
        {
            /// <summary>
            /// Be notified when a confirmation is necessary to quit.
            /// </summary>
            Confirmation,
            /// <summary>
            /// Be notified when the application is quitting.
            /// </summary>
            IsQuitting
        }

        public static Quitting instance => _instance.Value;
        static readonly Lazy<Quitting> _instance = new(() => new());
        Quitting()
        {
            Application.wantsToQuit += WantsToQuit;

            askForConfirmationNotifier = NotificationHub.Default.GetNotifier(
                this,
                ID.FromType<QuittingNotificationKeys.AskForConfirmation>()
            );

            applicationIsQuittingNotifier = NotificationHub.Default.GetNotifier(
               this,
               ID.FromType<QuittingNotificationKeys.ApplicationIsQuitting>()
           );
        }

        Notifier askForConfirmationNotifier;
        Notifier applicationIsQuittingNotifier;

        readonly object _lockObject = new object();
        QuittingState _state = QuittingState.NotQuitting;
        public QuittingState state
        {
            get
            {
                lock (_lockObject)
                {
                    return _state;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _state = value;
                }
            }
        }

        public bool isWaitingForConfirmation => state == QuittingState.WaitsForConfirmation;
        public bool isQuitting => state == QuittingState.IsQuitting;

        /// <summary>
        /// Initiates the quitting process. If confirmation is required, it will ask for confirmation before quitting.<br/>
        /// Otherwise, it will proceed to quit immediately.<br/>
        /// <br/>
        /// <example>
        /// Given a scenario where the application needs to quit:<br/>
        /// <code>
        /// Quitting.instance.Quit(this, true); // Asks for confirmation before quitting
        /// Quitting.instance.Quit(this, false); // Quits immediately without asking for confirmation
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="publisher">The object initiating the quit process.</param>
        /// <param name="askForConfirmation">If true, asks for confirmation before quitting; otherwise, quits immediately.</param>
        public void Quit(object publisher, bool askForConfirmation = true)
        {
            if (publisher == null)
            {
                UnityEngine.Debug.LogWarning($"[Quitting.Quit] Warning: publisher is null.");
            }

            if (askForConfirmation)
            {
                _AskToQuit(publisher);
            }
            else
            {
                _Quit(publisher);
            }
        }

        /// <summary>
        /// Confirms the quitting process. If the quit parameter is true, it proceeds to quit.<br/>
        /// Otherwise, it aborts the quitting process and sets the state to NotQuitting.<br/>
        /// <br/>
        /// <example>
        /// Given a scenario where the application needs to confirm or abort quitting:<br/>
        /// <code>
        /// Quitting.instance.Confirm(this, true); // Confirms and proceeds to quit
        /// Quitting.instance.Confirm(this, false); // Aborts the quitting process
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="publisher">The object confirming the quit process.</param>
        /// <param name="quit">If true, confirms and proceeds to quit; otherwise, aborts the quitting process.</param>
        public void Confirm(object publisher, bool quit = true)
        {
            if (publisher == null)
            {
                UnityEngine.Debug.LogWarning($"[Quitting.Quit] Warning: publisher is null.");
            }

            if (quit)
            {
                _Quit(publisher);
            }
            else
            {
                state = QuittingState.NotQuitting;
            }
        }

        /// <summary>
        /// Subscribes a given subscriber to a specific type of notification.<br/>
        /// Depending on the subscription type, it assigns an appropriate ID and subscribes the subscriber to the NotificationHub.<br/>
        /// <br/>
        /// <example>
        /// Given a valid subscriber and a subscription type, the subscriber will be added to the NotificationHub.<br/>
        /// <code>
        /// Quitting.instance.SubscribeFor(Quitting.SubscriptionType.Confirmation, this, (Callback)(() => { }));
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="type">The type of subscription to be made.</param>
        /// <param name="subscriber">The object that wants to subscribe to the notification.</param>
        /// <param name="action">The callback action to be executed when the notification is triggered.</param>
        public void SubscribeFor(SubscriptionType type, object subscriber, Callback action)
        {
            ID id = null;
            switch (type)
            {
                case SubscriptionType.Confirmation:
                    id = ID.FromType<QuittingNotificationKeys.AskForConfirmation>();
                    break;
                case SubscriptionType.IsQuitting:
                    id = ID.FromType<QuittingNotificationKeys.ApplicationIsQuitting>();
                    break;
                default:
                    UnityEngine.Debug.LogError($"[Quitting.SubscribeFor] Error: Unhandled case.");
                    break;
            }

            NotificationHub.Default.Subscribe(
                subscriber,
                id,
                action
            );
        }

        /// <summary>
        /// Unsubscribes a given subscriber from a specific type of notification.<br/>
        /// Depending on the subscription type, it assigns an appropriate ID and unsubscribes the subscriber from the NotificationHub.<br/>
        /// <br/>
        /// <example>
        /// Given a valid subscriber and a subscription type, the subscriber will be removed from the NotificationHub.<br/>
        /// <code>
        /// Quitting.instance.UnsubscribeFor(Quitting.SubscriptionType.Confirmation, this);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="type">The type of subscription to be removed.</param>
        /// <param name="subscriber">The object that wants to unsubscribe from the notification.</param>
        public void UnsubscribeFor(SubscriptionType type, object subscriber)
        {
            ID id = null;
            switch (type)
            {
                case SubscriptionType.Confirmation:
                    id = ID.FromType<QuittingNotificationKeys.AskForConfirmation>();
                    break;
                case SubscriptionType.IsQuitting:
                    id = ID.FromType<QuittingNotificationKeys.ApplicationIsQuitting>();
                    break;
                default:
                    UnityEngine.Debug.LogError($"[Quitting.UnsubscribeFor] Error: Unhandled case.");
                    break;
            }

            NotificationHub.Default.Unsubscribe(
                subscriber,
                id
            );
        }

        public static implicit operator bool(Quitting quitting)
        {
            return quitting.isQuitting;
        }

        bool WantsToQuit()
        {
            switch (state)
            {
                case QuittingState.NotQuitting:
                    // Happen when user try to quit by pressing the close application top bar button.
                    Quit(this, true);
                    return false;

                case QuittingState.WaitsForConfirmation:
                    return false;

                case QuittingState.IsQuitting:
                    return true;

                default:
                    UnityEngine.Debug.LogError($"[Quitting.WantsToQuit] Error: Unhandled case.");
                    return true;
            }
        }

        void _AskToQuit(object publisher)
        {
            state = QuittingState.WaitsForConfirmation;

            askForConfirmationNotifier[QuittingNotificationKeys.AskForConfirmation.Publisher] = publisher;
            askForConfirmationNotifier.Notify();

            Application.Quit();
        }

        void _Quit(object publisher)
        {
            state = QuittingState.IsQuitting;

            applicationIsQuittingNotifier[QuittingNotificationKeys.ApplicationIsQuitting.Publisher] = publisher;
            applicationIsQuittingNotifier.Notify();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void Reset()
        {
            _state = QuittingState.NotQuitting;
        }
    }
}