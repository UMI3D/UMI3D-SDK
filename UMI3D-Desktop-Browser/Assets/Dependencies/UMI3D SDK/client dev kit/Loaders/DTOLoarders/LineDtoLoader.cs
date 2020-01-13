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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class LineDtoLoader : AbstractObjectDTOLoader<LineDto>
    {

        [SerializeField]
        private Shader _lineShader = null;

        protected Shader lineShader { get { return _lineShader == null ? UMI3DBrowser.Scene.defaultLineShader : _lineShader; } }

        /// <summary>
        /// Create an Line from an LineDto and raise a given callback.
        /// </summary>
        /// <param name="dto">Line to load</param>
        /// <param name="callback">Callback to raise (the argument is the Line GameObject)</param>
        public override void LoadDTO(LineDto dto, Action<GameObject> callback)
        {
            GameObject res = new GameObject();
            var r = res.AddComponent<LineRenderer>();
            r.material = new Material(lineShader);
            callback(res);
            InitObjectFromDto(res, dto);
        }


        /// <summary>
        /// Update a Transform of a Line from dto.
        /// </summary>
        /// <param name="go">Line gameObject to update</param>
        /// <param name="olddto">Previous dto describing the Line</param>
        /// <param name="newdto">Dto to update the Line to</param>
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