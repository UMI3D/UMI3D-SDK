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
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using umi3d.edk;
using umi3d.common;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(CVEModel),true)]
    [CanEditMultipleObjects]
    public class CVEModelEditor : GenericObject3DEditor
    {
        CVEModel Target;

        SerializedProperty Resource;
        SerializedProperty material;
        SerializedProperty overrideModelMaterial;
        SerializedProperty colliderType;
        SerializedProperty convex;

        private Editor _materialEditor;

        bool foldout;



        protected override void OnEnable()
        {
            base.OnEnable();
            material = serializedObject.FindProperty("material");
            overrideModelMaterial = serializedObject.FindProperty("overrideModelMaterial");
            Resource = serializedObject.FindProperty("objResource");
            colliderType = serializedObject.FindProperty("colliderType");
            convex = serializedObject.FindProperty("convex");

            Target = (CVEModel)target;


            if (Target.Material != null)
            {
                // Create an instance of the default MaterialEditor
                _materialEditor = CreateEditor(Target.Material);
            }

        }

        void OnDisable()
        {
            if (_materialEditor != null) { DestroyImmediate(_materialEditor); }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.PropertyField(Resource, true);
            EditorGUILayout.PropertyField(colliderType);
            if (Target.colliderType == ColliderType.Mesh)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(convex);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(overrideModelMaterial);
            if (!overrideModelMaterial.boolValue) { }
            else
            {

                EditorGUILayout.PropertyField(material);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    if (_materialEditor != null)
                    {
                        // Free the memory used by the previous MaterialEditor
                        DestroyImmediate(_materialEditor);
                    }
                    if (Target.Material == null)
                    {
                        Target.Material = CVEMaterial.DefaultMaterial;
                        material.objectReferenceValue = Target.Material;
                    }
                    if (Target.Material != null)
                    {
                        // Create a new instance of the default MaterialEditor
                        _materialEditor = (/*CVEMaterial*/Editor)CreateEditor(Target.Material);
                    }
                }


                if (_materialEditor != null)
                {
                    // Draw the material's foldout and the material shader field
                    // Required to call _materialEditor.OnInspectorGUI ();
                    _materialEditor.DrawHeader();

                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(0));
                    rect.position -= new Vector2(0, 20);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    foldout = EditorGUI.Foldout(rect, foldout, "", true, EditorStyles.foldout);

                    if (foldout)
                    {

                        //  We need to prevent the user to edit Unity default materials
                        bool isDefaultMaterial = Target.Material == CVEMaterial.DefaultMaterial;
                        if (!isDefaultMaterial) Target.SyncMeshRenderer();
                        using (new EditorGUI.DisabledGroupScope(isDefaultMaterial))
                        {
                            // Draw the material properties
                            // Works only if the foldout of _materialEditor.DrawHeader () is open
                            _materialEditor.OnInspectorGUI();
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }


    }
}
#endif