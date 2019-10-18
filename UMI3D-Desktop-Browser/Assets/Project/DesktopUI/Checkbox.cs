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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopUI
{
    public class Checkbox : ListItem
    {


        [SerializeField] bool value = false;
        //[SerializeField] Sprite on;
        [SerializeField] Image _onImage;
        [SerializeField] Transform end = null;

        //internal fields
        Image background;
        Action<bool> onChange = null;

        void ToggleCollapse( bool value)
        {
            var index = transform.GetSiblingIndex();
            var endindex = end.GetSiblingIndex();
            foreach(Transform t in transform.parent)
            {
                var i = t.GetSiblingIndex();
                if (i > index && i < endindex)
                    t.gameObject.SetActive(value);
            }
        }

        void Toggle()
        {
            value = !value;
            _onImage.gameObject.SetActive(value);
            if (end != null)
                ToggleCollapse(value);
            if (onChange != null)
                onChange(value);
        }

        // Unity behaviour
        void Awake()
        {
            background = GetComponent<Image>();
            selectBtn.onClick.AddListener(Toggle);
        }

        public void SetEvent(Action<bool> action)
        {
            onChange = action;
        }

        public void SetValue(bool val)
        {
            value = val;
            _onImage.gameObject.SetActive(value);
            if (end != null)
                ToggleCollapse(value);
        }

    }
}