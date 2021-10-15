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

namespace umi3d.edk.volume
{
    public class VolumeManager : Singleton<VolumeManager>
    {
        public static Dictionary<ulong, IVolume> volumes = new Dictionary<ulong, IVolume>();


        static public void DispatchBrowserRequest(UMI3DUser user, uint operationKey, ByteContainer container)
        {
            switch (operationKey)
            {
                case UMI3DOperationKeys.VolumeUserTransit:
                    ulong volumeId = UMI3DNetworkingHelper.Read<ulong>(container);
                    bool direction = UMI3DNetworkingHelper.Read<bool>(container);
                    DispatchBrowserRequest(user, volumeId, direction);
                    break;
                default:
                    throw new System.Exception("Unknown volume operation key (" + operationKey + ")");
            }
        }

        static public void DispatchBrowserRequest(UMI3DUser user, ulong volumeId, bool direction)
        {
            if (volumes.TryGetValue(volumeId, out IVolume volume))
            {
                if (direction)
                    volume.GetUserEnter().Invoke(user);
                else
                    volume.GetUserExit().Invoke(user);
            }
            else
            {
                throw new System.Exception("Unknown volume id : " + volumeId);
            }
        }
    }
}