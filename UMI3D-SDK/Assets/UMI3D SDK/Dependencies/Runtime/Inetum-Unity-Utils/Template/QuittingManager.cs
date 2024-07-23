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

using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace inetum.unityUtils
{
    /// <summary>
    /// A manager to handle the application quitting.<br/>
    /// <br/>
    /// How to:<br/>
    /// <list type="number">
    /// <item>Register to <see cref="QuittingManagerNotificationKey.RequestToQuit"/> to be notify when the quitting process has started. This is where to update ui.</item>
    /// <item>Register to <see cref="QuittingManagerNotificationKey.ApplicationIsQuitting"/> to be notify when the quitting process has been accepted.</item>
    /// <item>Call <see cref="Application.Quit()"/> to start the quitting process.</item>
    /// </list>
    /// </summary>
    public static class QuittingManager
    {
        static QuittingManager()
        {
            Application.wantsToQuit += WantsToQuit;

            NotificationHub.Default.Subscribe(
                typeof(QuittingManager).FullName,
                QuittingManagerNotificationKey.QuittingConfirmation,
                null,
                QuittingConfirmation
            );
        }

        /// <summary>
        /// Method called when <see cref="Application.Quit()"/> is called.
        /// </summary>
        /// <returns></returns>
        static bool WantsToQuit()
        {
            // If applicationIsQuitting then return true to confirm the quitting process.
            if (applicationIsQuitting)
            {
                return true;
            }

            // Notify the observers that the application is asked to quit.
            // The observers have to display a pop up that let the user
            // choose between confirming or cancelling the quitting process.
            NotificationHub.Default.Notify(
                typeof(QuittingManager).FullName,
                QuittingManagerNotificationKey.RequestToQuit
            );

            // Cancel quitting process for now.
            return false;
        }

        /// <summary>
        /// Method call when the user confirm or cancel the quitting process.
        /// </summary>
        /// <param name="notification"></param>
        static void QuittingConfirmation(Notification notification)
        {
            // Try to get the status of the confirmation.
            if (!notification.TryGetInfoT(QuittingManagerNotificationKey.QuittingConfirmationInfo.Confirmation, out bool confirmation))
            {
                return;
            }

            // Set the status of the confirmation.
            applicationIsQuitting = confirmation;

            // If the quitting process is cancel then return.
            if (!applicationIsQuitting)
            {
                return;
            }

            // Notify observers that the application is quitting.
            NotificationHub.Default.Notify(
                typeof(QuittingManager).FullName,
                QuittingManagerNotificationKey.ApplicationIsQuitting
            );

            UnityEngine.Debug.Log($"*********** Application is quitting ***********");
            // Quit.
            Application.Quit();
        }

        /// <summary>
        /// Whether or not the application has started to quit.
        /// </summary>
        public static bool applicationIsQuitting { get; private set; }
    }

    public static class QuittingManagerNotificationKey
    {
        /// <summary>
        /// Notification send when <see cref="Application.Quit()"/> is call.
        /// </summary>
        public const string RequestToQuit = "RequestToQuit";
        /// <summary>
        /// Notification send to confirm or cancel the quitting process.
        /// </summary>
        public const string QuittingConfirmation = "QuittingConfirmation";
        /// <summary>
        /// Notification send when the process of quitting has been confirmed and the application is quitting.
        /// </summary>
        public const string ApplicationIsQuitting = "ApplicationIsQuitting";

        public static class QuittingConfirmationInfo
        {
            /// <summary>
            /// The confirmation value.
            /// </summary>
            public const string Confirmation = "Confirmation";
        }
    }
}
