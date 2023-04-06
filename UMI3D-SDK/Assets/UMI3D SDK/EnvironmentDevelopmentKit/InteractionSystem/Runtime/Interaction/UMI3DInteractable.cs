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
    /// <summary>
    /// <see cref="AbstractTool"/> attached to an object.
    /// </summary>
    public class UMI3DInteractable : AbstractTool, UMI3DLoadableEntity
    {
        #region Fields

        /// <summary>
        /// Should the object notify the environment when being hovered ?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should the object notify the environment when being hovered ?")]
        protected bool NotifyHoverPosition;
        /// <summary>
        /// Should sub-objects notify the environment when being hovered ?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should sub-objects notify the environment when being hovered ?")]
        protected bool NotifySubObject;
        /// <summary>
        /// <see cref="UMI3DNode"/> the interactable is attached to.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("UMI3D Node the interactable is attached to.")]
        protected UMI3DNode Node;
        /// <summary>
        /// Should be prioritized over others interactables when pointed at ?
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Should be prioritized over others interactables when pointed at ?")]
        protected bool HasPriority;

        /// <summary>
        /// Distance for a user to interact with this tool. If value < 0, no distance check.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Distance for a user to interact with this tool. If value < 0, no distance check")]
        protected float InteractionDistance = -1;

        #endregion

        #region UMI3DLoadableEntity

        /// <inheritdoc/>
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

        #endregion

        /// <summary>
        /// Class for event rising on hover when <see cref="NotifyHoverPosition"/> is enabled. 
        /// The first argument is the hovering user
        /// The second argument is the hovered position of the object in the object's local frame, 
        /// the third argument is the normal to the object's surface at the hovered position in the object's local frame.
        /// </summary>
        [Serializable]
        public class HoverEvent : UnityEvent<HoverEventContent> { }

        /// <summary>
        /// <see cref="AbstractInteraction.InteractionEventContent"/> specialized for hoverring.
        /// </summary>
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

        /// <summary>
        /// Triggered when the object starts to be hovered by a user.
        /// </summary>
        [SerializeField, Tooltip("Triggered when the object starts to be hovered by a user.")]
        public HoverEvent onHoverEnter = new HoverEvent();

        /// <summary>
        /// Triggered when the object is hovered by a user.
        /// </summary>
        [SerializeField, Tooltip("Triggered when the object is hovered by a user.")]
        public HoverEvent onHovered = new HoverEvent();

        /// <summary>
        /// Triggered when the object stops to be hovered by a user.
        /// </summary>
        [SerializeField, Tooltip("Triggered when the object stops to be hovered by a user.")]
        public HoverEvent onHoverExit = new HoverEvent();

        /// <summary>
        /// Should specific animations be played on hover enter / exit ?
        /// </summary>
        [Tooltip("Should specific animations be played on hover enter / exit ?")]
        public bool UseAnimations = false;

        // <summary>
        /// Triggered when the object is hovered by a user.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Triggered when the object is hovered by a user.")]
        protected UMI3DAbstractAnimation HoverEnterAnimation;
        /// <summary>
        /// Triggered when the object stops to be hovered by a user.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Triggered when the object stops to be hovered by a user.")]
        protected UMI3DAbstractAnimation HoverExitAnimation;

        #region Async Property 

        /// <summary>
        /// List of bones hovering this object (if any).
        /// </summary>
        public List<string> hoveringBones = new List<string>();
        private UMI3DAsyncProperty<bool> objectNotifyHoverPosition1;
        private UMI3DAsyncProperty<bool> objectNotifySubObject1;
        private UMI3DAsyncProperty<UMI3DNode> objectNodeId1;
        private UMI3DAsyncProperty<bool> _hasPriority;

        /// <summary>
        /// Private field associated to property <see cref="interactionDistance"/>.
        /// </summary>
        private UMI3DAsyncProperty<float> _interactionDistance;
        private UMI3DAsyncProperty<UMI3DAbstractAnimation> _hoverEnterAnimation;
        private UMI3DAsyncProperty<UMI3DAbstractAnimation> _hoverExitAnimation;

        /// <summary>
        /// True if the object is hovered by at least one bone
        /// </summary>
        public bool isHovered => hoveringBones.Count > 0;

        public UMI3DAsyncProperty<bool> objectNotifyHoverPosition { get { Register(); return objectNotifyHoverPosition1; } protected set => objectNotifyHoverPosition1 = value; }
        public UMI3DAsyncProperty<bool> objectNotifySubObject { get { Register(); return objectNotifySubObject1; } protected set => objectNotifySubObject1 = value; }
        public UMI3DAsyncProperty<UMI3DNode> objectNodeId { get { Register(); return objectNodeId1; } protected set => objectNodeId1 = value; }
        public UMI3DAsyncProperty<bool> hasPriority { get { Register(); return _hasPriority; } protected set => _hasPriority = value; }
        /// <summary>
        /// Property associated to <see cref="InteractionDistance"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> interactionDistance { get { Register(); return _interactionDistance; } protected set => _interactionDistance = value; }
        public UMI3DAsyncProperty<UMI3DAbstractAnimation> hoverEnterAnimation { get { Register(); return _hoverEnterAnimation; } set => _hoverEnterAnimation = value; }
        public UMI3DAsyncProperty<UMI3DAbstractAnimation> hoverExitAnimation { get { Register(); return _hoverExitAnimation; } set => _hoverExitAnimation = value; }

        #endregion

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
            Idto.interactionDistance = interactionDistance.GetValue(user);
            Idto.HoverEnterAnimationId = hoverEnterAnimation.GetValue(user)?.Id() ?? 0;
            Idto.HoverExitAnimationId = hoverExitAnimation.GetValue(user)?.Id() ?? 0;
        }

        /// <inheritdoc/>
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
            hasPriority.OnValueChanged += (b) => HasPriority = b;
            interactionDistance = new UMI3DAsyncProperty<float>(toolId, UMI3DPropertyKeys.InteractableInteractionDistance, InteractionDistance);
            interactionDistance.OnValueChanged += (f) => InteractionDistance = f;

            hoverEnterAnimation = new UMI3DAsyncProperty<UMI3DAbstractAnimation>(id, UMI3DPropertyKeys.InteractableHoverEnterAnimation, HoverEnterAnimation, (v, u) => v?.Id());
            hoverExitAnimation = new UMI3DAsyncProperty<UMI3DAbstractAnimation>(id, UMI3DPropertyKeys.InteractableHoverExitAnimation, HoverExitAnimation, (v, u) => v?.Id());
        }

        /// <summary>
        /// Called when a user points the interactable and that a <see cref="HoveredDto"/> is received.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dto"></param>
        public void Hovered(UMI3DUser user, HoveredDto dto)
        {
            onHovered?.Invoke(new HoverEventContent(user, dto));
        }

        /// <summary>
        /// Called when a user points the interactable and that a <see cref="HoveredDto"/> is received using bytes.
        /// <br/> Part of the bytes workflow.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dto"></param>
        public void Hovered(UMI3DUser user, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            Vector3 pos = UMI3DSerializer.Read<Vector3>(container);
            Vector3 norm = UMI3DSerializer.Read<Vector3>(container);
            Vector3 dir = UMI3DSerializer.Read<Vector3>(container);
            onHovered?.Invoke(new HoverEventContent(user, toolId, interactionId, hoverredId, boneType, pos, norm, dir));
        }


        /// <summary>
        /// Called when a user starts or ends to point an object and that a <see cref="HoverStateChangedDto"/> is received.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dto"></param>
        public void HoverStateChanged(UMI3DUser user, HoverStateChangedDto dto)
        {
            if (dto.state) onHoverEnter.Invoke(new HoverEventContent(user, dto));
            else onHoverExit.Invoke(new HoverEventContent(user, dto));
        }

        /// <summary>
        /// Called when a user starts or ends to point an object and that a <see cref="HoverStateChangedDto"/> is received.
        /// <br/> Part of the bytes workflow.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dto"></param>
        public void HoverStateChanged(UMI3DUser user, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            Vector3 pos = UMI3DSerializer.Read<Vector3>(container);
            Vector3 norm = UMI3DSerializer.Read<Vector3>(container);
            Vector3 dir = UMI3DSerializer.Read<Vector3>(container);
            bool state = UMI3DSerializer.Read<bool>(container);
            if (state) onHoverEnter.Invoke(new HoverEventContent(user, toolId, interactionId, hoverredId, boneType, pos, norm, dir));
            else onHoverExit.Invoke(new HoverEventContent(user, toolId, interactionId, hoverredId, boneType, pos, norm, dir));
        }

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user) as InteractableDto;
        }
    }
}