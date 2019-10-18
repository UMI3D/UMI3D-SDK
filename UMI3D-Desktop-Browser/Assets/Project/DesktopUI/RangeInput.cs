using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopUI
{
    public class RangeInput : ListItem
    {


        [Space(10)]
        [Header("sub components")]
        [SerializeField] Text _valueLabel;

        //internal fields
        Image background;
        CustomSlider slider;



        void Sync()
        {
            if (_valueLabel != null && slider != null)
                _valueLabel.text = slider.value.ToString();
        }


        // Unity behaviour
        void Awake()
        {
            background = GetComponent<Image>();
            slider = GetComponentInChildren<CustomSlider>(true);
            Sync();
        }

        void OnValidate()
        {
            if (slider == null)
                slider = GetComponentInChildren<CustomSlider>(true);
            Sync();
        }

        public void SetEvent(Action<float> action)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float val) => action.Invoke(val));
        }

        public void SetValue(float val)
        {
            slider.SetValue(val, false);
            if (_valueLabel != null)
                _valueLabel.text = slider.value.ToString();
        }

        public void SetRange(float min, float max)
        {
            slider.maxValue = max;
            slider.minValue = min;
        }

        public void SetWholeNumber(bool wholeNumbers)
        {
            slider.wholeNumbers = wholeNumbers;
        }

    }
}
