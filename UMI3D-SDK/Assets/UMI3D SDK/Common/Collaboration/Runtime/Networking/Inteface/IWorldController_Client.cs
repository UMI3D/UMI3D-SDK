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

using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;

namespace umi3d.worldController
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWorldController_Client
    {
        /// <summary>
        /// Connect to the World Controller.
        /// </summary>
        /// <param name="connectionDto"></param>
        /// <returns></returns>
        Task<UMI3DDto> Connect(ConnectionDto connectionDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identityDto"></param>
        /// <returns></returns>
        Task<PrivateIdentityDto> RenewCredential(PrivateIdentityDto identityDto);
    }
}