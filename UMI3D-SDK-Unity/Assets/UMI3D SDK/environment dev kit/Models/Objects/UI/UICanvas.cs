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

        float dynamicPixelsPerUnit { get { return GetComponent<CanvasScaler>().dynamicPixelsPerUnit; } }
        float referencePixelsPerUnit { get { return GetComponent<CanvasScaler>().referencePixelsPerUnit; } }
        int orderInLayer { get { return GetComponent<Canvas>().sortingOrder; } }

        /// <summary>
        /// Dynamic pixels per unit.
        /// </summary>
        public UMI3DAsyncProperty<float> DPU;

        /// <summary>
        /// Reference pixels per unit.
        /// </summary>
        public UMI3DAsyncProperty<float> RPU;

        /// <summary>
        /// Order In Layer.
        /// </summary>
        public UMI3DAsyncProperty<int> OIL;

        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();
            if (autoCascade)
            {
                Cascade();
            }
            DPU = new UMI3DAsyncProperty<float>(PropertiesHandler, dynamicPixelsPerUnit);
            RPU = new UMI3DAsyncProperty<float>(PropertiesHandler, referencePixelsPerUnit);
            OIL = new UMI3DAsyncProperty<int>(PropertiesHandler, orderInLayer);

        }

        /// <summary>
        /// Update properties.
        /// </summary>
        protected override void SyncProperties()
        {
            base.SyncProperties();
            if (inited)
            {
                DPU.SetValue(dynamicPixelsPerUnit);
                RPU.SetValue(referencePixelsPerUnit);
                OIL.SetValue(orderInLayer);

            }
        }

        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override UIRectDto CreateDto()
        {
            return new UICanvasDto();
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override UIRectDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user) as UICanvasDto;
            dto.dynamicPixelsPerUnit = DPU.GetValue(user);
            dto.referencePixelsPerUnit = RPU.GetValue(user);
            dto.orderInLayer = OIL.GetValue(user);

            return dto;
        }

        /// <summary>
        /// Convert in children every UI components (Text, Image, RectTransform) to UMI3D UI (UIText, UIImage, UIRect)
        /// </summary>
        /// <see cref="UIText"/>
        /// <see cref="UIImage"/>
        /// <see cref="UIRect"/>
        public void Cascade()
        {
            var texts = GetComponentsInChildren<Text>(true).Where(txt => txt.GetComponent<UIText>() == null);
            foreach (var t in texts)
                t.gameObject.AddComponent<UIText>();

            var images = GetComponentsInChildren<Image>(true).Where(img => img.GetComponent<UIImage>() == null);
            foreach (var i in images)
                i.gameObject.AddComponent<UIImage>();

            var rects = GetComponentsInChildren<RectTransform>(true).Where(rect => rect.GetComponent<UIRect>() == null);
            foreach (var r in rects)
                r.gameObject.AddComponent<UIRect>();
        }

    }
}