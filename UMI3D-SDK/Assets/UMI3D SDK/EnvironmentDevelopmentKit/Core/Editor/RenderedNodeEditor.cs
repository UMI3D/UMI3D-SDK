﻿using UnityEditor;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(AbstractRenderedNode), true)]
    [CanEditMultipleObjects]
    public class RenderedNodeEditor : UMI3DNodeEditor
    {

        SerializedProperty material;
        SerializedProperty overrideModelMaterials;
        SerializedProperty castShadow;
        SerializedProperty receiveShadow;
        SerializedProperty ignoreParent;

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