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
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomPropertyDrawer(typeof(MaterialOverrider.OverridedMaterialList), false)]
    public class MaterialsOverriderDrawer : PropertyDrawer
    {
        private SerializedProperty overrideAllMaterial;
        private SerializedProperty overrideMaterials;

        ///<inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Create property fields.
            overrideAllMaterial = property.FindPropertyRelative("overrideAllMaterial");
            overrideMaterials = property.FindPropertyRelative("overidedMaterials");

            if (overrideAllMaterial.boolValue)
            {
                EditorGUI.PropertyField(position, overrideAllMaterial);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        ///<inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 1;
            if (!property.isExpanded)
            {
                lineCount = 1;
            }
            else
            {
                //   if(overrideAllMaterial == null)
                overrideAllMaterial = property.FindPropertyRelative("overrideAllMaterial");
                //    if(overrideMaterials == null)
                overrideMaterials = property.FindPropertyRelative("overidedMaterials");
                //     lineCount += 2 + extraline;// + overrideMaterials.arraySize;
                if (overrideAllMaterial.boolValue)
                    lineCount = 1;
                else
                    lineCount = 3 + overrideMaterials.CountInProperty();
            }

            return (EditorGUIUtility.singleLineHeight * lineCount) + (EditorGUIUtility.standardVerticalSpacing * (lineCount - 1));
        }
    }
}
#endif