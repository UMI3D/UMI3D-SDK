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
    /// UMI3D scene description.
    /// </summary>
    [DisallowMultipleComponent]
    [SelectionBase]
    public class UMI3DScene : UMI3DAbstractNode
    {

        #region fields
        /// <summary>
        /// Libraries required for the access to the scene.
        /// </summary>
        [EditorReadOnly, Tooltip("Libraries required for the access to the scene.")]
        public List<AssetLibrary> libraries;

        private List<UMI3DNode> nodes;
        #endregion


        #region initialization

        /// <summary>
        /// Initialize scene's properties.
        /// </summary>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
        }

        #endregion

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected virtual UMI3DSceneNodeDto CreateDto()
        {
            return new UMI3DSceneNodeDto();
        }

        /// <summary>
        /// Convert to GlTFNodeDto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public virtual GlTFSceneDto ToGlTFNodeDto(UMI3DUser user)
        {
            //SyncProperties();
            var dto = new GlTFSceneDto
            {
                name = gameObject.name
            };
            nodes = GetAllChildrenInThisScene(user);
            dto.extensions.umi3d = ToUMI3DSceneNodeDto(user);
            WriteCollections(dto, user);

            nodes.Clear();
            return dto;
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected virtual UMI3DSceneNodeDto ToUMI3DSceneNodeDto(UMI3DUser user)
        {
            UMI3DSceneNodeDto dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
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
            var nodeDto = dto as UMI3DSceneNodeDto;
            if (nodeDto == null) return;
            nodeDto.position = objectPosition.GetValue(user);
            nodeDto.scale = objectScale.GetValue(user);
            nodeDto.rotation = objectRotation.GetValue(user);
            nodeDto.LibrariesId = libraries.Select(l => { return l.id; }).ToList();
            nodeDto.otherEntities = nodes.SelectMany(n => n.GetAllLoadableEntityUnderThisNode(user)).Select(e => e.ToEntityDto(user)).ToList();
            nodeDto.otherEntities.AddRange(GetAllLoadableEntityUnderThisNode(user).Select(e => e.ToEntityDto(user)));
        }

        /// <inheritdoc/>
        public override Bytable ToBytes(UMI3DUser user)
        {
            Bytable fp = base.ToBytes(user);
            var otherEntities = nodes.SelectMany(n => n.GetAllLoadableEntityUnderThisNode(user)).Select(o => o.ToBytes(user)).ToList();
            otherEntities.AddRange(GetAllLoadableEntityUnderThisNode(user).Select(o => o.ToBytes(user)));
            Bytable f = otherEntities.Aggregate((a, b) => { return a + b; });

            Vector3 position = objectPosition.GetValue(user);
            Vector3 scale = objectScale.GetValue(user);
            Quaternion rotation = objectRotation.GetValue(user);
            var LibrariesId = libraries.Select(l => { return l.id; }).ToList();

            return
                base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(position)
                + UMI3DNetworkingHelper.Write(scale)
                + UMI3DNetworkingHelper.Write(rotation)
                + UMI3DNetworkingHelper.Write(LibrariesId)
                + f;
        }

        //Remember already added entities
        /// <summary>
        /// UMI3D ids of <see cref="MaterialSO"/> that are required for the scene.
        /// </summary>
        [HideInInspector]
        public List<ulong> materialIds = new List<ulong>();
        /// <summary>
        /// UMI3D ids of <see cref="UMI3DAbstractAnimation"/> that are required for the scene.
        /// </summary>
        [HideInInspector]
        public List<ulong> animationIds = new List<ulong>();

        /// <summary>
        /// <see cref="MaterialSO"/> that are required for the scene.
        /// </summary>
        [EditorReadOnly, Tooltip("Materials required for the scene.")]
        public List<MaterialSO> materialSOs = new List<MaterialSO>();
        /// <summary>
        /// <see cref="MaterialSO"/> that are required for the scene.
        /// </summary>
        [EditorReadOnly, Tooltip("Materials required for the scene.")]
        public List<MaterialSO> PreloadedMaterials = new List<MaterialSO>();


        /// <summary>
        /// Write the scene contents in a GlTFSceneDto.
        /// </summary>
        /// <param name="scene">The GlTFSceneDto with any properties except content arrays ready</param>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        protected virtual void WriteCollections(GlTFSceneDto scene, UMI3DUser user)
        {
            //Clear materials lists
            materialIds.Clear();
            animationIds.Clear();

            materialIds.AddRange(PreloadedMaterials.Select(m => ((AbstractEntityDto)m.ToDto().extensions.umi3d).id));
            materialSOs.AddRange(PreloadedMaterials);
            scene.materials.AddRange(PreloadedMaterials.Select(m => m.ToDto()));

            //Fill arrays
            foreach (UMI3DNode node in nodes)
            {

                //Add nodes in the glTF scene
                scene.nodes.Add(node.ToGlTFNodeDto(user));

                //Get new materials
                IEnumerable<GlTFMaterialDto> materials = node.GetGlTFMaterialsFor(user).Where(m => !materialIds.Contains(((AbstractEntityDto)m.extensions.umi3d).id));


                //Add them to the glTF scene
                scene.materials.AddRange(materials);
                materialSOs = UMI3DEnvironment.GetEntities<MaterialSO>().ToList();

                //remember their ids
                materialIds.AddRange(materials.Select(m => ((AbstractEntityDto)m.extensions.umi3d).id));

                //Get new animations
                IEnumerable<UMI3DAbstractAnimationDto> animations = node.GetAnimationsFor(user).Where(a => !animationIds.Contains(a.id));
                //Add them to the glTF scene
                scene.extensions.umi3d.animations.AddRange(animations);
                //remember their ids
                animationIds.AddRange(animations.Select(a => a.id));
            }
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToGlTFNodeDto(user);
        }
    }
}

