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
    /// Interface for UMI3D entities which can be send to a client.
    /// </summary>
    public interface UMI3DMediaEntity : UMI3DEntity
    {
        bool LoadOnConnection(UMI3DUser user);
        bool AddConnectionFilter(UMI3DUserFilter filter);
        bool RemoveConnectionFilter(UMI3DUserFilter filter);
    }

    /// <summary>
    /// Interface for UMI3D entities.
    /// </summary>
    public interface UMI3DEntity
    {
        /// <summary>
        /// Id of an entity;
        /// Null id is 0;
        /// id are set between 1 and 18,446,744,073,709,551,615;
        /// Actualy between 1 and 9,223,372,036,854,775,807 due to the serializer not sutporting unsigned values.
        /// </summary>
        /// <returns></returns>
        ulong Id();
    }

}

