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

namespace umi3d.cdk
{
    /// <summary>
    /// Load UIImage from Dto.
    /// </summary>
    public class UIImageDtoLoader : UIRectDtoLoader
    {

        /// <summary>
        /// Create UIImage from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="dto">Dto to create object from</param>
        /// <param name="callback">Callback to execute</param>
        public override void LoadDTO(UIRectDto dto, System.Action<GameObject> callback)
        {
            base.LoadDTO(dto, gameObject =>
            {
                gameObject.AddComponent<Image>();
                InitObjectFromDto(gameObject, dto);
                callback(gameObject);
            });
        }

        /// <summary>
        /// Update UIImage from Dto, and pass it to a given callback.
        /// </summary>
        /// <param name="go">GameObject containing the UIImage to update</param>
        /// <param name="olddto">Old dto</param>
        /// <param name="newdto">Dto to update the UIImage to</param>
        public override void UpdateFromDTO(GameObject go, UIRectDto olddto, UIRectDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);

            var image = go.GetComponent<Image>();
            if (image != null)
            {
                ResourceDto spriteDto = ((UIImageDto)newdto).sprite;
                if (spriteDto != null)
                {
                    if ((spriteDto.Url != null) && (spriteDto.Url != ""))
                    {
                        HDResourceCache.Download(((UIImageDto)newdto).sprite, tex =>
                        {
                            if (tex != null)
                            {
                                image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to load texture from " + spriteDto.Url);
                            }
                        });
                    }
                }
                image.color = ((UIImageDto)newdto).color;
                image.type = ((UIImageDto)newdto).type;
            }

        }
    }
}
