using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.edk;
using umi3d.common;
using umi3d.common.userCapture;
using System;
using UnityEditor;
using UnityEngine.UI;

namespace umi3d.edk.userCapture
{
    public class UMI3DHandAnimation : UMI3DNodeAnimation
    {
        public string Name;
        public bool IsVisible = false;

        public bool IsRight = true;

        public bool Hand = false;
        public bool Thumb = false;
        public bool Index = false;
        public bool Middle = false;
        public bool Ring = false;
        public bool Little = false; 

        [HideInInspector]
        public Vector3 HandLocalPosition = Vector3.zero;
        [HideInInspector]
        public Vector3 HandLocalEulerRotation = Vector3.zero;

        [HideInInspector]
        public Color HandColor = Color.blue;
        [HideInInspector]
        public Color PhalanxColor = new Color(1f, 0.5f, 0f);

        [System.Serializable]
        public class PhalanxRotation
        {
            [ConstStringEnum(typeof(BoneType))]
            public string Phalanx;
            public Vector3 PhalanxEulerRotation;

            public PhalanxRotation(string BoneType, Vector3 rotation)
            {
                Phalanx = BoneType;
                PhalanxEulerRotation = rotation;
            }
        }

        public Dictionary<string, Tuple<Vector3, Vector3>> HandDictionary = new Dictionary<string, Tuple<Vector3, Vector3>>(); // phalanxLocalPos, rot

        private void Reset()
        {

            //HandColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            //PhalanxColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            SetHandDictionary();

        }

