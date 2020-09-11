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

    public class UMI3DSceneLoader : UMI3DAbstractNodeLoader
    {
        UMI3DEnvironmentLoader EnvironementLoader;

        public UMI3DSceneLoader(UMI3DEnvironmentLoader EnvironementLoader) {
            this.EnvironementLoader = EnvironementLoader;
        }

        /// <summary>
        /// Create a GLTFScene based on a GLTFSceneDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="finished"></param>
        public void LoadGlTFScene(GlTFSceneDto dto, System.Action finished, System.Action<int> LoadedNodesCount)
        {
            GameObject go = new GameObject(dto.name);
            UMI3DEnvironmentLoader.RegisterNodeInstance(dto.extensions.umi3d.id, dto, go);
            go.transform.SetParent(EnvironementLoader.transform);
            //Load Materials
            LoadSceneMaterials(dto, ()=> { EnvironementLoader.StartCoroutine(EnvironementLoader.nodeLoader.LoadNodes(dto.nodes, finished, LoadedNodesCount)); });
            //Load Nodes
       //     EnvironementLoader.StartCoroutine(EnvironementLoader.nodeLoader.LoadNodes(dto.nodes, finished, LoadedNodesCount));
        }

        /// <summary>
        /// Setup a scene node based on a UMI3DSceneNodeDto
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dto"></param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {
            base.ReadUMI3DExtension(dto, node,()=>
            {
                var sceneDto = dto as UMI3DSceneNodeDto;
                if (sceneDto == null) return;
                node.transform.localPosition = sceneDto.position;
                node.transform.localRotation = sceneDto.rotation;
                node.transform.localScale = sceneDto.scale;
                foreach (var library in sceneDto.LibrariesId)
                    UMI3DResourcesManager.LoadLibrary(library, null, sceneDto.id);
                int count = 0;
                if (sceneDto.otherEntities != null)
                {
                    foreach (var entity in sceneDto.otherEntities)
                    {
                        count++;
                        UMI3DEnvironmentLoader.LoadEntity(entity, () => { count--; if (count == 0) finished.Invoke(); });
                    }
                }

                if (count == 0)
                    finished.Invoke();
            },failed);
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property))
                return true;
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;
            UMI3DSceneNodeDto dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DSceneNodeDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.Position:
                    node.transform.localPosition = dto.position = (SerializableVector3)property.value;
                    break;
                case UMI3DPropertyKeys.Rotation:
                    node.transform.localRotation = dto.rotation = (SerializableVector4)property.value;
                    break;
                case UMI3DPropertyKeys.Scale:
                    node.transform.localScale = dto.scale = (SerializableVector3)property.value;
                    break;
                default:
                    return false;
            }
            return true;
        }


        public void LoadSceneMaterials(GlTFSceneDto dto, Action callback)
        {
            foreach (GlTFMaterialDto material in dto.materials)
            {
                try
                {
                    //question:
                    /*      comment on choisit le shader?
                          combien de type de matériaux (Que 2 et c est fixe ou prévoir plus ou N)?
                          les loaders de mat en brut dans le loader de scene on je fais un loader par type de mat ?
                          */

                    /*var newmat = */
                    EnvironementLoader.materialLoader.LoadMaterialFromExtension(material, (m) =>
                    {
                        m.name = material.name;
                        //register le mat
                        UMI3DEntityInstance entity = UMI3DEnvironmentLoader.RegisterEntityInstance(material.extensions.umi3d.id, material.extensions.umi3d, m);
                        //entity.Object = m;
                    }
                    );

                }
                catch
                {
                    Debug.LogError("this material failed to load : " + material.name);
                }
            }
            callback.Invoke();
        }

      


    }

}