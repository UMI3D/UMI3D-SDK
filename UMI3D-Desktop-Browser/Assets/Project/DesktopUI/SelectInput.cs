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
    public class SelectInput : ListItem
    {

        

        //internal fields
        Image background;
        Dropdown dropdown;
        Action<string> action;

        // Unity behaviour
        void Awake()
        {
            background = GetComponent<Image>();
            dropdown = GetComponentInChildren<Dropdown>(true);
        }


        public void SetEvent(Action<string> action)
        {
            this.action = action;
            dropdown.onValueChanged.RemoveAllListeners();
            if (action != null) {
                dropdown.onValueChanged.AddListener((int val) => {
                    action.Invoke(dropdown.options[val].text);
                });}
        }

        public void SetValue(string val)
        {
            Dropdown.OptionData current = null;
            foreach (var option in dropdown.options)
            {
                if (val == option.text)
                    current = option;
            }
            if (current != null)
                dropdown.Set(dropdown.options.IndexOf(current));
            else
                dropdown.Set(-1);
        }

        public void SetOptions(IEnumerable<string> options)
        {
            dropdown.options.Clear();
            Dropdown.OptionData current = null;
            foreach (var val in options)
            {
                var option = new Dropdown.OptionData(val);
                dropdown.options.Add(option);
            }
            //dropdown.Set(-1);
        }

    }
}