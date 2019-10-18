using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEditor;

[CustomEditor(typeof(DoubleGraphicsToggle))]
public class DoubleGraphicToggleEditor : ToggleEditor
{

    SerializedProperty OnGraphic;
    SerializedProperty OffGraphic;

    protected override void OnEnable()
    {
        OnGraphic = serializedObject.FindProperty("OnGraphic");
        OffGraphic = serializedObject.FindProperty("OffGraphic");
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(OnGraphic);
        EditorGUILayout.PropertyField(OffGraphic);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}
