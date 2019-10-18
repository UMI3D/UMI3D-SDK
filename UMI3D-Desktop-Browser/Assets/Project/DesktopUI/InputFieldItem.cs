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
    public class InputFieldItem : ListItem
    {

        //internal fields
        Image background;
        InputField field;

        // Unity behaviour
        void Awake()
        {
            background = GetComponent<Image>();
            field = GetComponentInChildren<InputField>(true);
        }

        public void SetEvent(Action<string> action)
        {
            if (field == null)
                field = GetComponentInChildren<InputField>(true);
            field.onValueChanged.RemoveAllListeners();
            field.onValueChanged.AddListener((string val) => action.Invoke(val));
        }

        public void SetValue(string val)
        {
            if (field == null)
                field = GetComponentInChildren<InputField>(true);
            field.text = val;
        }

        public void SetContentType(InputField.ContentType type)
        {
            if (field == null)
                field = GetComponentInChildren<InputField>(true);
            field.contentType = type;
        }

    }
}
