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