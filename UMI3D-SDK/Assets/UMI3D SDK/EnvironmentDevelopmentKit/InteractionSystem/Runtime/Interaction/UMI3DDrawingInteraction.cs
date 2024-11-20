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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Interaction to draw a line. <br/>
    /// See <seealso cref="ProjectTool"/> and <seealso cref="ReleaseTool"/>.
    /// </summary>
    public class UMI3DDrawingInteraction : AbstractInteraction
    {
        //TODO : link line renderer 
        // receive positions => set in local line => create new line => reset local line
        //Draw on mesh

        #region fields

        /// <summary>
        /// Called during the first frame when the interaction is held by a user.
        /// </summary>
        [SerializeField, Tooltip("Called during the first frame when the interaction is held by a user.")]
        public InteractionEvent onTrigger = new();

        /// <summary>
        /// Called during the first frame after the interaction is no longer held by for a user.
        /// </summary>
        [SerializeField, Tooltip("Called during the first frame after the interaction is no longer held by a user.")]
        public UnityEvent<DrawingEventContent> onRelease = new();

        /// <summary>
        /// Called during the first frame after the interaction is no longer held by for a user.
        /// </summary>
        [SerializeField, Tooltip("Called during the drawing process of a user.")]
        public UnityEvent<DrawingEventContent> onDrawing = new();

        /// <summary>
        /// <see cref="AbstractInteraction.InteractionEventContent"/> specialized for drawing.
        /// </summary>
        [Serializable]
        public class DrawingEventContent : AbstractInteraction.InteractionEventContent
        {
            public List<Vector3Dto> positions { get; private set; } = new List<Vector3Dto>();

            public DrawingEventContent(UMI3DUser user, DrawingDto dto) : base(user, dto)
            {
                positions = dto.positions;
            }

            public DrawingEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType, Vector3Dto bonePosition, Vector4Dto boneRotation, List<Vector3Dto> positions) : base(user, toolId, id, hoveredObjectId, boneType, bonePosition, boneRotation)
            {
                this.positions = positions;
            }
        }

        /// <summary>
        /// The line use to draw
        /// </summary>
        [Tooltip("Line the user use to draw")]
        public UMI3DLineRenderer line = null;
        /// <summary>
        /// The mesh the drawing is done on
        /// </summary>
        [Tooltip("Line the user use to draw")]
        public AbstractRenderedNode mesh = null;
        /// <summary>
        /// Animation triggered when the interaction is triggered.
        /// </summary>
        [SerializeField, Tooltip("Client animation triggered when the interaction is triggered by a user.")]
        public UMI3DAbstractAnimation TriggerAnimation;
        /// <summary>
        /// Animation triggered when the interaction is released.
        /// </summary>
        [SerializeField, Tooltip("Client animation triggered when is released by a user.")]
        public UMI3DAbstractAnimation ReleaseAnimation;


        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        private UMI3DAsyncProperty<UMI3DLineRenderer> _line;
        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        private UMI3DAsyncProperty<AbstractRenderedNode> _mesh;
        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        private UMI3DAsyncProperty<UMI3DAbstractAnimation> _triggerAnimation;
        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is released
        /// </summary>
        private UMI3DAsyncProperty<UMI3DAbstractAnimation> _releaseAnimation;

        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        public UMI3DAsyncProperty<UMI3DLineRenderer> Line { get { Register(); return _line; } set => _line = value; }
        /// <summary>
        /// Renderer Async property of the mesh the drawing is done on
        /// </summary>
        public UMI3DAsyncProperty<AbstractRenderedNode> Mesh { get { Register(); return _mesh; } set => _mesh = value; }
        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        public UMI3DAsyncProperty<UMI3DAbstractAnimation> triggerAnimation { get { Register(); return _triggerAnimation; } set => _triggerAnimation = value; }
        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is released
        /// </summary>
        public UMI3DAsyncProperty<UMI3DAbstractAnimation> releaseAnimation { get { Register(); return _releaseAnimation; } set => _releaseAnimation = value; }

        #endregion

        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            Line = new UMI3DAsyncProperty<UMI3DLineRenderer>(id, UMI3DPropertyKeys.DrawingLine, line, (v, u) => v?.Id());
            Mesh = new UMI3DAsyncProperty<AbstractRenderedNode>(id, UMI3DPropertyKeys.DrawingMesh, line, (v, u) => v?.Id());
            triggerAnimation = new UMI3DAsyncProperty<UMI3DAbstractAnimation>(id, UMI3DPropertyKeys.EventTriggerAnimation, TriggerAnimation, (v, u) => v?.Id());
            releaseAnimation = new UMI3DAsyncProperty<UMI3DAbstractAnimation>(id, UMI3DPropertyKeys.EventReleaseAnimation, ReleaseAnimation, (v, u) => v?.Id());
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">Interacting User</param>
        /// <param name="interactionRequest">Received interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case EventTriggeredDto eventTriggered:
                    onTrigger.Invoke(new InteractionEventContent(user, interactionRequest));
                    break;
                case DrawingDto drawing:
                    if (drawing.drawingEnd)
                        onRelease.Invoke(new DrawingEventContent(user, drawing));
                    else
                        onDrawing.Invoke(new DrawingEventContent(user, drawing));

                    break;
            }
        }

        /// <summary>
        /// Called by a user on interaction.
        /// </summary>
        /// <param name="user">Interacting user</param>
        /// <param name="operationId">Operatin id in <see cref="UMI3DOperationKeys"/></param>
        /// <param name="toolId">Tool id in </param>
        /// <param name="interactionId">Id of the interaction</param>
        /// <param name="hoveredId">The id of the currently hoverred object.</param>
        /// <param name="boneType">User's used bone</param>
        /// <param name="container">Byte container</param>
        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoveredId, uint boneType, Vector3Dto bonePosition, Vector4Dto boneRotation, ByteContainer container)
        {
            switch (operationId)
            {
                case UMI3DOperationKeys.EventTriggered:
                    onTrigger.Invoke(new InteractionEventContent(user, toolId, interactionId, hoveredId, boneType, bonePosition, boneRotation));
                    break;
                case UMI3DOperationKeys.Drawing:
                    bool active = UMI3DSerializer.Read<bool>(container);
                    List<Vector3Dto> positions = UMI3DSerializer.ReadList<Vector3Dto>(container);
                    if (active)
                        onDrawing.Invoke(new DrawingEventContent(user, toolId, interactionId, hoveredId, boneType, bonePosition, boneRotation, positions));
                    else
                        onRelease.Invoke(new DrawingEventContent(user, toolId, interactionId, hoveredId, boneType, bonePosition, boneRotation, positions));

                    break;
            }
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                    + UMI3DSerializer.Write(Line?.GetValue(user)?.Id() ?? 0)
                     + UMI3DSerializer.Write(Mesh?.GetValue(user)?.Id() ?? 0)
                     + UMI3DSerializer.Write(triggerAnimation?.GetValue(user)?.Id() ?? 0)
                     + UMI3DSerializer.Write(releaseAnimation?.GetValue(user)?.Id() ?? 0);
        }

        /// <inheritdoc/>
        protected override AbstractInteractionDto CreateDto()
        {
            return new DrawingInteractionDto();
        }

        /// <inheritdoc/>
        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.Event;
        }

        /// <inheritdoc/>
        protected override void WriteProperties(AbstractInteractionDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            if (dto is DrawingInteractionDto _dto)
            {
                _dto.LineId = Line.GetValue(user)?.Id() ?? 0;
                _dto.MeshId = Mesh.GetValue(user)?.Id() ?? 0;
                _dto.TriggerAnimationId = triggerAnimation.GetValue(user)?.Id() ?? 0;
                _dto.ReleaseAnimationId = releaseAnimation.GetValue(user)?.Id() ?? 0;
            }
        }
    }
}