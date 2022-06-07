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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    public class UMI3DInteractable : AbstractTool, UMI3DLoadableEntity
    {
        [SerializeField, EditorReadOnly]
        protected bool NotifyHoverPosition;
        [SerializeField, EditorReadOnly]
        protected bool NotifySubObject;
        [SerializeField, EditorReadOnly]
        protected UMI3DNode Node;
        [SerializeField, EditorReadOnly]
        protected bool HasPriority;

        ///<inheritdoc/>
        public override void Register()
        {
            base.Register();
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// Class for event rising on hover when <see cref="NotifyHoverPosition"/> is enabled. 
        /// The first argument is the hovering user
        /// The second argument is the hovered position of the object in the object's local frame, 
        /// the third argument is the normal to the object's surface at the hovered position in the object's local frame.
        /// </summary>
        [Serializable]
        public class HoverEvent : UnityEvent<HoverEventContent> { }

        [Serializable]
        public class HoverEventContent : AbstractInteraction.InteractionEventContent
        {
            public Vector3 position { get; private set; }
            public Vector3 normal { get; private set; }
            public Vector3 direction { get; private set; }

            public HoverEventContent(UMI3DUser user, HoveredDto dto) : base(user, dto)
            {
                position = dto.position;
                normal = dto.normal;
                direction = dto.direction;
            }

            public HoverEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType, Vector3 position, Vector3 normal, Vector3 direction) : base(user, toolId, id, hoveredObjectId, boneType)
            {
                this.position = position;
                this.normal = normal;
                this.direction = direction;
            }
        }

        [SerializeField]
        public HoverEvent onHoverEnter = new HoverEvent();

        [SerializeField]
        public HoverEvent onHovered = new HoverEvent();

        [SerializeField]
        public HoverEvent onHoverExit = new HoverEvent();

        public bool UseAnimations = false;
        public UMI3DNodeAnimation HoverEnterAnimation;
        public UMI3DNodeAnimation HoverExitAnimation;

        /// <summary>
        /// List of bones hovering this object (if any).
        /// </summary>
        public List<string> hoveringBones = new List<string>();
        private UMI3DAsyncProperty<bool> objectNotifyHoverPosition1;
        private UMI3DAsyncProperty<bool> objectNotifySubObject1;
        private UMI3DAsyncProperty<UMI3DNode> objectNodeId1;
        private UMI3DAsyncProperty<bool> _hasPriority;

        public bool isHovered => hoveringBones.Count > 0;

        public UMI3DAsyncProperty<bool> objectNotifyHoverPosition { get { Register(); return objectNotifyHoverPosition1; } protected set => objectNotifyHoverPosition1 = value; }
        public UMI3DAsyncProperty<bool> objectNotifySubObject { get { Register(); return objectNotifySubObject1; } protected set => objectNotifySubObject1 = value; }
        public UMI3DAsyncProperty<UMI3DNode> objectNodeId { get { Register(); return objectNodeId1; } protected set => objectNodeId1 = value; }
        public UMI3DAsyncProperty<bool> hasPriority { get { Register(); return _hasPriority; } protected set => _hasPriority = value; }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override AbstractToolDto CreateDto()
        {
            return new InteractableDto();
        }

        /// <summary>
        /// Write the AbstractTool properties in an object AbstractToolDto is assignable from.
        /// </summary>
        /// <param name="scene">The AbstractToolDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(AbstractToolDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Idto = dto as InteractableDto;
            Idto.notifyHoverPosition = objectNotifyHoverPosition.GetValue(user);
            Idto.notifySubObject = objectNotifySubObject.GetValue(user);
            Idto.nodeId = objectNodeId.GetValue(user).Id();
            Idto.hasPriority = hasPriority.GetValue(user);
            Idto.HoverEnterAnimationId = HoverEnterAnimation != null ? HoverEnterAnimation.Id() : 0;
            Idto.HoverExitAnimationId = HoverExitAnimation != null ? HoverExitAnimation.Id() : 0;
        }

        ///<inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            if (Node == null) Node = GetComponent<UMI3DNode>();
            objectNotifyHoverPosition = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.InteractableNotifyHoverPosition, NotifyHoverPosition);
            objectNotifySubObject = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.InteractableNotifySubObject, NotifySubObject);
            objectNodeId = new UMI3DAsyncProperty<UMI3DNode>(id, UMI3DPropertyKeys.InteractableNodeId, Node, (n, u) => n.Id());
            objectNotifyHoverPosition.OnValueChanged += (b) => NotifyHoverPosition = b;
            objectNotifySubObject.OnValueChanged += (b) => NotifySubObject = b;
            objectNodeId.OnValueChanged += (n) => Node = n;
            hasPriority = new UMI3DAsyncProperty<bool>(toolId, UMI3DPropertyKeys.InteractableHasPriority, HasPriority);
        }

        public void Hovered(UMI3DUser user, HoveredDto dto)
        {
            onHovered?.Invoke(new HoverEventContent(user, dto));
        }

        public void Hovered(UMI3DUser user, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            Vector3 pos = UMI3DNetworkingHelper.Read<Vector3>(container);
            Vector3 norm = UMI3DNetworkingHelper.Read<Vector3>(container);
            Vector3 dir = UMI3DNetworkingHelper.Read<Vector3>(container);
            onHovered?.Invoke(new HoverEventContent(user, toolId, interactionId, hoverredId, boneType, pos, norm, dir));
        }



        public void HoverStateChanged(UMI3DUser user, HoverStateChangedDto dto)
        {
            if (dto.state) onHoverEnter.Invoke(new HoverEventContent(user, dto));
            else onHoverExit.Invoke(new HoverEventContent(user, dto));
        }

        public void HoverStateChanged(UMI3DUser user, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            Vector3 pos = UMI3DNetworkingHelper.Read<Vector3>(container);
            Vector3 norm = UMI3DNetworkingHelper.Read<Vector3>(container);
            Vector3 dir = UMI3DNetworkingHelper.Read<Vector3>(container);
            bool state = UMI3DNetworkingHelper.Read<bool>(container);
            if (state) onHoverEnter.Invoke(new HoverEventContent(user, toolId, interactionId, hoverredId, boneType, pos, norm, dir));
            else onHoverExit.Invoke(new HoverEventContent(user, toolId, interactionId, hoverredId, boneType, pos, norm, dir));
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return (ToDto(user) as InteractableDto);
        }
    }
}