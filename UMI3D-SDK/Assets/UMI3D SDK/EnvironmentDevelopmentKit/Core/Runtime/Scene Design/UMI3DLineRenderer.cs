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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Rendered line.
    /// </summary>
    public partial class UMI3DLineRenderer : AbstractRenderedNode
    {
        /// <summary>
        /// Unity line renderer.
        /// </summary>
        public LineRenderer lineRenderer;

        #region asyncproperties

        /// <summary>
        /// See <see cref="startColor"/>.
        /// </summary>
        public UMI3DAsyncProperty<Color> objectStartColor { get { Register(); return _objectStartColor; } protected set => _objectStartColor = value; }
        /// <summary>
        /// See <see cref="endColor"/>.
        /// </summary
        public UMI3DAsyncProperty<Color> objectEndColor { get { Register(); return _objectEndColor; } protected set => _objectEndColor = value; }
        /// <summary>
        /// See <see cref="startWidth"/>.
        /// </summary
        public UMI3DAsyncProperty<float> objectStartWidth { get { Register(); return _objectStartWidth; } protected set => _objectStartWidth = value; }
        /// <summary>
        /// See <see cref="endWidth"/>.
        /// </summary
        public UMI3DAsyncProperty<float> objectEndWidth { get { Register(); return _objectEndWidth; } protected set => _objectEndWidth = value; }
        /// <summary>
        /// See <see cref="loop"/>.
        /// </summary
        public UMI3DAsyncProperty<bool> objectLoop { get { Register(); return _objectLoop; } protected set => _objectLoop = value; }
        /// <summary>
        /// See <see cref="useWorldSpace"/>.
        /// </summary
        public UMI3DAsyncProperty<bool> objectUseWorldSpace { get { Register(); return _objectUseWorldSpace; } protected set => _objectUseWorldSpace = value; }
        /// <summary>
        /// See <see cref="positions"/>.
        /// </summary
        public UMI3DAsyncListProperty<Vector3> objectPositions { get { Register(); return _objectPositions; } protected set => _objectPositions = value; }

        #endregion asyncproperties

        /// <summary>
        /// Color of the line.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Color of the start of the line.")]
        protected SerializableColor startColor = Color.white;

        /// <summary>
        /// Color of the line.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Color of the end of the line.")]
        protected SerializableColor endColor = Color.white;

        /// <summary>
        /// Line width on last point
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Line width on last point.")]
        protected float endWidth = 0.01f;

        /// <summary>
        /// line width on first point
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Line width on first point.")]
        protected float startWidth = 0.01f;

        /// <summary>
        /// If true, a line will be draw between the first and the last point.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("If true, a line will be draw between the first and the last point.")]
        protected bool loop = false;

        /// <summary>
        /// If true, draw line in world space.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("If true, draw line in world space.")]
        protected bool useWorldSpace = false;

        /// <summary>
        /// The position of points
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected List<Vector3> positions = new List<Vector3>();

        #region asyncproperties

        /// <summary>
        /// See <see cref="startColor"/>.
        /// </summary
        private UMI3DAsyncProperty<Color> _objectStartColor;
        /// <summary>
        /// See <see cref="endColor"/>.
        /// </summary
        private UMI3DAsyncProperty<Color> _objectEndColor;
        /// <summary>
        /// See <see cref="useWorldSpace"/>.
        /// </summary
        private UMI3DAsyncProperty<bool> _objectUseWorldSpace;
        /// <summary>
        /// See <see cref="loop"/>.
        /// </summary
        private UMI3DAsyncProperty<bool> _objectLoop;
        /// <summary>
        /// See <see cref="startWidth"/>.
        /// </summary
        private UMI3DAsyncProperty<float> _objectStartWidth;
        /// <summary>
        /// See <see cref="endWidth"/>.
        /// </summary
        private UMI3DAsyncProperty<float> _objectEndWidth;
        /// <summary>
        /// See <see cref="positions"/>.
        /// </summary
        private UMI3DAsyncListProperty<Vector3> _objectPositions;

        #endregion asyncproperties

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            objectLoop = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.LineLoop, lineRenderer == null ? loop : lineRenderer.loop, (r, u) => r);
            objectLoop.OnValueChanged += b => loop = b;
            objectUseWorldSpace = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.LineUseWorldSpace, lineRenderer == null ? useWorldSpace : lineRenderer.useWorldSpace, (r, u) => r);
            objectUseWorldSpace.OnValueChanged += b => useWorldSpace = b;
            objectStartColor = new UMI3DAsyncProperty<Color>(objectId, UMI3DPropertyKeys.LineStartColor, lineRenderer == null ? (Color)startColor : lineRenderer.startColor, (c, u) => ToUMI3DSerializable.ToSerializableColor(c, u));
            objectStartColor.OnValueChanged += c => startColor = c;
            objectEndColor = new UMI3DAsyncProperty<Color>(objectId, UMI3DPropertyKeys.LineEndColor, lineRenderer == null ? (Color)endColor : lineRenderer.endColor, (c, u) => ToUMI3DSerializable.ToSerializableColor(c, u));
            objectEndColor.OnValueChanged += c => endColor = c;

            objectStartWidth = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.LineStartWidth, lineRenderer == null ? startWidth : lineRenderer.startWidth, (f, u) => f);
            objectStartWidth.OnValueChanged += f => startWidth = f;
            objectEndWidth = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.LineEndWidth, lineRenderer == null ? endWidth : lineRenderer.endWidth, (f, u) => f);
            objectEndWidth.OnValueChanged += f => endWidth = f;

            if (lineRenderer != null)
            {
                SyncPositionsFromLineRenderer();
            }
            objectPositions = new UMI3DAsyncListProperty<Vector3>(objectId, UMI3DPropertyKeys.LinePositions, positions, (v, u) => ToUMI3DSerializable.ToSerializableVector3(v, u));
            objectPositions.OnValueChanged += b => positions = b;


        }

        public void SyncPositionsFromLineRenderer()
        {
            var tab = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(tab);
            positions = tab.ToList();
        }

        /// <inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DLineDto();
        }

        /// <summary>
        /// Write the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
        /// </summary>
        /// <param name="scene">The UMI3DNodeDto to be completed</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var lineDto = dto as UMI3DLineDto;
            lineDto.startColor = objectStartColor.GetValue(user);
            lineDto.endColor = objectEndColor.GetValue(user);
            lineDto.loop = objectLoop.GetValue(user);
            lineDto.useWorldSpace = objectUseWorldSpace.GetValue(user);
            lineDto.startWidth = objectStartWidth.GetValue(user);
            lineDto.endWidth = objectEndWidth.GetValue(user);
            lineDto.positions = objectPositions.GetValue(user).ConvertAll(vector => ToUMI3DSerializable.ToSerializableVector3(vector, user));
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DSerializer.Write(startColor)
                + UMI3DSerializer.Write(endColor)
                + UMI3DSerializer.Write(loop)
                + UMI3DSerializer.Write(useWorldSpace)
                + UMI3DSerializer.Write(startWidth)
                + UMI3DSerializer.Write(endWidth)
                + UMI3DSerializer.Write(positions);
        }
    }
}
