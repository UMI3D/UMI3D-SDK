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

[CustomEditor(typeof(BodyPoseSetter)), CanEditMultipleObjects]
public class BodyPoseSetterEditor : Editor
{
    // custom inspector

    static GUIStyle foldoutStyle;

    #region Inspector Properties

    static bool editPose = false;
    static bool editPosition = false;

    public bool bodySettings = false;
    public bool bodyRotations = false;

    static bool foldoutBody = false;

    static bool foldoutLeftLeg = false;
    static bool foldoutRightLeg = false;
    static bool foldoutLeftArm = false;
    static bool foldoutRightArm = false;
    static bool foldoutTrunk = false;

    #region Parts Foldouts
    static bool leftHip = true;
    static bool leftKnee = true;
    static bool leftAnkle = true;

    static bool rightHip = true;
    static bool rightKnee = true;
    static bool rightAnkle = true;

    static bool leftShoulder = true;
    static bool leftUpperArm = true;
    static bool leftForeArm = true;
    static bool leftHand = true;

    static bool rightShoulder = true;
    static bool rightUpperArm = true;
    static bool rightForeArm = true;
    static bool rightHand = true;

    static bool spine = true;
    static bool chest = true;
    static bool upperChest = true;
    static bool neck = true;
    static bool head = true;
    #endregion

    #region Parts Gizmos
    static bool leftHipGizmo = false;
    static bool leftKneeGizmo = false;
    static bool leftAnkleGizmo = false;

    static bool rightHipGizmo = false;
    static bool rightKneeGizmo = false;
    static bool rightAnkleGizmo = false;

    static bool leftShoulderGizmo = false;
    static bool leftUpperArmGizmo = false;
    static bool leftForeArmGizmo = false;
    static bool leftHandGizmo = false;

    static bool rightShoulderGizmo = false;
    static bool rightUpperArmGizmo = false;
    static bool rightForeArmGizmo = false;
    static bool rightHandGizmo = false;

    static bool hipsGizmo = false;
    static bool spineGizmo = false;
    static bool chestGizmo = false;
    static bool upperChestGizmo = false;
    static bool neckGizmo = false;
    static bool headGizmo = false;
    #endregion

    #endregion

    SerializedProperty PoseName;
    SerializedProperty IsRelativeToNode;
    SerializedProperty ShowBody;
    SerializedProperty EditBodyPosition;
    SerializedProperty EditLeftLeg;
    SerializedProperty EditRightLeg;
    SerializedProperty EditLeftArm;
    SerializedProperty EditRightArm;
    SerializedProperty EditTrunk;
    SerializedProperty DrawLine;
    SerializedProperty BodyPose;

    SerializedProperty TempValue;

    protected virtual void OnEnable()
    {
        PoseName = serializedObject.FindProperty("PoseName");
        IsRelativeToNode = serializedObject.FindProperty("IsRelativeToNode");
        ShowBody = serializedObject.FindProperty("ShowBody");
        EditBodyPosition = serializedObject.FindProperty("EditBodyPosition");
        EditLeftLeg = serializedObject.FindProperty("EditLeftLeg");
        EditRightLeg = serializedObject.FindProperty("EditRightLeg");
        EditLeftArm = serializedObject.FindProperty("EditLeftArm");
        EditRightArm = serializedObject.FindProperty("EditRightArm");
        EditTrunk = serializedObject.FindProperty("EditTrunk");
        DrawLine = serializedObject.FindProperty("DrawLine");
        BodyPose = serializedObject.FindProperty("BodyPose");

        TempValue = serializedObject.FindProperty("tempValueForTest");
    }

