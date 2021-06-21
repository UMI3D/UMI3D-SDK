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

namespace umi3d.common
{
    public class UMI3DAudioPlayerDto : UMI3DAbstractAnimationDto
    {
        public ResourceDto audioResource;
        public ulong nodeID;
        /// <summary>
        /// Spacial Blend.
        /// 0:not spacialized; 1:Spacialized on the node; 
        /// </summary>
        public float spatialBlend = 0f;
        public float volume = 1f;
        public float pitch = 1f;

    }
}