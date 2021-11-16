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
    [CustomEditor(typeof(UMI3DModel), true)]
    [CanEditMultipleObjects]
    public class UMI3DModelEditor : RenderedNodeEditor
    {
        private SerializedProperty variants;
        private SerializedProperty areSubobjectsTracked;
        private SerializedProperty isRightHanded;
        private SerializedProperty isPartOfNavmesh;
        private SerializedProperty isTraversable;

        private readonly Editor _materialEditor = null;
        private readonly bool foldout;

        ///<inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            variants = serializedObject.FindProperty("model.variants");
            areSubobjectsTracked = serializedObject.FindProperty("areSubobjectsTracked");
            isRightHanded = serializedObject.FindProperty("isRightHanded");
            isPartOfNavmesh = serializedObject.FindProperty("isPartOfNavmesh");
            isTraversable = serializedObject.FindProperty("isTraversable");

        }

        private void OnDisable()
        {
            if (_materialEditor != null) { DestroyImmediate(_materialEditor); }
        }

        ///<inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(variants, true);
            EditorGUILayout.PropertyField(areSubobjectsTracked);
            if (areSubobjectsTracked.boolValue)
                EditorGUILayout.PropertyField(isRightHanded);
            EditorGUILayout.PropertyField(isTraversable);
            EditorGUILayout.PropertyField(isPartOfNavmesh);

            serializedObject.ApplyModifiedProperties();
        }

        ///<inheritdoc/>
        protected override void InspectorForMeshCollider()
        {
            EditorGUILayout.PropertyField(isMeshCustom);
        }

    }
}
#endif