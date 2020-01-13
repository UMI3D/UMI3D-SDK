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
using UnityEngine;

/// <summary>
/// This Component is used to display children in circle.
/// </summary>
public class RotateChildren : MonoBehaviour
{
    [SerializeField]
    [Range(0, 10)]
    int virtualChildren = 0;
    [SerializeField]
    float Radius = 3;
    [SerializeField] 
    bool update = false;

    private void OnValidate()
    {
        if(update)
            update = false;
        int childCount = transform.childCount;
        int count = childCount + virtualChildren - 1;
        if (count <= 0) return;
        float delta = 360 / count;
        for(int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.localPosition = new Vector3(Radius, 0, 0);
            child.localRotation = (Quaternion.Euler(0, 180, 0));
            child.RotateAround(transform.position,Vector3.up, delta * i);
        }
    }
}
