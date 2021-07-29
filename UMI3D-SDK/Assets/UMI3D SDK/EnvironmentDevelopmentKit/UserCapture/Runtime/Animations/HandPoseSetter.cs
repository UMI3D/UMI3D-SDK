using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.edk;
using umi3d.common;
using umi3d.common.userCapture;
using System;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;

namespace umi3d.edk.userCapture
{
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class HandPoseSetter : MonoBehaviour
    {
        public string PoseName;

        public bool IsRelativeToNode = true;

        public bool IsRight = true;

        public bool IsVisible = false;

        public bool EditHandPosition = false;
        public bool EditThumb = false;
        public bool EditIndex = false;
        public bool EditMiddle = false;
        public bool EditRing = false;
        public bool EditLittle = false;
        public bool DrawLine = false;

        //[HideInInspector]
        //public Vector3 HandLocalPosition = Vector3.zero;
        //[HideInInspector]
        //public Vector3 HandLocalEulerRotation = Vector3.zero;

        [HideInInspector]
        public Color HandColor = Color.blue;
        [HideInInspector]
        public Color PhalanxColor = new Color(1f, 0.5f, 0f);
        [HideInInspector]
        public Color LineColor = Color.green;

        //[System.Serializable]
        //public class PhalanxRotation
        //{
        //    [ConstStringEnum(typeof(BoneType))]
        //    public string Phalanx;
        //    public Vector3 PhalanxEulerRotation;

        //    public PhalanxRotation(string BoneType, Vector3 rotation)
        //    {
        //        Phalanx = BoneType;
        //        PhalanxEulerRotation = rotation;
        //    }
        //}

        //public HandDescription ScriptableHandSave;
        [HideInInspector]
        public HandDescription ScriptableHand;
        public UMI3DHandPose HandPose;

        //public Dictionary<string, Tuple<Vector3, Vector3>> HandDictionary = new Dictionary<string, Tuple<Vector3, Vector3>>(); // phalanxLocalPos, rot

        private void Reset()
        {

            //HandColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            //PhalanxColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            ScriptableHand = ScriptableObject.CreateInstance<HandDescription>();
            ScriptableHand.name = "Hand Pose Information";
            ScriptableHand.IsRight = IsRight;
            SetHandDictionary();
        }

        private void OnDestroy()
        {
            ScriptableObject.Destroy(ScriptableHand);
        }

        // Optimize this
        void SetHandDictionary()
        {
            ScriptableHand.Add(BoneType.LeftThumbProximal.ToString(), new SpatialDataInfo(new Vector3(-0.03788809f, -0.02166997f, 0.03003088f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftThumbIntermediate.ToString(), new SpatialDataInfo(new Vector3(-3.675443f, -2.122008f, 2.122012f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftThumbDistal.ToString(), new SpatialDataInfo(new Vector3(-3.394359f, -1.959744f, 1.959793f), Vector3.zero));
            ScriptableHand.Add("LeftThumbEnd", new SpatialDataInfo(new Vector3(-2.679404f, -1.546945f, 1.54692f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftIndexProximal.ToString(), new SpatialDataInfo(new Vector3(-0.1226661f, -0.002316688f, 0.02822058f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftIndexIntermediate.ToString(), new SpatialDataInfo(new Vector3(-3.891967f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftIndexDistal.ToString(), new SpatialDataInfo(new Vector3(-3.415161f, 5.749108e-09f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftIndexEnd", new SpatialDataInfo(new Vector3(-3.077986f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftMiddleProximal.ToString(), new SpatialDataInfo(new Vector3(-0.1277552f, 1.334202e-07f, 2.041846e-07f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftMiddleIntermediate.ToString(), new SpatialDataInfo(new Vector3(-3.613972f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftMiddleDistal.ToString(), new SpatialDataInfo(new Vector3(-3.45976f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftMiddleEnd", new SpatialDataInfo(new Vector3(-3.680192f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftRingProximal.ToString(), new SpatialDataInfo(new Vector3(-0.12147f, 9.894983e-05f, -0.02216629f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftRingIntermediate.ToString(), new SpatialDataInfo(new Vector3(-3.60119f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftRingDistal.ToString(), new SpatialDataInfo(new Vector3(-3.307317f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftRingEnd", new SpatialDataInfo(new Vector3(-3.660115f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftLittleProximal.ToString(), new SpatialDataInfo(new Vector3(-0.1090819f, -0.00226365f, -0.04725818f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftLittleIntermediate.ToString(), new SpatialDataInfo(new Vector3(-4.136652f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftLittleDistal.ToString(), new SpatialDataInfo(new Vector3(-2.594836f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftLittleEnd", new SpatialDataInfo(new Vector3(-2.923869f, 0f, 0f), Vector3.zero));



            ScriptableHand.Add(BoneType.RightThumbProximal.ToString(), new SpatialDataInfo(new Vector3(0.03788812f, -0.02166999f, 0.03003085f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightThumbIntermediate.ToString(), new SpatialDataInfo(new Vector3(3.675445f, -2.122004f, 2.122034f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightThumbDistal.ToString(), new SpatialDataInfo(new Vector3(3.39436f, -1.959753f, 1.959789f), Vector3.zero));
            ScriptableHand.Add("RightThumbEnd", new SpatialDataInfo(new Vector3(2.679403f, -1.546945f, 1.546891f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightIndexProximal.ToString(), new SpatialDataInfo(new Vector3(0.1226662f, -0.002316732f, 0.02822055f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightIndexIntermediate.ToString(), new SpatialDataInfo(new Vector3(3.891967f, -7.205551e-08f, 3.751302e-05f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightIndexDistal.ToString(), new SpatialDataInfo(new Vector3(3.415161f, 2.529914e-07f, -8.605841e-06f), Vector3.zero));
            ScriptableHand.Add("RightIndexEnd", new SpatialDataInfo(new Vector3(3.077986f, -9.272842e-07f, -8.780579e-06f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightMiddleProximal.ToString(), new SpatialDataInfo(new Vector3(0.1277552f, 8.945774e-08f, 1.801164e-07f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightMiddleIntermediate.ToString(), new SpatialDataInfo(new Vector3(3.613972f, 2.756502e-06f, 3.080337e-06f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightMiddleDistal.ToString(), new SpatialDataInfo(new Vector3(3.45976f, -1.972182e-07f, -2.645789e-05f), Vector3.zero));
            ScriptableHand.Add("RightMiddleEnd", new SpatialDataInfo(new Vector3(3.680192f, -6.576055e-07f, 3.250981e-05f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightRingProximal.ToString(), new SpatialDataInfo(new Vector3(0.12147f, 9.894557e-05f, -0.02216627f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightRingIntermediate.ToString(), new SpatialDataInfo(new Vector3(3.601193f, 0f, 2.384185e-05f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightRingDistal.ToString(), new SpatialDataInfo(new Vector3(3.30732f, 0f, -1.341104e-05f), Vector3.zero));
            ScriptableHand.Add("RightRingEnd", new SpatialDataInfo(new Vector3(3.660113f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightLittleProximal.ToString(), new SpatialDataInfo(new Vector3(0.1090819f, -0.002263658f, -0.04725829f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightLittleIntermediate.ToString(), new SpatialDataInfo(new Vector3(4.136657f, 0f, 3.576278e-05f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightLittleDistal.ToString(), new SpatialDataInfo(new Vector3(2.594835f, 0f, -3.576278e-05f), Vector3.zero));
            ScriptableHand.Add("RightLittleEnd", new SpatialDataInfo(new Vector3(2.923873f, 0f, 1.112792e-05f), Vector3.zero));
        }

        public void ResetDictionary()
        {
            ScriptableHand.PhalangesData.Clear();
            SetHandDictionary();
            ScriptableHand.HandPosition = Vector3.zero;
            ScriptableHand.HandEulerRotation = Vector3.zero;
            SceneView.RepaintAll();
        }

        public void SavePose()
        {
            if (HandPose != null)
            {
                HandPose.PoseName = PoseName;
                HandPose.IsRight = IsRight;
                HandPose.isRelativeToNode = IsRelativeToNode;

                HandPose.HandPosition = ScriptableHand.HandPosition;

                if (IsRelativeToNode)
                    HandPose.HandEulerRotation = ScriptableHand.HandEulerRotation;
                else
                    HandPose.HandEulerRotation = (this.transform.rotation * Quaternion.Euler(ScriptableHand.HandEulerRotation)).eulerAngles;


                HandPose.PhalanxRotations.Clear();

                if (IsRight)
                {
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbProximal.ToString(), ScriptableHand.Get(BoneType.RightThumbProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbIntermediate.ToString(), ScriptableHand.Get(BoneType.RightThumbIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbDistal.ToString(), ScriptableHand.Get(BoneType.RightThumbDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexProximal.ToString(), ScriptableHand.Get(BoneType.RightIndexProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexIntermediate.ToString(), ScriptableHand.Get(BoneType.RightIndexIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexDistal.ToString(), ScriptableHand.Get(BoneType.RightIndexDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleProximal.ToString(), ScriptableHand.Get(BoneType.RightMiddleProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleIntermediate.ToString(), ScriptableHand.Get(BoneType.RightMiddleIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleDistal.ToString(), ScriptableHand.Get(BoneType.RightMiddleDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingProximal.ToString(), ScriptableHand.Get(BoneType.RightRingProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingIntermediate.ToString(), ScriptableHand.Get(BoneType.RightRingIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingDistal.ToString(), ScriptableHand.Get(BoneType.RightRingDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleProximal.ToString(), ScriptableHand.Get(BoneType.RightLittleProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleIntermediate.ToString(), ScriptableHand.Get(BoneType.RightLittleIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleDistal.ToString(), ScriptableHand.Get(BoneType.RightLittleDistal.ToString()).Rot));

                }
                else
                {
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbProximal.ToString(), ScriptableHand.Get(BoneType.LeftThumbProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbIntermediate.ToString(), ScriptableHand.Get(BoneType.LeftThumbIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbDistal.ToString(), ScriptableHand.Get(BoneType.LeftThumbDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexProximal.ToString(), ScriptableHand.Get(BoneType.LeftIndexProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexIntermediate.ToString(), ScriptableHand.Get(BoneType.LeftIndexIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexDistal.ToString(), ScriptableHand.Get(BoneType.LeftIndexDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleProximal.ToString(), ScriptableHand.Get(BoneType.LeftMiddleProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleIntermediate.ToString(), ScriptableHand.Get(BoneType.LeftMiddleIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleDistal.ToString(), ScriptableHand.Get(BoneType.LeftMiddleDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingProximal.ToString(), ScriptableHand.Get(BoneType.LeftRingProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingIntermediate.ToString(), ScriptableHand.Get(BoneType.LeftRingIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingDistal.ToString(), ScriptableHand.Get(BoneType.LeftRingDistal.ToString()).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleProximal.ToString(), ScriptableHand.Get(BoneType.LeftLittleProximal.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleIntermediate.ToString(), ScriptableHand.Get(BoneType.LeftLittleIntermediate.ToString()).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleDistal.ToString(), ScriptableHand.Get(BoneType.LeftLittleDistal.ToString()).Rot));
                }
            }
        }

        public void LoadPose()
        {
            if (HandPose != null)
            {
                IsVisible = true;
                IsRight = ScriptableHand.IsRight = HandPose.IsRight;
                IsRelativeToNode = HandPose.isRelativeToNode;
                PoseName = HandPose.PoseName;

                ScriptableHand.IsRight = HandPose.IsRight;

                ScriptableHand.HandPosition = HandPose.HandPosition;

                if (IsRelativeToNode)
                    ScriptableHand.HandEulerRotation = HandPose.HandEulerRotation;
                else
                    ScriptableHand.HandEulerRotation = (Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(HandPose.HandEulerRotation)).eulerAngles;

                if (HandPose.PhalanxRotations.Count > 0)
                    foreach (UMI3DHandPose.PhalanxRotation pr in HandPose.PhalanxRotations)
                    {
                        ScriptableHand.SetRotation(pr.Phalanx, pr.PhalanxEulerRotation);
                    }
                else
                {
                    ScriptableHand.PhalangesData.Clear();
                    SetHandDictionary();
                }


                SceneView.RepaintAll();
            }
        }
    }
}
