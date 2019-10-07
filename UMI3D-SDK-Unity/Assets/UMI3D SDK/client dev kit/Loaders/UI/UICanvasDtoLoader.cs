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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;
using UnityEngine.UI;
using System;

namespace umi3d.cdk
{
    public class UICanvasDtoLoader : UIRectDtoLoader
    {
        /// <summary>
        /// Create UICanvas from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="dto">Dto to create object from</param>
        /// <param name="callback">Callback to execute</param>
        public override void LoadDTO(UIRectDto dto, System.Action<GameObject> callback)
        {
            base.LoadDTO(dto, gameObject =>
            {
                gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<CanvasScaler>();
                InitObjectFromDto(gameObject, dto);
                callback(gameObject);
            });
        }

        /// <summary>
        /// Update UICanvas from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="go">GameObject containing the UICanvas to update</param>
        /// <param name="olddto">Old dto</param>
        /// <param name="newdto">Dto to update the UICanvas to</param>
        public override void UpdateFromDTO(GameObject go, UIRectDto olddto, UIRectDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);

            if (go.GetComponent<Canvas>() != null)
                go.GetComponent<Canvas>().sortingOrder = ((UICanvasDto)newdto).orderInLayer;

            var scaler = go.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.dynamicPixelsPerUnit = ((UICanvasDto)newdto).dynamicPixelsPerUnit;
                scaler.referencePixelsPerUnit = ((UICanvasDto)newdto).referencePixelsPerUnit;
            }
        }
    }
}