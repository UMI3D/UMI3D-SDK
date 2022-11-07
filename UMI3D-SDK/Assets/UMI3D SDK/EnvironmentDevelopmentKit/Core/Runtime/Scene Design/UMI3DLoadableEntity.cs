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

using System.Collections.Generic;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Interface for UMI3D entities that could be loaded on clients.
    /// </summary>
    public interface UMI3DLoadableEntity : UMI3DMediaEntity
    {
        /// <summary>
        /// Ask the clients to load the entity.
        /// </summary>
        /// <param name="users">Target clients user set.</param>
        /// <returns>The operation to send to makes clients load the entity</returns>
        LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null);

        /// <summary>
        /// Ask the clients to delete the entity.
        /// </summary>
        /// <param name="users">Target clients user set.</param>
        /// <returns>The operation to send to makes clients delete the entity</returns>
        DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null);

        /// <summary>
        /// Convert the entity to a DTO.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Entity as a DTO</returns>
        IEntity ToEntityDto(UMI3DUser user);

        /// <summary>
        ///  Convert the entity to a list of bytes as a bytable.
        /// </summary>
        /// <param name="baseSize"></param>
        /// <param name="user"></param>
        /// <returns> return a couple (size,func) with size as the required size more the baseSize given,
        /// and func a method taking a byte array in which the data should be written, a position at which it should be written, a base size.
        /// This method return the updated position (position + required size) and the (required size + base size);
        /// The byte array given should be at least of size position + required size;
        /// </returns>
        /// Used in networking with bytes.
        Bytable ToBytes(UMI3DUser user);
    }
}