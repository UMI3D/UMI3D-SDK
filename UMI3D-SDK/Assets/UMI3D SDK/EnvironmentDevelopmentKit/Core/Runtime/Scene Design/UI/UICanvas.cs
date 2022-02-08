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
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3d.edk
{
    /// <summary>
    /// UI Canvas
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class UICanvas : UIRect
    {
        /// <summary>
        /// If true, this component will on init convert every image, text and rectTransform to corresponding UMI3D component.
        /// </summary>
        /// <see cref="Cascade"/>
        public bool autoCascade = false;

        private float _dynamicPixelsPerUnit => GetComponent<CanvasScaler>().dynamicPixelsPerUnit;

        private float _referencePixelsPerUnit => GetComponent<CanvasScaler>().referencePixelsPerUnit;

        private int _orderInLayer => GetComponent<Canvas>().sortingOrder;

        /// <summary>
        /// Dynamic pixels per unit.
        /// </summary>
        public UMI3DAsyncProperty<float> DynamicPixelPerUnit { get { Register(); return dynamicPixelPerUnit; } protected set => dynamicPixelPerUnit = value; }

        /// <summary>
        /// Reference pixels per unit.
        /// </summary>
        public UMI3DAsyncProperty<float> ReferencePixelPerUnit { get { Register(); return referencePixelPerUnit; } protected set => referencePixelPerUnit = value; }

        /// <summary>
        /// Order In Layer.
        /// </summary>
        public UMI3DAsyncProperty<int> OrderInLayer { get { Register(); return orderInLayer; } protected set => orderInLayer = value; }

        private readonly UMI3DAsyncPropertyEquality equality = new UMI3DAsyncPropertyEquality();
        private UMI3DAsyncProperty<float> dynamicPixelPerUnit;
        private UMI3DAsyncProperty<float> referencePixelPerUnit;
        private UMI3DAsyncProperty<int> orderInLayer;

        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            if (autoCascade)
            {
                Cascade();
            }
            DynamicPixelPerUnit = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.DynamicPixelsPerUnit, _dynamicPixelsPerUnit, null, equality.FloatEquality);
            ReferencePixelPerUnit = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.ReferencePixelsPerUnit, _referencePixelsPerUnit, null, equality.FloatEquality);
            OrderInLayer = new UMI3DAsyncProperty<int>(objectId, UMI3DPropertyKeys.OrderInLayer, _orderInLayer);
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var canvasDto = dto as UICanvasDto;
            canvasDto.dynamicPixelsPerUnit = DynamicPixelPerUnit.GetValue(user);
            canvasDto.referencePixelsPerUnit = ReferencePixelPerUnit.GetValue(user);
            canvasDto.orderInLayer = OrderInLayer.GetValue(user);
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DNetworkingHelper.Write(DynamicPixelPerUnit.GetValue(user))
                + UMI3DNetworkingHelper.Write(ReferencePixelPerUnit.GetValue(user))
                + UMI3DNetworkingHelper.Write(OrderInLayer.GetValue(user));
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UICanvasDto();
        }

        /// <summary>
        /// Convert in children every UI components (Text, Image, RectTransform) to UMI3D UI (UIText, UIImage, UIRect)
        /// </summary>
        /// <see cref="UIText"/>
        /// <see cref="UIImage"/>
        /// <see cref="UIRect"/>
        public void Cascade()
        {
            System.Collections.Generic.IEnumerable<Text> texts = GetComponentsInChildren<Text>(true).Where(txt => txt.GetComponent<UIText>() == null);
            foreach (Text t in texts)
                t.gameObject.AddComponent<UIText>();

            System.Collections.Generic.IEnumerable<Image> images = GetComponentsInChildren<Image>(true).Where(img => img.GetComponent<UIImage>() == null);
            foreach (Image i in images)
                i.gameObject.AddComponent<UIImage>();

            System.Collections.Generic.IEnumerable<RectTransform> rects = GetComponentsInChildren<RectTransform>(true).Where(rect => rect.GetComponent<UIRect>() == null);
            foreach (RectTransform r in rects)
                r.gameObject.AddComponent<UIRect>();
        }

    }
}