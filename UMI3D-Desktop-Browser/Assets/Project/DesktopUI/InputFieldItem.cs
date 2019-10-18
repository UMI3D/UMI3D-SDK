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
