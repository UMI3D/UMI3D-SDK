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

using umi3d.common;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DNode), false)]
    [CanEditMultipleObjects]
    public class UMI3DNodeEditor : Editor
    {
        UMI3DNode Target;
        //protected bool hasCollider;

        SerializedProperty isStatic;
        SerializedProperty isActive;
        SerializedProperty xBillboard, yBillboard;
        SerializedProperty immersiveOnly;

        SerializedProperty colliderType;
        SerializedProperty convex;
        SerializedProperty colliderCenter;
        SerializedProperty colliderRadius;
        SerializedProperty colliderBoxSize;
        SerializedProperty colliderHeight;
        SerializedProperty colliderDirection;
        SerializedProperty customMeshCollider;
        SerializedProperty activeCollider;
        protected SerializedProperty isMeshCustom;


        protected virtual void OnEnable()
        {
            isStatic = serializedObject.FindProperty("isStatic");
            isActive = serializedObject.FindProperty("active");
            xBillboard = serializedObject.FindProperty("xBillboard");
            yBillboard = serializedObject.FindProperty("yBillboard");
            immersiveOnly = serializedObject.FindProperty("immersiveOnly");

            colliderType = serializedObject.FindProperty("colliderType");
            convex = serializedObject.FindProperty("convex");
            colliderBoxSize = serializedObject.FindProperty("colliderBoxSize");
            colliderCenter = serializedObject.FindProperty("colliderCenter");
            colliderDirection = serializedObject.FindProperty("colliderDirection");
            colliderHeight = serializedObject.FindProperty("colliderHeight");
            colliderRadius = serializedObject.FindProperty("colliderRadius");
            isMeshCustom = serializedObject.FindProperty("isMeshCustom");
            customMeshCollider = serializedObject.FindProperty("customMeshCollider");

            Target = (UMI3DNode)target;
            activeCollider = serializedObject.FindProperty("hasCollider");
        }

        ///<inheritdoc/>
        public override void OnInspectorGUI()
        {

            float filedWidth = EditorGUIUtility.fieldWidth;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.PropertyField(isStatic);
            EditorGUILayout.PropertyField(isActive);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Billboard", "Force the forward vector of the object in a client to be colinear with the forward vector of the client viewpoint"), GUILayout.Width(labelWidth));
            EditorGUILayout.LabelField(new GUIContent("X", "Enable the billboard on world axis X"), GUILayout.Width(10));

            EditorGUILayout.PropertyField(xBillboard, GUIContent.none, true, GUILayout.Width(10));
            EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(10));
            EditorGUILayout.LabelField(new GUIContent("Y", "Enable the billboard on world axis Y"), GUILayout.Width(10));
            EditorGUILayout.PropertyField(yBillboard, GUIContent.none, true, GUILayout.Width(10));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = filedWidth;

            EditorGUILayout.PropertyField(immersiveOnly);


            if (GUILayout.Button("Search Collider"))
            {
                Target.SearchCollider();
            }


            EditorGUILayout.PropertyField(activeCollider);
            if (activeCollider.boolValue)
            {
                EditorGUI.indentLevel++;




                EditorGUILayout.PropertyField(colliderType);

                EditorGUI.indentLevel++;
                switch ((ColliderType)colliderType.enumValueIndex)
                {
                    case ColliderType.Box:
                        //        EditorGUILayout.Vector3Field("Collider Center", colliderCenter.vector3Value);
                        EditorGUILayout.PropertyField(colliderCenter);
                        EditorGUILayout.PropertyField(colliderBoxSize);
                        break;

                    case ColliderType.Sphere:
                        EditorGUILayout.PropertyField(colliderCenter);
                        EditorGUILayout.PropertyField(colliderRadius);
                        break;

                    case ColliderType.Capsule:
                        EditorGUILayout.PropertyField(colliderCenter);
                        EditorGUILayout.PropertyField(colliderRadius);
                        EditorGUILayout.PropertyField(colliderHeight);
                        EditorGUILayout.PropertyField(colliderDirection);
                        break;

                    case ColliderType.Mesh:
                        InspectorForMeshCollider();
                        //EditorGUILayout.PropertyField(isMeshCustom);
                        if (!isMeshCustom.boolValue)
                            EditorGUILayout.PropertyField(convex);
                        else
                        {
                            EditorGUILayout.PropertyField(customMeshCollider);
                        }
                        break;

                    default:
                        break;
                }
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }



            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void InspectorForMeshCollider()
        {
            isMeshCustom.boolValue = true;
        }
    }
}
#endif