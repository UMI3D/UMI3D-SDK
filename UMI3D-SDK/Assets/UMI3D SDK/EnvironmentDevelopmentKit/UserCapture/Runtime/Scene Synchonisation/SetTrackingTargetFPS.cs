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
using umi3d.common;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="Operation"/> to control the frequency of target tracking updates.
    /// </summary>
    /// Using this operation enables to lower the number of tracked frame per second when required, 
    /// reducing the load on the networking system. 
    /// A higher FPS will result with a better tracking of the movement, but will have a high impact on the networking load.
    public class SetTrackingTargetFPS : Operation
    {
        /// <summary>
        /// New number of tracked frames per second.
        /// </summary>
        public int targetFPS;

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SetEntityProperty)
                + UMI3DNetworkingHelper.Write(targetFPS);
        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var targetFPS = new SetTrackingTargetFPSDto()
            {
                targetFPS = this.targetFPS
            };
            return targetFPS;
        }
    }
}
