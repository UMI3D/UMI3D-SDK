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
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Interface for UMI3D Server script
    /// </summary>
    public interface IUMI3DServer
    {
        void Init();

        string GetProtocol();

        string GetIP();

        int GetPort();

        string GetEnvironmentUrl();

        string GetSocketPath();

        /// <summary>
        /// Convert the object to a DTO.
        /// </summary>
        UMI3DDto ToDto();
    }
}
