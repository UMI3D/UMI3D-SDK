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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class HorizontalList : UnityEngine.UI.LayoutGroup
{

    public float lineHeight;
    public new Padding padding;
    public Vector2 spacing = Vector2.zero;

    [System.Serializable]
    public class Padding
    {
        public float top = 0f;
        public float left = 0f;
        public float right = 0f;
    }

	// Use this for initialization
	//void Start () {
    //    Refresh();
    //}
	
	void Refresh () {
        var rect = transform as RectTransform;
        var px = padding.left;
        var line = 0;
        var isfirst = true;
        var width = rect.rect.width;
        foreach(Transform child in rect)
        {
            var crect = child as RectTransform;
            var newTot = px + crect.rect.width;
            if (!isfirst)
                newTot += spacing.x;
            if (newTot + padding.right > width)
            {
                isfirst = true;
                px = padding.left;
                line++;
            }
            crect.sizeDelta = new Vector2(crect.rect.width, lineHeight - spacing.y);
            var newx = px + crect.rect.width / 2;
            if (!isfirst)
                newx += spacing.x;
            crect.localPosition = new Vector3(newx, - (line+0.5f) * lineHeight - padding.top, 0f);
            px += crect.rect.width;
            if (!isfirst)
                px += spacing.x;
            isfirst = false;
        }
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, padding.top + (line + 1) * lineHeight);

	}

    //private void OnValidate()
    //{
    //    Refresh();
    //}

    public override void CalculateLayoutInputVertical()
    {
        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    public override void SetLayoutHorizontal()
    {
        Refresh();
    }

    public override void SetLayoutVertical()
    {
        Refresh();
    }
}
