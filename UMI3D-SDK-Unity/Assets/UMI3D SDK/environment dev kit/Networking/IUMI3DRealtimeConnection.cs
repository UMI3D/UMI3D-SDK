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
    /// Define the common interface for any UMI3D realtime communication channel.
    /// </summary>
    public interface IUMI3DRealtimeConnection
    {
        /// <summary>
        /// Get the connection id.
        /// </summary>
        string GetId();

        /// <summary>
        /// Send a Data Tranfer Object through the connection.
        /// </summary>
        /// <param name="msg">a UMI3DDto</param>
        void SendData( UMI3DDto msg );
    }
}
