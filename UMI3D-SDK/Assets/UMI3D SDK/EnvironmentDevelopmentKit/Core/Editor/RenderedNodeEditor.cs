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

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(AbstractRenderedNode), true)]
    [CanEditMultipleObjects]
    public class RenderedNodeEditor : UMI3DNodeEditor
    {
        private SerializedProperty material;
        private SerializedProperty overrideModelMaterials;
        private SerializedProperty castShadow;
        private SerializedProperty receiveShadow;
        private SerializedProperty ignoreParent;

        ///<inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            material = serializedObject.FindProperty("materialsOverrider");
            overrideModelMaterials = serializedObject.FindProperty("overrideModelMaterials");

            castShadow = serializedObject.FindProperty("castShadow");
            receiveShadow = serializedObject.FindProperty("receiveShadow");

            ignoreParent = serializedObject.FindProperty("ignoreModelMaterialOverride"); // could be null

        }
        ///<inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();


            EditorGUILayout.PropertyField(overrideModelMaterials);
            if (overrideModelMaterials.boolValue)
            {
                EditorGUILayout.PropertyField(material);
            }

            if (target is UMI3DSubModel)
            {
                EditorGUILayout.PropertyField(ignoreParent);
            }

            EditorGUILayout.PropertyField(castShadow);
            EditorGUILayout.PropertyField(receiveShadow);


            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif