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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    [Serializable]
    /// <summary>
    /// Abstract class describing a list of interactions that could be projected on a client controller, grouped into a same entity.
    /// </summary>
    public abstract class AbstractTool: HasToolData
    {
        public ToolData data = new();

        public event Action<AbstractTool> toolUpdated;
        public event Action<AbstractInteractionDto> interactionAdded;
        public event Action<AbstractInteractionDto> interactionRemoved;
        public event Action<AbstractTool> destroyed;

        public ToolData Data
        {
            get
            {
                return data;
            }
        }

        public List<Task<AbstractInteractionDto>> interactions
        {
            get
            {
                return data.dto.interactions
                    .Select(
                        interactionId =>
                        {
                            return UMI3DEnvironmentLoader
                            .WaitForAnEntityToBeLoaded(
                                data.environmentId,
                                interactionId,
                                null
                            );
                        }
                    )
                    .Select(
                        async node =>
                        {
                            return (await node).dto as AbstractInteractionDto;
                        }
                    ).ToList();
            }
        }

        public List<AbstractInteractionDto> interactionsLoaded
        {
            get
            {
                return data.dto.interactions
                    .Select(
                        interactionId =>
                        {
                            return UMI3DEnvironmentLoader
                            .Instance
                            .TryGetEntityInstance(
                                data.environmentId,
                                interactionId
                            );
                        }
                    )
                    .Select(
                        node =>
                        {
                            return node.dto as AbstractInteractionDto;
                        }
                    ).ToList();
            }
        }

        protected AbstractTool(ulong environmentId, AbstractToolDto abstractDto)
        {
            data.dto = abstractDto;
            data.environmentId = environmentId;
        }

        public void ToolUpdated()
        {
            toolUpdated?.Invoke(this);
        }

        public void InteractionAdded(AbstractInteractionDto newInteraction)
        {
            interactionAdded?.Invoke(newInteraction);
        }

        public void InteractionRemoved(AbstractInteractionDto oldInteraction)
        {
            interactionRemoved?.Invoke(oldInteraction);
        }

        public void onProjected(uint boneType)
        {
            UMI3DClientServer.SendRequest(
                new ToolProjectedDto
                {
                    boneType = boneType,
                    toolId = data.dto.id
                }, 
                true
            );
        }

        public void onReleased(uint boneType)
        {
            UMI3DClientServer.SendRequest(
                new ToolReleasedDto
                {
                    boneType = boneType,
                    toolId = data.dto.id
                }, 
                true
            );
        }

        /// <summary>
        /// Safely destroy a tool.
        /// </summary>
        public virtual void Destroy()
        {
            destroyed?.Invoke(this);
        }
    }
}