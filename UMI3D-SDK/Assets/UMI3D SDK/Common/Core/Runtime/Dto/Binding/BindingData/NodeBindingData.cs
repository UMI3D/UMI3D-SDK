/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common
{
    [System.Serializable]
    public class NodeBindingDataDto : AbstractSimpleBindingDataDto
    {
        public NodeBindingDataDto()
        { }

        public NodeBindingDataDto(ulong nodeId, bool syncRotation, bool syncScale, bool syncPosition,
                        SerializableVector3 offSetPosition, SerializableVector4 offSetRotation, SerializableVector3 offSetScale, SerializableVector3 anchorPosition,
                        int priority, bool partialFit)
            : base(syncRotation, syncScale, syncPosition, offSetPosition, offSetRotation, offSetScale, anchorPosition, priority, partialFit)
        {
            this.nodeId = nodeId;
        }

        public ulong nodeId { get; private set; }
    }
}