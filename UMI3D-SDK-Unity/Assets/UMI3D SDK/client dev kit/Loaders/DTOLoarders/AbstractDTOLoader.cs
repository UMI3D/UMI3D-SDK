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
using System;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for DTO loading.
    /// </summary>
    /// <typeparam name="DTO">Data To Object</typeparam>
    /// <typeparam name="A">Object type</typeparam>
    public abstract class AbstractDTOLoader<DTO, A>: MonoBehaviour where DTO : UMI3DDto
    {
        /// <summary>
        /// Load an object from dto and pass its value to a given callback.
        /// </summary>
        /// <param name="dto">Data to load from</param>
        /// <param name="onSuccess">Callback to execute after loading</param>
        public abstract void LoadDTO(DTO dto, Action<A> onSuccess, Action<string> onError);     
 

        /// <summary>
        /// Update an object from dto.
        /// </summary>
        /// <param name="entity">Object to update</param>
        /// <param name="olddto">Old dto describing the object</param>
        /// <param name="newdto">Dto to update the object to</param>
        public abstract void UpdateFromDTO(A entity, DTO olddto, DTO newdto);
    }
}