    public override void OnInspectorGUI()
    {
        foldoutStyle = EditorStyles.foldout;
        FontStyle previousStyle = foldoutStyle.fontStyle;
        foldoutStyle.fontStyle = FontStyle.Bold;

        editPose = ShowBody.boolValue;

        BodyPoseSetter bodyAnimation = (BodyPoseSetter)target;
        EditorGUILayout.PropertyField(BodyPose);

        EditorGUILayout.Space(8f);

        EditorGUILayout.PropertyField(PoseName);

        EditorGUILayout.PropertyField(IsRelativeToNode);

        EditorGUILayout.PropertyField(ShowBody);

        EditorGUILayout.Space(1f);

        if (editPose)
        {
            bodySettings = EditorGUILayout.Foldout(bodySettings, "Body Settings", foldoutStyle);

            if (bodySettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(DrawLine);
                EditorGUILayout.PropertyField(EditBodyPosition);

                editPosition = EditBodyPosition.boolValue;

                if (editPosition)
                {
                    Tools.current = Tool.View;

                    if (ShowBody.boolValue)
                    {
                        foldoutBody = EditorGUILayout.Foldout(foldoutBody, "Body", foldoutStyle);

                        if (foldoutBody)
                        {
                            bodyAnimation.ScriptableBody.BodyPosition = EditorGUILayout.Vector3Field("Body Position", bodyAnimation.ScriptableBody.BodyPosition);
                            bodyAnimation.ScriptableBody.BodyEulerRotation = EditorGUILayout.Vector3Field("Body Rotation", bodyAnimation.ScriptableBody.BodyEulerRotation);
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(1f);

            bodyRotations = EditorGUILayout.Foldout(bodyRotations, "Body Parts Rotations", foldoutStyle);

            if (bodyRotations)
            {
                if (EditLeftLeg.boolValue || EditRightLeg.boolValue || EditLeftArm.boolValue || EditRightArm.boolValue || EditTrunk.boolValue)
                    Tools.current = Tool.View;

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(EditTrunk);

                if (EditTrunk.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowBody.boolValue)
                    {
                        foldoutTrunk = EditorGUILayout.Foldout(foldoutTrunk, "Trunk Rotations", foldoutStyle);

                        if (foldoutTrunk)
                        {
                            EditorGUI.indentLevel++;

                            if (spine)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = spineGizmo = EditorGUILayout.ToggleLeft("Spine Gizmo", spineGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Spine));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.Spine), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Spine Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (chest)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = chestGizmo = EditorGUILayout.ToggleLeft("Chest Gizmo", chestGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Chest));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.Chest), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Chest Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (upperChest)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = upperChestGizmo = EditorGUILayout.ToggleLeft("Upper Chest Gizmo", upperChestGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.UpperChest));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.UpperChest), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Upper Chest Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (neck)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = neckGizmo = EditorGUILayout.ToggleLeft("Neck Gizmo", neckGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Neck));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.Neck), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Neck Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (head)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = headGizmo = EditorGUILayout.ToggleLeft("Head Gizmo", headGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Head));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.Head), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Head Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.PropertyField(EditLeftArm);

                if (EditLeftArm.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowBody.boolValue)
                    {
                        foldoutLeftArm = EditorGUILayout.Foldout(foldoutLeftArm, "Left Arm Rotations", foldoutStyle);

                        if (foldoutLeftArm)
                        {
                            EditorGUI.indentLevel++;

                            if (leftShoulder)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftShoulderGizmo = EditorGUILayout.ToggleLeft("Left Shoulder Gizmo", leftShoulderGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftShoulder));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftShoulder), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left Shoulder Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftUpperArm)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftUpperArmGizmo = EditorGUILayout.ToggleLeft("Left UpperArm Gizmo", leftUpperArmGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftUpperArm));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftUpperArm), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left UpperArm Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftForeArm)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftForeArmGizmo = EditorGUILayout.ToggleLeft("Left ForeArm Gizmo", leftForeArmGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftForearm));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftForearm), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left ForeArm Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftHand)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftHandGizmo = EditorGUILayout.ToggleLeft("Left Hand Gizmo", leftHandGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftHand));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftHand), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left Hand Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.PropertyField(EditRightArm);

                if (EditRightArm.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowBody.boolValue)
                    {
                        foldoutRightArm = EditorGUILayout.Foldout(foldoutRightArm, "Right Arm Rotations", foldoutStyle);

                        if (foldoutRightArm)
                        {
                            EditorGUI.indentLevel++;

                            if (rightShoulder)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightShoulderGizmo = EditorGUILayout.ToggleLeft("Right Shoulder Gizmo", rightShoulderGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightShoulder));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightShoulder), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right Shoulder Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightUpperArm)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightUpperArmGizmo = EditorGUILayout.ToggleLeft("Right UpperArm Gizmo", rightUpperArmGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightUpperArm));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightUpperArm), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right UpperArm Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightForeArm)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightForeArmGizmo = EditorGUILayout.ToggleLeft("Right ForeArm Gizmo", rightForeArmGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightForearm));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightForearm), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right ForeArm Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightHand)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightHandGizmo = EditorGUILayout.ToggleLeft("Right Hand Gizmo", rightHandGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightHand));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightHand), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right Hand Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.PropertyField(EditLeftLeg);

                if (EditLeftLeg.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowBody.boolValue)
                    {
                        foldoutLeftLeg = EditorGUILayout.Foldout(foldoutLeftLeg, "Left Leg Rotations", foldoutStyle);

                        if (foldoutLeftLeg)
                        {
                            EditorGUI.indentLevel++;

                            if (leftHip)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftHipGizmo = EditorGUILayout.ToggleLeft("Left Hip Gizmo", leftHipGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftHip));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftHip), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left Hip Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftKnee)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftKneeGizmo = EditorGUILayout.ToggleLeft("Left Knee Gizmo", leftKneeGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftKnee));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftKnee), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left Knee. Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (leftAnkle)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = leftAnkleGizmo = EditorGUILayout.ToggleLeft("Left Ankle Gizmo", leftAnkleGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftAnkle));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftAnkle), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Left Ankle Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.PropertyField(EditRightLeg);

                if (EditRightLeg.boolValue)
                {
                    EditorGUI.indentLevel++;

                    if (ShowBody.boolValue)
                    {
                        foldoutRightLeg = EditorGUILayout.Foldout(foldoutRightLeg, "Right Leg Rotations", foldoutStyle);

                        if (foldoutRightLeg)
                        {
                            EditorGUI.indentLevel++;

                            if (rightHip)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightHipGizmo = EditorGUILayout.ToggleLeft("Right Hip Gizmo", rightHipGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightHip));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightHip), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right Hip Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightKnee)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightKneeGizmo = EditorGUILayout.ToggleLeft("Right Knee Gizmo", rightKneeGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightKnee));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightKnee), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right Knee Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            if (rightAnkle)
                            {
                                EditorGUILayout.BeginHorizontal();

                                TempValue.boolValue = rightAnkleGizmo = EditorGUILayout.ToggleLeft("Right Ankle Gizmo", rightAnkleGizmo, GUILayout.Width(140));

                                var data = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightAnkle));
                                bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightAnkle), new SpatialDataInfo(data.Pos, EditorGUILayout.Vector3Field("Right Ankle Rotation", data.Rot)));

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.Space(3f);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Left Symmetry"))
            {
                bodyAnimation.CreateLeftSymmetry();
            }

            if (GUILayout.Button("Create Right Symmetry"))
            {
                bodyAnimation.CreateRightSymmetry();
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Pose"))
        {
            bodyAnimation.SavePose();
        }

        if (GUILayout.Button("Load Pose"))
        {
            bodyAnimation.LoadPose();
        }

        if (GUILayout.Button("Reset Body"))
        {
            bodyAnimation.ResetDictionary();
        }

        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        BodyPoseSetter bodyAnimation = (BodyPoseSetter)target;

        if (bodyAnimation != null)
        {
            #region bodyPosition

            EditorGUI.BeginChangeCheck();

            Handles.color = bodyAnimation.BodyColor;

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, bodyAnimation.transform.TransformPoint(bodyAnimation.ScriptableBody.BodyPosition), bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation), 0.02f, EventType.Repaint);

            Vector3 bodyLocalPos;
            bodyLocalPos = Vector3.zero;
            Quaternion bodyLocalRot;
            bodyLocalRot = Quaternion.identity;

            if (bodyAnimation.EditBodyPosition)
            {
                if (bodyAnimation.ShowBody)
                {
                    bodyLocalPos = Handles.PositionHandle(bodyAnimation.transform.TransformPoint(bodyAnimation.ScriptableBody.BodyPosition), bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation));
                    bodyLocalRot = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation), bodyAnimation.transform.TransformPoint(bodyAnimation.ScriptableBody.BodyPosition));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditBodyPosition)
                {
                    if (bodyAnimation.ShowBody)
                    {
                        bodyAnimation.ScriptableBody.BodyPosition = bodyAnimation.transform.InverseTransformPoint(bodyLocalPos);
                        bodyAnimation.ScriptableBody.BodyEulerRotation = (Quaternion.Inverse(bodyAnimation.transform.rotation) * bodyLocalRot).eulerAngles;
                    }
                }
            }

            #endregion

            #region trunk

            SpatialDataInfo HipsJoint, SpineJoint, ChestJoint, UpperChestJoint, NeckJoint, HeadJoint, TopHeadJoint;

            HipsJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Hips));
            SpineJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Spine));
            ChestJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Chest));
            UpperChestJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.UpperChest));
            NeckJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Neck));
            HeadJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.Head));
            TopHeadJoint = bodyAnimation.ScriptableBody.Get("TopHeadEnd");

            var matrixBodyJoint = Matrix4x4.TRS(bodyAnimation.transform.TransformPoint(bodyAnimation.ScriptableBody.BodyPosition), bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation), Vector3.one);
            var posHipsJoint = matrixBodyJoint.MultiplyPoint3x4(HipsJoint.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = bodyAnimation.JointColor;

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posHipsJoint, Quaternion.Euler(HipsJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotHipsJoint = Quaternion.identity;

            if (bodyAnimation.EditTrunk)
            {
                if (bodyAnimation.ShowBody && hipsGizmo)
                    rotHipsJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot), matrixBodyJoint.MultiplyPoint3x4(HipsJoint.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditTrunk)
                {
                    if (bodyAnimation.ShowBody && hipsGizmo)
                    {
                        var newTuple = new SpatialDataInfo(HipsJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation)) * rotHipsJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.Hips), newTuple);
                    }
                }
            }

            var matrixHipsJoint = Matrix4x4.TRS(posHipsJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot), Vector3.one);
            var posSpineJoint = matrixHipsJoint.MultiplyPoint3x4(SpineJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posSpineJoint, Quaternion.Euler(SpineJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotSpineJoint = Quaternion.identity;

            if (bodyAnimation.EditTrunk)
            {
                if (bodyAnimation.ShowBody && spineGizmo)
                    rotSpineJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot), posSpineJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditTrunk)
                {
                    if (bodyAnimation.ShowBody && spineGizmo)
                    {
                        var newTuple = new SpatialDataInfo(SpineJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotSpineJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.Spine), newTuple);
                    }
                }
            }

            var matrixSpineJoint = Matrix4x4.TRS(posSpineJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot), Vector3.one);
            var posChestJoint = matrixSpineJoint.MultiplyPoint3x4(ChestJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posChestJoint, Quaternion.Euler(ChestJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotChestJoint = Quaternion.identity;

            if (bodyAnimation.EditTrunk)
            {
                if (bodyAnimation.ShowBody && chestGizmo)
                    rotChestJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot), posChestJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditTrunk)
                {
                    if (bodyAnimation.ShowBody && chestGizmo)
                    {
                        var newTuple = new SpatialDataInfo(ChestJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot)) * rotChestJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.Chest), newTuple);
                    }
                }
            }

            var matrixChestJoint = Matrix4x4.TRS(posChestJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot), Vector3.one);
            var posUpperChestJoint = matrixChestJoint.MultiplyPoint3x4(UpperChestJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posUpperChestJoint, Quaternion.Euler(UpperChestJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotUpperChestJoint = Quaternion.identity;

            if (bodyAnimation.EditTrunk)
            {
                if (bodyAnimation.ShowBody && upperChestGizmo)
                    rotUpperChestJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot), posUpperChestJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditTrunk)
                {
                    if (bodyAnimation.ShowBody && upperChestGizmo)
                    {
                        var newTuple = new SpatialDataInfo(UpperChestJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot)) * rotUpperChestJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.UpperChest), newTuple);
                    }
                }
            }

            var matrixUpperChestJoint = Matrix4x4.TRS(posUpperChestJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot), Vector3.one);
            var posNeckJoint = matrixUpperChestJoint.MultiplyPoint3x4(NeckJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posNeckJoint, Quaternion.Euler(NeckJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotNeckJoint = Quaternion.identity;

            if (bodyAnimation.EditTrunk)
            {
                if (bodyAnimation.ShowBody && neckGizmo)
                    rotNeckJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(NeckJoint.Rot), posNeckJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditTrunk)
                {
                    if (bodyAnimation.ShowBody && neckGizmo)
                    {
                        var newTuple = new SpatialDataInfo(NeckJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot)) * rotNeckJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.Neck), newTuple);
                    }
                }
            }

            var matrixNeckJoint = Matrix4x4.TRS(posNeckJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(NeckJoint.Rot), Vector3.one);
            var posHeadJoint = matrixNeckJoint.MultiplyPoint3x4(HeadJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posHeadJoint, Quaternion.Euler(HeadJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotHeadJoint = Quaternion.identity;

            if (bodyAnimation.EditTrunk)
            {
                if (bodyAnimation.ShowBody && headGizmo)
                    rotHeadJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(NeckJoint.Rot) * Quaternion.Euler(HeadJoint.Rot), posHeadJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditTrunk)
                {
                    if (bodyAnimation.ShowBody && headGizmo)
                    {
                        var newTuple = new SpatialDataInfo(HeadJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(NeckJoint.Rot)) * rotHeadJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.Head), newTuple);
                    }
                }
            }

            var matrixHeadJoint = Matrix4x4.TRS(posHeadJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(NeckJoint.Rot) * Quaternion.Euler(HeadJoint.Rot), Vector3.one);
            var posTopHeadJoint = matrixHeadJoint.MultiplyPoint3x4(TopHeadJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posTopHeadJoint, Quaternion.identity, 0.02f, EventType.Repaint);
            #endregion

            #region leftLeg

            SpatialDataInfo LeftHipJoint, LeftKneeJoint, LeftAnkleJoint, LeftToeBaseJoint, LeftToeEndJoint;

            LeftHipJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftHip));
            LeftKneeJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftKnee));
            LeftAnkleJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftAnkle));
            LeftToeBaseJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftToeBase));
            LeftToeEndJoint = bodyAnimation.ScriptableBody.Get("LeftToeBaseEnd");

            var posLeftHipJoint = matrixHipsJoint.MultiplyPoint3x4(LeftHipJoint.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = bodyAnimation.JointColor;

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftHipJoint, Quaternion.Euler(HipsJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftHipJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftLeg)
            {
                if (bodyAnimation.ShowBody && leftHipGizmo)
                    rotLeftHipJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot), matrixHipsJoint.MultiplyPoint3x4(LeftHipJoint.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftLeg)
                {
                    if (bodyAnimation.ShowBody && leftHipGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftHipJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotLeftHipJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftHip), newTuple);
                    }
                }
            }

            var matrixLeftHipJoint = Matrix4x4.TRS(posLeftHipJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot), Vector3.one);
            var posLeftKneeJoint = matrixLeftHipJoint.MultiplyPoint3x4(LeftKneeJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftKneeJoint, Quaternion.Euler(LeftKneeJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftKneeJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftLeg)
            {
                if (bodyAnimation.ShowBody && leftKneeGizmo)
                    rotLeftKneeJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot) * Quaternion.Euler(LeftKneeJoint.Rot), posLeftKneeJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftLeg)
                {
                    if (bodyAnimation.ShowBody && leftKneeGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftKneeJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot)) * rotLeftKneeJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftKnee), newTuple);
                    }
                }
            }

            var matrixLeftKneeJoint = Matrix4x4.TRS(posLeftKneeJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot) * Quaternion.Euler(LeftKneeJoint.Rot), Vector3.one);
            var posLeftAnkleJoint = matrixLeftKneeJoint.MultiplyPoint3x4(LeftAnkleJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftAnkleJoint, Quaternion.Euler(LeftAnkleJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftAnkleJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftLeg)
            {
                bodyAnimation.ScriptableBody.LeftAnklePosition = matrixHipsJoint.inverse.MultiplyPoint3x4(posLeftAnkleJoint);

                if (bodyAnimation.ShowBody && leftAnkleGizmo)
                {
                    rotLeftAnkleJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot) * Quaternion.Euler(LeftKneeJoint.Rot) * Quaternion.Euler(LeftAnkleJoint.Rot), posLeftAnkleJoint);
                    bodyAnimation.ScriptableBody.LeftAnkleEulerRotation = (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotLeftAnkleJoint.eulerAngles);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftLeg)
                {
                    if (bodyAnimation.ShowBody && leftAnkleGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftAnkleJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot) * Quaternion.Euler(LeftKneeJoint.Rot)) * rotLeftAnkleJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftAnkle), newTuple);
                    }
                }
            }

            var matrixLeftAnkleJoint = Matrix4x4.TRS(posLeftAnkleJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot) * Quaternion.Euler(LeftKneeJoint.Rot) * Quaternion.Euler(LeftAnkleJoint.Rot), Vector3.one);
            var posLeftToeBaseJoint = matrixLeftAnkleJoint.MultiplyPoint3x4(LeftToeBaseJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftToeBaseJoint, Quaternion.Euler(LeftToeBaseJoint.Rot), 0.02f, EventType.Repaint);

            var matrixLeftToeBaseJoint = Matrix4x4.TRS(posLeftToeBaseJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(LeftHipJoint.Rot) * Quaternion.Euler(LeftKneeJoint.Rot) * Quaternion.Euler(LeftAnkleJoint.Rot) * Quaternion.Euler(LeftToeBaseJoint.Rot), Vector3.one);
            var posLeftToeEndJoint = matrixLeftToeBaseJoint.MultiplyPoint3x4(LeftToeEndJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftToeEndJoint, Quaternion.identity, 0.02f, EventType.Repaint);

            #endregion

            #region rightLeg

            SpatialDataInfo RightHipJoint, RightKneeJoint, RightAnkleJoint, RightToeBaseJoint, RightToeEndJoint;

            RightHipJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightHip));
            RightKneeJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightKnee));
            RightAnkleJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightAnkle));
            RightToeBaseJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightToeBase));
            RightToeEndJoint = bodyAnimation.ScriptableBody.Get("RightToeBaseEnd");

            var posRightHipJoint = matrixHipsJoint.MultiplyPoint3x4(RightHipJoint.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = bodyAnimation.JointColor;

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightHipJoint, Quaternion.Euler(HipsJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotRightHipJoint = Quaternion.identity;

            if (bodyAnimation.EditRightLeg)
            {
                if (bodyAnimation.ShowBody && rightHipGizmo)
                    rotRightHipJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot), matrixHipsJoint.MultiplyPoint3x4(RightHipJoint.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightLeg)
                {
                    if (bodyAnimation.ShowBody && rightHipGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightHipJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotRightHipJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightHip), newTuple);
                    }
                }
            }

            var matrixRightHipJoint = Matrix4x4.TRS(posRightHipJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot), Vector3.one);
            var posRightKneeJoint = matrixRightHipJoint.MultiplyPoint3x4(RightKneeJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightKneeJoint, Quaternion.Euler(RightKneeJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotRightKneeJoint = Quaternion.identity;

            if (bodyAnimation.EditRightLeg)
            {
                if (bodyAnimation.ShowBody && rightKneeGizmo)
                    rotRightKneeJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot) * Quaternion.Euler(RightKneeJoint.Rot), posRightKneeJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightLeg)
                {
                    if (bodyAnimation.ShowBody && rightKneeGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightKneeJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot)) * rotRightKneeJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightKnee), newTuple);
                    }
                }
            }

            var matrixRightKneeJoint = Matrix4x4.TRS(posRightKneeJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot) * Quaternion.Euler(RightKneeJoint.Rot), Vector3.one);
            var posRightAnkleJoint = matrixRightKneeJoint.MultiplyPoint3x4(RightAnkleJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
            {
                Handles.SphereHandleCap(0, posRightAnkleJoint, Quaternion.Euler(RightAnkleJoint.Rot), 0.02f, EventType.Repaint);
            }

            Quaternion rotRightAnkleJoint = Quaternion.identity;

            if (bodyAnimation.EditRightLeg)
            {
                bodyAnimation.ScriptableBody.RightAnklePosition = matrixHipsJoint.inverse.MultiplyPoint3x4(posRightAnkleJoint);

                if (bodyAnimation.ShowBody && rightAnkleGizmo)
                {
                    rotRightAnkleJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot) * Quaternion.Euler(RightKneeJoint.Rot) * Quaternion.Euler(RightAnkleJoint.Rot), posRightAnkleJoint);
                    bodyAnimation.ScriptableBody.RightAnkleEulerRotation = (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotRightAnkleJoint).eulerAngles;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightLeg)
                {
                    if (bodyAnimation.ShowBody && rightAnkleGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightAnkleJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot) * Quaternion.Euler(RightKneeJoint.Rot)) * rotRightAnkleJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightAnkle), newTuple);
                    }
                }
            }

            var matrixRightAnkleJoint = Matrix4x4.TRS(posRightAnkleJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot) * Quaternion.Euler(RightKneeJoint.Rot) * Quaternion.Euler(RightAnkleJoint.Rot), Vector3.one);
            var posRightToeBaseJoint = matrixRightAnkleJoint.MultiplyPoint3x4(RightToeBaseJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightToeBaseJoint, Quaternion.Euler(RightToeBaseJoint.Rot), 0.02f, EventType.Repaint);

            var matrixRightToeBaseJoint = Matrix4x4.TRS(posRightToeBaseJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(RightHipJoint.Rot) * Quaternion.Euler(RightKneeJoint.Rot) * Quaternion.Euler(RightAnkleJoint.Rot) * Quaternion.Euler(RightToeBaseJoint.Rot), Vector3.one);
            var posRightToeEndJoint = matrixRightToeBaseJoint.MultiplyPoint3x4(RightToeEndJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightToeEndJoint, Quaternion.identity, 0.02f, EventType.Repaint);

            #endregion

            #region leftArm

            SpatialDataInfo LeftShoulderJoint, LeftUpperArmJoint, LeftForeArmJoint, LeftHandJoint, LeftHandEndJoint, LeftThumbEndJoint;

            LeftShoulderJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftShoulder));
            LeftUpperArmJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftUpperArm));
            LeftForeArmJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftForearm));
            LeftHandJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.LeftHand));
            LeftHandEndJoint = bodyAnimation.ScriptableBody.Get("LeftHandEnd");
            LeftThumbEndJoint = bodyAnimation.ScriptableBody.Get("LeftThumbEnd");

            var posLeftShoulderJoint = matrixUpperChestJoint.MultiplyPoint3x4(LeftShoulderJoint.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = bodyAnimation.JointColor;

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftShoulderJoint, Quaternion.Euler(LeftShoulderJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftShoulderJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftArm)
            {
                if (bodyAnimation.ShowBody && leftShoulderGizmo)
                    rotLeftShoulderJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot), matrixUpperChestJoint.MultiplyPoint3x4(LeftShoulderJoint.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftArm)
                {
                    if (bodyAnimation.ShowBody && leftShoulderGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftShoulderJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot)) * rotLeftShoulderJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftShoulder), newTuple);
                    }
                }
            }

            var matrixLeftShoulderJoint = Matrix4x4.TRS(posLeftShoulderJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot), Vector3.one);
            var posLeftUpperArmJoint = matrixLeftShoulderJoint.MultiplyPoint3x4(LeftUpperArmJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftUpperArmJoint, Quaternion.Euler(LeftUpperArmJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftUpperArmJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftArm)
            {
                if (bodyAnimation.ShowBody && leftUpperArmGizmo)
                    rotLeftUpperArmJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot), posLeftUpperArmJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftArm)
                {
                    if (bodyAnimation.ShowBody && leftUpperArmGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftUpperArmJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot)) * rotLeftUpperArmJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftUpperArm), newTuple);
                    }
                }
            }

            var matrixLeftUpperArmJoint = Matrix4x4.TRS(posLeftUpperArmJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot), Vector3.one);
            var posLeftForeArmJoint = matrixLeftUpperArmJoint.MultiplyPoint3x4(LeftForeArmJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftForeArmJoint, Quaternion.Euler(LeftForeArmJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftForeArmJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftArm)
            {
                if (bodyAnimation.ShowBody && leftForeArmGizmo)
                    rotLeftForeArmJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot) * Quaternion.Euler(LeftForeArmJoint.Rot), posLeftForeArmJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftArm)
                {
                    if (bodyAnimation.ShowBody && leftForeArmGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftForeArmJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot)) * rotLeftForeArmJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftForearm), newTuple);
                    }
                }
            }

            var matrixLeftForeArmJoint = Matrix4x4.TRS(posLeftForeArmJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot) * Quaternion.Euler(LeftForeArmJoint.Rot), Vector3.one);
            var posLeftHandJoint = matrixLeftForeArmJoint.MultiplyPoint3x4(LeftHandJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftHandJoint, Quaternion.Euler(LeftHandJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotLeftHandJoint = Quaternion.identity;

            if (bodyAnimation.EditLeftArm)
            {
                bodyAnimation.ScriptableBody.LeftHandPosition = matrixHipsJoint.inverse.MultiplyPoint3x4(posLeftHandJoint);

                if (bodyAnimation.ShowBody && leftHandGizmo)
                {
                    rotLeftHandJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot) * Quaternion.Euler(LeftForeArmJoint.Rot) * Quaternion.Euler(LeftHandJoint.Rot), posLeftHandJoint);
                    bodyAnimation.ScriptableBody.LeftHandEulerRotation = (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotLeftHandJoint).eulerAngles;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditLeftArm)
                {
                    if (bodyAnimation.ShowBody && leftHandGizmo)
                    {
                        var newTuple = new SpatialDataInfo(LeftHandJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot) * Quaternion.Euler(LeftForeArmJoint.Rot)) * rotLeftHandJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.LeftHand), newTuple);
                    }
                }
            }

            var matrixLeftHandJoint = Matrix4x4.TRS(posLeftHandJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(LeftShoulderJoint.Rot) * Quaternion.Euler(LeftUpperArmJoint.Rot) * Quaternion.Euler(LeftForeArmJoint.Rot) * Quaternion.Euler(LeftHandJoint.Rot), Vector3.one);
            var posLeftHandEndJoint = matrixLeftHandJoint.MultiplyPoint3x4(LeftHandEndJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftHandEndJoint, Quaternion.identity, 0.02f, EventType.Repaint);

            var posLeftThumbEndJoint = matrixLeftHandJoint.MultiplyPoint3x4(LeftThumbEndJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posLeftThumbEndJoint, Quaternion.identity, 0.02f, EventType.Repaint);

            #endregion

            #region rightArm

            SpatialDataInfo RightShoulderJoint, RightUpperArmJoint, RightForeArmJoint, RightHandJoint, RightHandEndJoint, RightThumbEndJoint;

            RightShoulderJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightShoulder));
            RightUpperArmJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightUpperArm));
            RightForeArmJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightForearm));
            RightHandJoint = bodyAnimation.ScriptableBody.Get(nameof(BoneType.RightHand));
            RightHandEndJoint = bodyAnimation.ScriptableBody.Get("RightHandEnd");
            RightThumbEndJoint = bodyAnimation.ScriptableBody.Get("RightThumbEnd");

            var posRightShoulderJoint = matrixUpperChestJoint.MultiplyPoint3x4(RightShoulderJoint.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = bodyAnimation.JointColor;

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightShoulderJoint, Quaternion.Euler(RightShoulderJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotRightShoulderJoint = Quaternion.identity;

            if (bodyAnimation.EditRightArm)
            {
                if (bodyAnimation.ShowBody && rightShoulderGizmo)
                    rotRightShoulderJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot), matrixUpperChestJoint.MultiplyPoint3x4(RightShoulderJoint.Pos));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightArm)
                {
                    if (bodyAnimation.ShowBody && rightShoulderGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightShoulderJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot)) * rotRightShoulderJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightShoulder), newTuple);
                    }
                }
            }

            var matrixRightShoulderJoint = Matrix4x4.TRS(posRightShoulderJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot), Vector3.one);
            var posRightUpperArmJoint = matrixRightShoulderJoint.MultiplyPoint3x4(RightUpperArmJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightUpperArmJoint, Quaternion.Euler(RightUpperArmJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotRightUpperArmJoint = Quaternion.identity;

            if (bodyAnimation.EditRightArm)
            {
                if (bodyAnimation.ShowBody && rightUpperArmGizmo)
                    rotRightUpperArmJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot), posRightUpperArmJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightArm)
                {
                    if (bodyAnimation.ShowBody && rightUpperArmGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightUpperArmJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot)) * rotRightUpperArmJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightUpperArm), newTuple);
                    }
                }
            }

            var matrixRightUpperArmJoint = Matrix4x4.TRS(posRightUpperArmJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot), Vector3.one);
            var posRightForeArmJoint = matrixRightUpperArmJoint.MultiplyPoint3x4(RightForeArmJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightForeArmJoint, Quaternion.Euler(RightForeArmJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotRightForeArmJoint = Quaternion.identity;

            if (bodyAnimation.EditRightArm)
            {
                if (bodyAnimation.ShowBody && rightForeArmGizmo)
                    rotRightForeArmJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot) * Quaternion.Euler(RightForeArmJoint.Rot), posRightForeArmJoint);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightArm)
                {
                    if (bodyAnimation.ShowBody && rightForeArmGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightForeArmJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot)) * rotRightForeArmJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightForearm), newTuple);
                    }
                }
            }

            var matrixRightForeArmJoint = Matrix4x4.TRS(posRightForeArmJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot) * Quaternion.Euler(RightForeArmJoint.Rot), Vector3.one);
            var posRightHandJoint = matrixRightForeArmJoint.MultiplyPoint3x4(RightHandJoint.Pos);

            EditorGUI.BeginChangeCheck();

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightHandJoint, Quaternion.Euler(RightHandJoint.Rot), 0.02f, EventType.Repaint);

            Quaternion rotRightHandJoint = Quaternion.identity;

            if (bodyAnimation.EditRightArm)
            {
                bodyAnimation.ScriptableBody.RightHandPosition = matrixHipsJoint.inverse.MultiplyPoint3x4(posRightHandJoint);

                if (bodyAnimation.ShowBody && rightHandGizmo)
                {
                    rotRightHandJoint = Handles.RotationHandle(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot) * Quaternion.Euler(RightForeArmJoint.Rot) * Quaternion.Euler(RightHandJoint.Rot), posRightHandJoint);
                    bodyAnimation.ScriptableBody.RightHandEulerRotation = (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot)) * rotRightHandJoint.eulerAngles);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (bodyAnimation.EditRightArm)
                {
                    if (bodyAnimation.ShowBody && rightHandGizmo)
                    {
                        var newTuple = new SpatialDataInfo(RightHandJoint.Pos, (Quaternion.Inverse(bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot) * Quaternion.Euler(RightForeArmJoint.Rot)) * rotRightHandJoint).eulerAngles);
                        bodyAnimation.ScriptableBody.Set(nameof(BoneType.RightHand), newTuple);
                    }
                }
            }

            var matrixRightHandJoint = Matrix4x4.TRS(posRightHandJoint, bodyAnimation.transform.rotation * Quaternion.Euler(bodyAnimation.ScriptableBody.BodyEulerRotation) * Quaternion.Euler(HipsJoint.Rot) * Quaternion.Euler(SpineJoint.Rot) * Quaternion.Euler(ChestJoint.Rot) * Quaternion.Euler(UpperChestJoint.Rot) * Quaternion.Euler(RightShoulderJoint.Rot) * Quaternion.Euler(RightUpperArmJoint.Rot) * Quaternion.Euler(RightForeArmJoint.Rot) * Quaternion.Euler(RightHandJoint.Rot), Vector3.one);
            var posRightHandEndJoint = matrixRightHandJoint.MultiplyPoint3x4(RightHandEndJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightHandEndJoint, Quaternion.identity, 0.02f, EventType.Repaint);

            var posRightThumbEndJoint = matrixRightHandJoint.MultiplyPoint3x4(RightThumbEndJoint.Pos);

            if (bodyAnimation.ShowBody)
                Handles.SphereHandleCap(0, posRightThumbEndJoint, Quaternion.identity, 0.02f, EventType.Repaint);

            #endregion

            #region lines

            Handles.color = bodyAnimation.LineColor;

            if (bodyAnimation.EditTrunk || bodyAnimation.DrawLine)
            {
                if (bodyAnimation.ShowBody)
                {
                    Handles.DrawLine(posHeadJoint, posTopHeadJoint);
                    Handles.DrawLine(posNeckJoint, posHeadJoint);
                    Handles.DrawLine(posUpperChestJoint, posNeckJoint);
                    Handles.DrawLine(posChestJoint, posUpperChestJoint);
                    Handles.DrawLine(posSpineJoint, posChestJoint);
                    Handles.DrawLine(posHipsJoint, posSpineJoint);
                }
            }

            if (bodyAnimation.EditLeftArm || bodyAnimation.DrawLine)
            {
                if (bodyAnimation.ShowBody)
                {
                    Handles.DrawLine(posLeftHandJoint, posLeftThumbEndJoint);
                    Handles.DrawLine(posLeftHandJoint, posLeftHandEndJoint);
                    Handles.DrawLine(posLeftForeArmJoint, posLeftHandJoint);
                    Handles.DrawLine(posLeftUpperArmJoint, posLeftForeArmJoint);
                    Handles.DrawLine(posLeftShoulderJoint, posLeftUpperArmJoint);
                    Handles.DrawLine(posUpperChestJoint, posLeftShoulderJoint);
                }
            }

            if (bodyAnimation.EditRightArm || bodyAnimation.DrawLine)
            {
                if (bodyAnimation.ShowBody)
                {
                    Handles.DrawLine(posRightHandJoint, posRightThumbEndJoint);
                    Handles.DrawLine(posRightHandJoint, posRightHandEndJoint);
                    Handles.DrawLine(posRightForeArmJoint, posRightHandJoint);
                    Handles.DrawLine(posRightUpperArmJoint, posRightForeArmJoint);
                    Handles.DrawLine(posRightShoulderJoint, posRightUpperArmJoint);
                    Handles.DrawLine(posUpperChestJoint, posRightShoulderJoint);
                }
            }

            if (bodyAnimation.EditLeftLeg || bodyAnimation.DrawLine)
            {
                if (bodyAnimation.ShowBody)
                {
                    Handles.DrawLine(posLeftToeBaseJoint, posLeftToeEndJoint);
                    Handles.DrawLine(posLeftAnkleJoint, posLeftToeBaseJoint);
                    Handles.DrawLine(posLeftKneeJoint, posLeftAnkleJoint);
                    Handles.DrawLine(posLeftHipJoint, posLeftKneeJoint);
                    Handles.DrawLine(posHipsJoint, posLeftHipJoint);
                }
            }

            if (bodyAnimation.EditRightLeg || bodyAnimation.DrawLine)
            {
                if (bodyAnimation.ShowBody)
                {
                    Handles.DrawLine(posRightToeBaseJoint, posRightToeEndJoint);
                    Handles.DrawLine(posRightAnkleJoint, posRightToeBaseJoint);
                    Handles.DrawLine(posRightKneeJoint, posRightAnkleJoint);
                    Handles.DrawLine(posRightHipJoint, posRightKneeJoint);
                    Handles.DrawLine(posHipsJoint, posRightHipJoint);
                }
            }

            #endregion
        }
    }
}
#endif