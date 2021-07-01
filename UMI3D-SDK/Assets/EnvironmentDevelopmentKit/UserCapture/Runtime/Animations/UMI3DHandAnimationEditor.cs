using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using umi3d.edk.userCapture;
using umi3d.common.userCapture;

[CustomEditor(typeof(UMI3DHandAnimation)), CanEditMultipleObjects]
public class UMI3DHandAnimationEditor : Editor
{
    // custom inspector
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UMI3DHandAnimation handAnimation = (UMI3DHandAnimation)target;
        if (GUILayout.Button("Reset Hand"))
        {
            handAnimation.ResetDictionary();
        }
    }

    private void OnSceneGUI()
    {
        UMI3DHandAnimation handAnimation = (UMI3DHandAnimation)target;

        if (handAnimation != null && handAnimation.IsVisible)
        {
            #region handPosition

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.HandColor;

            Handles.SphereHandleCap(0, handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), 0.02f, EventType.Repaint);

            Vector3 HandLocalPos = Vector3.zero;
            Quaternion HandLocalRot = Quaternion.identity;

            if (handAnimation.Hand)
            {
                HandLocalPos = Handles.PositionHandle(handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation));
                HandLocalRot = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), handAnimation.transform.position + handAnimation.HandLocalPosition);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Hand)
                {
                    handAnimation.HandLocalPosition = HandLocalPos - handAnimation.transform.position;
                    handAnimation.HandLocalEulerRotation = (Quaternion.Inverse(handAnimation.transform.rotation) * HandLocalRot).eulerAngles;
                }
            }

            #endregion


            #region thumb

            System.Tuple<Vector3, Vector3> ThumbFirstPhalanx;
            System.Tuple<Vector3, Vector3> ThumbSecondPhalanx;
            System.Tuple<Vector3, Vector3> ThumbThirdPhalanx;
            System.Tuple<Vector3, Vector3> ThumbLastPhalanx;

            if (handAnimation.IsRight)
            {
                ThumbFirstPhalanx = handAnimation.HandDictionary[BoneType.RightThumbProximal];
                ThumbSecondPhalanx = handAnimation.HandDictionary[BoneType.RightThumbIntermediate];
                ThumbThirdPhalanx = handAnimation.HandDictionary[BoneType.RightThumbDistal];
                ThumbLastPhalanx = handAnimation.HandDictionary["RightThumbEnd"];
            }
            else
            {
                ThumbFirstPhalanx = handAnimation.HandDictionary[BoneType.LeftThumbProximal];
                ThumbSecondPhalanx = handAnimation.HandDictionary[BoneType.LeftThumbIntermediate];
                ThumbThirdPhalanx = handAnimation.HandDictionary[BoneType.LeftThumbDistal];
                ThumbLastPhalanx = handAnimation.HandDictionary["LeftThumbEnd"];
            }

            var matrixThumbProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), Vector3.one);
            var posThumbProximal = matrixThumbProximal.MultiplyPoint3x4(ThumbFirstPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posThumbProximal, Quaternion.Euler(ThumbFirstPhalanx.Item1), 0.01f, EventType.Repaint);

            Quaternion rotThumbProximal = Quaternion.identity;

            if (handAnimation.Thumb)
                rotThumbProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2), matrixThumbProximal.MultiplyPoint3x4(ThumbFirstPhalanx.Item1));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Thumb)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(ThumbFirstPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation)) * rotThumbProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightThumbProximal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftThumbProximal] = newTuple;
                }
            }       

            var matrixThumbIntermediate = Matrix4x4.TRS(posThumbProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbIntermediate = matrixThumbIntermediate.MultiplyPoint3x4(ThumbSecondPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posThumbIntermediate, Quaternion.Euler(ThumbSecondPhalanx.Item2), 0.01f, EventType.Repaint);

            Quaternion rotThumbIntermediate = Quaternion.identity;

            if (handAnimation.Thumb)
                rotThumbIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2) * Quaternion.Euler(ThumbSecondPhalanx.Item2), posThumbIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Thumb)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(ThumbSecondPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2)) * rotThumbIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightThumbIntermediate] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftThumbIntermediate] = newTuple;
                }
            }

            var matrixThumbDistal = Matrix4x4.TRS(posThumbIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2) * Quaternion.Euler(ThumbSecondPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbDistal = matrixThumbDistal.MultiplyPoint3x4(ThumbThirdPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posThumbDistal, Quaternion.Euler(handAnimation.HandDictionary[BoneType.RightThumbDistal].Item2), 0.01f, EventType.Repaint);

            Quaternion rotThumbDistal = Quaternion.identity;

            if (handAnimation.Thumb)
                rotThumbDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2) * Quaternion.Euler(ThumbSecondPhalanx.Item2) * Quaternion.Euler(ThumbThirdPhalanx.Item2), posThumbDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Thumb)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(ThumbThirdPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2) * Quaternion.Euler(ThumbSecondPhalanx.Item2)) * rotThumbDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightThumbDistal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftThumbDistal] = newTuple;
                }
            }

            var matrixThumbEnd = Matrix4x4.TRS(posThumbDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(ThumbFirstPhalanx.Item2) * Quaternion.Euler(ThumbSecondPhalanx.Item2) * Quaternion.Euler(ThumbThirdPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posThumbEnd = matrixThumbEnd.MultiplyPoint3x4(ThumbLastPhalanx.Item1);

            Handles.SphereHandleCap(0, posThumbEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region index

            System.Tuple<Vector3, Vector3> IndexFirstPhalanx;
            System.Tuple<Vector3, Vector3> IndexSecondPhalanx;
            System.Tuple<Vector3, Vector3> IndexThirdPhalanx;
            System.Tuple<Vector3, Vector3> IndexLastPhalanx;

            if (handAnimation.IsRight)
            {
                IndexFirstPhalanx = handAnimation.HandDictionary[BoneType.RightIndexProximal];
                IndexSecondPhalanx = handAnimation.HandDictionary[BoneType.RightIndexIntermediate];
                IndexThirdPhalanx = handAnimation.HandDictionary[BoneType.RightIndexDistal];
                IndexLastPhalanx = handAnimation.HandDictionary["RightIndexEnd"];
            }
            else
            {
                IndexFirstPhalanx = handAnimation.HandDictionary[BoneType.LeftIndexProximal];
                IndexSecondPhalanx = handAnimation.HandDictionary[BoneType.LeftIndexIntermediate];
                IndexThirdPhalanx = handAnimation.HandDictionary[BoneType.LeftIndexDistal];
                IndexLastPhalanx = handAnimation.HandDictionary["LeftIndexEnd"];
            }

            var matrixIndexProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), Vector3.one);
            var posIndexProximal = matrixThumbProximal.MultiplyPoint3x4(IndexFirstPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posIndexProximal, Quaternion.Euler(IndexFirstPhalanx.Item1), 0.01f, EventType.Repaint);

            Quaternion rotIndexProximal = Quaternion.identity;

            if (handAnimation.Index)
                rotIndexProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2), matrixIndexProximal.MultiplyPoint3x4(IndexFirstPhalanx.Item1));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Index)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(IndexFirstPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation)) * rotIndexProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightIndexProximal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftIndexProximal] = newTuple;
                }
            }

            var matrixIndexIntermediate = Matrix4x4.TRS(posIndexProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexIntermediate = matrixIndexIntermediate.MultiplyPoint3x4(IndexSecondPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posIndexIntermediate, Quaternion.Euler(IndexSecondPhalanx.Item2), 0.01f, EventType.Repaint);

            Quaternion rotIndexIntermediate = Quaternion.identity;
            
            if (handAnimation.Index)
                rotIndexIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2) * Quaternion.Euler(IndexSecondPhalanx.Item2), posIndexIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Index)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(IndexSecondPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2)) * rotIndexIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightIndexIntermediate] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftIndexIntermediate] = newTuple;
                }
            }

            var matrixIndexDistal = Matrix4x4.TRS(posIndexIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2) * Quaternion.Euler(IndexSecondPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexDistal = matrixIndexDistal.MultiplyPoint3x4(IndexThirdPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posIndexDistal, Quaternion.Euler(IndexThirdPhalanx.Item2), 0.01f, EventType.Repaint);

            Quaternion rotIndexDistal = Quaternion.identity;
            
            if (handAnimation.Index)
                rotIndexDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2) * Quaternion.Euler(IndexSecondPhalanx.Item2) * Quaternion.Euler(IndexThirdPhalanx.Item2), posIndexDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Index)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(IndexThirdPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2) * Quaternion.Euler(IndexSecondPhalanx.Item2)) * rotIndexDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightIndexDistal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftIndexDistal] = newTuple;
                }
            }

            var matrixIndexEnd = Matrix4x4.TRS(posIndexDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(IndexFirstPhalanx.Item2) * Quaternion.Euler(IndexSecondPhalanx.Item2) * Quaternion.Euler(IndexThirdPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posIndexEnd = matrixIndexEnd.MultiplyPoint3x4(IndexLastPhalanx.Item1);

            Handles.SphereHandleCap(0, posIndexEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region middle

            System.Tuple<Vector3, Vector3> MiddleFirstPhalanx;
            System.Tuple<Vector3, Vector3> MiddleSecondPhalanx;
            System.Tuple<Vector3, Vector3> MiddleThirdPhalanx;
            System.Tuple<Vector3, Vector3> MiddleLastPhalanx;

            if (handAnimation.IsRight)
            {
                MiddleFirstPhalanx = handAnimation.HandDictionary[BoneType.RightMiddleProximal];
                MiddleSecondPhalanx = handAnimation.HandDictionary[BoneType.RightMiddleIntermediate];
                MiddleThirdPhalanx = handAnimation.HandDictionary[BoneType.RightMiddleDistal];
                MiddleLastPhalanx = handAnimation.HandDictionary["RightMiddleEnd"];
            }
            else
            {
                MiddleFirstPhalanx = handAnimation.HandDictionary[BoneType.LeftMiddleProximal];
                MiddleSecondPhalanx = handAnimation.HandDictionary[BoneType.LeftMiddleIntermediate];
                MiddleThirdPhalanx = handAnimation.HandDictionary[BoneType.LeftMiddleDistal];
                MiddleLastPhalanx = handAnimation.HandDictionary["LeftMiddleEnd"];
            }

            var matrixMiddleProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), Vector3.one);
            var posMiddleProximal = matrixMiddleProximal.MultiplyPoint3x4(MiddleFirstPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posMiddleProximal, Quaternion.Euler(MiddleFirstPhalanx.Item1), 0.01f, EventType.Repaint);

            Quaternion rotMiddleProximal = Quaternion.identity;
            
            if (handAnimation.Middle)
                rotMiddleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2), matrixMiddleProximal.MultiplyPoint3x4(MiddleFirstPhalanx.Item1));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Middle)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(MiddleFirstPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation)) * rotMiddleProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightMiddleProximal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftMiddleProximal] = newTuple;
                }
            }

            var matrixMiddleIntermediate = Matrix4x4.TRS(posMiddleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleIntermediate = matrixMiddleIntermediate.MultiplyPoint3x4(MiddleSecondPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posMiddleIntermediate, Quaternion.Euler(MiddleSecondPhalanx.Item2), 0.01f, EventType.Repaint);
            
            Quaternion rotMiddleIntermediate = Quaternion.identity;

            if (handAnimation.Middle)
                rotMiddleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2) * Quaternion.Euler(MiddleSecondPhalanx.Item2), posMiddleIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Middle)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(MiddleSecondPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2)) * rotMiddleIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightMiddleIntermediate] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftMiddleIntermediate] = newTuple;
                }
            }

            var matrixMiddleDistal = Matrix4x4.TRS(posMiddleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2) * Quaternion.Euler(MiddleSecondPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleDistal = matrixMiddleDistal.MultiplyPoint3x4(MiddleThirdPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posMiddleDistal, Quaternion.Euler(MiddleThirdPhalanx.Item2), 0.01f, EventType.Repaint);
            
            Quaternion rotMiddleDistal = Quaternion.identity;

            if (handAnimation.Middle)
                rotMiddleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2) * Quaternion.Euler(MiddleSecondPhalanx.Item2) * Quaternion.Euler(MiddleThirdPhalanx.Item2), posMiddleDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Middle)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(MiddleThirdPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2) * Quaternion.Euler(MiddleSecondPhalanx.Item2)) * rotMiddleDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightMiddleDistal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftMiddleDistal] = newTuple;
                }
            }

            var matrixMiddleEnd = Matrix4x4.TRS(posMiddleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(MiddleFirstPhalanx.Item2) * Quaternion.Euler(MiddleSecondPhalanx.Item2) * Quaternion.Euler(MiddleThirdPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posMiddleEnd = matrixMiddleEnd.MultiplyPoint3x4(MiddleLastPhalanx.Item1);

            Handles.SphereHandleCap(0, posMiddleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region ring

            System.Tuple<Vector3, Vector3> RingFirstPhalanx;
            System.Tuple<Vector3, Vector3> RingSecondPhalanx;
            System.Tuple<Vector3, Vector3> RingThirdPhalanx;
            System.Tuple<Vector3, Vector3> RingLastPhalanx;

            if (handAnimation.IsRight)
            {
                RingFirstPhalanx = handAnimation.HandDictionary[BoneType.RightRingProximal];
                RingSecondPhalanx = handAnimation.HandDictionary[BoneType.RightRingIntermediate];
                RingThirdPhalanx = handAnimation.HandDictionary[BoneType.RightRingDistal];
                RingLastPhalanx = handAnimation.HandDictionary["RightRingEnd"];
            }
            else
            {
                RingFirstPhalanx = handAnimation.HandDictionary[BoneType.LeftRingProximal];
                RingSecondPhalanx = handAnimation.HandDictionary[BoneType.LeftRingIntermediate];
                RingThirdPhalanx = handAnimation.HandDictionary[BoneType.LeftRingDistal];
                RingLastPhalanx = handAnimation.HandDictionary["LeftRingEnd"];
            }

            var matrixRingProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), Vector3.one);
            var posRingProximal = matrixRingProximal.MultiplyPoint3x4(RingFirstPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posRingProximal, Quaternion.Euler(RingFirstPhalanx.Item1), 0.01f, EventType.Repaint);

            Quaternion rotRingProximal = Quaternion.identity;

            if (handAnimation.Ring)
                rotRingProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2), matrixRingProximal.MultiplyPoint3x4(RingFirstPhalanx.Item1));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Ring)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(RingFirstPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation)) * rotRingProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightRingProximal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftRingProximal] = newTuple;
                }
            }
           
            var matrixRingIntermediate = Matrix4x4.TRS(posRingProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingIntermediate = matrixRingIntermediate.MultiplyPoint3x4(RingSecondPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posRingIntermediate, Quaternion.Euler(RingSecondPhalanx.Item2), 0.01f, EventType.Repaint);
            
            Quaternion rotRingIntermediate = Quaternion.identity;

            if (handAnimation.Ring)
                rotRingIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2) * Quaternion.Euler(RingSecondPhalanx.Item2), posRingIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Ring)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(RingSecondPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2)) * rotRingIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightRingIntermediate] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftRingIntermediate] = newTuple;
                }
            }

            var matrixRingDistal = Matrix4x4.TRS(posRingIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2) * Quaternion.Euler(RingSecondPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingDistal = matrixRingDistal.MultiplyPoint3x4(RingThirdPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posRingDistal, Quaternion.Euler(RingThirdPhalanx.Item2), 0.01f, EventType.Repaint);
            
            Quaternion rotRingDistal = Quaternion.identity;

            if (handAnimation.Ring)
                rotRingDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2) * Quaternion.Euler(RingSecondPhalanx.Item2) * Quaternion.Euler(RingThirdPhalanx.Item2), posRingDistal);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Ring)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(RingThirdPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2) * Quaternion.Euler(RingSecondPhalanx.Item2)) * rotRingDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightRingDistal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftRingDistal] = newTuple;
                }
            }

            var matrixRingEnd = Matrix4x4.TRS(posRingDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(RingFirstPhalanx.Item2) * Quaternion.Euler(RingSecondPhalanx.Item2) * Quaternion.Euler(RingThirdPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posRingEnd = matrixRingEnd.MultiplyPoint3x4(RingLastPhalanx.Item1);

            Handles.SphereHandleCap(0, posRingEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion


            #region little

            System.Tuple<Vector3, Vector3> LittleFirstPhalanx;
            System.Tuple<Vector3, Vector3> LittleSecondPhalanx;
            System.Tuple<Vector3, Vector3> LittleThirdPhalanx;
            System.Tuple<Vector3, Vector3> LittleLastPhalanx;

            if (handAnimation.IsRight)
            {
                LittleFirstPhalanx = handAnimation.HandDictionary[BoneType.RightLittleProximal];
                LittleSecondPhalanx = handAnimation.HandDictionary[BoneType.RightLittleIntermediate];
                LittleThirdPhalanx = handAnimation.HandDictionary[BoneType.RightLittleDistal];
                LittleLastPhalanx = handAnimation.HandDictionary["RightLittleEnd"];
            }
            else
            {
                LittleFirstPhalanx = handAnimation.HandDictionary[BoneType.LeftLittleProximal];
                LittleSecondPhalanx = handAnimation.HandDictionary[BoneType.LeftLittleIntermediate];
                LittleThirdPhalanx = handAnimation.HandDictionary[BoneType.LeftLittleDistal];
                LittleLastPhalanx = handAnimation.HandDictionary["LeftLittleEnd"];
            }

            var matrixLittleProximal = Matrix4x4.TRS(handAnimation.transform.position + handAnimation.HandLocalPosition, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation), Vector3.one);
            var posLittleProximal = matrixLittleProximal.MultiplyPoint3x4(LittleFirstPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.color = handAnimation.PhalanxColor;
            Handles.SphereHandleCap(0, posLittleProximal, Quaternion.Euler(LittleFirstPhalanx.Item1), 0.01f, EventType.Repaint);

            Quaternion rotLittleProximal = Quaternion.identity;

            if (handAnimation.Little)
                rotLittleProximal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2), matrixLittleProximal.MultiplyPoint3x4(LittleFirstPhalanx.Item1));

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Little)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(LittleFirstPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation)) * rotLittleProximal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightLittleProximal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftLittleProximal] = newTuple;
                }
            }

            var matrixLittleIntermediate = Matrix4x4.TRS(posLittleProximal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleIntermediate = matrixLittleIntermediate.MultiplyPoint3x4(LittleSecondPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posLittleIntermediate, Quaternion.Euler(LittleSecondPhalanx.Item2), 0.01f, EventType.Repaint);
            
            Quaternion rotLittleIntermediate = Quaternion.identity;

            if (handAnimation.Little)
                rotLittleIntermediate = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2) * Quaternion.Euler(LittleSecondPhalanx.Item2), posLittleIntermediate);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Little)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(LittleSecondPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2)) * rotLittleIntermediate).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightLittleIntermediate] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftLittleIntermediate] = newTuple;
                }
            }

            var matrixLittleDistal = Matrix4x4.TRS(posLittleIntermediate, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2) * Quaternion.Euler(LittleSecondPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleDistal = matrixLittleDistal.MultiplyPoint3x4(LittleThirdPhalanx.Item1);

            EditorGUI.BeginChangeCheck();

            Handles.SphereHandleCap(0, posLittleDistal, Quaternion.Euler(LittleThirdPhalanx.Item2), 0.01f, EventType.Repaint);
           
            Quaternion rotLittleDistal = Quaternion.identity;

            if (handAnimation.Little)
                rotLittleDistal = Handles.RotationHandle(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2) * Quaternion.Euler(LittleSecondPhalanx.Item2) * Quaternion.Euler(LittleThirdPhalanx.Item2), posLittleDistal);

            if (EditorGUI.EndChangeCheck())
            {
                if (handAnimation.Little)
                {
                    var newTuple = new System.Tuple<Vector3, Vector3>(LittleThirdPhalanx.Item1, (Quaternion.Inverse(handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2) * Quaternion.Euler(LittleSecondPhalanx.Item2)) * rotLittleDistal).eulerAngles);
                    if (handAnimation.IsRight)
                        handAnimation.HandDictionary[BoneType.RightLittleDistal] = newTuple;
                    else
                        handAnimation.HandDictionary[BoneType.LeftLittleDistal] = newTuple;
                }
            }

            var matrixLittleEnd = Matrix4x4.TRS(posLittleDistal, handAnimation.transform.rotation * Quaternion.Euler(handAnimation.HandLocalEulerRotation) * Quaternion.Euler(LittleFirstPhalanx.Item2) * Quaternion.Euler(LittleSecondPhalanx.Item2) * Quaternion.Euler(LittleThirdPhalanx.Item2), new Vector3(0.01f, 0.01f, 0.01f));
            var posLittleEnd = matrixLittleEnd.MultiplyPoint3x4(LittleLastPhalanx.Item1);

            Handles.SphereHandleCap(0, posLittleEnd, Quaternion.identity, 0.01f, EventType.Repaint);

            #endregion
        }

    }
   
}
