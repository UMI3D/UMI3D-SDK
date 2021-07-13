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
    public class HandPoseSetter : MonoBehaviour
    {
        public string Name;

        public bool IsRight = true;

        public bool IsVisible = false;

        public bool EditHandPosition = false;
        public bool EditThumb = false;
        public bool EditIndex = false;
        public bool EditMiddle = false;
        public bool EditRing = false;
        public bool EditLittle = false; 

        //[HideInInspector]
        //public Vector3 HandLocalPosition = Vector3.zero;
        //[HideInInspector]
        //public Vector3 HandLocalEulerRotation = Vector3.zero;

        [HideInInspector]
        public Color HandColor = Color.blue;
        [HideInInspector]
        public Color PhalanxColor = new Color(1f, 0.5f, 0f);

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
            SetHandDictionary(); // doit etre remplacé par un truc en dur
        }

        private void OnDestroy()
        {
            ScriptableObject.Destroy(ScriptableHand);
        }

        // Optimize this
        void SetHandDictionary()
        {
            ScriptableHand.Add(BoneType.LeftThumbProximal, new SpatialDataInfo(new Vector3(-0.03788809f, -0.02166997f, 0.03003088f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftThumbIntermediate, new SpatialDataInfo(new Vector3(-3.675443f, -2.122008f, 2.122012f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftThumbDistal, new SpatialDataInfo(new Vector3(-3.394359f, -1.959744f, 1.959793f), Vector3.zero));
            ScriptableHand.Add("LeftThumbEnd", new SpatialDataInfo(new Vector3(-2.679404f, -1.546945f, 1.54692f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftIndexProximal, new SpatialDataInfo(new Vector3(-0.1226661f, -0.002316688f, 0.02822058f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftIndexIntermediate, new SpatialDataInfo(new Vector3(-3.891967f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftIndexDistal, new SpatialDataInfo(new Vector3(-3.415161f, 5.749108e-09f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftIndexEnd", new SpatialDataInfo(new Vector3(-3.077986f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftMiddleProximal, new SpatialDataInfo(new Vector3(-0.1277552f, 1.334202e-07f, 2.041846e-07f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftMiddleIntermediate, new SpatialDataInfo(new Vector3(-3.613972f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftMiddleDistal, new SpatialDataInfo(new Vector3(-3.45976f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftMiddleEnd", new SpatialDataInfo(new Vector3(-3.680192f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftRingProximal, new SpatialDataInfo(new Vector3(-0.12147f, 9.894983e-05f, -0.02216629f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftRingIntermediate, new SpatialDataInfo(new Vector3(-3.60119f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftRingDistal, new SpatialDataInfo(new Vector3(-3.307317f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftRingEnd", new SpatialDataInfo(new Vector3(-3.660115f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.LeftLittleProximal, new SpatialDataInfo(new Vector3(-0.1090819f, -0.00226365f, -0.04725818f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftLittleIntermediate, new SpatialDataInfo(new Vector3(-4.136652f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(BoneType.LeftLittleDistal, new SpatialDataInfo(new Vector3(-2.594836f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftLittleEnd", new SpatialDataInfo(new Vector3(-2.923869f, 0f, 0f), Vector3.zero));



            ScriptableHand.Add(BoneType.RightThumbProximal, new SpatialDataInfo(new Vector3(0.03788812f, -0.02166999f, 0.03003085f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightThumbIntermediate, new SpatialDataInfo(new Vector3(3.675445f, -2.122004f, 2.122034f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightThumbDistal, new SpatialDataInfo(new Vector3(3.39436f, -1.959753f, 1.959789f), Vector3.zero));
            ScriptableHand.Add("RightThumbEnd", new SpatialDataInfo(new Vector3(2.679403f, -1.546945f, 1.546891f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightIndexProximal, new SpatialDataInfo(new Vector3(0.1226662f, -0.002316732f, 0.02822055f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightIndexIntermediate, new SpatialDataInfo(new Vector3(3.891967f, -7.205551e-08f, 3.751302e-05f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightIndexDistal, new SpatialDataInfo(new Vector3(3.415161f, 2.529914e-07f, -8.605841e-06f), Vector3.zero));
            ScriptableHand.Add("RightIndexEnd", new SpatialDataInfo(new Vector3(3.077986f, -9.272842e-07f, -8.780579e-06f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightMiddleProximal, new SpatialDataInfo(new Vector3(0.1277552f, 8.945774e-08f, 1.801164e-07f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightMiddleIntermediate, new SpatialDataInfo(new Vector3(3.613972f, 2.756502e-06f, 3.080337e-06f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightMiddleDistal, new SpatialDataInfo(new Vector3(3.45976f, -1.972182e-07f, -2.645789e-05f), Vector3.zero));
            ScriptableHand.Add("RightMiddleEnd", new SpatialDataInfo(new Vector3(3.680192f, -6.576055e-07f, 3.250981e-05f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightRingProximal, new SpatialDataInfo(new Vector3(0.12147f, 9.894557e-05f, -0.02216627f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightRingIntermediate, new SpatialDataInfo(new Vector3(3.601193f, 0f, 2.384185e-05f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightRingDistal, new SpatialDataInfo(new Vector3(3.30732f, 0f, -1.341104e-05f), Vector3.zero));
            ScriptableHand.Add("RightRingEnd", new SpatialDataInfo(new Vector3(3.660113f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(BoneType.RightLittleProximal, new SpatialDataInfo(new Vector3(0.1090819f, -0.002263658f, -0.04725829f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightLittleIntermediate, new SpatialDataInfo(new Vector3(4.136657f, 0f, 3.576278e-05f), Vector3.zero));
            ScriptableHand.Add(BoneType.RightLittleDistal, new SpatialDataInfo(new Vector3(2.594835f, 0f, -3.576278e-05f), Vector3.zero));
            ScriptableHand.Add("RightLittleEnd", new SpatialDataInfo(new Vector3(2.923873f, 0f, 1.112792e-05f), Vector3.zero));
        }

        public void ResetDictionary()
        {
            ScriptableHand.PhalangesData.Clear();
            SetHandDictionary();
            ScriptableHand.HandLocalPosition = Vector3.zero;
            ScriptableHand.HandLocalEulerRotation = Vector3.zero;
            SceneView.RepaintAll();
        }

        public void SavePose()
        {
            if (HandPose != null)
            {
                HandPose.Name = Name;
                HandPose.IsRight = IsRight;

                HandPose.HandLocalPosition = ScriptableHand.HandLocalPosition;
                HandPose.HandLocalEulerRotation = ScriptableHand.HandLocalEulerRotation;

                HandPose.PhalanxRotations.Clear();

                if (IsRight)
                {
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbProximal, ScriptableHand.Get(BoneType.RightThumbProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbIntermediate, ScriptableHand.Get(BoneType.RightThumbIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbDistal, ScriptableHand.Get(BoneType.RightThumbDistal).Rot));
                    
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexProximal, ScriptableHand.Get(BoneType.RightIndexProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexIntermediate, ScriptableHand.Get(BoneType.RightIndexIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexDistal, ScriptableHand.Get(BoneType.RightIndexDistal).Rot));
                    
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleProximal, ScriptableHand.Get(BoneType.RightMiddleProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleIntermediate, ScriptableHand.Get(BoneType.RightMiddleIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleDistal, ScriptableHand.Get(BoneType.RightMiddleDistal).Rot));
                    
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingProximal, ScriptableHand.Get(BoneType.RightRingProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingIntermediate, ScriptableHand.Get(BoneType.RightRingIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingDistal, ScriptableHand.Get(BoneType.RightRingDistal).Rot));
                    
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleProximal, ScriptableHand.Get(BoneType.RightLittleProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleIntermediate, ScriptableHand.Get(BoneType.RightLittleIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleDistal, ScriptableHand.Get(BoneType.RightLittleDistal).Rot));

                }
                else
                {
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbProximal, ScriptableHand.Get(BoneType.LeftThumbProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbIntermediate, ScriptableHand.Get(BoneType.LeftThumbIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbDistal, ScriptableHand.Get(BoneType.LeftThumbDistal).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexProximal, ScriptableHand.Get(BoneType.LeftIndexProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexIntermediate, ScriptableHand.Get(BoneType.LeftIndexIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexDistal, ScriptableHand.Get(BoneType.LeftIndexDistal).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleProximal, ScriptableHand.Get(BoneType.LeftMiddleProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleIntermediate, ScriptableHand.Get(BoneType.LeftMiddleIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleDistal, ScriptableHand.Get(BoneType.LeftMiddleDistal).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingProximal, ScriptableHand.Get(BoneType.LeftRingProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingIntermediate, ScriptableHand.Get(BoneType.LeftRingIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingDistal, ScriptableHand.Get(BoneType.LeftRingDistal).Rot));

                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleProximal, ScriptableHand.Get(BoneType.LeftLittleProximal).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleIntermediate, ScriptableHand.Get(BoneType.LeftLittleIntermediate).Rot));
                    HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleDistal, ScriptableHand.Get(BoneType.LeftLittleDistal).Rot));
                }
            }
        }

        public void LoadPose()
        {
            if (HandPose != null)
            {
                IsVisible = true;
                IsRight = ScriptableHand.IsRight = HandPose.IsRight;

                ScriptableHand.IsRight = HandPose.IsRight;

                ScriptableHand.HandLocalPosition = HandPose.HandLocalPosition;
                ScriptableHand.HandLocalEulerRotation = HandPose.HandLocalEulerRotation;

                foreach (UMI3DHandPose.PhalanxRotation pr in HandPose.PhalanxRotations)
                {
                    ScriptableHand.SetRotation(pr.Phalanx, pr.PhalanxEulerRotation);
                }

                SceneView.RepaintAll();
            }
        }
    }
}
