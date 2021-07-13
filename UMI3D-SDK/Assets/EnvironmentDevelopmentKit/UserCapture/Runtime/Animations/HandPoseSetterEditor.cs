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
        if (GUILayout.Button("Reset Hand"))
        {
            handAnimation.ResetDictionary();
        }

        if (GUILayout.Button("Load Pose"))
        {
            handAnimation.LoadPose();
        }
        
        if (GUILayout.Button("Save Pose"))
        {
            handAnimation.SavePose();
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

            Handles.SphereHandleCap(0, handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), 0.02f, EventType.Repaint);

            Vector3 HandLocalPos = Vector3.zero;
            Quaternion HandLocalRot = Quaternion.identity;

            if (handAnimation.EditHandPosition)
            {
                HandLocalPos = Handles.PositionHandle(handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation));
                HandLocalRot = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditHandPosition)
                {
                    handAnimation.ScriptableHand.HandLocalPosition = HandLocalPos - handAnimation.transform.position;
                    handAnimation.ScriptableHand.HandLocalEulerRotation = (Quaternion.Inverse(handAnimation.transform.rotation) * HandLocalRot).eulerAngles;
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
                ThumbFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightThumbProximal);
                ThumbSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightThumbIntermediate);
                ThumbThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightThumbDistal);
                ThumbLastPhalanx = handAnimation.ScriptableHand.Get("RightThumbEnd");
            }
            else
            {
                ThumbFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftThumbProximal);
                ThumbSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftThumbIntermediate);
                ThumbThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftThumbDistal);
                ThumbLastPhalanx = handAnimation.ScriptableHand.Get("LeftThumbEnd");
            }

            var matrixThumbProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), Vector3.one);
            var posThumbProximal = matrixThumbProximal.MultiplyPoint3x4(ThumbFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posThumbProximal, Quaternion.Euler(ThumbFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotThumbProximal = Quaternion.identity;

            if (handAnimation.EditThumb)
                rotThumbProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot), matrixThumbProximal.MultiplyPoint3x4(ThumbFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    var newTuple = new SpatialDataInfo(ThumbFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation)) * rotThumbProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightThumbProximal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftThumbProximal, newTuple);
                }
            }

            var matrixThumbIntermediate = Matrix4x4.TRS(posThumbProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbIntermediate = matrixThumbIntermediate.MultiplyPoint3x4(ThumbSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posThumbIntermediate, Quaternion.Euler(ThumbSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotThumbIntermediate = Quaternion.identity;

            if (handAnimation.EditThumb)
                rotThumbIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot), posThumbIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    var newTuple = new SpatialDataInfo(ThumbSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot)) * rotThumbIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightThumbIntermediate, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftThumbIntermediate, newTuple);
                }
            }

            var matrixThumbDistal = Matrix4x4.TRS(posThumbIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbDistal = matrixThumbDistal.MultiplyPoint3x4(ThumbThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posThumbDistal, Quaternion.Euler(ThumbThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotThumbDistal = Quaternion.identity;

            if (handAnimation.EditThumb)
                rotThumbDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot) * Quaternion.Euler(ThumbThirdPhalanx.Rot), posThumbDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditThumb)
                {
                    var newTuple = new SpatialDataInfo(ThumbThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot)) * rotThumbDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightThumbDistal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftThumbDistal, newTuple);
                }
            }

            var matrixThumbEnd = Matrix4x4.TRS(posThumbDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Rot) * Quaternion.Euler(ThumbSecondPhalanx.Rot) * Quaternion.Euler(ThumbThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
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
                IndexFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightIndexProximal);
                IndexSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightIndexIntermediate);
                IndexThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightIndexDistal);
                IndexLastPhalanx = handAnimation.ScriptableHand.Get("RightIndexEnd");
            }
            else
            {
                IndexFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftIndexProximal);
                IndexSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftIndexIntermediate);
                IndexThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftIndexDistal);
                IndexLastPhalanx = handAnimation.ScriptableHand.Get("LeftIndexEnd");
            }

            var matrixIndexProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), Vector3.one);
            var posIndexProximal = matrixThumbProximal.MultiplyPoint3x4(IndexFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posIndexProximal, Quaternion.Euler(IndexFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotIndexProximal = Quaternion.identity;

            if (handAnimation.EditIndex)
                rotIndexProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot), matrixIndexProximal.MultiplyPoint3x4(IndexFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    var newTuple = new SpatialDataInfo(IndexFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation)) * rotIndexProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightIndexProximal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftIndexProximal, newTuple);
                }
            }

            var matrixIndexIntermediate = Matrix4x4.TRS(posIndexProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexIntermediate = matrixIndexIntermediate.MultiplyPoint3x4(IndexSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posIndexIntermediate, Quaternion.Euler(IndexSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotIndexIntermediate = Quaternion.identity;

            if (handAnimation.EditIndex)
                rotIndexIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot), posIndexIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    var newTuple = new SpatialDataInfo(IndexSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot)) * rotIndexIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightIndexIntermediate, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftIndexIntermediate, newTuple);
                }
            }

            var matrixIndexDistal = Matrix4x4.TRS(posIndexIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexDistal = matrixIndexDistal.MultiplyPoint3x4(IndexThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posIndexDistal, Quaternion.Euler(IndexThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotIndexDistal = Quaternion.identity;

            if (handAnimation.EditIndex)
                rotIndexDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot) * Quaternion.Euler(IndexThirdPhalanx.Rot), posIndexDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditIndex)
                {
                    var newTuple = new SpatialDataInfo(IndexThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot)) * rotIndexDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightIndexDistal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftIndexDistal, newTuple);
                }
            }

            var matrixIndexEnd = Matrix4x4.TRS(posIndexDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Rot) * Quaternion.Euler(IndexSecondPhalanx.Rot) * Quaternion.Euler(IndexThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
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
                MiddleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightMiddleProximal);
                MiddleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightMiddleIntermediate);
                MiddleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightMiddleDistal);
                MiddleLastPhalanx = handAnimation.ScriptableHand.Get("RightMiddleEnd");
            }
            else
            {
                MiddleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftMiddleProximal);
                MiddleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftMiddleIntermediate);
                MiddleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftMiddleDistal);
                MiddleLastPhalanx = handAnimation.ScriptableHand.Get("LeftMiddleEnd");
            }

            var matrixMiddleProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), Vector3.one);
            var posMiddleProximal = matrixMiddleProximal.MultiplyPoint3x4(MiddleFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posMiddleProximal, Quaternion.Euler(MiddleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotMiddleProximal = Quaternion.identity;

            if (handAnimation.EditMiddle)
                rotMiddleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot), matrixMiddleProximal.MultiplyPoint3x4(MiddleFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    var newTuple = new SpatialDataInfo(MiddleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation)) * rotMiddleProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightMiddleProximal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftMiddleProximal, newTuple);
                }
            }

            var matrixMiddleIntermediate = Matrix4x4.TRS(posMiddleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleIntermediate = matrixMiddleIntermediate.MultiplyPoint3x4(MiddleSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posMiddleIntermediate, Quaternion.Euler(MiddleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotMiddleIntermediate = Quaternion.identity;

            if (handAnimation.EditMiddle)
                rotMiddleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot), posMiddleIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    var newTuple = new SpatialDataInfo(MiddleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot)) * rotMiddleIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightMiddleIntermediate, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftMiddleIntermediate, newTuple);
                }
            }

            var matrixMiddleDistal = Matrix4x4.TRS(posMiddleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleDistal = matrixMiddleDistal.MultiplyPoint3x4(MiddleThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posMiddleDistal, Quaternion.Euler(MiddleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotMiddleDistal = Quaternion.identity;

            if (handAnimation.EditMiddle)
                rotMiddleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot) * Quaternion.Euler(MiddleThirdPhalanx.Rot), posMiddleDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditMiddle)
                {
                    var newTuple = new SpatialDataInfo(MiddleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot)) * rotMiddleDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightMiddleDistal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftMiddleDistal, newTuple);
                }
            }

            var matrixMiddleEnd = Matrix4x4.TRS(posMiddleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Rot) * Quaternion.Euler(MiddleSecondPhalanx.Rot) * Quaternion.Euler(MiddleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
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
                RingFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightRingProximal);
                RingSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightRingIntermediate);
                RingThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightRingDistal);
                RingLastPhalanx = handAnimation.ScriptableHand.Get("RightRingEnd");
            }
            else
            {
                RingFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftRingProximal);
                RingSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftRingIntermediate);
                RingThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftRingDistal);
                RingLastPhalanx = handAnimation.ScriptableHand.Get("LeftRingEnd");
            }

            var matrixRingProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), Vector3.one);
            var posRingProximal = matrixRingProximal.MultiplyPoint3x4(RingFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posRingProximal, Quaternion.Euler(RingFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotRingProximal = Quaternion.identity;

            if (handAnimation.EditRing)
                rotRingProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot), matrixRingProximal.MultiplyPoint3x4(RingFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    var newTuple = new SpatialDataInfo(RingFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation)) * rotRingProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightRingProximal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftRingProximal, newTuple);
                }
            }

            var matrixRingIntermediate = Matrix4x4.TRS(posRingProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingIntermediate = matrixRingIntermediate.MultiplyPoint3x4(RingSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posRingIntermediate, Quaternion.Euler(RingSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRingIntermediate = Quaternion.identity;

            if (handAnimation.EditRing)
                rotRingIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot), posRingIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    var newTuple = new SpatialDataInfo(RingSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot)) * rotRingIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightRingIntermediate, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftRingIntermediate, newTuple);
                }
            }

            var matrixRingDistal = Matrix4x4.TRS(posRingIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingDistal = matrixRingDistal.MultiplyPoint3x4(RingThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posRingDistal, Quaternion.Euler(RingThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotRingDistal = Quaternion.identity;

            if (handAnimation.EditRing)
                rotRingDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot) * Quaternion.Euler(RingThirdPhalanx.Rot), posRingDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditRing)
                {
                    var newTuple = new SpatialDataInfo(RingThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot)) * rotRingDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightRingDistal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftRingDistal, newTuple);
                }
            }

            var matrixRingEnd = Matrix4x4.TRS(posRingDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Rot) * Quaternion.Euler(RingSecondPhalanx.Rot) * Quaternion.Euler(RingThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
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
                LittleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightLittleProximal);
                LittleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightLittleIntermediate);
                LittleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.RightLittleDistal);
                LittleLastPhalanx = handAnimation.ScriptableHand.Get("RightLittleEnd");
            }
            else
            {
                LittleFirstPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftLittleProximal);
                LittleSecondPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftLittleIntermediate);
                LittleThirdPhalanx = handAnimation.ScriptableHand.Get(BoneType.LeftLittleDistal);
                LittleLastPhalanx = handAnimation.ScriptableHand.Get("LeftLittleEnd");
            }

            var matrixLittleProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.ScriptableHand.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation), Vector3.one);
            var posLittleProximal = matrixLittleProximal.MultiplyPoint3x4(LittleFirstPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posLittleProximal, Quaternion.Euler(LittleFirstPhalanx.Pos), 0.01f, EventType.Repaint);

            Quaternion rotLittleProximal = Quaternion.identity;

            if (handAnimation.EditLittle)
                rotLittleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot), matrixLittleProximal.MultiplyPoint3x4(LittleFirstPhalanx.Pos));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    var newTuple = new SpatialDataInfo(LittleFirstPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation)) * rotLittleProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightLittleProximal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftLittleProximal, newTuple);
                }
            }

            var matrixLittleIntermediate = Matrix4x4.TRS(posLittleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleIntermediate = matrixLittleIntermediate.MultiplyPoint3x4(LittleSecondPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posLittleIntermediate, Quaternion.Euler(LittleSecondPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotLittleIntermediate = Quaternion.identity;

            if (handAnimation.EditLittle)
                rotLittleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot), posLittleIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    var newTuple = new SpatialDataInfo(LittleSecondPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot)) * rotLittleIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightLittleIntermediate, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftLittleIntermediate, newTuple);
                }
            }

            var matrixLittleDistal = Matrix4x4.TRS(posLittleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleDistal = matrixLittleDistal.MultiplyPoint3x4(LittleThirdPhalanx.Pos);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posLittleDistal, Quaternion.Euler(LittleThirdPhalanx.Rot), 0.01f, EventType.Repaint);

            Quaternion rotLittleDistal = Quaternion.identity;

            if (handAnimation.EditLittle)
                rotLittleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot) * Quaternion.Euler(LittleThirdPhalanx.Rot), posLittleDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.EditLittle)
                {
                    var newTuple = new SpatialDataInfo(LittleThirdPhalanx.Pos, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot)) * rotLittleDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.ScriptableHand.Set(BoneType.RightLittleDistal, newTuple);
                    else
                        handAnimation.ScriptableHand.Set(BoneType.LeftLittleDistal, newTuple);
                }
            }

            var matrixLittleEnd = Matrix4x4.TRS(posLittleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.ScriptableHand.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Rot) * Quaternion.Euler(LittleSecondPhalanx.Rot) * Quaternion.Euler(LittleThirdPhalanx.Rot), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleEnd = matrixLittleEnd.MultiplyPoint3x4(LittleLastPhalanx.Pos);

            Handles.SphereHandleCap(0, posLittleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion
        }

    }
   
}
