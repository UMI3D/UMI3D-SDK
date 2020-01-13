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
using UnityEngine;
using umi3d.common;
using System;
using UnityEngine.UI;

namespace umi3d.cdk {

    /// <summary>
    /// Load UIRect from Dto.
    /// </summary>
    public class UIRectDtoLoader : AbstractObjectDTOLoader<UIRectDto>
    {
        /// <summary>
        /// Create UIRect from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="dto">Dto to create object from</param>
        /// <param name="callback">Callback to execute</param>
        public override void LoadDTO(UIRectDto dto, Action<GameObject> callback)
        {
            GameObject instance = new GameObject();
            instance.AddComponent<RectTransform>();
            InitObjectFromDto(instance, dto);
            callback(instance);
        }

        /// <summary>
        /// Update UIRect from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="go">GameObject containing the UIRect to update</param>
        /// <param name="olddto">Old dto</param>
        /// <param name="newdto">Dto to update the UIRect to</param>
        public override void UpdateFromDTO(GameObject go, UIRectDto olddto, UIRectDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);

            RectTransform dest = go.GetComponent<RectTransform>();
            dest.pivot = newdto.pivot;
            dest.anchorMax = newdto.anchorMax;
            dest.anchorMin = newdto.anchorMin;
            dest.offsetMax = newdto.offsetMax;
            dest.offsetMin = newdto.offsetMin;
            dest.sizeDelta = newdto.sizeDelta;
            dest.anchoredPosition = newdto.anchoredPosition;
            dest.anchoredPosition3D = newdto.anchoredPosition3D;

            var mask = dest.GetComponent<RectMask2D>();
            if (mask == null && newdto.rectMask)
                dest.gameObject.AddComponent<RectMask2D>();
            else if (mask != null && !newdto.rectMask)
                GameObject.Destroy(mask);

        }

    }
}