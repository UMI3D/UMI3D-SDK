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
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        HandPoseSetter handAnimation = (HandPoseSetter)target;

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
                if (handAnimation.ShowRightHand)
                    rotRightThumbProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot), matrixRightThumbProximal.MultiplyPoint3x4(RightThumbFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightThumbIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot), posRightThumbIntermediate);

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightThumbDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightThumbFirstPhalanx.Rot) * Quaternion.Euler(RightThumbSecondPhalanx.Rot) * Quaternion.Euler(RightThumbThirdPhalanx.Rot), posRightThumbDistal);

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightIndexProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot), matrixRightIndexProximal.MultiplyPoint3x4(RightIndexFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightIndexIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot), posRightIndexIntermediate);

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightIndexDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightIndexFirstPhalanx.Rot) * Quaternion.Euler(RightIndexSecondPhalanx.Rot) * Quaternion.Euler(RightIndexThirdPhalanx.Rot), posRightIndexDistal);

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightMiddleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot), matrixRightMiddleProximal.MultiplyPoint3x4(RightMiddleFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightMiddleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot), posRightMiddleIntermediate);

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightMiddleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightMiddleFirstPhalanx.Rot) * Quaternion.Euler(RightMiddleSecondPhalanx.Rot) * Quaternion.Euler(RightMiddleThirdPhalanx.Rot), posRightMiddleDistal);

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightRingProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot), matrixRightRingProximal.MultiplyPoint3x4(RightRingFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightRingIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot), posRightRingIntermediate);

                if (handAnimation.ShowLeftHand)
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

                    if (handAnimation.ShowRightHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightRingDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightRingFirstPhalanx.Rot) * Quaternion.Euler(RightRingSecondPhalanx.Rot) * Quaternion.Euler(RightRingThirdPhalanx.Rot), posRightRingDistal);

                if (handAnimation.ShowLeftHand)
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

                    if (handAnimation.ShowRightHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightLittleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot), matrixRightLittleProximal.MultiplyPoint3x4(RightLittleFirstPhalanx.Pos));

                if (handAnimation.ShowLeftHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightLittleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot), posRightLittleIntermediate);

                if (handAnimation.ShowLeftHand)
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

                    if (handAnimation.ShowRightHand)
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
                if (handAnimation.ShowRightHand)
                    rotRightLittleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.RightHandEulerRotation) * Quaternion.Euler(RightLittleFirstPhalanx.Rot) * Quaternion.Euler(RightLittleSecondPhalanx.Rot) * Quaternion.Euler(RightLittleThirdPhalanx.Rot), posRightLittleDistal);

                if (handAnimation.ShowLeftHand)
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

                    if (handAnimation.ShowRightHand)
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