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
using UnityEngine.UI;
using umi3d.cdk.menu.view;

namespace BrowserDesktop.Menu
{
    /// <summary>
    /// 2D Displayer for range input.
    /// </summary>
    public class RangeInputDisplayer2D : AbstractRangeInputDisplayer, IMenuItemAnchor
    {

        /// <summary>
        /// Slider.
        /// </summary>
        public Slider slider;
        public TMPro.TextMeshProUGUI label;
        public TMPro.TMP_InputField sliderValue;

        Vector3 anchor;
        Vector3 direction;

        /// <summary>
        /// Frame rate applied to message emission through network (high values can cause network flood).
        /// </summary>
        public float networkFrameRate = 30;

        /// <summary>
        /// Launched coroutine for network message sending (if any).
        /// </summary>
        /// <see cref="networkMessageSender"/>
        protected Coroutine messageSenderCoroutine;

        protected bool valueChanged = false;

        protected IEnumerator networkMessageSender()
        {
            while (true)
            {
                if (valueChanged)
                {
                    NotifyValueChange(slider.value);
                    valueChanged = false;
                }
                yield return new WaitForSeconds(1f / networkFrameRate);
            }
        }


        Vector2 size = Vector2.zero;

        public void Set(Vector3 anchor, Vector3 direction)
        {
            this.anchor = anchor;
            this.direction = direction;
            size = Vector2.one;
            RectTransform rect = GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            LayoutRebuilder.MarkLayoutForRebuild(rect);
            rect.localPosition = ComputePosition(rect);
        }

        private void Update()
        {
            RectTransform rect = GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            if (size != this.size)
            {
                this.size = size;
                rect.localPosition = ComputePosition(rect);
            }
        }

        Vector3 ComputePosition(RectTransform rect)
        {
            Vector3 projection = new Vector3(Vector2.Dot(direction, Vector2.right), Vector2.Dot(direction, Vector2.up));
            if (projection.x < 0.5f && projection.x > -0.5f) projection.x = 0;
            else projection.x = Mathf.Sign(projection.x) * size.x / 2;
            if (projection.y < 0.5f && projection.y > -0.5f) projection.y = 0;
            else projection.y = Mathf.Sign(projection.y) * size.y / 2;
            return anchor + projection;
        }

        public override void Clear()
        {
            slider.onValueChanged.RemoveAllListeners();
            StopAllCoroutines();
        }

        /// <summary>
        /// Display the range input.
        /// </summary>
        public override void Display(bool forceUpdate = false)
        {
            slider.gameObject.SetActive(true);
            if (menuItem.continuousRange)
            {
                slider.minValue = menuItem.min;
                slider.maxValue = menuItem.max;
                slider.wholeNumbers = false;
            }
            else
            {
                slider.minValue = 0;
                slider.maxValue = (menuItem.max - menuItem.min) / menuItem.increment;
                slider.wholeNumbers = true;
            }
            label.text = menuItem.ToString();
            slider.value = menuItem.GetValue();
            sliderValue.text = FormatValue(slider.value);
            slider.onValueChanged.AddListener((i) => { valueChanged = true; sliderValue.text = FormatValue(slider.value); });
            sliderValue.onEndEdit.AddListener((i) => { float f = 0; if (float.TryParse(i, out f) && slider.value != f) { valueChanged = true; slider.value = f; sliderValue.text = FormatValue(slider.value); } });
            if (gameObject.activeInHierarchy)
                messageSenderCoroutine = StartCoroutine(networkMessageSender());
        }

        string FormatValue(float f)
        {
            return string.Format("{0:###0.##}", f);
        }


        /// <summary>
        /// Hide the range input.
        /// </summary>
        public override void Hide()
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.gameObject.SetActive(false);
            StopCoroutine(messageSenderCoroutine);
        }
    }
}