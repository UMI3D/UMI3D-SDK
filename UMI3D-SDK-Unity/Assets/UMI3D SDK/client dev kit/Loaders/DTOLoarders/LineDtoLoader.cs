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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;
using VolumetricLines;

namespace umi3d.cdk
{
    public class LineDtoLoader : AbstractObjectDTOLoader<LineDto>
    {

        [SerializeField]
        private Shader _lineShader = null;

        protected Shader lineShader { get { return _lineShader == null ? UMI3DBrowser.Scene.defaultLineShader : _lineShader; } }

        public override void LoadDTO(LineDto dto, Action<GameObject> callback)
        {
            GameObject res = new GameObject();
            var r = res.AddComponent<LineRenderer>();
            r.material = new Material(lineShader);
            callback(res);
            InitObjectFromDto(res, dto);
        }

        public override void UpdateFromDTO(GameObject go, LineDto olddto, LineDto newdto)
        {
            var lineRenderer = go.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = newdto.Points.Count;
                lineRenderer.loop = newdto.Loop;
                for(var i = 0; i < newdto.Points.Count; i++)
                    lineRenderer.SetPosition(i,newdto.Points[i]);
                lineRenderer.material.SetColor("_Color", newdto.Color);
                lineRenderer.startWidth = newdto.Width;
                lineRenderer.endWidth = newdto.Width;
                lineRenderer.useWorldSpace = false;
            }
            base.UpdateFromDTO(go, olddto, newdto);
        }

    }
}