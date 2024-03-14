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
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public interface IProjectionTreeNodeDelegate<Dto>
    where Dto : AbstractInteractionDto
    {
        /// <summary>
        /// Return a predicate that is true if the node is compatible with the interaction.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        Predicate<ProjectionTreeNodeData> IsNodeCompatible(Dto interaction);

        /// <summary>
        /// Return a <see cref="Func{ProjectionTreeNode}"/> that will create a tree node for this control found by <paramref name="getControl"/>.<br/>
        /// This <see cref="Func{ProjectionTreeNode}"/> must throw a <see cref="NoInputFoundException"/> if no controlId is found.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="getControl"></param>
        /// <returns></returns>
        Func<ProjectionTreeNodeData> CreateNodeForControl(
            string treeId,
            Dto interaction,
            Func<AbstractControlEntity> getControl
        );

        /// <summary>
        /// Return a delegate that<br/>
        /// - if <paramref name="environmentId"/> and <paramref name="toolId"/> and <paramref name="hoveredObjectId"/> are not null then associated the interaction with its input.<br/>
        /// - if <paramref name="selectedInputs"/> is not null select the node's input.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        /// <returns></returns>
        Action<ProjectionTreeNodeData> ChooseProjection(
            UMI3DControlManager controlManager,
            ulong? environmentId = null,
            ulong? toolId = null,
            ulong? hoveredObjectId = null
        );
    }
}