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

namespace umi3d.edk
{
    /// <summary>
    /// Interface for UMI3D entities which can be sent to a client.
    /// </summary>
    public interface UMI3DMediaEntity : UMI3DEntity
    {
        /// <summary>
        /// Condition to be loaded on connection.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool LoadOnConnection(UMI3DUser user);
        /// <summary>
        /// Add a filtering condition from the user filtering.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        bool AddConnectionFilter(UMI3DUserFilter filter);
        /// <summary>
        /// Remove a filtering condition from the user filtering.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        bool RemoveConnectionFilter(UMI3DUserFilter filter);
    }

    /// <summary>
    /// Interface for UMI3D entities.
    /// </summary>
    public interface UMI3DEntity
    {
        /// <summary>
        /// UMI3D Id of an entity.
        /// </summary>
        /// Null id is 0 <br/>
        /// Ids are set between 1 and 18,446,744,073,709,551,615. <br/>
        /// Actualy between 1 and 9,223,372,036,854,775,807 due to the serializer not sutporting unsigned values.
        /// <returns></returns>
        ulong Id();
    }
}

