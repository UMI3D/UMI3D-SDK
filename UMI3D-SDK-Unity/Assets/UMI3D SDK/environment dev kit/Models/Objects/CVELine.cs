/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;


namespace umi3d.edk
{
    public class CVELine : AbstractObject3D<LineDto>
    {
        public bool loop = false;
        public float width = 0.01f;
        public List<Vector3> points = new List<Vector3>();
        public Color color = Color.gray;
        
        public UMI3DAsyncProperty<float> lineWidth;
        public UMI3DAsyncProperty<bool> lineLoop;
        public UMI3DAsyncProperty<List<Vector3>> linePoints;
        public UMI3DAsyncProperty<Color> lineColor;
        
        public override LineDto CreateDto()
        {
            return new LineDto();
        }

        protected override void initDefinition()
        {
            base.initDefinition();

            lineWidth = new UMI3DAsyncProperty<float>(PropertiesHandler, width);
            lineWidth.OnValueChanged += (float value) => width = value;

            lineLoop = new UMI3DAsyncProperty<bool>(PropertiesHandler, loop);
            lineLoop.OnValueChanged += (bool value) => loop = value;

            lineColor = new UMI3DAsyncProperty<Color>(PropertiesHandler, color);
            lineColor.OnValueChanged += (Color value) => color = value;

            linePoints = new UMI3DAsyncProperty<List<Vector3>>(PropertiesHandler, points);
            linePoints.OnValueChanged += (List<Vector3> value) => points = new List<Vector3> (value);
        }
        
        protected override void SyncProperties()
        {
            base.SyncProperties();
            if (inited)
            {
                lineLoop.SetValue(loop);
                lineWidth.SetValue(width);
                linePoints.SetValue(new List<Vector3>(points));
            }
            SyncPreview();
        }

        public void SyncPreview()
        {
            var lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            if(lineRenderer.sharedMaterial == null)
                lineRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Color"));
            lineRenderer.positionCount = points.Count;
            lineRenderer.loop = loop;
            lineRenderer.SetPositions(points.ToArray());
            if(lineRenderer.sharedMaterial != null)
                lineRenderer.sharedMaterial.SetColor("_Color", color);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = false;
        }

        public override LineDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.Color = lineColor.GetValue(user);
            dto.Loop = lineLoop.GetValue(user);
            dto.Width = lineWidth.GetValue(user);
            foreach (var p in linePoints.GetValue(user))
                dto.Points.Add(p);
            return dto;
        }
        
    }

}