        // Optimize this
        void SetHandDictionary()
        {
            HandDictionary.Add(BoneType.LeftThumbProximal, new Tuple<Vector3, Vector3>(new Vector3(-0.03788809f, -0.02166997f, 0.03003088f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftThumbIntermediate, new Tuple<Vector3, Vector3>(new Vector3(-3.675443f, -2.122008f, 2.122012f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftThumbDistal, new Tuple<Vector3, Vector3>(new Vector3(-3.394359f, -1.959744f, 1.959793f), Vector3.zero));
            HandDictionary.Add("LeftThumbEnd", new Tuple<Vector3, Vector3>(new Vector3(-2.679404f, -1.546945f, 1.54692f), Vector3.zero));

            HandDictionary.Add(BoneType.LeftIndexProximal, new Tuple<Vector3, Vector3>(new Vector3(-0.1226661f, -0.002316688f, 0.02822058f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftIndexIntermediate, new Tuple<Vector3, Vector3>(new Vector3(-3.891967f, 0f, 0f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftIndexDistal, new Tuple<Vector3, Vector3>(new Vector3(-3.415161f, 5.749108e-09f, 0f), Vector3.zero));
            HandDictionary.Add("LeftIndexEnd", new Tuple<Vector3, Vector3>(new Vector3(-3.077986f, 0f, 0f), Vector3.zero));

            HandDictionary.Add(BoneType.LeftMiddleProximal, new Tuple<Vector3, Vector3>(new Vector3(-0.1277552f, 1.334202e-07f, 2.041846e-07f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftMiddleIntermediate, new Tuple<Vector3, Vector3>(new Vector3(-3.613972f, 0f, 0f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftMiddleDistal, new Tuple<Vector3, Vector3>(new Vector3(-3.45976f, 0f, 0f), Vector3.zero));
            HandDictionary.Add("LeftMiddleEnd", new Tuple<Vector3, Vector3>(new Vector3(-3.680192f, 0f, 0f), Vector3.zero));

            HandDictionary.Add(BoneType.LeftRingProximal, new Tuple<Vector3, Vector3>(new Vector3(-0.12147f, 9.894983e-05f, -0.02216629f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftRingIntermediate, new Tuple<Vector3, Vector3>(new Vector3(-3.60119f, 0f, 0f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftRingDistal, new Tuple<Vector3, Vector3>(new Vector3(-3.307317f, 0f, 0f), Vector3.zero));
            HandDictionary.Add("LeftRingEnd", new Tuple<Vector3, Vector3>(new Vector3(-3.660115f, 0f, 0f), Vector3.zero));

            HandDictionary.Add(BoneType.LeftLittleProximal, new Tuple<Vector3, Vector3>(new Vector3(-0.1090819f, -0.00226365f, -0.04725818f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftLittleIntermediate, new Tuple<Vector3, Vector3>(new Vector3(-4.136652f, 0f, 0f), Vector3.zero));
            HandDictionary.Add(BoneType.LeftLittleDistal, new Tuple<Vector3, Vector3>(new Vector3(-2.594836f, 0f, 0f), Vector3.zero));
            HandDictionary.Add("LeftLittleEnd", new Tuple<Vector3, Vector3>(new Vector3(-2.923869f, 0f, 0f), Vector3.zero));



            HandDictionary.Add(BoneType.RightThumbProximal, new Tuple<Vector3, Vector3>(new Vector3(0.03788812f, -0.02166999f, 0.03003085f), Vector3.zero));
            HandDictionary.Add(BoneType.RightThumbIntermediate, new Tuple<Vector3, Vector3>(new Vector3(3.675445f, -2.122004f, 2.122034f), Vector3.zero));
            HandDictionary.Add(BoneType.RightThumbDistal, new Tuple<Vector3, Vector3>(new Vector3(3.39436f, -1.959753f, 1.959789f), Vector3.zero));
            HandDictionary.Add("RightThumbEnd", new Tuple<Vector3, Vector3>(new Vector3(2.679403f, -1.546945f, 1.546891f), Vector3.zero));

            HandDictionary.Add(BoneType.RightIndexProximal, new Tuple<Vector3, Vector3>(new Vector3(0.1226662f, -0.002316732f, 0.02822055f), Vector3.zero));
            HandDictionary.Add(BoneType.RightIndexIntermediate, new Tuple<Vector3, Vector3>(new Vector3(3.891967f, -7.205551e-08f, 3.751302e-05f), Vector3.zero));
            HandDictionary.Add(BoneType.RightIndexDistal, new Tuple<Vector3, Vector3>(new Vector3(3.415161f, 2.529914e-07f, -8.605841e-06f), Vector3.zero));
            HandDictionary.Add("RightIndexEnd", new Tuple<Vector3, Vector3>(new Vector3(3.077986f, -9.272842e-07f, -8.780579e-06f), Vector3.zero));

            HandDictionary.Add(BoneType.RightMiddleProximal, new Tuple<Vector3, Vector3>(new Vector3(0.1277552f, 8.945774e-08f, 1.801164e-07f), Vector3.zero));
            HandDictionary.Add(BoneType.RightMiddleIntermediate, new Tuple<Vector3, Vector3>(new Vector3(3.613972f, 2.756502e-06f, 3.080337e-06f), Vector3.zero));
            HandDictionary.Add(BoneType.RightMiddleDistal, new Tuple<Vector3, Vector3>(new Vector3(3.45976f, -1.972182e-07f, -2.645789e-05f), Vector3.zero));
            HandDictionary.Add("RightMiddleEnd", new Tuple<Vector3, Vector3>(new Vector3(3.680192f, -6.576055e-07f, 3.250981e-05f), Vector3.zero));

            HandDictionary.Add(BoneType.RightRingProximal, new Tuple<Vector3, Vector3>(new Vector3(0.12147f, 9.894557e-05f, -0.02216627f), Vector3.zero));
            HandDictionary.Add(BoneType.RightRingIntermediate, new Tuple<Vector3, Vector3>(new Vector3(3.601193f, 0f, 2.384185e-05f), Vector3.zero));
            HandDictionary.Add(BoneType.RightRingDistal, new Tuple<Vector3, Vector3>(new Vector3(3.30732f, 0f, -1.341104e-05f), Vector3.zero));
            HandDictionary.Add("RightRingEnd", new Tuple<Vector3, Vector3>(new Vector3(3.660113f, 0f, 0f), Vector3.zero));

            HandDictionary.Add(BoneType.RightLittleProximal, new Tuple<Vector3, Vector3>(new Vector3(0.1090819f, -0.002263658f, -0.04725829f), Vector3.zero));
            HandDictionary.Add(BoneType.RightLittleIntermediate, new Tuple<Vector3, Vector3>(new Vector3(4.136657f, 0f, 3.576278e-05f), Vector3.zero));
            HandDictionary.Add(BoneType.RightLittleDistal, new Tuple<Vector3, Vector3>(new Vector3(2.594835f, 0f, -3.576278e-05f), Vector3.zero));
            HandDictionary.Add("RightLittleEnd", new Tuple<Vector3, Vector3>(new Vector3(2.923873f, 0f, 1.112792e-05f), Vector3.zero));
        }

        public void ResetDictionary()
        {
            HandDictionary.Clear();
            SetHandDictionary();
            HandLocalPosition = Vector3.zero;
            HandLocalEulerRotation = Vector3.zero;
            SceneView.RepaintAll();
        }
    }
}
