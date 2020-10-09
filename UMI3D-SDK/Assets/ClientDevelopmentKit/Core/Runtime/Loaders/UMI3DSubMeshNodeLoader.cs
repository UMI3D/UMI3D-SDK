using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;


namespace umi3d.cdk
{
    public class UMI3DSubMeshNodeLoader : AbstractRenderedNodeLoader
    {


        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {

            base.ReadUMI3DExtension(dto, node, () =>
            {
                var nodeDto = dto as SubModelDto;
                if (nodeDto != null)
                {

                    string sub = nodeDto.id.Split(new string[] { "==_[" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    sub = sub.Remove(sub.Length - 1);
                    UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode((nodeDto).modelId);
                    GlTFNodeDto modelDto = (GlTFNodeDto)nodeInstance.dto;

                    string modelInCache = UMI3DEnvironmentLoader.Parameters.ChooseVariante(((UMI3DMeshNodeDto)modelDto.extensions.umi3d).mesh.variants).url;

                    UMI3DMeshNodeDto rootDto = (UMI3DMeshNodeDto)modelDto.extensions.umi3d;
                    var rootGO = UMI3DEnvironmentLoader.GetNode((nodeDto).pid).gameObject;
                    GameObject instance = null;
                    if (UMI3DResourcesManager.Instance.subModelsCache.ContainsKey(modelInCache))
                    {
                        instance = GameObject.Instantiate(UMI3DResourcesManager.Instance.subModelsCache[modelInCache][sub].gameObject, node.gameObject.transform);
                        if (rootDto.applyCustomMaterial)
                        {
                            // apply root model override
                            SetMaterialOverided(rootDto, instance);
                        }
                        if (nodeDto.applyCustomMaterial)
                        {
                            SetMaterialOverided(nodeDto, instance);
                            // apply sub model overrider
                        }

                    }
                    else
                    {
                        UMI3DResourcesManager.Instance.GetSubModel(modelInCache, sub, (o) =>
                        {

                            instance = GameObject.Instantiate((GameObject)o, node.gameObject.transform, true);
                            AbstractMeshDtoLoader.ShowModelRecursively(instance);
                            instance.transform.localPosition = Vector3.zero;
                            instance.transform.localEulerAngles = new Vector3(0, 180, 0);
                            instance.transform.localScale = Vector3.one;
                            SetCollider(UMI3DEnvironmentLoader.GetNode(nodeDto.id), ((UMI3DNodeDto)dto).colliderDto);

                            if (rootDto.applyCustomMaterial)
                            {
                                // apply root model override
                                SetMaterialOverided(rootDto, instance);
                            }
                            if (nodeDto.applyCustomMaterial)
                            {
                                SetMaterialOverided(nodeDto, instance);
                                // apply sub model overrider
                            }

                        });
                    }
                    finished?.Invoke();
                }
                else failed?.Invoke("nodeDto should not be null");
            }, failed);
        }


        protected override void RevertToOriginalMaterial(UMI3DNodeInstance entity)
        {

            Renderer renderer = entity.gameObject.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return;

            UMI3DMeshNodeDto parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(((SubModelDto)((GlTFNodeDto)entity.dto).extensions.umi3d).modelId).dto).extensions.umi3d;

            OldMaterialContainer oldMaterialContainer = renderer.gameObject.GetComponent<OldMaterialContainer>();
            if (oldMaterialContainer != null)
            {
                Material[] oldMats = oldMaterialContainer.oldMats;
                Material[] matsToApply = renderer.sharedMaterials;
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (oldMats[i] != null)
                    {
                        matsToApply[i] = (oldMats[i]);
                    }
                }
                renderer.materials = matsToApply;
                if (parentDto.applyCustomMaterial)
                {
                    SetMaterialOverided(parentDto, entity.gameObject);
                }
            }
        }

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if ((entity?.dto as GlTFNodeDto)?.extensions?.umi3d is SubModelDto)

                return base.SetUMI3DProperty(entity, property);
            else
                return false;
        }


    }
}
