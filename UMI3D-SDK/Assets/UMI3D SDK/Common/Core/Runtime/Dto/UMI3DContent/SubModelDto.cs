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
    /// <summary>
    /// Model Dto.
    /// </summary>
    [System.Serializable]
    public class SubModelDto : UMI3DRenderedNodeDto
    {
        /// <summary>
        /// Id of the root object of the model 
        /// </summary>
        public ulong modelId;

        /// <summary>
        /// Name of the submodel 
        /// </summary>
        public string subModelName;

        /// <summary>
        /// List of submodel name in hierachy from root to this subModel
        /// </summary>
        public List<string> subModelHierachyNames;

        /// <summary>
        /// List of submodel index in hierachy from root to this subModel
        /// </summary>
        public List<int> subModelHierachyIndexes;

        /// <summary>
        /// subModel Loader should apply root model material overrider or ignore it
        /// </summary>
        public bool ignoreModelMaterialOverride;

        /// <summary>
        /// If true, the mesh will be used for navmesh generation on the browser.
        /// </summary>
        public bool isPartOfNavmesh = false;

        /// <summary>
        /// Indicate whether or not the user is allowed to navigate through this object.
        /// </summary>
        public bool isTraversable = true;

        public SubModelDto() : base() { }

    }
}
