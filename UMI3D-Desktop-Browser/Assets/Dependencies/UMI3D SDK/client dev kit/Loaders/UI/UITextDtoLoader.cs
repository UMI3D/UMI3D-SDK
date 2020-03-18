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
using UnityEngine.UI;

namespace umi3d.cdk
{
    /// <summary>
    /// Load UIText from Dto.
    /// </summary>
    public class UITextDtoLoader : UIRectDtoLoader
    {

        /// <summary>
        /// Create UIText from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="dto">Dto to create object from</param>
        /// <param name="callback">Callback to execute</param>
        public override void LoadDTO(UIRectDto dto, System.Action<GameObject> onSuccess, System.Action<string> onError)
        {
            base.LoadDTO(dto, gameObject =>
            {
                gameObject.AddComponent<Text>();
                InitObjectFromDto(gameObject, dto);
                onSuccess(gameObject);
            }, 
            onError);            
            
        }

        /// <summary>
        /// Update UIText from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="go">GameObject containing the UIText to update</param>
        /// <param name="olddto">Old dto</param>
        /// <param name="newdto">Dto to update the UIText to</param>
        public override void UpdateFromDTO(GameObject go, UIRectDto olddto, UIRectDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);

            var label = go.GetComponent<Text>();
            if (label != null)
            {
                label.alignment = ((UITextDto)newdto).alignment;
                label.alignByGeometry = ((UITextDto)newdto).alignByGeometry;
                label.color = ((UITextDto)newdto).color;
                label.font = Resources.GetBuiltinResource<Font>(((UITextDto)newdto).font+".ttf");
                label.fontSize = ((UITextDto)newdto).fontSize;
                label.fontStyle = ((UITextDto)newdto).fontStyle;
                label.horizontalOverflow = ((UITextDto)newdto).horizontalOverflow;
                label.lineSpacing = ((UITextDto)newdto).lineSpacing;
                label.resizeTextForBestFit = ((UITextDto)newdto).resizeTextForBestFit;
                label.resizeTextMaxSize = ((UITextDto)newdto).resizeTextMaxSize;
                label.resizeTextMinSize = ((UITextDto)newdto).resizeTextMinSize;
                label.supportRichText = ((UITextDto)newdto).supportRichText;
                label.text = ((UITextDto)newdto).text;
                label.verticalOverflow = ((UITextDto)newdto).verticalOverflow;
            }
        }
    }
}