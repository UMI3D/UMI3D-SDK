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

namespace umi3d.edk
{
    /// <summary>
    /// Any object with UMI3DAsyncProperty should implement this interface to notified of updates.
    /// </summary>
    public interface IHasAsyncProperties
    {
        /// <summary>
        /// Notify that the property default value has changed.
        /// </summary>
        void NotifyUpdate();
        /// <summary>
        /// Notify that the property value has changed for a given user.
        /// </summary>
        /// <param name="u">the user</param>
        void NotifyUpdate(UMI3DUser u);
    }
}
