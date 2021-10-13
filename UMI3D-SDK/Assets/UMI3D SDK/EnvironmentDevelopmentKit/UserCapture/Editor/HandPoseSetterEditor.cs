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
using UnityEngine;
using UnityEditor;
using umi3d.edk.userCapture;
using umi3d.common.userCapture;

[CustomEditor(typeof(HandPoseSetter)), CanEditMultipleObjects]
public class HandPoseSetterEditor : Editor
{
    // custom inspector

    static GUIStyle foldoutStyle;

    #region Inspector Properties

    static bool editPose = false;
    static bool editPosition = false;

    public bool handSettings = false;
    public bool fingerRotations = false;

    static bool foldoutLeftHand = false;
    static bool foldoutRightHand = false;

    static bool foldoutLeftThumb = false;
    static bool foldoutLeftIndex = false;
    static bool foldoutLeftMiddle = false;
    static bool foldoutLeftRing = false;
    static bool foldoutLeftLittle = false;

    static bool foldoutRightThumb = false;
    static bool foldoutRightIndex = false;
    static bool foldoutRightMiddle = false;
    static bool foldoutRightRing = false;
    static bool foldoutRightLittle = false;

    #region Finger Foldouts
    static bool rightThumbProximal = true;
    static bool rightThumbIntermediate = true;
    static bool rightThumbDistal = true;

    static bool leftThumbProximal = true;
    static bool leftThumbIntermediate = true;
    static bool leftThumbDistal = true;

    static bool rightIndexProximal = true;
    static bool rightIndexIntermediate = true;
    static bool rightIndexDistal = true;

    static bool leftIndexProximal = true;
    static bool leftIndexIntermediate = true;
    static bool leftIndexDistal = true;

    static bool rightMiddleProximal = true;
    static bool rightMiddleIntermediate = true;
    static bool rightMiddleDistal = true;

    static bool leftMiddleProximal = true;
    static bool leftMiddleIntermediate = true;
    static bool leftMiddleDistal = true;

    static bool rightRingProximal = true;
    static bool rightRingIntermediate = true;
    static bool rightRingDistal = true;

    static bool leftRingProximal = true;
    static bool leftRingIntermediate = true;
    static bool leftRingDistal = true;

    static bool rightLittleProximal = true;
    static bool rightLittleIntermediate = true;
    static bool rightLittleDistal = true;

    static bool leftLittleProximal = true;
    static bool leftLittleIntermediate = true;
    static bool leftLittleDistal = true;
    #endregion

    #region Finger Gizmos
    static bool rightThumbProxGizmo = false;
    static bool rightThumbInterGizmo = false;
    static bool rightThumbDistGizmo = false;

    static bool leftThumbProxGizmo = false;
    static bool leftThumbInterGizmo = false;
    static bool leftThumbDistGizmo = false;

    static bool rightIndexProxGizmo = false;
    static bool rightIndexInterGizmo = false;
    static bool rightIndexDistGizmo = false;

    static bool leftIndexProxGizmo = false;
    static bool leftIndexInterGizmo = false;
    static bool leftIndexDistGizmo = false;

    static bool rightMiddleProxGizmo = false;
    static bool rightMiddleInterGizmo = false;
    static bool rightMiddleDistGizmo = false;

    static bool leftMiddleProxGizmo = false;
    static bool leftMiddleInterGizmo = false;
    static bool leftMiddleDistGizmo = false;

    static bool rightRingProxGizmo = false;
    static bool rightRingInterGizmo = false;
    static bool rightRingDistGizmo = false;

    static bool leftRingProxGizmo = false;
    static bool leftRingInterGizmo = false;
    static bool leftRingDistGizmo = false;

    static bool rightLittleProxGizmo = false;
    static bool rightLittleInterGizmo = false;
    static bool rightLittleDistGizmo = false;

    static bool leftLittleProxGizmo = false;
    static bool leftLittleInterGizmo = false;
    static bool leftLittleDistGizmo = false;
    #endregion

    #endregion

    SerializedProperty PoseName;
    SerializedProperty IsRelativeToNode;
    SerializedProperty ShowRightHand;
    SerializedProperty ShowLeftHand;
    SerializedProperty EditHandPosition;
    SerializedProperty EditThumb;
    SerializedProperty EditIndex;
    SerializedProperty EditMiddle;
    SerializedProperty EditRing;
    SerializedProperty EditLittle;
    SerializedProperty DrawLine;
    SerializedProperty HandPose;

    SerializedProperty TempValue;

