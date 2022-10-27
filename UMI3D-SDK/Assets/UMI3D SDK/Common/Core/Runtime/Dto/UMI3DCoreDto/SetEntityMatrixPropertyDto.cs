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
    /// <summary>
    /// Operation DTO to inform a browser the value of a entity's matrix property changed.
    /// </summary>
    public class SetEntityMatrixPropertyDto : SetEntityPropertyDto
    {
        /// <summary>
        /// X position value of a matrix
        /// </summary>
        public int X;

        /// <summary>
        /// Y position value of a matrix
        /// </summary>
        public int Y;
    }
}