using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static bool isUI = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isUI = true;
        Debug.Log("enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isUI = false;
    }

    private void OnDisable()
    {
        isUI = false;
    }

    private void OnDestroy()
    {
        isUI = false;
    }

}

