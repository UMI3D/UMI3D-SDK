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

namespace umi3d.common.volume
{
    /// <summary>
    /// Notify that a user entered/exits a volume.
    /// </summary>
    public class VolumeUserTransitDto : AbstractBrowserRequestDto
    {
        public ulong volumeId;

        /// <summary>
        /// True if the user entered in the volume, false if the user exited the volume.
        /// </summary>
        public bool direction;

        protected override uint GetOperationId() { return UMI3DOperationKeys.VolumeUserTransit; }

        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters) 
                + UMI3DNetworkingHelper.Write(volumeId) 
                + UMI3DNetworkingHelper.Write(direction);
        }
    }
}