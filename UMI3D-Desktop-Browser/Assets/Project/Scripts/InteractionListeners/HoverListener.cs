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

public class HoverListener : MonoBehaviour {
    public System.Action<bool> onHover;
    public System.Action<Vector3,Vector3> onHovered;
    public void OnHover(bool hover)
    {
        if (onHover != null)
            onHover(hover);
    }

    public void OnHovered(Vector3 hoveredLocalPosition,Vector3 normal)
    {
        if (onHovered != null)
            onHovered(hoveredLocalPosition,normal);
    }

    private void Update()
    {
        if (transform is RectTransform)
        {
            var c = GetComponent<BoxCollider>();
            if (c == null)
                c = gameObject.AddComponent<BoxCollider>();
            var s = (transform as RectTransform).rect.size;
            var p = (transform as RectTransform).pivot;
            c.size = new Vector3(s.x, s.y, 0f);
            c.center = new Vector3(s.x * (.5f - p.x), s.y * (.5f - p.y));
        }
    }

    private void OnDestroy()
    {
        if (transform is RectTransform)
        {
            var c = GetComponent<BoxCollider>();
            if (c != null)
                Destroy(c);
        }
    }
}
