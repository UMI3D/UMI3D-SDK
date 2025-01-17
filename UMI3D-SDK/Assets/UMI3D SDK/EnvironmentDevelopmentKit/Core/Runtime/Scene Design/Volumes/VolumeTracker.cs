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

using System;
using System.Collections.Generic;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Utility class to track user entries and exits in a volume. Resistant to user leave.
    /// </summary>
    public class VolumeTracker : IDisposable
    {
        private readonly IVolume volume;
        private readonly IUMI3DServer umi3dServer;
        private readonly HashSet<UMI3DUser> usersInVolume = new();

        /// <summary>
        /// Gets the volume being tracked.
        /// </summary>
        public IVolume Volume => volume;

        /// <summary>
        /// Gets the set of users currently in the volume.
        /// </summary>
        /// <returns>A read-only collection of users currently in the volume.</returns>
        public IReadOnlyCollection<UMI3DUser> UsersInVolume => usersInVolume;

        /// <summary>
        /// Event triggered when a user enters the volume.
        /// </summary>
        public event Action<UMI3DUser> UserEntered;

        /// <summary>
        /// Event triggered when a user exits the volume.
        /// </summary>
        public event Action<UMI3DUser> UserExited;

        /// <summary>
        /// Initializes the VolumeTracker with a specific volume.
        /// </summary>
        /// <param name="volume">The volume to be tracked.</param>
        /// <param name="umi3dServer">Server instance to listen to user leave.</param>
        public VolumeTracker(IVolume volume, IUMI3DServer umi3dServer)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            if (umi3dServer == null) throw new ArgumentNullException(nameof(umi3dServer));
            this.volume = volume;
            this.umi3dServer = umi3dServer;
            SubscribeToVolumeEvents();
        }

        /// <summary>
        /// Subscribes to the volume's user enter and exit events.
        /// </summary>
        /// <param name="umi3dServer">Server instance to listen to user leave.</param>
        private void SubscribeToVolumeEvents()
        {
            volume.GetUserEnter().AddListener(OnUserEnter);
            volume.GetUserExit().AddListener(OnUserExit);
            umi3dServer.OnUserLeave.AddListener(OnUserExit);
        }

        /// <summary>
        /// Unsubscribes from the volume's user enter and exit events.
        /// </summary>
        private void UnsubscribeFromVolumeEvents()
        {
            if (volume == null) return; // could have been destroyed

            volume.GetUserEnter().RemoveListener(OnUserEnter);
            volume.GetUserExit().RemoveListener(OnUserExit);

            if (umi3dServer != null) // could have been destroyed
                umi3dServer.OnUserLeave.RemoveListener(OnUserExit);
        }

        /// <summary>
        /// Called when a user enters the volume.
        /// </summary>
        /// <param name="user">The user who entered the volume.</param>
        private void OnUserEnter(UMI3DUser user)
        {
            if (user == null || usersInVolume.Contains(user)) return;
            usersInVolume.Add(user);
            UserEntered?.Invoke(user);
        }

        /// <summary>
        /// Called when a user exits the volume.
        /// </summary>
        /// <param name="user">The user who exited the volume.</param>
        private void OnUserExit(UMI3DUser user)
        {
            if (user == null || !usersInVolume.Contains(user)) return;
            usersInVolume.Remove(user);
            UserExited?.Invoke(user);
        }

        /// <summary>
        /// Cleans up the event subscriptions.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromVolumeEvents();
        }
    }

    /// <summary>
    /// Generic version of VolumeTracker for a specific type of volume.
    /// </summary>
    /// <typeparam name="T">The type of volume to be tracked.</typeparam>
    public class VolumeTracker<T> : VolumeTracker where T : IVolume
    {
        private readonly T volumeTyped;

        /// <summary>
        /// Gets the specific typed volume being tracked.
        /// </summary>
        public T VolumeTyped => volumeTyped;

        /// <summary>
        /// Initializes the VolumeTracker with a specific typed volume.
        /// </summary>
        /// <param name="volume">The specific typed volume to be tracked.</param>
        /// <param name="umi3dServer">Server instance to listen to user leave.</param>
        public VolumeTracker(T volume, IUMI3DServer umi3dServer) : base(volume, umi3dServer)
        {
            volumeTyped = volume;
        }
    }
}