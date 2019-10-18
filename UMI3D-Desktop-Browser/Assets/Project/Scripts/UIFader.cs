using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIFader : MonoBehaviour {

    public float duration = 2f;
    Image image;
    float T = 0;

    // Use this for initialization
    void OnEnable () {
        image = GetComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
        T = Time.realtimeSinceStartup;
	}
	
	void Update()
    {
        var coeff = Mathf.Min( (Time.realtimeSinceStartup - T) / duration , 1f);
        //var alpha = 1f - Mathf.Abs(0.5f - coeff) * 2f;
        image.color = new Color(0, 0, 0, 1f - coeff);
        if(coeff == 1f)
            gameObject.SetActive(false);
    }
}
