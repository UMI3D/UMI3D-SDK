using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using umi3d.edk.userCapture;
using umi3d.common.userCapture;

[CustomEditor(typeof(UMI3DHandPose)), CanEditMultipleObjects]
public class UMI3DHandPoseEditor : Editor
{
    // Custom Inspector
    //SerializedProperty handPoseName;
    //SerializedProperty handPoseIsRight;

    //protected virtual void OnEnable()
    //{
    //    handPoseName = serializedObject.FindProperty("Name");
    //    handPoseIsRight = serializedObject.FindProperty("IsRight");
    //}

    //public override void OnInspectorGUI()
    //{
    //    EditorGUILayout.PropertyField(handPoseName);
    //    EditorGUILayout.PropertyField(handPoseIsRight);
    //}
}
