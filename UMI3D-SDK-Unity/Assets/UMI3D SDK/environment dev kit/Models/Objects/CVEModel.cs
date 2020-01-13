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
using umi3d.common;
using UnityEngine;


namespace umi3d.edk
{
    public class CVEModel : AbstractObject3D<ModelDto>
    {
        [Obsolete("will be removed soon")]
        public bool lockColliders = false;

        /// <summary>
        /// Type of the collider generated in front end.
        /// </summary>
        public ColliderType colliderType = ColliderType.Auto;

        /// <summary>
        /// In case of a mesh collider, should it be convex ?
        /// </summary>
        public bool convex = false;

        public CVEResource objResource = new CVEResource();

        public bool overrideModelMaterial = false;

        [Serializable]
        public class MaterialMemory
        {
            public MeshRenderer renderer;
            public Material material;
        }
        public List<MaterialMemory> Renderers = new List<MaterialMemory>();

        [SerializeField]
        private CVEMaterial material;
        public CVEMaterial Material
        {
            get {
                if (!material)
                    try
                    {
                        material = CVEMaterial.DefaultMaterial;
                    }
                    catch { }
                return material;
            }

            set
            {
                if (material != null)
                    material.removeListener(PropertiesHandler);
                material = value;
                Material.initDefinition();
                Material.addListener(PropertiesHandler);
            }
        }
        CVEVideo video { get { return GetComponent<CVEVideo>(); } }

        public UMI3DAsyncProperty<bool> objectMaterialOverrided;
        
        protected override void initDefinition()
        {
            base.initDefinition();

            objectMaterialOverrided = new UMI3DAsyncProperty<bool>(PropertiesHandler, overrideModelMaterial);
            objectMaterialOverrided.OnValueChanged += (bool value) => overrideModelMaterial = value;
            objectMaterialOverrided.OnValueChanged += (bool value) => SyncMeshRenderer();

            Material.initDefinition();
            Material.addListener(PropertiesHandler);

            if (video)
            {
                video.initDefinition();
                video.addListener(PropertiesHandler);
            }

            objResource.initDefinition();
            objResource.addListener(PropertiesHandler);
        }
        
        protected override void SyncProperties()
        {
            if (Material == null)
                Material = CVEMaterial.DefaultMaterial;

            base.SyncProperties();
            objResource.SyncProperties();
            SyncMaterialProperties();
        }

        void SyncMaterialProperties()
        {
            if (inited)
            {
                objectMaterialOverrided.SetValue(overrideModelMaterial);

                Material.SyncMaterialProperties();
            }
            SyncMeshRenderer();
        }

        public override ModelDto CreateDto()
        {
            return new ModelDto();
        }

        public override ModelDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.OverrideModelMaterial = objectMaterialOverrided.GetValue(user);

            
            dto.material = Material.ToDto(user);
            dto.video = video ? video.ToDto(user) : null;
            dto.objResource = objResource.ToDto(user);
            dto.colliderType = colliderType;
            dto.convex = convex;

            return dto;
        }

        public void SyncMeshRenderer()
        {
            try
            {
                var mat = Material.GetMaterial();
                foreach (var mem in Renderers)
                {
                    MeshRenderer renderer = mem.renderer;
                    if (objectMaterialOverrided.GetValue())
                    {
                        renderer.sharedMaterial = mat;
                        Material.SyncRenderer(renderer);
                    }
                    else
                    {
                        renderer.sharedMaterial = mem.material;
                    }
                }
                if (video) video.SyncVideo();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
        }

        public void LoadMaterial(MeshRenderer renderer)
        {
            if(renderer)
                renderer.sharedMaterial = Material.GetMaterial();
        }
    }

}
