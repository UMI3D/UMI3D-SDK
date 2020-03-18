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
using System.Reflection;
using UnityEditorInternal;
using umi3d.edk;
using umi3d.common;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(umi3d.edk.CVEPrimitive),true)]
    [CanEditMultipleObjects]
    public class CVEPrimitiveEditor : GenericObject3DEditor
    {

        PreviewRenderUtility previewRenderUtility;
        Light previewLight;
        GameObject previewObject;
        Vector2 previewDir;

        umi3d.edk.CVEPrimitive Target;

        SerializedProperty preview;
        SerializedProperty primitive;
        SerializedProperty material;
        SerializedProperty colliderType;
        SerializedProperty convex;

        private /*CVEMaterial*/Editor _materialEditor;

        bool foldout;

        bool AddPreview()
        {
            Target.preview = GameObject.CreatePrimitive(((umi3d.common.MeshPrimitive)primitive.enumValueIndex).Convert());
            Target.preview.name = Target.name + "_preview";
            Target.preview.hideFlags = HideFlags.NotEditable;
            var collider = Target.preview.gameObject.GetComponent<Collider>();
            DestroyImmediate(collider);
            Target.preview.transform.SetParent(Target.transform, false);
            previewObject = Target.preview;
            preview.objectReferenceValue = Target.preview;
            return previewObject;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            primitive = serializedObject.FindProperty("primitive");
            material = serializedObject.FindProperty("material");
            preview = serializedObject.FindProperty("preview");
            colliderType = serializedObject.FindProperty("colliderType");
            convex = serializedObject.FindProperty("convex");

            previewRenderUtility = new PreviewRenderUtility(true);
            previewRenderUtility.cameraFieldOfView = 30f;

            previewRenderUtility.camera.farClipPlane = 1000;
            previewRenderUtility.camera.nearClipPlane = 0.3f;

            previewLight = previewRenderUtility.camera.gameObject.AddComponent<Light>();
            previewLight.type = LightType.Directional;

            Target = (umi3d.edk.CVEPrimitive)target;
            previewObject = (GameObject)preview.objectReferenceValue;
            if (!previewObject)
            {
                if (!AddPreview()) Debug.LogError("preview is still null");
                serializedObject.ApplyModifiedProperties();
            }
            previewDir = Vector2.zero;

            if (Target.Material != null)
            {
                // Create an instance of the default MaterialEditor
                _materialEditor = (/*Material*//*CVEMaterial*/Editor)CreateEditor(Target.Material);
            }

        }

        void OnDisable()
        {
            DestroyImmediate(previewLight);
            previewRenderUtility.Cleanup();
            previewRenderUtility = null;
            previewObject = null;
            InternalEditorUtility.RemoveCustomLighting();
            if (_materialEditor != null) { DestroyImmediate(_materialEditor); }
        }

        public override void OnInspectorGUI()
        {
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            serializedObject.Update();
            int primitiveValue = primitive.intValue;
            EditorGUILayout.PropertyField(primitive);

            EditorGUILayout.PropertyField(colliderType);
            if (Target.colliderType == ColliderType.Mesh)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(convex);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(material);


            MeshPrimitive state = (MeshPrimitive)primitive.enumValueIndex;

            if (Target.primitive != state)
            {
                DestroyImmediate(preview.objectReferenceValue);
                if (!AddPreview()) Debug.LogError("preview is still null");
            }

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
                _materialEditor.DrawHeader();
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(0));
                rect.position -= new Vector2(0, 21);
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
            serializedObject.ApplyModifiedProperties();
        }

        #region Preview

        public override bool HasPreviewGUI()
        {
            return targets.Length == 1;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (previewRenderUtility == null) previewRenderUtility = new PreviewRenderUtility(true);

            previewDir = Drag2D(previewDir, r);
            previewRenderUtility.BeginPreview(r, background);

            var previewCamera = previewRenderUtility.camera;
            Vector3 rot = new Vector3(-previewDir.y, -previewDir.x, 0);

            previewCamera.transform.position = previewObject.transform.position + Quaternion.Euler(rot) * Vector3.forward * 5;
            previewCamera.transform.LookAt(previewObject.transform);
            previewCamera.Render();
            previewRenderUtility.DrawMesh(previewObject.GetComponent<MeshFilter>().sharedMesh, previewObject.transform.position, previewObject.transform.rotation, previewObject.GetComponent<Renderer>().sharedMaterial, 0);
            previewRenderUtility.EndAndDrawPreview(r);
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Primitive");
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            base.OnInteractivePreviewGUI(r, background);
        }



        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        scrollPosition -= current.delta / Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = scrollPosition.y % 360;
                        if (scrollPosition.y < 0) scrollPosition.y += 360;
                        scrollPosition.x = scrollPosition.x % 180;
                        if (scrollPosition.x < 0) scrollPosition.x += 360;
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scrollPosition;
        }

        #endregion

    }
}
#endif