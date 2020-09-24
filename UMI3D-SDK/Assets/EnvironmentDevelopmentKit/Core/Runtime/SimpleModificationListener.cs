﻿/*
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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.edk
{
    [Obsolete("This class isn't mean to be use in production", false)]
    public partial class SimpleModificationListener : MonoBehaviour
    {
        UMI3DNode[] nodes;
        UMI3DScene[] scenes;
        public float time = 0f;
        float timeTmp = 0;
        public int max = 0;

        Dictionary<string, Dictionary<string, SetEntityProperty>> sets;


        // Start is called before the first frame update
        void Start()
        {
            nodes = GetComponentsInChildren<UMI3DNode>();
            scenes = GetComponentsInChildren<UMI3DScene>();
        }

        // Update is called once per frame
        void Update()
        {
            if (UMI3DServer.Exists)
            {
                foreach (var node in nodes)
                    Update(node);

                foreach (var scene in scenes)
                    MaterialUpdate(scene);

                Dispatch();
            }
        }

        void Dispatch()
        {
            if (checkTime() || checkMax())
            {

                var transaction = new Transaction();
                transaction.Operations = sets.SelectMany(p => p.Value).Select(p => (Operation)p.Value).ToList();
                if (transaction.Operations.Count > 0)
                {
                    transaction.reliable = false;
                    UMI3DServer.Dispatch(transaction);
                    sets = new Dictionary<string, Dictionary<string, SetEntityProperty>>();
                }
            }
        }

        bool checkTime()
        {
            timeTmp -= Time.deltaTime;
            if (time == 0 || timeTmp <= 0)
            {
                timeTmp = time;
                return true;
            }
            return false;
        }

        bool checkMax()
        {
            return (max != 0) && (sets.SelectMany(p => p.Value).Count() > max);
        }

        private void Update(UMI3DNode obj)
        {
            if (sets == null) sets = new Dictionary<string, Dictionary<string, SetEntityProperty>>();
            if (!sets.ContainsKey(obj.Id())) sets[obj.Id()] = new Dictionary<string, SetEntityProperty>();

            setOperation(obj.objectPosition.SetValue(obj.transform.localPosition));
            setOperation(obj.objectRotation.SetValue(obj.transform.localRotation));
            setOperation(obj.objectScale.SetValue(obj.transform.localScale));
            setOperation(obj.objectXBillboard.SetValue(obj.xBillboard));
            setOperation(obj.objectYBillboard.SetValue(obj.yBillboard));

            UIUpdate(obj as UIRect);

            ModelUpdate(obj);

        }

        private void MaterialUpdate(UMI3DScene scene)
        {
            if (sets == null) sets = new Dictionary<string, Dictionary<string, SetEntityProperty>>();


            foreach (MaterialSO mat in scene.materialSOs)
            {
                if (!sets.ContainsKey(mat.Id())) sets[mat.Id()] = new Dictionary<string, SetEntityProperty>();
                if (mat as PBRMaterial)
                {
                    setOperation(((PBRMaterial)mat).objectBaseColorFactor.SetValue(((PBRMaterial)mat).baseColorFactor));
                    setOperation(((PBRMaterial)mat).objectEmissiveFactor.SetValue(((PBRMaterial)mat).emissive));
                    setOperation(((PBRMaterial)mat).objectEmissiveTexture.SetValue(((PBRMaterial)mat).textures.emissiveTexture));
                    setOperation(((PBRMaterial)mat).objectHeightTexture.SetValue(((PBRMaterial)mat).textures.heightTexture));
                    setOperation(((PBRMaterial)mat).objectHeightTextureScale.SetValue(((PBRMaterial)mat).textures.heightTexture.scale));
                    setOperation(((PBRMaterial)mat).objectMaintexture.SetValue(((PBRMaterial)mat).textures.baseColorTexture));
                    setOperation(((PBRMaterial)mat).objectMetallicFactor.SetValue(((PBRMaterial)mat).metallicFactor));
                    setOperation(((PBRMaterial)mat).objectMetallicRoughnessTexture.SetValue(((PBRMaterial)mat).textures.metallicRoughnessTexture));
                    setOperation(((PBRMaterial)mat).objectMetallicTexture.SetValue(((PBRMaterial)mat).textures.metallicTexture));
                    setOperation(((PBRMaterial)mat).objectNormalTexture.SetValue(((PBRMaterial)mat).textures.normalTexture));
                    setOperation(((PBRMaterial)mat).objectNormalTextureScale.SetValue(((PBRMaterial)mat).textures.normalTexture.scale));
                    setOperation(((PBRMaterial)mat).objectOcclusionTexture.SetValue(((PBRMaterial)mat).textures.occlusionTexture));
                    setOperation(((PBRMaterial)mat).objectRoughnessFactor.SetValue(((PBRMaterial)mat).roughnessFactor));
                    setOperation(((PBRMaterial)mat).objectRoughnessTexture.SetValue(((PBRMaterial)mat).textures.roughnessTexture));
                    setOperation(((PBRMaterial)mat).objectShaderProperties.SetValue(((PBRMaterial)mat).shaderProperties));
                    setOperation(((PBRMaterial)mat).objectTextureTilingOffset.SetValue(((PBRMaterial)mat).tilingOffset));
                    setOperation(((PBRMaterial)mat).objectTextureTilingScale.SetValue(((PBRMaterial)mat).tilingScale));
                }
                else
                {
                    Debug.LogWarning("unsupported material type");
                }
            }
        }

        private void ModelUpdate(UMI3DNode obj)
        {
            //setOperation(obj.hasCollider)
            // setOperation(obj.objectColliderBoxSize.SetValue(o)
            setOperation(obj.objectColliderRadius.SetValue(obj.colliderRadius));
            setOperation(obj.objectColliderCenter.SetValue(obj.colliderCenter));
            setOperation(obj.objectColliderBoxSize.SetValue(obj.colliderBoxSize));
            setOperation(obj.objectColliderDirection.SetValue(obj.colliderDirection));
            setOperation(obj.objectColliderHeight.SetValue(obj.colliderHeight));
            setOperation(obj.objectColliderType.SetValue(obj.colliderType));
            setOperation(obj.objectCustomMeshCollider.SetValue(obj.customMeshCollider));
            setOperation(obj.objectHasCollider.SetValue(obj.hasCollider));
            setOperation(obj.objectIsConvexe.SetValue(obj.convex));
            setOperation(obj.objectIsMeshCustom.SetValue(obj.isMeshCustom));
            //var model = obj as UMI3DModel;
            //if (model)
            //{
            //    setOperation(model.objectCastShadow.SetValue(model.gameObject.GetComponentInChildren<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On));
            //    setOperation(model.objectCastShadow.SetValue(model.gameObject.GetComponentInChildren<Renderer>().receiveShadows));
            //}
        }

        private void UIUpdate(UIRect obj)
        {
            if (obj != null)
            {
                var rectTransform = obj.GetComponent<RectTransform>();
                setOperation(obj.AnchoredPosition.SetValue(rectTransform.anchoredPosition));
                setOperation(obj.AnchoredPosition3D.SetValue(rectTransform.anchoredPosition3D));
                setOperation(obj.AnchorMax.SetValue(rectTransform.anchorMax));
                setOperation(obj.AnchorMin.SetValue(rectTransform.anchorMin));
                setOperation(obj.OffsetMax.SetValue(rectTransform.offsetMax));
                setOperation(obj.OffsetMin.SetValue(rectTransform.offsetMin));
                setOperation(obj.Pivot.SetValue(rectTransform.pivot));
                setOperation(obj.SizeDelta.SetValue(rectTransform.sizeDelta));
                setOperation(obj.RectMask.SetValue(obj.GetComponent<RectMask2D>() != null));
                if (obj is UICanvas)
                {
                    var canvas = obj as UICanvas;
                    var canvasScaler = obj.GetComponent<CanvasScaler>();
                    var Canvas = obj.GetComponent<Canvas>();
                    setOperation(canvas.DynamicPixelPerUnit.SetValue(canvasScaler.dynamicPixelsPerUnit));
                    setOperation(canvas.ReferencePixelPerUnit.SetValue(canvasScaler.referencePixelsPerUnit));
                    setOperation(canvas.OrderInLayer.SetValue(Canvas.sortingOrder));
                }
                if (obj is UIImage)
                {
                    var image = obj as UIImage;
                    var Image = obj.GetComponent<Image>();
                    setOperation(image.Color.SetValue(Image.color));
                    setOperation(image.ImageType.SetValue(Image.type));
                    //update sprite
                }

                if (obj is UIText)
                {
                    var text = obj as UIText;
                    var Text = obj.GetComponent<Text>();
                    setOperation(text.Alignment.SetValue(Text.alignment));
                    setOperation(text.AlignByGeometry.SetValue(Text.alignByGeometry));
                    setOperation(text.TextColor.SetValue(Text.color));
                    setOperation(text.TextFont.SetValue(Text.font));
                    setOperation(text.FontSize.SetValue(Text.fontSize));
                    setOperation(text.FontStyle.SetValue(Text.fontStyle));
                    setOperation(text.HorizontalOverflow.SetValue(Text.horizontalOverflow));
                    setOperation(text.VerticalOverflow.SetValue(Text.verticalOverflow));
                    setOperation(text.LineSpacing.SetValue(Text.lineSpacing));
                    setOperation(text.ResizeTextForBestFit.SetValue(Text.resizeTextForBestFit));
                    setOperation(text.ResizeTextMaxSize.SetValue(Text.resizeTextMaxSize));
                    setOperation(text.ResizeTextMinSize.SetValue(Text.resizeTextMinSize));
                    setOperation(text.SupportRichText.SetValue(Text.supportRichText));
                    setOperation(text.Text.SetValue(Text.text));
                }
            }
        }

        void setOperation(SetEntityProperty operation)
        {
            if (operation != null)
            {

                try
                {
                    sets[operation.entityId][operation.property] = operation;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Debug.Log("entityid = " + operation.entityId);
                    Debug.Log("property = " + operation.property);

                }

            }
        }

    }
}