/*
Copyright 2019 - 2021 Inetum

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

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;


namespace umi3d.edk.volume
{
    [CustomEditor(typeof(Box))]
    [CanEditMultipleObjects]
    public class BoxEditor : Editor
    {
        Box serializedBox;

        void OnEnable()
        {
            serializedBox = target as Box;
        }

        public override void OnInspectorGUI()
        {
            bool old_extendFromBottom = serializedBox.extendFromBottom.GetValue();
            Bounds old_bounds = serializedBox.bounds.GetValue();

            DrawDefaultInspector();
            if (serializedBox.extendFromBottom.GetValue() != old_extendFromBottom)
                serializedBox.extendFromBottom.ForceNotification();
            if (serializedBox.bounds.GetValue() != old_bounds)
                serializedBox.bounds.ForceNotification();
        }
    }
}
#endif