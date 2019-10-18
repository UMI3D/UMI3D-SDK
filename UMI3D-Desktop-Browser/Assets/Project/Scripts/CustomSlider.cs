using UnityEngine;
using UnityEngine.UI;

public class CustomSlider:Slider
{
    public void SetValue( float val , bool callEvent = true)
    {
        Set(val, callEvent);
    }
}

