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
    /// UI RectTransform
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIRect : AbstractObject3D<UIRectDto>
    {
        /// <summary>
        /// Pivot position relative to the anchor.
        /// </summary>
        Vector2 anchoredPosition { get { return GetComponent<RectTransform>().anchoredPosition; } }

        /// <summary>
        /// 3D pivot position relative to the anchor.
        /// </summary>
        Vector3 anchoredPosition3D { get { return GetComponent<RectTransform>().anchoredPosition3D; } }

        /// <summary>
        /// Normalized position of the upper right corner in the parent UIRect.
        /// </summary>
        Vector2 anchorMax { get { return GetComponent<RectTransform>().anchorMax; } }

        /// <summary>
        /// NNormalized position of the lower left corner in the parent UIRect.
        /// </summary>
        Vector2 anchorMin { get { return GetComponent<RectTransform>().anchorMin; } }

        /// <summary>
        /// Distance between the upper right corner of this rectangle and the upper right anchor.
        /// </summary>
        Vector2 offsetMax { get { return GetComponent<RectTransform>().offsetMax; } }

        /// <summary>
        /// Distance between the lower left corner of this rectangle and the lower left anchor.
        /// </summary>
        Vector2 offsetMin { get { return GetComponent<RectTransform>().offsetMin; } }

        /// <summary>
        /// Pivot point.
        /// </summary>
        Vector2 pivot { get { return GetComponent<RectTransform>().pivot; } }

        /// <summary>
        /// Relative size of the rectangle relative to the distance between upper right and lower left anchors.
        /// </summary>
        Vector2 sizeDelta { get { return GetComponent<RectTransform>().sizeDelta; } }

        
        bool rectMask { get { return GetComponent<RectMask2D>() != null; } }


        ///<see cref="anchoredPosition"/>
        public UMI3DAsyncProperty<Vector2> AnchoredPosition;

        ///<see cref="anchoredPosition3D"/>
        public UMI3DAsyncProperty<Vector3> AnchoredPosition3D;

        ///<see cref="anchorMax"/>
        public UMI3DAsyncProperty<Vector2> AnchorMax;

        ///<see cref="anchorMin"/>
        public UMI3DAsyncProperty<Vector2> AnchorMin;

        ///<see cref="offsetMax"/>
        public UMI3DAsyncProperty<Vector2> OffsetMax;

        ///<see cref="offsetMin"/>
        public UMI3DAsyncProperty<Vector2> OffsetMin;

        ///<see cref="pivot"/>
        public UMI3DAsyncProperty<Vector2> Pivot;

        ///<see cref="sizeDelta"/>
        public UMI3DAsyncProperty<Vector2> SizeDelta;

        ///<see cref="rectMask"/>
        public UMI3DAsyncProperty<bool> RectMask;


        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();
            AnchoredPosition = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, anchoredPosition);
            AnchoredPosition3D = new UMI3DAsyncProperty<Vector3>(PropertiesHandler, anchoredPosition3D);
            AnchorMax = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, anchorMax);
            AnchorMin = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, anchorMin);
            OffsetMax = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, offsetMax);
            OffsetMin = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, offsetMin);
            Pivot = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, pivot);
            SizeDelta = new UMI3DAsyncProperty<Vector2>(PropertiesHandler, sizeDelta);
            RectMask = new UMI3DAsyncProperty<bool>(PropertiesHandler, rectMask);
        }

        /// <summary>
        /// Update properties.
        /// </summary>
        protected override void SyncProperties()
        {
            base.SyncProperties();
            if (inited)
            {
                AnchoredPosition.SetValue(anchoredPosition);
                AnchoredPosition3D.SetValue(anchoredPosition3D);
                AnchorMax.SetValue(anchorMax);
                AnchorMin.SetValue(anchorMin);
                OffsetMax.SetValue(offsetMax);
                OffsetMin.SetValue(offsetMin);
                Pivot.SetValue(pivot);
                SizeDelta.SetValue(sizeDelta);
                RectMask.SetValue(rectMask);
            }
        }

        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override UIRectDto CreateDto()
        {
            return new UIRectDto();
        }

        /// <summary>
        /// Convert to dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override UIRectDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.anchoredPosition = AnchoredPosition.GetValue(user);
            dto.anchoredPosition3D = AnchoredPosition3D.GetValue(user);
            dto.anchorMax = AnchorMax.GetValue(user);
            dto.anchorMin = AnchorMin.GetValue(user);
            dto.offsetMax = OffsetMax.GetValue(user);
            dto.offsetMin = OffsetMin.GetValue(user);
            dto.pivot = Pivot.GetValue(user);
            dto.sizeDelta = SizeDelta.GetValue(user);
            dto.rectMask = RectMask.GetValue(user);
            return dto;
        }

        protected override void Update()
        {
            base.Update();
            SyncProperties();
        }

    }
}
