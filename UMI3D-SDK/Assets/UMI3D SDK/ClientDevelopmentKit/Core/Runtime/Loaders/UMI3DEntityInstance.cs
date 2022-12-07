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

using System;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Intanciated storage of a DTO for a non-gameobject UMI3D entity.
    /// </summary>
    public class UMI3DEntityInstance
    {
        /// <summary>
        /// Local copy of the received DTO.
        /// </summary>
        public UMI3DDto dto;

        /// <summary>
        /// Entity as an instanciated object.
        /// </summary>
        public object Object;

        /// <summary>
        /// Callback executed when the object is deleted.
        /// </summary>
        public Action Delete;

        /// <summary>
        /// Callback executed when the object is loaded.
        /// </summary>
        private Action LoadedCallback;

        public UMI3DEntityInstance(Action loadedCallback)
        {
            if (loadedCallback == null)
                throw new Umi3dException("No instance should be created without loadedCallback");

            this.LoadedCallback = loadedCallback;
        }

        /// <summary>
        /// Is the UMI3D entity loaded?
        /// </summary>
        public bool IsLoaded => LoadedCallback == null;

        /// <summary>
        /// Notify that the entity has been loaded.
        /// </summary>
        public void NotifyLoaded()
        {
            if (!IsLoaded)
            {
                LoadedCallback();
                LoadedCallback = null;
            }
        }
    }
}