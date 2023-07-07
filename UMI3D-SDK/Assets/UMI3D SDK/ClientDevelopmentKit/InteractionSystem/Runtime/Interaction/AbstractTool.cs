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
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Abstract class describing a list of interactions that could be projected on a client controller, grouped into a same entity.
    /// </summary>
    public abstract class AbstractTool
    {
        /// <summary>
        /// Tool Id.
        /// </summary>
        public ulong id => abstractDto.id;

        /// <summary>
        /// Toolbox name.
        /// </summary>
        public string name => abstractDto.name;

        /// <summary>
        /// Toolbox description.
        /// </summary>
        public string description => abstractDto.description;

        /// <summary>
        /// 2D icon.
        /// </summary>
        public ResourceDto icon2D => abstractDto.icon2D;

        /// <summary>
        /// 3D icon.
        /// </summary>
        public ResourceDto icon3D => abstractDto.icon3D;

        /// <summary>
        /// Is the tool active ?
        /// </summary>
        public bool Active => abstractDto?.active ?? false;

        /// <summary>
        /// Contained tools.
        /// </summary>
        public List<ulong> interactionsId => abstractDto.interactions;
        public List<Task<AbstractInteractionDto>> interactions => abstractDto.interactions.Select(inta => UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(inta,null)).Select(async node => (await node).dto as AbstractInteractionDto).ToList();

        public List<AbstractInteractionDto> interactionsLoaded => abstractDto.interactions.Select(UMI3DEnvironmentLoader.GetEntity).Select(node => node.dto as AbstractInteractionDto).ToList();

        // could be removed if unity project version is 2020.1 or newer 
        private class AbstractInteractionDtoEvent : UnityEvent<AbstractInteractionDto> { }

        /// <summary>
        /// Trigerred when the tool is updated.
        /// </summary>
        public UnityEvent OnUpdated = new UnityEvent();
        /// <summary>
        /// Trigerred when the tool is added.
        /// </summary>
        public UnityEvent<AbstractInteractionDto> OnAdded = new AbstractInteractionDtoEvent();
        /// <summary>
        /// Trigerred when the tool is removed.
        /// </summary>
        public UnityEvent<AbstractInteractionDto> OnRemoved = new AbstractInteractionDtoEvent();

        public void Updated() { OnUpdated.Invoke(); }
        public void Added(AbstractInteractionDto abstractInteractionDto) { OnAdded.Invoke(abstractInteractionDto); }
        public void Removed(AbstractInteractionDto abstractInteractionDto) { OnRemoved.Invoke(abstractInteractionDto); }

        /// <summary>
        /// Event raised when the abstract tool is projected.
        /// </summary>
        public UnityEvent onProject = new UnityEvent();

        /// <summary>
        /// Event raised when the abstract tool is released.
        /// </summary>
        public UnityEvent onRelease = new UnityEvent();

        public void onProjected(uint boneType)
        {
            onProject.Invoke();
            var projectedDto = new ToolProjectedDto
            {
                boneType = boneType,
                toolId = id
            };
            UMI3DClientServer.SendRequest(projectedDto, true);
        }

        public void onReleased(uint boneType)
        {
            onRelease.Invoke();
            var releasedDto = new ToolReleasedDto
            {
                boneType = boneType,
                toolId = id
            };
            UMI3DClientServer.SendRequest(releasedDto, true);
        }

        protected AbstractTool(AbstractToolDto abstractDto)
        {
            this.abstractDto = abstractDto;

        }

        /// <summary>
        /// Abstract DTO associated to the tool.
        /// </summary>
        protected abstract AbstractToolDto abstractDto { get; set; }

        /// <summary>
        /// Safely destroy a tool.
        /// </summary>
        public virtual void Destroy()
        {
            if (InteractionMapper.Instance.IsToolSelected(id))
                InteractionMapper.Instance.ReleaseTool(id, new RequestedByEnvironment());
        }
    }
}