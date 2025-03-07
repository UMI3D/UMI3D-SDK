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
    public class UMI3DDrawingInteraction : UMI3DEvent
    {
        //TODO : link line renderer 
        // receive positions => set in local line => create new line => reset local line
        //Draw on mesh

        #region fields

        /// <summary>
        /// Called during the first frame after the interaction is no longer held by for a user.
        /// </summary>
        [SerializeField, Tooltip("Called during the drawing process of a user.")]
        public UnityEvent<DrawingEventContent> onDrawing = new();

        /// <summary>
        /// Called during the first frame after the interaction is no longer held by for a user.
        /// </summary>
        [SerializeField, Tooltip("Called at the end of a drawing.")]
        public UnityEvent<DrawingEventContent> onDrawingEnd = new();

        /// <summary>
        /// <see cref="AbstractInteraction.InteractionEventContent"/> specialized for drawing.
        /// </summary>
        [Serializable]
        public class DrawingEventContent : InteractionEventContent
        {
            public UMI3DLineRenderer line { get; private set; } = null;
            public List<Vector3Dto> positions { get; private set; } = new List<Vector3Dto>();
            public UMI3DNode node { get; private set; }

            public ulong clientDrawingId { get; private set; }

            public DrawingEventContent(UMI3DUser user, DrawingDto dto, UMI3DLineRenderer line, UMI3DNode node) : base(user, dto)
            {
                positions = dto.positions;
                this.line = line;
                this.node = node;
                this.clientDrawingId = dto.clientDrawingId;
            }

            public DrawingEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType, Vector3Dto bonePosition, Vector4Dto boneRotation, List<Vector3Dto> positions, UMI3DLineRenderer line, UMI3DNode node, ulong clientDrawingId ) : base(user, toolId, id, hoveredObjectId, boneType, bonePosition, boneRotation)
            {
                this.positions = positions;
                this.line = line;
                this.node = node;
                this.clientDrawingId= clientDrawingId;
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
        /// State if the interaction can be done in the air
        /// </summary>
        [Tooltip("Line the user use to draw")]
        public bool canDrawInTheAir = true;

        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        private UMI3DAsyncProperty<UMI3DLineRenderer> _line;
        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        private UMI3DAsyncListProperty<AbstractRenderedNode> _mesh;
        /// <summary>
        /// State if the interaction can be done in the air
        /// </summary>
        private UMI3DAsyncProperty<bool> _canDrawInTheAir;

        /// <summary>
        /// Animation Async property of the animation triggered when the interaction is triggered
        /// </summary>
        public UMI3DAsyncProperty<UMI3DLineRenderer> Line { get { Register(); return _line; } set => _line = value; }
        /// <summary>
        /// Renderer Async property of the mesh the drawing is done on
        /// </summary>
        public UMI3DAsyncListProperty<AbstractRenderedNode> Mesh { get { Register(); return _mesh; } set => _mesh = value; }

        /// <summary>
        /// State if the interaction can be done in the air
        /// </summary>
        public UMI3DAsyncProperty<bool> CanDrawInTheAir { get { Register(); return _canDrawInTheAir; } set => _canDrawInTheAir = value; }



        public Dictionary<(ulong, ulong), UMI3DLineRenderer> LineMap = new();


        #endregion

        protected override void InitDefinition(ulong id)
        {
            Hold = true;

            base.InitDefinition(id);
            Line = new UMI3DAsyncProperty<UMI3DLineRenderer>(id, UMI3DPropertyKeys.DrawingLine, line, (v, u) => v?.Id());
            Mesh = new UMI3DAsyncListProperty<AbstractRenderedNode>(id, UMI3DPropertyKeys.DrawingMesh, mesh != null ? new() { mesh } : null, (v, u) => v?.Id());
            CanDrawInTheAir = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.DrawingInTheAir, canDrawInTheAir);
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
                case DrawingDto drawing:
                    UMI3DLineRenderer line = null;
                    this.LineMap.TryGetValue((user.Id(), drawing.clientLineId), out line);
                    UMI3DNode node = UMI3DEnvironment.GetEntityInstance<UMI3DNode>(drawing.surfaceId);
                    if (drawing.drawingEnd)
                        onDrawingEnd.Invoke(new DrawingEventContent(user, drawing, line, node));
                    else
                        onDrawing.Invoke(new DrawingEventContent(user, drawing, line, node));
                    break;
                default:
                    base.OnUserInteraction(user, interactionRequest);
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
                case UMI3DOperationKeys.Drawing:
                    bool drawingEnd = UMI3DSerializer.Read<bool>(container);
                    ulong clientDrawingId = UMI3DSerializer.Read<ulong>(container);
                    ulong clientLineId = UMI3DSerializer.Read<ulong>(container);
                    ulong surfaceId = UMI3DSerializer.Read<ulong>(container);
                    List<Vector3Dto> positions = UMI3DSerializer.ReadList<Vector3Dto>(container);

                    UMI3DLineRenderer line = null;
                    UMI3DNode surface = null;

                    if (clientLineId != 0)
                    {
                        if (!this.LineMap.TryGetValue((user.Id(), clientLineId), out line))
                        {
                            var Line = this.Line.GetValue(user);
                            line = Line == null ? null : CreateLine(Line, user, clientLineId, positions, drawingEnd);
                        }
                        else
                            UpdateLine(line, user, positions, drawingEnd);
                    }

                    if (surfaceId != 0)
                        surface = UMI3DEnvironment.GetEntityInstance<UMI3DNode>(surfaceId);

                    var DrawingEvent = new DrawingEventContent(user, toolId, interactionId, hoveredId, boneType, bonePosition, boneRotation, positions, line, surface, clientDrawingId);
                    
                    if (drawingEnd)
                        onDrawingEnd.Invoke(DrawingEvent);
                    else
                        onDrawing.Invoke(DrawingEvent);
                    break;

                default:
                    base.OnUserInteraction(user, operationId, toolId, interactionId, hoveredId, boneType, bonePosition, boneRotation, container);
                    break;
            }
        }

        protected override void InternalOnTrigger(UMI3DUser user) 
        {
            var id = user.Id();
            foreach(var key in this.LineMap.Where(kp => kp.Key.Item1 == id).Select(kp => kp.Key).ToList())
                this.LineMap.Remove(key);
        }

        protected override void InternalOnRelease(UMI3DUser user) {
            var id = user.Id();
            foreach (var key in this.LineMap.Where(kp => kp.Key.Item1 == id).Select(kp => kp.Key).ToList())
                this.LineMap.Remove(key);
        }

        public UMI3DLineRenderer CreateLine(UMI3DLineRenderer template, UMI3DUser user, ulong ClientLineID, List<Vector3Dto> positions, bool endDrawing)
        {
            GameObject gm = new GameObject();
            gm.transform.SetParent(template.transform.parent);
            gm.transform.position = Vector3.zero;

            UMI3DLineRenderer lr = gm.AddComponent<UMI3DLineRenderer>();
            lr.objectStartColor.SetValue(template.objectStartColor.GetValue(user));
            lr.objectEndColor.SetValue(template.objectEndColor.GetValue(user));
            lr.objectStartWidth.SetValue(template.objectStartWidth.GetValue(user));
            lr.objectEndWidth.SetValue(template.objectEndWidth.GetValue(user));
            lr.objectLoop.SetValue(template.objectLoop.GetValue(user));
            lr.objectUseWorldSpace.SetValue(template.objectUseWorldSpace.GetValue(user));
            lr.objectPositions.SetValue(positions.Select(p => p.Struct()).ToList());
            lr.objectClientLineId.SetValue(user,ClientLineID);

            lr.objectMaterialOverriders.SetValue(template.objectMaterialOverriders.GetValue(user));
            lr.objectMaterialsOverrided.SetValue(template.objectMaterialsOverrided.GetValue(user));

            lr.objectHasCollider.SetValue(template.objectHasCollider.GetValue(user));
            lr.objectColliderType.SetValue(template.objectColliderType.GetValue(user));
            lr.objectIsConvexe.SetValue(template.objectIsConvexe.GetValue(user));

            lr.objectIsMeshCustom.SetValue(template.objectIsMeshCustom.GetValue(user));

            if (!endDrawing)
                lr.objectPositions.DeSync(user, false);

            LoadEntity entity = lr.GetLoadEntity();
            entity.ToTransaction(true).Dispatch();

            LineMap.Add((user.Id(), ClientLineID), lr);

            return lr;
        }

        public void UpdateLine(UMI3DLineRenderer lr, UMI3DUser except, List<Vector3Dto> positions, bool endDrawing)
        {
            Transaction t = new(except == null);
            t.AddIfNotNull(lr.objectPositions.DeSync(except, endDrawing));
            t.AddIfNotNull(lr.objectPositions.SetValue(positions.Select(dto => dto.Struct()).ToList()));
            t.Dispatch();
        }


        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                    + UMI3DSerializer.Write(Line?.GetValue(user)?.Id() ?? 0)
                     + UMI3DSerializer.Write(CanDrawInTheAir?.GetValue(user) ?? true)
                     + UMI3DSerializer.Write(Mesh?.GetValue(user)?.Select(m => m.Id()).ToList() ?? new List<ulong>());
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
                _dto.lineId = Line.GetValue(user)?.Id() ?? 0;
                _dto.canDrawInSpace = CanDrawInTheAir?.GetValue(user) ?? true;
                _dto.meshIds = Mesh?.GetValue(user)?.Select(m => m?.Id() ?? 0).ToList() ?? new List<ulong>();
            }
        }


        public class SplitLineOperation : Operation
        {
            public ulong interactionId;

            /// <inheritdoc/>
            public override Bytable ToBytable(UMI3DUser user)
            {
                return UMI3DSerializer.Write(UMI3DOperationKeys.DrawingSplitLineRequest)
                    + UMI3DSerializer.Write(interactionId);
            }

            /// <inheritdoc/>
            public override AbstractOperationDto ToOperationDto(UMI3DUser user)
            {
                return new SplitLineDto() { interactionId = interactionId };
            }

            public static SplitLineOperation operator +(SplitLineOperation a, IEnumerable<UMI3DUser> b)
            {
                a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
                return a;
            }

            public static SplitLineOperation operator +(SplitLineOperation a, SplitLineOperation b)
            {
                return a + b.users;
            }

            public static SplitLineOperation operator -(SplitLineOperation a, SplitLineOperation b)
            {
                return a - b.users;
            }

            public static SplitLineOperation operator -(SplitLineOperation a, IEnumerable<UMI3DUser> b)
            {
                foreach (UMI3DUser u in b)
                {
                    if (a.users.Contains(u)) a.users.Remove(u);
                }
                return a;
            }
        }
    }
}