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

namespace umi3d.common
{
    [System.Serializable]
    public class UMI3DNodeDto : UMI3DAbstractNodeDto
    {
        public bool xBillboard = false;
        public bool yBillboard = false;
        public ColliderDto colliderDto = null;
        public UMI3DLodDto lodDto;

        /// <summary>
        /// Contains a collection of UMI3DId refering entities with skinnedMeshRenderer and an interger that is the position of this node in the bones array of the skinnedMeshRenderer.
        /// Used only with Model with tracked sub object and skinnedMeshRenderer
        /// </summary>
        public Dictionary<ulong, int> skinnedRendererLinks = new Dictionary<ulong, int>();

    }
}