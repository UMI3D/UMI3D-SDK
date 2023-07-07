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

namespace umi3d.common.interaction
{
    [System.Serializable]
    public class DofGroupDto : UMI3DDto
    {
        /// <summary>
        /// name of the degree of freedom group
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// degree of freedom combination used by this group
        /// </summary>
        public DofGroupEnum dofs { get; set; }

        public DofGroupDto() : base()
        {
        }
    }
}