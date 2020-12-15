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
    public class UIRect : UMI3DNode
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
        public UMI3DAsyncProperty<Vector2> AnchoredPosition { get { Register(); return _anchoredPosition; } protected set => _anchoredPosition = value; }

        ///<see cref="anchoredPosition3D"/>
        public UMI3DAsyncProperty<Vector3> AnchoredPosition3D { get { Register(); return _anchoredPosition3D; } protected set => _anchoredPosition3D = value; }

        ///<see cref="anchorMax"/>
        public UMI3DAsyncProperty<Vector2> AnchorMax { get { Register(); return _anchorMax; } protected set => _anchorMax = value; }

        ///<see cref="anchorMin"/>
        public UMI3DAsyncProperty<Vector2> AnchorMin { get { Register(); return _anchorMin; } protected set => _anchorMin = value; }

        ///<see cref="offsetMax"/>
        public UMI3DAsyncProperty<Vector2> OffsetMax { get { Register(); return _offsetMax; } protected set => _offsetMax = value; }

        ///<see cref="offsetMin"/>
        public UMI3DAsyncProperty<Vector2> OffsetMin { get { Register(); return _offsetMin; } protected set => _offsetMin = value; }

        ///<see cref="pivot"/>
        public UMI3DAsyncProperty<Vector2> Pivot { get { Register(); return _pivot; } protected set => _pivot = value; }

        ///<see cref="sizeDelta"/>
        public UMI3DAsyncProperty<Vector2> SizeDelta { get { Register(); return _sizeDelta; } protected set => _sizeDelta = value; }

        ///<see cref="rectMask"/>
        public UMI3DAsyncProperty<bool> RectMask { get { Register(); return _rectMask; } protected set => _rectMask = value; }

        UMI3DAsyncPropertyEquality equality = new UMI3DAsyncPropertyEquality();

        private UMI3DAsyncProperty<Vector2> _anchoredPosition;
        private UMI3DAsyncProperty<Vector3> _anchoredPosition3D;
        private UMI3DAsyncProperty<Vector2> _anchorMax;
        private UMI3DAsyncProperty<Vector2> _anchorMin;
        private UMI3DAsyncProperty<Vector2> _offsetMax;
        private UMI3DAsyncProperty<Vector2> _offsetMin;
        private UMI3DAsyncProperty<Vector2> _pivot;
        private UMI3DAsyncProperty<Vector2> _sizeDelta;
        private UMI3DAsyncProperty<bool> _rectMask;

        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void InitDefinition(string id)
        {
            base.InitDefinition(id);
            AnchoredPosition = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.AnchoredPosition, anchoredPosition, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            AnchoredPosition3D = new UMI3DAsyncProperty<Vector3>(objectId, UMI3DPropertyKeys.AnchoredPosition3D, anchoredPosition3D, ToUMI3DSerializable.ToSerializableVector3, equality.Vector3Equality);
            AnchorMax = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.AnchorMax, anchorMax, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            AnchorMin = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.AnchorMin, anchorMin, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            OffsetMax = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.OffsetMax, offsetMax, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            OffsetMin = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.OffsetMin, offsetMin, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            Pivot = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.Pivot, pivot, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            SizeDelta = new UMI3DAsyncProperty<Vector2>(objectId, UMI3DPropertyKeys.SizeDelta, sizeDelta, ToUMI3DSerializable.ToSerializableVector2, equality.Vector2Equality);
            RectMask = new UMI3DAsyncProperty<bool>(objectId, UMI3DPropertyKeys.RectMask, rectMask);
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UIRectDto();
        }

        ///<inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            UIRectDto rectDto = dto as UIRectDto;
            rectDto.anchoredPosition = AnchoredPosition.GetValue(user);
            rectDto.anchoredPosition3D = AnchoredPosition3D.GetValue(user);
            rectDto.anchorMax = AnchorMax.GetValue(user);
            rectDto.anchorMin = AnchorMin.GetValue(user);
            rectDto.offsetMax = OffsetMax.GetValue(user);
            rectDto.offsetMin = OffsetMin.GetValue(user);
            rectDto.pivot = Pivot.GetValue(user);
            rectDto.sizeDelta = SizeDelta.GetValue(user);
            rectDto.rectMask = RectMask.GetValue(user);
        }
    }
}
