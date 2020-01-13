/*
Copyright 2019 Gfi Informatique

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
using UnityEngine.Events;

namespace umi3d
{
    /// <summary>
    /// Interface for Togglable objects.
    /// </summary>
    public interface ITogglable
    {
        /// <summary>
        /// Raise selection event.
        /// </summary>
        void Select();

        /// <summary>
        /// Raise deselection event.
        /// </summary>
        void Deselect();

        /// <summary>
        /// Subscribe a callback from the selection or deselection event.
        /// </summary>
        /// <param name="callback">Callback to subscribe</param>
        /// <see cref="UnSubscribe(UnityAction{bool})"/>
        void Subscribe(UnityAction<bool> callback);

        /// <summary>
        /// Unsubscribe a callback from the selection or deselection event.
        /// </summary>
        /// <param name="callback">Callback to unsubscribe</param>
        /// <see cref="Subscribe(UnityAction{bool})"/>
        void UnSubscribe(UnityAction<bool> callback);
    }
}
