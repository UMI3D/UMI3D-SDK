/*
Copyright 2019 - 2024 Inetum

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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    [Serializable]
    public struct ProjectionTreeNodeDto 
    {
        /// <summary>
        /// Node id.
        /// </summary>
        public ulong id;
        /// <summary>
        /// Node's parent id.
        /// </summary>
        public ulong parentId;
        /// <summary>
        /// Tree's id the node belongs to.
        /// </summary>
        public string treeId;
        /// <summary>
        /// Node's children IDs.
        /// </summary>
        public List<ProjectionTreeNodeDto> children;
        /// <summary>
        /// The concreate interaction Dto.
        /// </summary>
        public IProjectionTreeNodeDto interactionDto;
        /// <summary>
        /// The input associated with this interaction.
        /// </summary>
        public AbstractUMI3DInput input;
    }
}