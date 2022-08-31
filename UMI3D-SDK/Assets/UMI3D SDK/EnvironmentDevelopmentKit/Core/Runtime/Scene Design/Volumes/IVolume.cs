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

namespace umi3d.edk.volume
{
    /// <summary>
    /// Interface for volume cells.
    /// </summary>
    public interface IVolume : IVolumeDescriptor
    {
        /// <summary>
        /// Can the volume be traversed?
        /// </summary>
        /// <returns></returns>
        bool IsTraversable();

        /// <summary>
        /// Return the event raised when a user enters the volume cell.
        /// </summary>
        UMI3DUserEvent GetUserEnter();

        /// <summary>
        /// Return the event raised when a user exits the volume cell.
        /// </summary>
        UMI3DUserEvent GetUserExit();
    }
}


