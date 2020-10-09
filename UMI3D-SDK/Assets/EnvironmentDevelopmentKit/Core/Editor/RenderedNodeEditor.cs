using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

        protected override void OnEnable()
        {
            base.OnEnable();
            material = serializedObject.FindProperty("materialsOverider");
            overrideModelMaterials = serializedObject.FindProperty("overrideModelMaterials");

            castShadow = serializedObject.FindProperty("castShadow");
            receiveShadow = serializedObject.FindProperty("receiveShadow");


        }
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


            EditorGUILayout.PropertyField(castShadow);
            EditorGUILayout.PropertyField(receiveShadow);


            serializedObject.ApplyModifiedProperties();
        }

    }
}