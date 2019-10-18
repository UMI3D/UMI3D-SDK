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