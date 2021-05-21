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
using System;
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
        /// Image's sprite url
        /// </summary>
        [SerializeField, EditorReadOnly]
        public UMI3DResource sprite = new UMI3DResource();

        private UMI3DAsyncProperty<UMI3DResource> _sprite;
        private UMI3DAsyncProperty<Color> _color;
        private UMI3DAsyncProperty<Image.Type> _imageType;

        /// <summary>
        /// Sprite url (if any).
        /// </summary>
        public UMI3DAsyncProperty<UMI3DResource> Sprite { get { Register(); return _sprite; } protected set => _sprite = value; }

        /// <summary>
        /// Image's color.
        /// </summary>
        public UMI3DAsyncProperty<Color> Color { get { Register(); return _color; } protected set => _color = value; }

        /// <summary>
        /// Image's type.
        /// </summary>
        public UMI3DAsyncProperty<Image.Type> ImageType { get { Register(); return _imageType; } protected set => _imageType = value; }


        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void InitDefinition(ulong Id)
        {
            base.InitDefinition(Id);
            Color = new UMI3DAsyncProperty<Color>(objectId, UMI3DPropertyKeys.ImageColor, color, ToUMI3DSerializable.ToSerializableColor);
            ImageType = new UMI3DAsyncProperty<Image.Type>(objectId, UMI3DPropertyKeys.ImageType, type, (a, u) => a.Convert());
            Sprite = new UMI3DAsyncProperty<UMI3DResource>(objectId, UMI3DPropertyKeys.Image, sprite, (s, u) => s.ToDto());
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UIImageDto();
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            UIImageDto rectDto = dto as UIImageDto;
            rectDto.color = Color.GetValue(user);
            rectDto.type = ImageType.GetValue(user).Convert();
            if (sprite != null)
                rectDto.sprite = Sprite.GetValue().ToDto();
        }

        public override (int, Func<byte[], int, int>) ToBytes(UMI3DUser user)
        {
            var fp = base.ToBytes(user);
            var fi = Sprite.GetValue().ToByte();
            var c = Color.GetValue(user);

            int size = sizeof(int)
                + UMI3DNetworkingHelper.GetSize(c)
                + fp.Item1
                + fi.Item1;
            Func<byte[], int, int> func = (b, i) => {
                i += fp.Item2(b, i);
                i += UMI3DNetworkingHelper.Write(c, b, i);
                i += UMI3DNetworkingHelper.Write((int)ImageType.GetValue(user).Convert(), b, i);
                i += fi.Item2(b, i);
                return size;
            };
            return (size, func);
        }

    }
}
