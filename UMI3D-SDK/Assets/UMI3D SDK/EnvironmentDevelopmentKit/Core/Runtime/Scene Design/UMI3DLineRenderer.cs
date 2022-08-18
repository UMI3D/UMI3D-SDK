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
    public partial class UMI3DLineRenderer : AbstractRenderedNode
    {
        public LineRenderer lineRenderer;

        public UMI3DAsyncProperty<Color> objectStartColor { get { Register(); return _objectStartColor; } protected set => _objectStartColor = value; }
        public UMI3DAsyncProperty<Color> objectEndColor { get { Register(); return _objectEndColor; } protected set => _objectEndColor = value; }
        public UMI3DAsyncProperty<float> objectStartWidth { get { Register(); return _objectStartWidth; } protected set => _objectStartWidth = value; }
        public UMI3DAsyncProperty<float> objectEndWidth { get { Register(); return _objectEndWidth; } protected set => _objectEndWidth = value; }
        public UMI3DAsyncProperty<bool> objectLoop { get { Register(); return _objectLoop; } protected set => _objectLoop = value; }
        public UMI3DAsyncProperty<bool> objectUseWorldSpace { get { Register(); return _objectUseWorldSpace; } protected set => _objectUseWorldSpace = value; }

        public UMI3DAsyncListProperty<Vector3> objectPositions { get { Register(); return _objectPositions; } protected set => _objectPositions = value; }

        /// <summary>
        /// Color of the line.
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected SerializableColor startColor = Color.white;

        /// <summary>
        /// Color of the line.
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected SerializableColor endColor = Color.white;

        /// <summary>
        /// line width on first point
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected float endWidth = 0.01f;

        /// <summary>
        /// line width on last point
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected float startWidth = 0.01f;

        /// <summary>
        /// If true, a line will be draw between the first and the last point.
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected bool loop = false;

        /// <summary>
        /// Draw line in world space
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected bool useWorldSpace = false;

        /// <summary>
        /// The position of points
        /// </summary>
        [SerializeField, EditorReadOnly]
        protected List<Vector3> positions = new List<Vector3>();

        private UMI3DAsyncProperty<Color> _objectStartColor;
        private UMI3DAsyncProperty<Color> _objectEndColor;
        private UMI3DAsyncProperty<bool> _objectUseWorldSpace;
        private UMI3DAsyncProperty<bool> _objectLoop;
        private UMI3DAsyncProperty<float> _objectStartWidth;
        private UMI3DAsyncProperty<float> _objectEndWidth;
        private UMI3DAsyncListProperty<Vector3> _objectPositions;

        ///<inheritdoc/>
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

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
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

        ///<inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(startColor)
                + UMI3DNetworkingHelper.Write(endColor)
                + UMI3DNetworkingHelper.Write(loop)
                + UMI3DNetworkingHelper.Write(useWorldSpace)
                + UMI3DNetworkingHelper.Write(startWidth)
                + UMI3DNetworkingHelper.Write(endWidth)
                + UMI3DNetworkingHelper.Write(positions);
        }
    }
}
