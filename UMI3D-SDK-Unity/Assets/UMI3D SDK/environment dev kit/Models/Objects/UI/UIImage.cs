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
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.edk
{
    /// <summary>
    /// UI Image
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIImage : UIRect
    {
        /// <summary>
        /// Image's color.
        /// </summary>
        Color color { get { return GetComponent<Image>().color; } }

        /// <summary>
        /// Image's type
        /// </summary>
        Image.Type type { get { return GetComponent<Image>().type; } }

        /// <summary>
        /// Sprite url (if any).
        /// </summary>
        public CVEResource sprite = null;

        /// <summary>
        /// Image's color.
        /// </summary>
        public UMI3DAsyncProperty<Color> Color;

        /// <summary>
        /// Image's type.
        /// </summary>
        public UMI3DAsyncProperty<Image.Type> ImageType;


        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();
            Color = new UMI3DAsyncProperty<Color>(PropertiesHandler, color);
            ImageType = new UMI3DAsyncProperty<Image.Type>(PropertiesHandler, type);
            if (sprite != null)
            {
                sprite.initDefinition();
                sprite.addListener(PropertiesHandler);
            }
        }


        /// <summary>
        /// Update properties.
        /// </summary>
        protected override void SyncProperties()
        {
            base.SyncProperties();
            if (inited)
            {
                Color.SetValue(color);
                ImageType.SetValue(type);
            }
        }


        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override UIRectDto CreateDto()
        {
            return new UIImageDto();
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override UIRectDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user) as UIImageDto;
            dto.color = Color.GetValue(user);
            dto.type = ImageType.GetValue(user);
            if (sprite != null)
                dto.sprite = sprite.ToDto(user);
            return dto;
        }

    }
}
