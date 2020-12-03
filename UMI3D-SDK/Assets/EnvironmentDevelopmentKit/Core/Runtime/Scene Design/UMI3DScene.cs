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
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// UMI3D empty object.
    /// </summary>
    [DisallowMultipleComponent]
    [SelectionBase]
    public class UMI3DScene : UMI3DAbstractNode
    {

        #region properties
        [EditorReadOnly]
        public List<AssetLibrary> libraries;
        #endregion
        List<UMI3DNode> nodes;

        #region initialization

        /// <summary>
        /// Initialize scene's properties.
        /// </summary>
        protected override void InitDefinition(string id)
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
            GlTFSceneDto dto = new GlTFSceneDto();
            dto.name = gameObject.name;
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
            var dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        /// <summary>
        /// Writte the UMI3DNode properties in an object UMI3DNodeDto is assignable from.
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
            nodeDto.LibrariesId = libraries.Select(l => { return l.Id(); }).ToList();
            nodeDto.otherEntities = nodes.SelectMany(n => n.GetAllLoadableEntityUnderThisNode(user)).Select(e => e.ToEntityDto(user)).ToList();
            nodeDto.otherEntities.AddRange(GetAllLoadableEntityUnderThisNode(user).Select(e => e.ToEntityDto(user)));
        }


        //Remember already added entities
        [HideInInspector]
        public List<string> materialIds = new List<string>();
        [HideInInspector]
        public List<string> animationIds = new List<string>();

        [EditorReadOnly]
        public List<MaterialSO> materialSOs = new List<MaterialSO>();
        [EditorReadOnly]
        public List<MaterialSO> PreloadedMaterials = new List<MaterialSO>();


        /// <summary>
        /// Writte the scene contents in a GlTFSceneDto.
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
                IEnumerable<GlTFMaterialDto> materials = node.GetGlTFMaterialsFor(user).Where(m => !(materialIds).Contains(((AbstractEntityDto)m.extensions.umi3d).id));


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

        ///<inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToGlTFNodeDto(user);
        }

    }

}