    protected virtual void OnEnable()
    {
        PoseName = serializedObject.FindProperty("PoseName");
        IsRelativeToNode = serializedObject.FindProperty("IsRelativeToNode");
        ShowRightHand = serializedObject.FindProperty("ShowRightHand");
        ShowLeftHand = serializedObject.FindProperty("ShowLeftHand");
        EditHandPosition = serializedObject.FindProperty("EditHandPosition");
        EditThumb = serializedObject.FindProperty("EditThumb");
        EditIndex = serializedObject.FindProperty("EditIndex");
        EditMiddle = serializedObject.FindProperty("EditMiddle");
        EditRing = serializedObject.FindProperty("EditRing");
        EditLittle = serializedObject.FindProperty("EditLittle");
        DrawLine = serializedObject.FindProperty("DrawLine");
        HandPose = serializedObject.FindProperty("HandPose");

        TempValue = serializedObject.FindProperty("tempValueForTest");
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        foldoutStyle = EditorStyles.foldout;
        FontStyle previousStyle = foldoutStyle.fontStyle;
        foldoutStyle.fontStyle = FontStyle.Bold;

        editPose = ShowLeftHand.boolValue || ShowRightHand.boolValue;

        HandPoseSetter handAnimation = (HandPoseSetter)target;
        EditorGUILayout.PropertyField(HandPose);

        EditorGUILayout.Space(8f);

        EditorGUILayout.PropertyField(PoseName);

        EditorGUILayout.PropertyField(IsRelativeToNode);

        EditorGUILayout.PropertyField(ShowLeftHand);
        EditorGUILayout.PropertyField(ShowRightHand);

        EditorGUILayout.Space(1f);

        if (editPose)
        {
            handSettings = EditorGUILayout.Foldout(handSettings, "Hand Settings", foldoutStyle);

            if (handSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(DrawLine);
                EditorGUILayout.PropertyField(EditHandPosition);

                editPosition = EditHandPosition.boolValue;

                if (editPosition)
                {
                    Tools.current = Tool.View;

                    if (ShowLeftHand.boolValue)
                    {
                        foldoutLeftHand = EditorGUILayout.Foldout(foldoutLeftHand, "Left Hand", foldoutStyle);

                        if (foldoutLeftHand)
                        {
                            handAnimation.ScriptableHand.LeftHandPosition = EditorGUILayout.Vector3Field("Left Hand Position", handAnimation.ScriptableHand.LeftHandPosition);
                            handAnimation.ScriptableHand.LeftHandEulerRotation = EditorGUILayout.Vector3Field("Left Hand Rotation", handAnimation.ScriptableHand.LeftHandEulerRotation);
                        }
                    }

                    if (ShowRightHand.boolValue)
                    {

                        foldoutRightHand = EditorGUILayout.Foldout(foldoutRightHand, "Right Hand", foldoutStyle);

                        if (foldoutRightHand)
                        {
                            handAnimation.ScriptableHand.RightHandPosition = EditorGUILayout.Vector3Field("Right Hand Position", handAnimation.ScriptableHand.RightHandPosition);
                            handAnimation.ScriptableHand.RightHandEulerRotation = EditorGUILayout.Vector3Field("Right Hand Rotation", handAnimation.ScriptableHand.RightHandEulerRotation);
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(1f);

            fingerRotations = EditorGUILayout.Foldout(fingerRotations, "Finger Rotations", foldoutStyle);

            if (fingerRotations)
            {
                if (EditThumb.boolValue || EditIndex.boolValue || EditMiddle.boolValue || EditRing.boolValue || EditLittle.boolValue)
                    Tools.current = Tool.View;

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(EditThumb);

                if (EditThumb.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowLeftHand.boolValue)
                    {
                        foldoutLeftThumb = EditorGUILayout.Foldout(foldoutLeftThumb, "Left Thumb Rotations", foldoutStyle);

                        if (foldoutLeftThumb)
                        {
                            EditorGUI.indentLevel++;

                            if (leftThumbProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftThumbProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", leftThumbProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftThumbProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftThumbProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Thumb Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftThumbIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftThumbInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", leftThumbInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftThumbIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftThumbIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Thumb Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftThumbDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftThumbDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", leftThumbDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftThumbDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftThumbDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Thumb Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (ShowRightHand.boolValue)
                    {
                        foldoutRightThumb = EditorGUILayout.Foldout(foldoutRightThumb, "Right Thumb Rotations", foldoutStyle);

                        if (foldoutRightThumb)
                        {
                            EditorGUI.indentLevel++;

                            if (rightThumbProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightThumbProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", rightThumbProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightThumbProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightThumbProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Thumb Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightThumbIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightThumbInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", rightThumbInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightThumbIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightThumbIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Thumb Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightThumbDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightThumbDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", rightThumbDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightThumbDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightThumbDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Thumb Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(EditIndex);

                if (EditIndex.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowLeftHand.boolValue)
                    {
                        foldoutLeftIndex = EditorGUILayout.Foldout(foldoutLeftIndex, "Left Index Rotations", foldoutStyle);

                        if (foldoutLeftIndex)
                        {
                            EditorGUI.indentLevel++;

                            if (leftIndexProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftIndexProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", leftIndexProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftIndexProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftIndexProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Index Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftIndexIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftIndexInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", leftIndexInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftIndexIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftIndexIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Index Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftIndexDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftIndexDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", leftIndexDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftIndexDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftIndexDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Index Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (ShowRightHand.boolValue)
                    {
                        foldoutRightIndex = EditorGUILayout.Foldout(foldoutRightIndex, "Right Index Rotations", foldoutStyle);

                        if (foldoutRightIndex)
                        {
                            EditorGUI.indentLevel++;

                            if (rightIndexProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightIndexProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", rightIndexProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightIndexProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightIndexProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Index Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightIndexIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightIndexInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", rightIndexInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightIndexIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightIndexIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Index Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightIndexDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightIndexDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", rightIndexDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightIndexDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightIndexDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Index Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(EditMiddle);

                if (EditMiddle.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowLeftHand.boolValue)
                    {
                        foldoutLeftMiddle = EditorGUILayout.Foldout(foldoutLeftMiddle, "Left Middle Rotations", foldoutStyle);

                        if (foldoutLeftMiddle)
                        {
                            EditorGUI.indentLevel++;

                            if (leftMiddleProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftMiddleProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", leftMiddleProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftMiddleProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftMiddleProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Middle Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftMiddleIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftMiddleInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", leftMiddleInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftMiddleIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftMiddleIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Middle Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftMiddleDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftMiddleDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", leftMiddleDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftMiddleDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftMiddleDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Middle Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (ShowRightHand.boolValue)
                    {
                        foldoutRightMiddle = EditorGUILayout.Foldout(foldoutRightMiddle, "Right Middle Rotations", foldoutStyle);

                        if (foldoutRightMiddle)
                        {
                            EditorGUI.indentLevel++;

                            if (rightMiddleProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightMiddleProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", rightMiddleProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightMiddleProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightMiddleProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Middle Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightMiddleIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightMiddleInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", rightMiddleInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightMiddleIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightMiddleIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Middle Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightMiddleDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightMiddleDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", rightMiddleDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightMiddleDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightMiddleDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Middle Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(EditRing);

                if (EditRing.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowLeftHand.boolValue)
                    {
                        foldoutLeftRing = EditorGUILayout.Foldout(foldoutLeftRing, "Left Ring Rotations", foldoutStyle);

                        if (foldoutLeftRing)
                        {
                            EditorGUI.indentLevel++;

                            if (leftRingProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftRingProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", leftRingProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftRingProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftRingProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Ring Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftRingIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftRingInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", leftRingInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftRingIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftRingIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Ring Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftRingDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftRingDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", leftRingDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftRingDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftRingDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Ring Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (ShowRightHand.boolValue)
                    {
                        foldoutRightRing = EditorGUILayout.Foldout(foldoutRightRing, "Right Ring Rotations", foldoutStyle);

                        if (foldoutRightRing)
                        {
                            EditorGUI.indentLevel++;

                            if (rightRingProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightRingProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", rightRingProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightRingProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightRingProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Ring Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightRingIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightRingInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", rightRingInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightRingIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightRingIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Ring Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightRingDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightRingDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", rightRingDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightRingDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightRingDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Ring Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(EditLittle);

                if (EditLittle.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowLeftHand.boolValue)
                    {
                        foldoutLeftLittle = EditorGUILayout.Foldout(foldoutLeftLittle, "Left Little Rotations", foldoutStyle);

                        if (foldoutLeftLittle)
                        {
                            EditorGUI.indentLevel++;

                            if (leftLittleProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftLittleProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", leftLittleProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftLittleProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftLittleProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Little Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftLittleIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftLittleInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", leftLittleInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftLittleIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftLittleIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Little Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftLittleDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftLittleDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", leftLittleDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftLittleDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.LeftLittleDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Little Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (ShowRightHand.boolValue)
                    {
                        foldoutRightLittle = EditorGUILayout.Foldout(foldoutRightLittle, "Right Little Rotations", foldoutStyle);

                        if (foldoutRightLittle)
                        {
                            EditorGUI.indentLevel++;

                            if (rightLittleProximal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightLittleProxGizmo = EditorGUILayout.ToggleLeft("Prox. Gizmo", rightLittleProxGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightLittleProximal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightLittleProximal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Little Prox. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightLittleIntermediate)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightLittleInterGizmo = EditorGUILayout.ToggleLeft("Inter. Gizmo", rightLittleInterGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightLittleIntermediate));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightLittleIntermediate), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Little Inter. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightLittleDistal)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightLittleDistGizmo = EditorGUILayout.ToggleLeft("Dist. Gizmo", rightLittleDistGizmo, GUILayout.Width(140));

                                var data = handAnimation.ScriptableHand.Get(nameof(BoneType.RightLittleDistal));
                                handAnimation.ScriptableHand.Set(nameof(BoneType.RightLittleDistal), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Little Dist. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

            }

            EditorGUILayout.Space(3f);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Left Symmetry"))
            {

            }

            if (GUILayout.Button("Right Symmetry"))
            {

            }

            GUILayout.EndHorizontal();

        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Pose"))
        {
            handAnimation.SavePose();
        }

        if (GUILayout.Button("Load Pose"))
        {
            handAnimation.LoadPose();
        }

        if (GUILayout.Button("Reset Hand"))
        {
            handAnimation.ResetDictionary();
        }

        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        HandPoseSetter handAnimation = (HandPoseSetter)target;

        if (handAnimation != null)
        {
            #region handPosition

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.HandColor;

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), 0.02f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), 0.02f, EventType.Repaint);

            Vector3 rightHandLocalPos, leftHandLocalPos;
            rightHandLocalPos = leftHandLocalPos = Vector3.zero;
            Quaternion rightHandLocalRot, leftHandLocalRot;
            rightHandLocalRot = leftHandLocalRot = Quaternion.identity;

            if (handAnimation.EditHandPosition)
            {
                if (handAnimation.ShowRightHand)
                {
                    rightHandLocalPos = Handles.PositionHandle(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation));
                    rightHandLocalRot = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition));
                }

                if (handAnimation.ShowLeftHand)
                {
                    leftHandLocalPos = Handles.PositionHandle(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation));
                    leftHandLocalRot = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditHandPosition)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        handAnimation.ScriptableHand.RightHandPosition = handAnimation.transform.InverseTransformPoint(rightHandLocalPos);
                        handAnimation.ScriptableHand.RightHandEulerRotation = (Quaternion.Inverse(handAnimation.transform.rotation) * rightHandLocalRot).eulerAngles;
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        handAnimation.ScriptableHand.LeftHandPosition = handAnimation.transform.InverseTransformPoint(leftHandLocalPos);
                        handAnimation.ScriptableHand.LeftHandEulerRotation = (Quaternion.Inverse(handAnimation.transform.rotation) * leftHandLocalRot).eulerAngles;
                    }
                }
            }

            #endregion


            #region thumb

            SpatialDataInfo RightThumbFirstPhalanx, RightThumbSecondPhalanx, RightThumbThirdPhalanx, RightThumbLastPhalanx,
                LeftThumbFirstPhalanx, LeftThumbSecondPhalanx, LeftThumbThirdPhalanx, LeftThumbLastPhalanx;

            RightThumbFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightThumbProximal));
            RightThumbSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightThumbIntermediate));
            RightThumbThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightThumbDistal));
            RightThumbLastPhalanx = handAnimation.ScriptableHand.Get("RightThumbEnd");

            LeftThumbFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftThumbProximal));
            LeftThumbSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftThumbIntermediate));
            LeftThumbThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftThumbDistal));
            LeftThumbLastPhalanx = handAnimation.ScriptableHand.Get("LeftThumbEnd");


            var matrixRightThumbProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), Vector3.one);
            var posRightThumbProximal = matrixRightThumbProximal.MultiplyPoint3x4(RightThumbFirstPhalanx.Pos);

            var matrixLeftThumbProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), Vector3.one);
            var posLeftThumbProximal = matrixLeftThumbProximal.MultiplyPoint3x4(LeftThumbFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightThumbProximal, Quaternion.Euler(RightThumbFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftThumbProximal, Quaternion.Euler(LeftThumbFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRightThumbProximal, rotLeftThumbProximal;
            rotRightThumbProximal = rotLeftThumbProximal = Quaternion.identity;

            if (handAnimation.EditThumb)
            {
                if (handAnimation.ShowRightHand && rightThumbProxGizmo)
                    rotRightThumbProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot), matrixRightThumbProximal.MultiplyPoint3x4(RightThumbFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand && leftThumbProxGizmo)
                    rotLeftThumbProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot), matrixLeftThumbProximal.MultiplyPoint3x4(LeftThumbFirstPhalanx.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightThumbFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation)) * rotRightThumbProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightThumbProximal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftThumbFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation)) * rotLeftThumbProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftThumbProximal), newTuple);
                    }
                }
            }


            var matrixRightThumbIntermediate = Matrix4x4.TRS(posRightThumbProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightThumbIntermediate = matrixRightThumbIntermediate.MultiplyPoint3x4(RightThumbSecondPhalanx.Pos);

            var matrixLeftThumbIntermediate = Matrix4x4.TRS(posLeftThumbProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftThumbIntermediate = matrixLeftThumbIntermediate.MultiplyPoint3x4(LeftThumbSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightThumbIntermediate, Quaternion.Euler(RightThumbSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftThumbIntermediate, Quaternion.Euler(LeftThumbSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightThumbIntermediate, rotLeftThumbIntermediate;
            rotRightThumbIntermediate = rotLeftThumbIntermediate = Quaternion.identity;

            if (handAnimation.EditThumb)
            {
                if (handAnimation.ShowRightHand && rightThumbInterGizmo)
                    rotRightThumbIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot), posRightThumbIntermediate);

                if (handAnimation.ShowLeftHand && leftThumbInterGizmo)
                    rotLeftThumbIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot) * Quaternion.Euler(LeftThumbSecondPhalanx.Rot), posLeftThumbIntermediate);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightThumbSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot)) * rotRightThumbIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightThumbIntermediate), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftThumbSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot)) * rotLeftThumbIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftThumbIntermediate), newTuple);
                    }
                }
            }


            var matrixRightThumbDistal = Matrix4x4.TRS(posRightThumbIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightThumbDistal = matrixRightThumbDistal.MultiplyPoint3x4(RightThumbThirdPhalanx.Pos);

            var matrixLeftThumbDistal = Matrix4x4.TRS(posLeftThumbIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot) * Quaternion.Euler(LeftThumbSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftThumbDistal = matrixLeftThumbDistal.MultiplyPoint3x4(LeftThumbThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightThumbDistal, Quaternion.Euler(RightThumbThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftThumbDistal, Quaternion.Euler(LeftThumbThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightThumbDistal, rotLeftThumbDistal;
            rotRightThumbDistal = rotLeftThumbDistal = Quaternion.identity;

            if (handAnimation.EditThumb)
            {
                if (handAnimation.ShowRightHand && rightThumbDistGizmo)
                    rotRightThumbDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot) * Quaternion.Euler(RightThumbThirdPhalanx.Rot), posRightThumbDistal);

                if (handAnimation.ShowLeftHand && leftThumbDistGizmo)
                    rotLeftThumbDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot) * Quaternion.Euler(LeftThumbSecondPhalanx.Rot) * Quaternion.Euler(LeftThumbThirdPhalanx.Rot), posLeftThumbDistal);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightThumbThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot)) * rotRightThumbDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightThumbDistal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftThumbThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot) * Quaternion.Euler(LeftThumbSecondPhalanx.Rot)) * rotLeftThumbDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftThumbDistal), newTuple);
                    }
                }
            }

            var matrixRightThumbEnd = Matrix4x4.TRS(posRightThumbDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot) * Quaternion.Euler(RightThumbThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightThumbEnd = matrixRightThumbEnd.MultiplyPoint3x4(RightThumbLastPhalanx.Pos);

            var matrixLeftThumbEnd = Matrix4x4.TRS(posLeftThumbDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftThumbFirstPhalanx.Rot) * Quaternion.Euler(LeftThumbSecondPhalanx.Rot) * Quaternion.Euler(LeftThumbThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftThumbEnd = matrixLeftThumbEnd.MultiplyPoint3x4(LeftThumbLastPhalanx.Pos);

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightThumbEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftThumbEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region index

            SpatialDataInfo RightIndexFirstPhalanx, RightIndexSecondPhalanx, RightIndexThirdPhalanx, RightIndexLastPhalanx,
                LeftIndexFirstPhalanx, LeftIndexSecondPhalanx, LeftIndexThirdPhalanx, LeftIndexLastPhalanx;

            RightIndexFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightIndexProximal));
            RightIndexSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightIndexIntermediate));
            RightIndexThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightIndexDistal));
            RightIndexLastPhalanx = handAnimation.ScriptableHand.Get("RightIndexEnd");

            LeftIndexFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftIndexProximal));
            LeftIndexSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftIndexIntermediate));
            LeftIndexThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftIndexDistal));
            LeftIndexLastPhalanx = handAnimation.ScriptableHand.Get("LeftIndexEnd");

            var matrixRightIndexProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), Vector3.one);
            var posRightIndexProximal = matrixRightThumbProximal.MultiplyPoint3x4(RightIndexFirstPhalanx.Pos);

            var matrixLeftIndexProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), Vector3.one);
            var posLeftIndexProximal = matrixLeftThumbProximal.MultiplyPoint3x4(LeftIndexFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightIndexProximal, Quaternion.Euler(RightIndexFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftIndexProximal, Quaternion.Euler(LeftIndexFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRightIndexProximal, rotLeftIndexProximal;
            rotRightIndexProximal = rotLeftIndexProximal = Quaternion.identity;

            if (handAnimation.EditIndex)
            {
                if (handAnimation.ShowRightHand && rightIndexProxGizmo)
                    rotRightIndexProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot), matrixRightIndexProximal.MultiplyPoint3x4(RightIndexFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand && leftIndexProxGizmo)
                    rotLeftIndexProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot), matrixLeftIndexProximal.MultiplyPoint3x4(LeftIndexFirstPhalanx.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightIndexFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation)) * rotRightIndexProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightIndexProximal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftIndexFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation)) * rotLeftIndexProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftIndexProximal), newTuple);
                    }
                }
            }

            var matrixRightIndexIntermediate = Matrix4x4.TRS(posRightIndexProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightIndexIntermediate = matrixRightIndexIntermediate.MultiplyPoint3x4(RightIndexSecondPhalanx.Pos);

            var matrixLeftIndexIntermediate = Matrix4x4.TRS(posLeftIndexProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftIndexIntermediate = matrixLeftIndexIntermediate.MultiplyPoint3x4(LeftIndexSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightIndexIntermediate, Quaternion.Euler(RightIndexSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftIndexIntermediate, Quaternion.Euler(LeftIndexSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightIndexIntermediate, rotLeftIndexIntermediate;
            rotRightIndexIntermediate = rotLeftIndexIntermediate = Quaternion.identity;

            if (handAnimation.EditIndex)
            {
                if (handAnimation.ShowRightHand && rightIndexInterGizmo)
                    rotRightIndexIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot), posRightIndexIntermediate);

                if (handAnimation.ShowLeftHand && leftIndexInterGizmo)
                    rotLeftIndexIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot) * Quaternion.Euler(LeftIndexSecondPhalanx.Rot), posLeftIndexIntermediate);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightIndexSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot)) * rotRightIndexIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightIndexIntermediate), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftIndexSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot)) * rotLeftIndexIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftIndexIntermediate), newTuple);
                    }
                }
            }

            var matrixRightIndexDistal = Matrix4x4.TRS(posRightIndexIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightIndexDistal = matrixRightIndexDistal.MultiplyPoint3x4(RightIndexThirdPhalanx.Pos);

            var matrixLeftIndexDistal = Matrix4x4.TRS(posLeftIndexIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot) * Quaternion.Euler(LeftIndexSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftIndexDistal = matrixLeftIndexDistal.MultiplyPoint3x4(LeftIndexThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightIndexDistal, Quaternion.Euler(RightIndexThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftIndexDistal, Quaternion.Euler(LeftIndexThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightIndexDistal, rotLeftIndexDistal;
            rotRightIndexDistal = rotLeftIndexDistal = Quaternion.identity;

            if (handAnimation.EditIndex)
            {
                if (handAnimation.ShowRightHand && rightIndexDistGizmo)
                    rotRightIndexDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot) * Quaternion.Euler(RightIndexThirdPhalanx.Rot), posRightIndexDistal);

                if (handAnimation.ShowLeftHand && leftIndexDistGizmo)
                    rotLeftIndexDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot) * Quaternion.Euler(LeftIndexSecondPhalanx.Rot) * Quaternion.Euler(LeftIndexThirdPhalanx.Rot), posLeftIndexDistal);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightIndexThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot)) * rotRightIndexDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightIndexDistal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftIndexThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot) * Quaternion.Euler(LeftIndexSecondPhalanx.Rot)) * rotLeftIndexDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftIndexDistal), newTuple);
                    }
                }
            }

            var matrixRightIndexEnd = Matrix4x4.TRS(posRightIndexDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot) * Quaternion.Euler(RightIndexThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightIndexEnd = matrixRightIndexEnd.MultiplyPoint3x4(RightIndexLastPhalanx.Pos);

            var matrixLeftIndexEnd = Matrix4x4.TRS(posLeftIndexDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftIndexFirstPhalanx.Rot) * Quaternion.Euler(LeftIndexSecondPhalanx.Rot) * Quaternion.Euler(LeftIndexThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftIndexEnd = matrixLeftIndexEnd.MultiplyPoint3x4(LeftIndexLastPhalanx.Pos);

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightIndexEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftIndexEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region middle

            SpatialDataInfo RightMiddleFirstPhalanx, RightMiddleSecondPhalanx, RightMiddleThirdPhalanx, RightMiddleLastPhalanx,
                LeftMiddleFirstPhalanx, LeftMiddleSecondPhalanx, LeftMiddleThirdPhalanx, LeftMiddleLastPhalanx;

            RightMiddleFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightMiddleProximal));
            RightMiddleSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightMiddleIntermediate));
            RightMiddleThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightMiddleDistal));
            RightMiddleLastPhalanx = handAnimation.ScriptableHand.Get("RightMiddleEnd");

            LeftMiddleFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftMiddleProximal));
            LeftMiddleSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftMiddleIntermediate));
            LeftMiddleThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftMiddleDistal));
            LeftMiddleLastPhalanx = handAnimation.ScriptableHand.Get("LeftMiddleEnd");


            var matrixRightMiddleProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), Vector3.one);
            var posRightMiddleProximal = matrixRightMiddleProximal.MultiplyPoint3x4(RightMiddleFirstPhalanx.Pos);

            var matrixLeftMiddleProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), Vector3.one);
            var posLeftMiddleProximal = matrixLeftMiddleProximal.MultiplyPoint3x4(LeftMiddleFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightMiddleProximal, Quaternion.Euler(RightMiddleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftMiddleProximal, Quaternion.Euler(LeftMiddleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRightMiddleProximal, rotLeftMiddleProximal;
            rotRightMiddleProximal = rotLeftMiddleProximal = Quaternion.identity;

            if (handAnimation.EditMiddle)
            {
                if (handAnimation.ShowRightHand && rightMiddleProxGizmo)
                    rotRightMiddleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot), matrixRightMiddleProximal.MultiplyPoint3x4(RightMiddleFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand && leftMiddleProxGizmo)
                    rotLeftMiddleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot), matrixLeftMiddleProximal.MultiplyPoint3x4(LeftMiddleFirstPhalanx.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightMiddleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation)) * rotRightMiddleProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightMiddleProximal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftMiddleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation)) * rotLeftMiddleProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftMiddleProximal), newTuple);
                    }
                }
            }

            var matrixRightMiddleIntermediate = Matrix4x4.TRS(posRightMiddleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightMiddleIntermediate = matrixRightMiddleIntermediate.MultiplyPoint3x4(RightMiddleSecondPhalanx.Pos);

            var matrixLeftMiddleIntermediate = Matrix4x4.TRS(posLeftMiddleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftMiddleIntermediate = matrixLeftMiddleIntermediate.MultiplyPoint3x4(LeftMiddleSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightMiddleIntermediate, Quaternion.Euler(RightMiddleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftMiddleIntermediate, Quaternion.Euler(LeftMiddleSecondPhalanx.Rot), 0.01f, EventType.Repaint);


            Quaternion rotRightMiddleIntermediate, rotLeftMiddleIntermediate;
            rotRightMiddleIntermediate = rotLeftMiddleIntermediate = Quaternion.identity;

            if (handAnimation.EditMiddle)
            {
                if (handAnimation.ShowRightHand && rightMiddleInterGizmo)
                    rotRightMiddleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot), posRightMiddleIntermediate);

                if (handAnimation.ShowLeftHand && leftMiddleInterGizmo)
                    rotLeftMiddleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot) * Quaternion.Euler(LeftMiddleSecondPhalanx.Rot), posLeftMiddleIntermediate);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightMiddleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot)) * rotRightMiddleIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightMiddleIntermediate), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftMiddleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot)) * rotLeftMiddleIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftMiddleIntermediate), newTuple);
                    }
                }
            }

            var matrixRightMiddleDistal = Matrix4x4.TRS(posRightMiddleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightMiddleDistal = matrixRightMiddleDistal.MultiplyPoint3x4(RightMiddleThirdPhalanx.Pos);

            var matrixLeftMiddleDistal = Matrix4x4.TRS(posLeftMiddleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot) * Quaternion.Euler(LeftMiddleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftMiddleDistal = matrixLeftMiddleDistal.MultiplyPoint3x4(LeftMiddleThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightMiddleDistal, Quaternion.Euler(RightMiddleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftMiddleDistal, Quaternion.Euler(LeftMiddleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightMiddleDistal, rotLeftMiddleDistal;
            rotRightMiddleDistal = rotLeftMiddleDistal = Quaternion.identity;

            if (handAnimation.EditMiddle)
            {
                if (handAnimation.ShowRightHand && rightMiddleDistGizmo)
                    rotRightMiddleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot) * Quaternion.Euler(RightMiddleThirdPhalanx.Rot), posRightMiddleDistal);

                if (handAnimation.ShowLeftHand && leftMiddleDistGizmo)
                    rotLeftMiddleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot) * Quaternion.Euler(LeftMiddleSecondPhalanx.Rot) * Quaternion.Euler(LeftMiddleThirdPhalanx.Rot), posLeftMiddleDistal);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightMiddleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot)) * rotRightMiddleDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightMiddleDistal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftMiddleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot) * Quaternion.Euler(LeftMiddleSecondPhalanx.Rot)) * rotLeftMiddleDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftMiddleDistal), newTuple);
                    }
                }
            }

            var matrixRightMiddleEnd = Matrix4x4.TRS(posRightMiddleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot) * Quaternion.Euler(RightMiddleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightMiddleEnd = matrixRightMiddleEnd.MultiplyPoint3x4(RightMiddleLastPhalanx.Pos);

            var matrixLeftMiddleEnd = Matrix4x4.TRS(posLeftMiddleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftMiddleFirstPhalanx.Rot) * Quaternion.Euler(LeftMiddleSecondPhalanx.Rot) * Quaternion.Euler(LeftMiddleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftMiddleEnd = matrixLeftMiddleEnd.MultiplyPoint3x4(LeftMiddleLastPhalanx.Pos);

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightMiddleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftMiddleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region ring

            SpatialDataInfo RightRingFirstPhalanx, RightRingSecondPhalanx, RightRingThirdPhalanx, RightRingLastPhalanx,
                LeftRingFirstPhalanx, LeftRingSecondPhalanx, LeftRingThirdPhalanx, LeftRingLastPhalanx;

            RightRingFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightRingProximal));
            RightRingSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightRingIntermediate));
            RightRingThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightRingDistal));
            RightRingLastPhalanx = handAnimation.ScriptableHand.Get("RightRingEnd");

            LeftRingFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftRingProximal));
            LeftRingSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftRingIntermediate));
            LeftRingThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftRingDistal));
            LeftRingLastPhalanx = handAnimation.ScriptableHand.Get("LeftRingEnd");


            var matrixRightRingProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), Vector3.one);
            var posRightRingProximal = matrixRightRingProximal.MultiplyPoint3x4(RightRingFirstPhalanx.Pos);

            var matrixLeftRingProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), Vector3.one);
            var posLeftRingProximal = matrixLeftRingProximal.MultiplyPoint3x4(LeftRingFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightRingProximal, Quaternion.Euler(RightRingFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftRingProximal, Quaternion.Euler(LeftRingFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRightRingProximal, rotLeftRingProximal;
            rotRightRingProximal = rotLeftRingProximal = Quaternion.identity;

            if (handAnimation.EditRing)
            {
                if (handAnimation.ShowRightHand && rightRingProxGizmo)
                    rotRightRingProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot), matrixRightRingProximal.MultiplyPoint3x4(RightRingFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand && leftRingProxGizmo)
                    rotLeftRingProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot), matrixLeftRingProximal.MultiplyPoint3x4(LeftRingFirstPhalanx.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightRingFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation)) * rotRightRingProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightRingProximal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftRingFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation)) * rotLeftRingProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftRingProximal), newTuple);
                    }
                }
            }

            var matrixRightRingIntermediate = Matrix4x4.TRS(posRightRingProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightRingIntermediate = matrixRightRingIntermediate.MultiplyPoint3x4(RightRingSecondPhalanx.Pos);

            var matrixLeftRingIntermediate = Matrix4x4.TRS(posLeftRingProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftRingIntermediate = matrixLeftRingIntermediate.MultiplyPoint3x4(LeftRingSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightRingIntermediate, Quaternion.Euler(RightRingSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftRingIntermediate, Quaternion.Euler(LeftRingSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightRingIntermediate, rotLeftRingIntermediate;
            rotRightRingIntermediate = rotLeftRingIntermediate = Quaternion.identity;

            if (handAnimation.EditRing)
            {
                if (handAnimation.ShowRightHand && rightRingInterGizmo)
                    rotRightRingIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot), posRightRingIntermediate);

                if (handAnimation.ShowLeftHand && leftRingInterGizmo)
                    rotLeftRingIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot) * Quaternion.Euler(LeftRingSecondPhalanx.Rot), posLeftRingIntermediate);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightRingSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot)) * rotRightRingIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightRingIntermediate), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftRingSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot)) * rotLeftRingIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftRingIntermediate), newTuple);
                    }
                }
            }

            var matrixRightRingDistal = Matrix4x4.TRS(posRightRingIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightRingDistal = matrixRightRingDistal.MultiplyPoint3x4(RightRingThirdPhalanx.Pos);

            var matrixLeftRingDistal = Matrix4x4.TRS(posLeftRingIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot) * Quaternion.Euler(LeftRingSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftRingDistal = matrixLeftRingDistal.MultiplyPoint3x4(LeftRingThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightRingDistal, Quaternion.Euler(RightRingThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftRingDistal, Quaternion.Euler(LeftRingThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightRingDistal, rotLeftRingDistal;
            rotRightRingDistal = rotLeftRingDistal = Quaternion.identity;

            if (handAnimation.EditRing)
            {
                if (handAnimation.ShowRightHand && rightRingDistGizmo)
                    rotRightRingDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot) * Quaternion.Euler(RightRingThirdPhalanx.Rot), posRightRingDistal);

                if (handAnimation.ShowLeftHand && leftRingDistGizmo)
                    rotLeftRingDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot) * Quaternion.Euler(LeftRingSecondPhalanx.Rot) * Quaternion.Euler(LeftRingThirdPhalanx.Rot), posLeftRingDistal);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightRingThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot)) * rotRightRingDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightRingDistal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftRingThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot) * Quaternion.Euler(LeftRingSecondPhalanx.Rot)) * rotLeftRingDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftRingDistal), newTuple);
                    }
                }
            }

            var matrixRightRingEnd = Matrix4x4.TRS(posRightRingDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot) * Quaternion.Euler(RightRingThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightRingEnd = matrixRightRingEnd.MultiplyPoint3x4(RightRingLastPhalanx.Pos);

            var matrixLeftRingEnd = Matrix4x4.TRS(posLeftRingDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftRingFirstPhalanx.Rot) * Quaternion.Euler(LeftRingSecondPhalanx.Rot) * Quaternion.Euler(LeftRingThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftRingEnd = matrixLeftRingEnd.MultiplyPoint3x4(LeftRingLastPhalanx.Pos);

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightRingEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftRingEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region little

            SpatialDataInfo RightLittleFirstPhalanx, RightLittleSecondPhalanx, RightLittleThirdPhalanx, RightLittleLastPhalanx,
                LeftLittleFirstPhalanx, LeftLittleSecondPhalanx, LeftLittleThirdPhalanx, LeftLittleLastPhalanx;

            RightLittleFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightLittleProximal));
            RightLittleSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightLittleIntermediate));
            RightLittleThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.RightLittleDistal));
            RightLittleLastPhalanx = handAnimation.ScriptableHand.Get("RightLittleEnd");

            LeftLittleFirstPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftLittleProximal));
            LeftLittleSecondPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftLittleIntermediate));
            LeftLittleThirdPhalanx = handAnimation.ScriptableHand.Get(nameof(BoneType.LeftLittleDistal));
            LeftLittleLastPhalanx = handAnimation.ScriptableHand.Get("LeftLittleEnd");

            var matrixRightLittleProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation), Vector3.one);
            var posRightLittleProximal = matrixRightLittleProximal.MultiplyPoint3x4(RightLittleFirstPhalanx.Pos);

            var matrixLeftLittleProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation), Vector3.one);
            var posLeftLittleProximal = matrixLeftLittleProximal.MultiplyPoint3x4(LeftLittleFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightLittleProximal, Quaternion.Euler(RightLittleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftLittleProximal, Quaternion.Euler(LeftLittleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRightLittleProximal, rotLeftLittleProximal;
            rotRightLittleProximal = rotLeftLittleProximal = Quaternion.identity;

            if (handAnimation.EditLittle)
            {
                if (handAnimation.ShowRightHand && rightLittleProxGizmo)
                    rotRightLittleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot), matrixRightLittleProximal.MultiplyPoint3x4(RightLittleFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand && leftLittleProxGizmo)
                    rotLeftLittleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot), matrixLeftLittleProximal.MultiplyPoint3x4(LeftLittleFirstPhalanx.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightLittleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation)) * rotRightLittleProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightLittleProximal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftLittleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation)) * rotLeftLittleProximal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftLittleProximal), newTuple);
                    }
                }
            }

            var matrixRightLittleIntermediate = Matrix4x4.TRS(posRightLittleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightLittleIntermediate = matrixRightLittleIntermediate.MultiplyPoint3x4(RightLittleSecondPhalanx.Pos);

            var matrixLeftLittleIntermediate = Matrix4x4.TRS(posLeftLittleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftLittleIntermediate = matrixLeftLittleIntermediate.MultiplyPoint3x4(LeftLittleSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightLittleIntermediate, Quaternion.Euler(RightLittleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftLittleIntermediate, Quaternion.Euler(LeftLittleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightLittleIntermediate, rotLeftLittleIntermediate;
            rotRightLittleIntermediate = rotLeftLittleIntermediate = Quaternion.identity;

            if (handAnimation.EditLittle)
            {
                if (handAnimation.ShowRightHand && rightLittleInterGizmo)
                    rotRightLittleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot), posRightLittleIntermediate);

                if (handAnimation.ShowLeftHand && leftLittleInterGizmo)
                    rotLeftLittleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot) * Quaternion.Euler(LeftLittleSecondPhalanx.Rot), posLeftLittleIntermediate);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightLittleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot)) * rotRightLittleIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightLittleIntermediate), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftLittleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot)) * rotLeftLittleIntermediate).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftLittleIntermediate), newTuple);
                    }
                }
            }

            var matrixRightLittleDistal = Matrix4x4.TRS(posRightLittleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightLittleDistal = matrixRightLittleDistal.MultiplyPoint3x4(RightLittleThirdPhalanx.Pos);

            var matrixLeftLittleDistal = Matrix4x4.TRS(posLeftLittleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot) * Quaternion.Euler(LeftLittleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftLittleDistal = matrixLeftLittleDistal.MultiplyPoint3x4(LeftLittleThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightLittleDistal, Quaternion.Euler(RightLittleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftLittleDistal, Quaternion.Euler(LeftLittleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRightLittleDistal, rotLeftLittleDistal;
            rotRightLittleDistal = rotLeftLittleDistal = Quaternion.identity;

            if (handAnimation.EditLittle)
            {
                if (handAnimation.ShowRightHand && rightLittleDistGizmo)
                    rotRightLittleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot) * Quaternion.Euler(RightLittleThirdPhalanx.Rot), posRightLittleDistal);

                if (handAnimation.ShowLeftHand && leftLittleDistGizmo)
                    rotLeftLittleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot) * Quaternion.Euler(LeftLittleSecondPhalanx.Rot) * Quaternion.Euler(LeftLittleThirdPhalanx.Rot), posLeftLittleDistal);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    if (handAnimation.ShowRightHand)
                    {
                        var newTuple = new SpatialDataInfo(RightLittleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot)) * rotRightLittleDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.RightLittleDistal), newTuple);
                    }

                    if (handAnimation.ShowLeftHand)
                    {
                        var newTuple = new SpatialDataInfo(LeftLittleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot) * Quaternion.Euler(LeftLittleSecondPhalanx.Rot)) * rotLeftLittleDistal).eulerAngles);
                        handAnimation.ScriptableHand.Set(nameof(BoneType.LeftLittleDistal), newTuple);
                    }
                }
            }

            var matrixRightLittleEnd = Matrix4x4.TRS(posRightLittleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot) * Quaternion.Euler(RightLittleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRightLittleEnd = matrixRightLittleEnd.MultiplyPoint3x4(RightLittleLastPhalanx.Pos);

            var matrixLeftLittleEnd = Matrix4x4.TRS(posLeftLittleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.LeftHandEulerRotation) * Quaternion.Euler(LeftLittleFirstPhalanx.Rot) * Quaternion.Euler(LeftLittleSecondPhalanx.Rot) * Quaternion.Euler(LeftLittleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLeftLittleEnd = matrixLeftLittleEnd.MultiplyPoint3x4(LeftLittleLastPhalanx.Pos);

            if (handAnimation.ShowRightHand)
                Handles.SphereHandleCap(0, posRightLittleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            if (handAnimation.ShowLeftHand)
                Handles.SphereHandleCap(0, posLeftLittleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region lines

            Handles.color = handAnimation.LineColor;

            if (handAnimation.EditThumb || handAnimation.DrawLine)
            {
                if (handAnimation.ShowRightHand)
                {
                    Handles.DrawLine(posRightThumbDistal, posRightThumbEnd);
                    Handles.DrawLine(posRightThumbIntermediate, posRightThumbDistal);
                    Handles.DrawLine(posRightThumbProximal, posRightThumbIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), posRightThumbProximal);
                }

                if (handAnimation.ShowLeftHand)
                {
                    Handles.DrawLine(posLeftThumbDistal, posLeftThumbEnd);
                    Handles.DrawLine(posLeftThumbIntermediate, posLeftThumbDistal);
                    Handles.DrawLine(posLeftThumbProximal, posLeftThumbIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), posLeftThumbProximal);
                }
            }

            if (handAnimation.EditIndex || handAnimation.DrawLine)
            {
                if (handAnimation.ShowRightHand)
                {
                    Handles.DrawLine(posRightIndexDistal, posRightIndexEnd);
                    Handles.DrawLine(posRightIndexIntermediate, posRightIndexDistal);
                    Handles.DrawLine(posRightIndexProximal, posRightIndexIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), posRightIndexProximal);

                }

                if (handAnimation.ShowLeftHand)
                {
                    Handles.DrawLine(posLeftIndexDistal, posLeftIndexEnd);
                    Handles.DrawLine(posLeftIndexIntermediate, posLeftIndexDistal);
                    Handles.DrawLine(posLeftIndexProximal, posLeftIndexIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), posLeftIndexProximal);
                }
            }

            if (handAnimation.EditMiddle || handAnimation.DrawLine)
            {
                if (handAnimation.ShowRightHand)
                {
                    Handles.DrawLine(posRightMiddleDistal, posRightMiddleEnd);
                    Handles.DrawLine(posRightMiddleIntermediate, posRightMiddleDistal);
                    Handles.DrawLine(posRightMiddleProximal, posRightMiddleIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), posRightMiddleProximal);
                }

                if (handAnimation.ShowLeftHand)
                {
                    Handles.DrawLine(posLeftMiddleDistal, posLeftMiddleEnd);
                    Handles.DrawLine(posLeftMiddleIntermediate, posLeftMiddleDistal);
                    Handles.DrawLine(posLeftMiddleProximal, posLeftMiddleIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), posLeftMiddleProximal);
                }
            }

            if (handAnimation.EditRing || handAnimation.DrawLine)
            {
                if (handAnimation.ShowRightHand)
                {
                    Handles.DrawLine(posRightRingDistal, posRightRingEnd);
                    Handles.DrawLine(posRightRingIntermediate, posRightRingDistal);
                    Handles.DrawLine(posRightRingProximal, posRightRingIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), posRightRingProximal);
                }

                if (handAnimation.ShowLeftHand)
                {
                    Handles.DrawLine(posLeftRingDistal, posLeftRingEnd);
                    Handles.DrawLine(posLeftRingIntermediate, posLeftRingDistal);
                    Handles.DrawLine(posLeftRingProximal, posLeftRingIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), posLeftRingProximal);
                }
            }

            if (handAnimation.EditLittle || handAnimation.DrawLine)
            {
                if (handAnimation.ShowRightHand)
                {
                    Handles.DrawLine(posRightLittleDistal, posRightLittleEnd);
                    Handles.DrawLine(posRightLittleIntermediate, posRightLittleDistal);
                    Handles.DrawLine(posRightLittleProximal, posRightLittleIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.RightHandPosition), posRightLittleProximal);
                }

                if (handAnimation.ShowLeftHand)
                {
                    Handles.DrawLine(posLeftLittleDistal, posLeftLittleEnd);
                    Handles.DrawLine(posLeftLittleIntermediate, posLeftLittleDistal);
                    Handles.DrawLine(posLeftLittleProximal, posLeftLittleIntermediate);
                    Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.LeftHandPosition), posLeftLittleProximal);
                }
            }

            #endregion

        }
    }
}
#endif