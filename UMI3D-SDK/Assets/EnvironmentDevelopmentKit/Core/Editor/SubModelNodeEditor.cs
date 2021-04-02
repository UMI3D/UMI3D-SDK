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
    [CustomEditor(typeof(UMI3DSubModel), true)]
    [CanEditMultipleObjects]
    public class SubModelNodeEditor : RenderedNodeEditor
    {
        SerializedProperty isPartOfNavmesh;
        SerializedProperty isTraversable;


        protected override void OnEnable()
        {
            base.OnEnable();
            isPartOfNavmesh = serializedObject.FindProperty("isPartOfNavmesh");
            isTraversable = serializedObject.FindProperty("isTraversable");
        }


        ///<inheritdoc/>
        protected override void InspectorForMeshCollider()
        {
            EditorGUILayout.PropertyField(isMeshCustom);
        }

        ///<inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(isPartOfNavmesh);
            EditorGUILayout.PropertyField(isTraversable);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif