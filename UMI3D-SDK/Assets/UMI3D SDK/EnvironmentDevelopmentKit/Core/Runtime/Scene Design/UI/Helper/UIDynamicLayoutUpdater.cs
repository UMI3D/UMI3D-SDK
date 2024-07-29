/*
Copyright 2019 - 2024 Inetum

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

using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Update components on a dynamically managed layout.
    /// </summary>
    /// Add this component on the top of a hierarchy that contains a layoutGroup.
    /// Related to a issue of Unity LayoutGroup that are totally defined only two frames after start.
    [RequireComponent(typeof(UIRect))]
    public class UIDynamicLayoutUpdater : MonoBehaviour
    {
        private void Start()
        {
            UpdateDynamicLayoutProperties();
        }

        /// <summary>
        /// Update components on a dynamically managed layout.
        /// </summary>
        public void UpdateDynamicLayoutProperties()
        {
            var rectLayouts = GetComponentsInChildren<UIRect>();

            foreach (var layout in rectLayouts)
            {
                layout.useLayoutGrouping = true;
            }

            // layout groups are initialized 2 frames after start
            StartCoroutine(WaitForLayoutGroupInitialization(rectLayouts));
        }

        private IEnumerator WaitForLayoutGroupInitialization(UIRect[] rectLayouts)
        {
            foreach (var layout in rectLayouts)
            {
                int startingFrame = Time.frameCount;
                yield return new WaitUntil(() => Time.frameCount > startingFrame + 2);
                layout.objectPosition.SetValue(layout.rectTransform.localPosition);
                layout.AnchoredPosition.SetValue(layout.rectTransform.anchoredPosition);
                layout.AnchoredPosition.SetValue(layout.rectTransform.anchoredPosition);
                layout.AnchoredPosition.SetValue(layout.rectTransform.anchoredPosition);
                layout.AnchoredPosition3D.SetValue(layout.rectTransform.anchoredPosition3D);
                layout.AnchorMin.SetValue(layout.rectTransform.anchorMin);
                layout.AnchorMax.SetValue(layout.rectTransform.anchorMax);
                layout.Pivot.SetValue(layout.rectTransform.pivot);
                layout.OffsetMax.SetValue(layout.rectTransform.offsetMax);
                layout.OffsetMin.SetValue(layout.rectTransform.offsetMin);
                layout.SizeDelta.SetValue(layout.rectTransform.sizeDelta);
            }
        }
    }
}