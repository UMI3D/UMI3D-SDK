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

using inetum.unityUtils.saveSystem;
using System;
using System.Collections.Generic;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public abstract class AbstractProjectionTreeNodeDelegate<Dto> : SerializableScriptableObject
    where Dto : AbstractInteractionDto
    {
        public ProjectionTree_SO projectionTree_SO;
        public string treeId;

        /// <summary>
        /// Init this object with dependency injection.
        /// </summary>
        /// <param name="projectionTree_SO"></param>
        /// <param name="treeId"></param>
        public virtual void Init(ProjectionTree_SO projectionTree_SO, string treeId)
        {
            this.projectionTree_SO = projectionTree_SO;
            this.treeId = treeId;
        }

        /// <summary>
        /// Return a predicate the is true if the node is compatible with the dto.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public abstract Predicate<ProjectionTreeNodeDto> IsNodeCompatible(Dto interaction);

        /// <summary>
        /// Return a <see cref="Func{ProjectionTreeNode}"/> that will create a tree node for this input found by <paramref name="findInput"/>.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="findInput"></param>
        /// <returns></returns>
        public abstract Func<ProjectionTreeNodeDto> CreateNodeForInput(
            Dto interaction,
            Func<AbstractUMI3DInput> findInput
        );

        /// <summary>
        /// Return a delegate that<br/>
        /// - if <paramref name="environmentId"/> and <paramref name="toolId"/> and <paramref name="hoveredObjectId"/> are not null then associated the dto with its input.<br/>
        /// - if <paramref name="selectedInputs"/> is not null select the node's input.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="selectedInputs"></param>
        /// <returns></returns>
        public abstract Action<ProjectionTreeNodeDto> ChooseProjection(
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null,
            List<AbstractUMI3DInput> selectedInputs = null
        );
    }
}