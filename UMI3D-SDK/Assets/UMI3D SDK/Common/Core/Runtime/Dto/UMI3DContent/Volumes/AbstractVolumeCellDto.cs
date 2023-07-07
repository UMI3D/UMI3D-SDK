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
    /// Abstract DTO describing volume cells.
    /// </summary>
    public abstract class AbstractVolumeCellDto : AbstractVolumeDescriptorDto
    {
        /// <summary>
        /// Node to position the cell in relation to.
        /// </summary>
        public ulong rootNodeId { get; set; }

        /// <summary>
        /// Should user be able to enter this volume?
        /// </summary>
        public bool isTraversable { get; set; } = true;
    }
}