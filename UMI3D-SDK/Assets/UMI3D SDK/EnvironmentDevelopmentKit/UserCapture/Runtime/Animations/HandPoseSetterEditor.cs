using System.Collections;
using System.Collections.Generic;
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

        if (handAnimation != null && handAnimation.IsVisible)
        {
            #region handPosition

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.HandColor;

            Handles.SphereHandleCap(0, handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), 0.02f, EventType.Repaint);

            Vector3 HandLocalPos = Vector3.zero;
            Quaternion HandLocalRot = Quaternion.identity;

            if (handAnimation.EditHandPosition)
            {
                HandLocalPos = Handles.PositionHandle(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation));
                HandLocalRot = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition));
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditHandPosition)
                {
                    handAnimation.ScriptableHand.HandPosition = handAnimation.transform.InverseTransformPoint(HandLocalPos);
                    handAnimation.ScriptableHand.HandEulerRotation = (Quaternion.Inverse(handAnimation.transform.rotation) * HandLocalRot).eulerAngles;
                }
            }

            #endregion


            #region thumb

            SpatialDataInfo ThumbFirstPhalanx;
            SpatialDataInfo ThumbSecondPhalanx;
            SpatialDataInfo ThumbThirdPhalanx;
            SpatialDataInfo ThumbLastPhalanx;

            if (handAnimation.IsRight)
            {
                ThumbFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightThumbProximal.ToString());
                ThumbSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightThumbIntermediate.ToString());
                ThumbThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightThumbDistal.ToString());
                ThumbLastPhalanx = handAnimation.ScriptableHand.Get("RightThumbEnd");
            }
            else
            {
                ThumbFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftThumbProximal.ToString());
                ThumbSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftThumbIntermediate.ToString());
                ThumbThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftThumbDistal.ToString());
                ThumbLastPhalanx = handAnimation.ScriptableHand.Get("LeftThumbEnd");
            }

            var matrixThumbProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), Vector3.one);
            var posThumbProximal = matrixThumbProximal.MultiplyPoint3x4(ThumbFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posThumbProximal, Quaternion.Euler(ThumbFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotThumbProximal = Quaternion.identity;

            if (handAnimation.EditThumb)
                rotThumbProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot), matrixThumbProximal.MultiplyPoint3x4(ThumbFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    var newTuple = new SpatialDataInfo(ThumbFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation)) * rotThumbProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightThumbProximal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftThumbProximal.ToString(), newTuple);
                }
            }

            var matrixThumbIntermediate = Matrix4x4.TRS(posThumbProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbIntermediate = matrixThumbIntermediate.MultiplyPoint3x4(ThumbSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posThumbIntermediate, Quaternion.Euler(ThumbSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotThumbIntermediate = Quaternion.identity;

            if (handAnimation.EditThumb)
                rotThumbIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot), posThumbIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    var newTuple = new SpatialDataInfo(ThumbSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot)) * rotThumbIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightThumbIntermediate.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftThumbIntermediate.ToString(), newTuple);
                }
            }

            var matrixThumbDistal = Matrix4x4.TRS(posThumbIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbDistal = matrixThumbDistal.MultiplyPoint3x4(ThumbThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posThumbDistal, Quaternion.Euler(ThumbThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotThumbDistal = Quaternion.identity;

            if (handAnimation.EditThumb)
                rotThumbDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot) * Quaternion.Euler(ThumbThirdPhalanx.Rot), posThumbDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    var newTuple = new SpatialDataInfo(ThumbThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot)) * rotThumbDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightThumbDistal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftThumbDistal.ToString(), newTuple);
                }
            }

            var matrixThumbEnd = Matrix4x4.TRS(posThumbDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot) * Quaternion.Euler(ThumbThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbEnd = matrixThumbEnd.MultiplyPoint3x4(ThumbLastPhalanx.Pos);

            Handles.SphereHandleCap(0, posThumbEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region index

            SpatialDataInfo IndexFirstPhalanx;
            SpatialDataInfo IndexSecondPhalanx;
            SpatialDataInfo IndexThirdPhalanx;
            SpatialDataInfo IndexLastPhalanx;

            if (handAnimation.IsRight)
            {
                IndexFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightIndexProximal.ToString());
                IndexSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightIndexIntermediate.ToString());
                IndexThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightIndexDistal.ToString());
                IndexLastPhalanx = handAnimation.ScriptableHand.Get("RightIndexEnd");
            }
            else
            {
                IndexFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftIndexProximal.ToString());
                IndexSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftIndexIntermediate.ToString());
                IndexThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftIndexDistal.ToString());
                IndexLastPhalanx = handAnimation.ScriptableHand.Get("LeftIndexEnd");
            }

            var matrixIndexProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), Vector3.one);
            var posIndexProximal = matrixThumbProximal.MultiplyPoint3x4(IndexFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posIndexProximal, Quaternion.Euler(IndexFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotIndexProximal = Quaternion.identity;

            if (handAnimation.EditIndex)
                rotIndexProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot), matrixIndexProximal.MultiplyPoint3x4(IndexFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    var newTuple = new SpatialDataInfo(IndexFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation)) * rotIndexProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightIndexProximal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftIndexProximal.ToString(), newTuple);
                }
            }

            var matrixIndexIntermediate = Matrix4x4.TRS(posIndexProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexIntermediate = matrixIndexIntermediate.MultiplyPoint3x4(IndexSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posIndexIntermediate, Quaternion.Euler(IndexSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotIndexIntermediate = Quaternion.identity;

            if (handAnimation.EditIndex)
                rotIndexIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot), posIndexIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    var newTuple = new SpatialDataInfo(IndexSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot)) * rotIndexIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightIndexIntermediate.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftIndexIntermediate.ToString(), newTuple);
                }
            }

            var matrixIndexDistal = Matrix4x4.TRS(posIndexIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexDistal = matrixIndexDistal.MultiplyPoint3x4(IndexThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posIndexDistal, Quaternion.Euler(IndexThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotIndexDistal = Quaternion.identity;

            if (handAnimation.EditIndex)
                rotIndexDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot) * Quaternion.Euler(IndexThirdPhalanx.Rot), posIndexDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    var newTuple = new SpatialDataInfo(IndexThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot)) * rotIndexDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightIndexDistal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftIndexDistal.ToString(), newTuple);
                }
            }

            var matrixIndexEnd = Matrix4x4.TRS(posIndexDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot) * Quaternion.Euler(IndexThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexEnd = matrixIndexEnd.MultiplyPoint3x4(IndexLastPhalanx.Pos);

            Handles.SphereHandleCap(0, posIndexEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region middle

            SpatialDataInfo MiddleFirstPhalanx;
            SpatialDataInfo MiddleSecondPhalanx;
            SpatialDataInfo MiddleThirdPhalanx;
            SpatialDataInfo MiddleLastPhalanx;

            if (handAnimation.IsRight)
            {
                MiddleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightMiddleProximal.ToString());
                MiddleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightMiddleIntermediate.ToString());
                MiddleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightMiddleDistal.ToString());
                MiddleLastPhalanx = handAnimation.ScriptableHand.Get("RightMiddleEnd");
            }
            else
            {
                MiddleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftMiddleProximal.ToString());
                MiddleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftMiddleIntermediate.ToString());
                MiddleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftMiddleDistal.ToString());
                MiddleLastPhalanx = handAnimation.ScriptableHand.Get("LeftMiddleEnd");
            }

            var matrixMiddleProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), Vector3.one);
            var posMiddleProximal = matrixMiddleProximal.MultiplyPoint3x4(MiddleFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posMiddleProximal, Quaternion.Euler(MiddleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotMiddleProximal = Quaternion.identity;

            if (handAnimation.EditMiddle)
                rotMiddleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot), matrixMiddleProximal.MultiplyPoint3x4(MiddleFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    var newTuple = new SpatialDataInfo(MiddleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation)) * rotMiddleProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightMiddleProximal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftMiddleProximal.ToString(), newTuple);
                }
            }

            var matrixMiddleIntermediate = Matrix4x4.TRS(posMiddleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleIntermediate = matrixMiddleIntermediate.MultiplyPoint3x4(MiddleSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posMiddleIntermediate, Quaternion.Euler(MiddleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotMiddleIntermediate = Quaternion.identity;

            if (handAnimation.EditMiddle)
                rotMiddleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot), posMiddleIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    var newTuple = new SpatialDataInfo(MiddleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot)) * rotMiddleIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightMiddleIntermediate.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftMiddleIntermediate.ToString(), newTuple);
                }
            }

            var matrixMiddleDistal = Matrix4x4.TRS(posMiddleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleDistal = matrixMiddleDistal.MultiplyPoint3x4(MiddleThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posMiddleDistal, Quaternion.Euler(MiddleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotMiddleDistal = Quaternion.identity;

            if (handAnimation.EditMiddle)
                rotMiddleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot) * Quaternion.Euler(MiddleThirdPhalanx.Rot), posMiddleDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    var newTuple = new SpatialDataInfo(MiddleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot)) * rotMiddleDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightMiddleDistal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftMiddleDistal.ToString(), newTuple);
                }
            }

            var matrixMiddleEnd = Matrix4x4.TRS(posMiddleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot) * Quaternion.Euler(MiddleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleEnd = matrixMiddleEnd.MultiplyPoint3x4(MiddleLastPhalanx.Pos);

            Handles.SphereHandleCap(0, posMiddleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region ring

            SpatialDataInfo RingFirstPhalanx;
            SpatialDataInfo RingSecondPhalanx;
            SpatialDataInfo RingThirdPhalanx;
            SpatialDataInfo RingLastPhalanx;

            if (handAnimation.IsRight)
            {
                RingFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightRingProximal.ToString());
                RingSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightRingIntermediate.ToString());
                RingThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightRingDistal.ToString());
                RingLastPhalanx = handAnimation.ScriptableHand.Get("RightRingEnd");
            }
            else
            {
                RingFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftRingProximal.ToString());
                RingSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftRingIntermediate.ToString());
                RingThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftRingDistal.ToString());
                RingLastPhalanx = handAnimation.ScriptableHand.Get("LeftRingEnd");
            }

            var matrixRingProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), Vector3.one);
            var posRingProximal = matrixRingProximal.MultiplyPoint3x4(RingFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posRingProximal, Quaternion.Euler(RingFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRingProximal = Quaternion.identity;

            if (handAnimation.EditRing)
                rotRingProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot), matrixRingProximal.MultiplyPoint3x4(RingFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    var newTuple = new SpatialDataInfo(RingFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation)) * rotRingProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightRingProximal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftRingProximal.ToString(), newTuple);
                }
            }

            var matrixRingIntermediate = Matrix4x4.TRS(posRingProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingIntermediate = matrixRingIntermediate.MultiplyPoint3x4(RingSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posRingIntermediate, Quaternion.Euler(RingSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRingIntermediate = Quaternion.identity;

            if (handAnimation.EditRing)
                rotRingIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot), posRingIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    var newTuple = new SpatialDataInfo(RingSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot)) * rotRingIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightRingIntermediate.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftRingIntermediate.ToString(), newTuple);
                }
            }

            var matrixRingDistal = Matrix4x4.TRS(posRingIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingDistal = matrixRingDistal.MultiplyPoint3x4(RingThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posRingDistal, Quaternion.Euler(RingThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRingDistal = Quaternion.identity;

            if (handAnimation.EditRing)
                rotRingDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot) * Quaternion.Euler(RingThirdPhalanx.Rot), posRingDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    var newTuple = new SpatialDataInfo(RingThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot)) * rotRingDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightRingDistal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftRingDistal.ToString(), newTuple);
                }
            }

            var matrixRingEnd = Matrix4x4.TRS(posRingDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot) * Quaternion.Euler(RingThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingEnd = matrixRingEnd.MultiplyPoint3x4(RingLastPhalanx.Pos);

            Handles.SphereHandleCap(0, posRingEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region little

            SpatialDataInfo LittleFirstPhalanx;
            SpatialDataInfo LittleSecondPhalanx;
            SpatialDataInfo LittleThirdPhalanx;
            SpatialDataInfo LittleLastPhalanx;

            if (handAnimation.IsRight)
            {
                LittleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightLittleProximal.ToString());
                LittleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightLittleIntermediate.ToString());
                LittleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightLittleDistal.ToString());
                LittleLastPhalanx = handAnimation.ScriptableHand.Get("RightLittleEnd");
            }
            else
            {
                LittleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftLittleProximal.ToString());
                LittleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftLittleIntermediate.ToString());
                LittleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftLittleDistal.ToString());
                LittleLastPhalanx = handAnimation.ScriptableHand.Get("LeftLittleEnd");
            }

            var matrixLittleProximal = Matrix4x4.TRS(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation), Vector3.one);
            var posLittleProximal = matrixLittleProximal.MultiplyPoint3x4(LittleFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posLittleProximal, Quaternion.Euler(LittleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotLittleProximal = Quaternion.identity;

            if (handAnimation.EditLittle)
                rotLittleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot), matrixLittleProximal.MultiplyPoint3x4(LittleFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    var newTuple = new SpatialDataInfo(LittleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation)) * rotLittleProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightLittleProximal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftLittleProximal.ToString(), newTuple);
                }
            }

            var matrixLittleIntermediate = Matrix4x4.TRS(posLittleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleIntermediate = matrixLittleIntermediate.MultiplyPoint3x4(LittleSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posLittleIntermediate, Quaternion.Euler(LittleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotLittleIntermediate = Quaternion.identity;

            if (handAnimation.EditLittle)
                rotLittleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot), posLittleIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    var newTuple = new SpatialDataInfo(LittleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot)) * rotLittleIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightLittleIntermediate.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftLittleIntermediate.ToString(), newTuple);
                }
            }

            var matrixLittleDistal = Matrix4x4.TRS(posLittleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleDistal = matrixLittleDistal.MultiplyPoint3x4(LittleThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posLittleDistal, Quaternion.Euler(LittleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotLittleDistal = Quaternion.identity;

            if (handAnimation.EditLittle)
                rotLittleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot) * Quaternion.Euler(LittleThirdPhalanx.Rot), posLittleDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    var newTuple = new SpatialDataInfo(LittleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot)) * rotLittleDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightLittleDistal.ToString(), newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftLittleDistal.ToString(), newTuple);
                }
            }

            var matrixLittleEnd = Matrix4x4.TRS(posLittleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot) * Quaternion.Euler(LittleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleEnd = matrixLittleEnd.MultiplyPoint3x4(LittleLastPhalanx.Pos);

            Handles.SphereHandleCap(0, posLittleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region lines

            Handles.color = handAnimation.LineColor;

            if (handAnimation.EditThumb || handAnimation.DrawLine)
            {
                Handles.DrawLine(posThumbDistal, posThumbEnd);
                Handles.DrawLine(posThumbIntermediate, posThumbDistal);
                Handles.DrawLine(posThumbProximal, posThumbIntermediate);
                Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), posThumbProximal);
            }

            if (handAnimation.EditIndex || handAnimation.DrawLine)
            {
                Handles.DrawLine(posIndexDistal, posIndexEnd);
                Handles.DrawLine(posIndexIntermediate, posIndexDistal);
                Handles.DrawLine(posIndexProximal, posIndexIntermediate);
                Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), posIndexProximal);
            }

            if (handAnimation.EditMiddle || handAnimation.DrawLine)
            {
                Handles.DrawLine(posMiddleDistal, posMiddleEnd);
                Handles.DrawLine(posMiddleIntermediate, posMiddleDistal);
                Handles.DrawLine(posMiddleProximal, posMiddleIntermediate);
                Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), posMiddleProximal);
            }

            if (handAnimation.EditRing || handAnimation.DrawLine)
            {
                Handles.DrawLine(posRingDistal, posRingEnd);
                Handles.DrawLine(posRingIntermediate, posRingDistal);
                Handles.DrawLine(posRingProximal, posRingIntermediate);
                Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), posRingProximal);
            }

            if (handAnimation.EditLittle || handAnimation.DrawLine)
            {
                Handles.DrawLine(posLittleDistal, posLittleEnd);
                Handles.DrawLine(posLittleIntermediate, posLittleDistal);
                Handles.DrawLine(posLittleProximal, posLittleIntermediate);
                Handles.DrawLine(handAnimation.transform.TransformPoint(handAnimation.ScriptableHand.HandPosition), posLittleProximal);
            }

            #endregion
        }
    }

}
