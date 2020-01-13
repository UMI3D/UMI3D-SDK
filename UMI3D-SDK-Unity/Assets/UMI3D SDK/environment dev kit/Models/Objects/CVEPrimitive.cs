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


namespace umi3d.edk
{
    /// <summary>
    /// 3D Basic Primitives
    /// </summary>
    public class CVEPrimitive : AbstractObject3D<PrimitiveDto>
    {
        #region CVE description
        public bool lockColliders = false;

        /// <summary>
        /// Type of the collider generated in front end.
        /// </summary>
        public ColliderType colliderType = ColliderType.Auto;

        /// <summary>
        /// In case of a mesh collider, should it be convex ?
        /// </summary>
        public bool convex = false;

        /// <summary>
        /// Primitive type.
        /// </summary>
        public MeshPrimitive primitive = MeshPrimitive.Cube;

        [SerializeField]
        private CVEMaterial material;
        /// <summary>
        /// Object's material.
        /// </summary>
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

        /// <summary>
        /// Object's material at last frame.
        /// </summary>
        private CVEMaterial previousMaterial;
        CVEVideo video { get { return GetComponent<CVEVideo>(); } }


        /// <summary>
        /// Initialise internal components.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();

            Material.initDefinition();
            Material.addListener(PropertiesHandler);

            if (video)
            {
                video.initDefinition();
                video.addListener(PropertiesHandler);
            }

        }

        protected override void SyncProperties()
        {
            base.SyncProperties();
            SyncMaterialProperties();
        }

        void SyncMaterialProperties()
        {
            if (inited)
            {
                Material.SyncMaterialProperties();
            }
            SyncMeshRenderer();
        }

        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override PrimitiveDto CreateDto()
        {
            return new PrimitiveDto();
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for.</param>
        /// <returns></returns>
        public override PrimitiveDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.Primitive = (int)primitive;
            dto.material = Material.ToDto(user);
            dto.video = video ? video.ToDto(user) : null;
            dto.colliderType = colliderType;
            dto.convex = convex;
            return dto;
        }


        public void SyncMeshRenderer()
        {
            try
            {
                if (!preview) return;

                MeshRenderer renderer = preview.GetComponent<MeshRenderer>();
                if (renderer == null)
                    return;

                if (!Material)
                    Material = CVEMaterial.DefaultMaterial;


                LoadMaterial(renderer);

                Material.SyncRenderer(renderer);
                if (video)
                    video.SyncVideo();
            }
            catch (Exception e)
            {
                return;
            }
        }

        public void LoadMaterial(MeshRenderer renderer)
        {
            if(renderer)
                renderer.material = Material.GetMaterial();
        }

        #endregion

        #region Unity implementation

        void Init_Preview()
        {
            if (!preview)
            {
                preview = GameObject.CreatePrimitive(primitive.Convert());
                preview.transform.SetParent(transform,false);
            }
        }

        protected new void Start()
        {
            base.Start();

            Init_Preview();
            previousMaterial = material;
        }

        protected new void Update()
        {
            base.Update();
            if (previousMaterial != material)
            {
                SyncMeshRenderer();
                PropertiesHandler.NotifyUpdate();
                previousMaterial = material;
            }

        }
        #endregion

    }

}
