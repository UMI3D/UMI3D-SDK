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

using umi3d.common.userCapture;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Allow the edition of hand poses directly in the environment.
    /// </summary>
    /// Displays gizmos for all the bones and enables to move them relatively one to another.
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class HandPoseSetter : MonoBehaviour
    {
        /// <summary>
        /// Name of the hand pose.
        /// </summary
        [Tooltip("Name of the hand pose")]
        public string PoseName;

        /// <summary>
        /// Should the pose have a precise position reatively to the node?
        /// </summary>
        [Tooltip("Should the pose have a precise position reatively to the node?")]
        public bool IsRelativeToNode = true;

        /// <summary>
        /// Display the right hand in the editor.
        /// </summary>
        [Tooltip("Display the right hand in the editor.")]
        public bool ShowRightHand = false;
        /// <summary>
        /// Display the left hand in the editor.
        /// </summary>
        [Tooltip("Display the left hand in the editor.")]
        public bool ShowLeftHand = false;

        public bool EditHandPosition = false;
        /// <summary>
        /// Displays the gizmos to move the thumb finger's bones.
        /// </summary>
        [Tooltip("Displays the gizmos to move the thumb finger's bones.")]
        public bool EditThumb = false;
        /// <summary>
        /// Displays the gizmos to move the index finger's bones.
        /// </summary>
        [Tooltip("Displays the gizmos to move the index finger's bones.")]
        public bool EditIndex = false;
        /// <summary>
        /// Displays the gizmos to move the middle finger's bones.
        /// </summary>
        [Tooltip("Displays the gizmos to move the middle finger's bones.")]
        public bool EditMiddle = false;
        /// <summary>
        /// Displays the gizmos to move the ring finger's bones.
        /// </summary>
        [Tooltip("Displays the gizmos to move the ring finger's bones.")]
        public bool EditRing = false;
        /// <summary>
        /// Displays the gizmos to move the little finger's bones.
        /// </summary>
        [Tooltip("Displays the gizmos to move the little finger's bones.")]
        public bool EditLittle = false;

        /// <summary>
        /// Displays line gizmos between bones to improve the vizualisation.
        /// </summary>
        [Tooltip("Displays line gizmos between bones to improve the vizualisation.")]
        public bool DrawLine = false;

        /// <summary>
        /// Color of the hand in the editor.
        /// </summary>
        [HideInInspector]
        public Color HandColor = Color.blue;
        /// <summary>
        /// Color of the phalanx joints in the editor.
        /// </summary>
        [HideInInspector]
        public Color PhalanxColor = new Color(1f, 0.5f, 0f);
        /// <summary>
        /// Color of the helping line gizmos in the editor.
        /// </summary>
        [HideInInspector]
        public Color LineColor = Color.green;

        [HideInInspector]
        public HandDescription ScriptableHand;
        /// <summary>
        /// The hand pose to edit.
        /// </summary>
        [Tooltip("The hand pose to edit.")]
        public UMI3DHandPose HandPose;

        // todo To remove.
        public bool tempValueForTest = true;

        private void Reset()
        {
            ScriptableHand = ScriptableObject.CreateInstance<HandDescription>();
            ScriptableHand.name = "Hand Pose Information";
            SetHandDictionary();
        }

        private void OnDestroy()
        {
            ScriptableObject.Destroy(ScriptableHand);
        }

        /// <summary>
        /// Set hand bones' positions to default values.
        /// </summary>
        private void SetHandDictionary()
        {
            ScriptableHand.Add(nameof(BoneType.LeftThumbProximal), new SpatialDataInfo(new Vector3(-0.03788809f, -0.02166997f, 0.03003088f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftThumbIntermediate), new SpatialDataInfo(new Vector3(-3.675443f, -2.122008f, 2.122012f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftThumbDistal), new SpatialDataInfo(new Vector3(-3.394359f, -1.959744f, 1.959793f), Vector3.zero));
            ScriptableHand.Add("LeftThumbEnd", new SpatialDataInfo(new Vector3(-2.679404f, -1.546945f, 1.54692f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.LeftIndexProximal), new SpatialDataInfo(new Vector3(-0.1226661f, -0.002316688f, 0.02822058f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftIndexIntermediate), new SpatialDataInfo(new Vector3(-3.891967f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftIndexDistal), new SpatialDataInfo(new Vector3(-3.415161f, 5.749108e-09f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftIndexEnd", new SpatialDataInfo(new Vector3(-3.077986f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.LeftMiddleProximal), new SpatialDataInfo(new Vector3(-0.1277552f, 1.334202e-07f, 2.041846e-07f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftMiddleIntermediate), new SpatialDataInfo(new Vector3(-3.613972f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftMiddleDistal), new SpatialDataInfo(new Vector3(-3.45976f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftMiddleEnd", new SpatialDataInfo(new Vector3(-3.680192f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.LeftRingProximal), new SpatialDataInfo(new Vector3(-0.12147f, 9.894983e-05f, -0.02216629f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftRingIntermediate), new SpatialDataInfo(new Vector3(-3.60119f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftRingDistal), new SpatialDataInfo(new Vector3(-3.307317f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftRingEnd", new SpatialDataInfo(new Vector3(-3.660115f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.LeftLittleProximal), new SpatialDataInfo(new Vector3(-0.1090819f, -0.00226365f, -0.04725818f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftLittleIntermediate), new SpatialDataInfo(new Vector3(-4.136652f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.LeftLittleDistal), new SpatialDataInfo(new Vector3(-2.594836f, 0f, 0f), Vector3.zero));
            ScriptableHand.Add("LeftLittleEnd", new SpatialDataInfo(new Vector3(-2.923869f, 0f, 0f), Vector3.zero));



            ScriptableHand.Add(nameof(BoneType.RightThumbProximal), new SpatialDataInfo(new Vector3(0.03788812f, -0.02166999f, 0.03003085f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightThumbIntermediate), new SpatialDataInfo(new Vector3(3.675445f, -2.122004f, 2.122034f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightThumbDistal), new SpatialDataInfo(new Vector3(3.39436f, -1.959753f, 1.959789f), Vector3.zero));
            ScriptableHand.Add("RightThumbEnd", new SpatialDataInfo(new Vector3(2.679403f, -1.546945f, 1.546891f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.RightIndexProximal), new SpatialDataInfo(new Vector3(0.1226662f, -0.002316732f, 0.02822055f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightIndexIntermediate), new SpatialDataInfo(new Vector3(3.891967f, -7.205551e-08f, 3.751302e-05f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightIndexDistal), new SpatialDataInfo(new Vector3(3.415161f, 2.529914e-07f, -8.605841e-06f), Vector3.zero));
            ScriptableHand.Add("RightIndexEnd", new SpatialDataInfo(new Vector3(3.077986f, -9.272842e-07f, -8.780579e-06f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.RightMiddleProximal), new SpatialDataInfo(new Vector3(0.1277552f, 8.945774e-08f, 1.801164e-07f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightMiddleIntermediate), new SpatialDataInfo(new Vector3(3.613972f, 2.756502e-06f, 3.080337e-06f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightMiddleDistal), new SpatialDataInfo(new Vector3(3.45976f, -1.972182e-07f, -2.645789e-05f), Vector3.zero));
            ScriptableHand.Add("RightMiddleEnd", new SpatialDataInfo(new Vector3(3.680192f, -6.576055e-07f, 3.250981e-05f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.RightRingProximal), new SpatialDataInfo(new Vector3(0.12147f, 9.894557e-05f, -0.02216627f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightRingIntermediate), new SpatialDataInfo(new Vector3(3.601193f, 0f, 2.384185e-05f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightRingDistal), new SpatialDataInfo(new Vector3(3.30732f, 0f, -1.341104e-05f), Vector3.zero));
            ScriptableHand.Add("RightRingEnd", new SpatialDataInfo(new Vector3(3.660113f, 0f, 0f), Vector3.zero));

            ScriptableHand.Add(nameof(BoneType.RightLittleProximal), new SpatialDataInfo(new Vector3(0.1090819f, -0.002263658f, -0.04725829f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightLittleIntermediate), new SpatialDataInfo(new Vector3(4.136657f, 0f, 3.576278e-05f), Vector3.zero));
            ScriptableHand.Add(nameof(BoneType.RightLittleDistal), new SpatialDataInfo(new Vector3(2.594835f, 0f, -3.576278e-05f), Vector3.zero));
            ScriptableHand.Add("RightLittleEnd", new SpatialDataInfo(new Vector3(2.923873f, 0f, 1.112792e-05f), Vector3.zero));
        }

        public void ResetDictionary()
        {
            ScriptableHand.PhalangesData.Clear();
            SetHandDictionary();
            ScriptableHand.RightHandPosition = Vector3.zero;
            ScriptableHand.RightHandEulerRotation = Vector3.zero;
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Save an edited hand pose.
        /// </summary>
        public void SavePose()
        {
            if (HandPose != null)
            {
                HandPose.PoseName = PoseName;

                HandPose.isRelativeToNode = IsRelativeToNode;

                HandPose.RightHandPosition = ScriptableHand.RightHandPosition;
                HandPose.LeftHandPosition = ScriptableHand.LeftHandPosition;

                if (IsRelativeToNode)
                {
                    HandPose.RightHandEulerRotation = ScriptableHand.RightHandEulerRotation;
                    HandPose.LeftHandEulerRotation = ScriptableHand.LeftHandEulerRotation;
                }
                else
                {
                    HandPose.RightHandEulerRotation = (this.transform.rotation * Quaternion.Euler(ScriptableHand.RightHandEulerRotation)).eulerAngles;
                    HandPose.LeftHandEulerRotation = (this.transform.rotation * Quaternion.Euler(ScriptableHand.LeftHandEulerRotation)).eulerAngles;
                }

                HandPose.PhalanxRotations.Clear();

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbProximal, nameof(BoneType.RightThumbProximal), ScriptableHand.Get(nameof(BoneType.RightThumbProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbIntermediate, nameof(BoneType.RightThumbIntermediate), ScriptableHand.Get(nameof(BoneType.RightThumbIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightThumbDistal, nameof(BoneType.RightThumbDistal), ScriptableHand.Get(nameof(BoneType.RightThumbDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexProximal, nameof(BoneType.RightIndexProximal), ScriptableHand.Get(nameof(BoneType.RightIndexProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexIntermediate, nameof(BoneType.RightIndexIntermediate), ScriptableHand.Get(nameof(BoneType.RightIndexIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightIndexDistal, nameof(BoneType.RightIndexDistal), ScriptableHand.Get(nameof(BoneType.RightIndexDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleProximal, nameof(BoneType.RightMiddleProximal), ScriptableHand.Get(nameof(BoneType.RightMiddleProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleIntermediate, nameof(BoneType.RightMiddleIntermediate), ScriptableHand.Get(nameof(BoneType.RightMiddleIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightMiddleDistal, nameof(BoneType.RightMiddleDistal), ScriptableHand.Get(nameof(BoneType.RightMiddleDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingProximal, nameof(BoneType.RightRingProximal), ScriptableHand.Get(nameof(BoneType.RightRingProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingIntermediate, nameof(BoneType.RightRingIntermediate), ScriptableHand.Get(nameof(BoneType.RightRingIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightRingDistal, nameof(BoneType.RightRingDistal), ScriptableHand.Get(nameof(BoneType.RightRingDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleProximal, nameof(BoneType.RightLittleProximal), ScriptableHand.Get(nameof(BoneType.RightLittleProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleIntermediate, nameof(BoneType.RightLittleIntermediate), ScriptableHand.Get(nameof(BoneType.RightLittleIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.RightLittleDistal, nameof(BoneType.RightLittleDistal), ScriptableHand.Get(nameof(BoneType.RightLittleDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbProximal, nameof(BoneType.LeftThumbProximal), ScriptableHand.Get(nameof(BoneType.LeftThumbProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbIntermediate, nameof(BoneType.LeftThumbIntermediate), ScriptableHand.Get(nameof(BoneType.LeftThumbIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftThumbDistal, nameof(BoneType.LeftThumbDistal), ScriptableHand.Get(nameof(BoneType.LeftThumbDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexProximal, nameof(BoneType.LeftIndexProximal), ScriptableHand.Get(nameof(BoneType.LeftIndexProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexIntermediate, nameof(BoneType.LeftIndexIntermediate), ScriptableHand.Get(nameof(BoneType.LeftIndexIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftIndexDistal, nameof(BoneType.LeftIndexDistal), ScriptableHand.Get(nameof(BoneType.LeftIndexDistal)).Rot)); ;

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleProximal, nameof(BoneType.LeftMiddleProximal), ScriptableHand.Get(nameof(BoneType.LeftMiddleProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleIntermediate, nameof(BoneType.LeftMiddleIntermediate), ScriptableHand.Get(nameof(BoneType.LeftMiddleIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftMiddleDistal, nameof(BoneType.LeftMiddleDistal), ScriptableHand.Get(nameof(BoneType.LeftMiddleDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingProximal, nameof(BoneType.LeftRingProximal), ScriptableHand.Get(nameof(BoneType.LeftRingProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingIntermediate, nameof(BoneType.LeftRingIntermediate), ScriptableHand.Get(nameof(BoneType.LeftRingIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftRingDistal, nameof(BoneType.LeftRingDistal), ScriptableHand.Get(nameof(BoneType.LeftRingDistal)).Rot));

                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleProximal, nameof(BoneType.LeftLittleProximal), ScriptableHand.Get(nameof(BoneType.LeftLittleProximal)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleIntermediate, nameof(BoneType.LeftLittleIntermediate), ScriptableHand.Get(nameof(BoneType.LeftLittleIntermediate)).Rot));
                HandPose.PhalanxRotations.Add(new UMI3DHandPose.PhalanxRotation(BoneType.LeftLittleDistal, nameof(BoneType.LeftLittleDistal), ScriptableHand.Get(nameof(BoneType.LeftLittleDistal)).Rot));
            }

            EditorUtility.SetDirty(HandPose);
        }

        /// <summary>
        /// Load a hand pose.
        /// </summary>
        public void LoadPose()
        {
            if (HandPose != null)
            {
                IsRelativeToNode = HandPose.isRelativeToNode;
                PoseName = HandPose.PoseName;

                ScriptableHand.RightHandPosition = HandPose.RightHandPosition;
                ScriptableHand.LeftHandPosition = HandPose.LeftHandPosition;

                if (IsRelativeToNode)
                {
                    ScriptableHand.RightHandEulerRotation = HandPose.RightHandEulerRotation;
                    ScriptableHand.LeftHandEulerRotation = HandPose.LeftHandEulerRotation;
                }
                else
                {
                    ScriptableHand.RightHandEulerRotation = (Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(HandPose.RightHandEulerRotation)).eulerAngles;
                    ScriptableHand.LeftHandEulerRotation = (Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(HandPose.RightHandEulerRotation)).eulerAngles;
                }

                if (HandPose.PhalanxRotations.Count > 0)
                {
                    foreach (UMI3DHandPose.PhalanxRotation pr in HandPose.PhalanxRotations)
                    {
                        ScriptableHand.SetRotation(pr.Phalanx, pr.PhalanxEulerRotation);
                    }
                }
                else
                {
                    ScriptableHand.PhalangesData.Clear();
                    SetHandDictionary();
                }

                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Transfer the settings of the right hand to the left hand by planar symmetry.
        /// </summary>
        public void CreateLeftSymmetry()
        {
            Vector3 tempData;

            tempData = ScriptableHand.Get(nameof(BoneType.RightThumbProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftThumbProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftThumbProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightThumbIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftThumbIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftThumbIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightThumbDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftThumbDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftThumbDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.RightIndexProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftIndexProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftIndexProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightIndexIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftIndexIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftIndexIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightIndexDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftIndexDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftIndexDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.RightMiddleDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftMiddleDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftMiddleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightMiddleIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftMiddleIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftMiddleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightMiddleProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftMiddleProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftMiddleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.RightRingDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftRingDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftRingDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightRingIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftRingIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftRingIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightRingProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftRingProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftRingProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.RightLittleDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftLittleDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftLittleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightLittleIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftLittleIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftLittleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.RightLittleProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.LeftLittleProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.LeftLittleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.RightHandPosition;
            ScriptableHand.LeftHandPosition = new Vector3(-tempData.x, tempData.y, tempData.z);

            tempData = ScriptableHand.RightHandEulerRotation;
            ScriptableHand.LeftHandEulerRotation = new Vector3(tempData.x, -tempData.y, -tempData.z);

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Transfer the settings of the left hand to the right hand by planar symmetry.
        /// </summary>
        public void CreateRightSymmetry()
        {
            Vector3 tempData;

            tempData = ScriptableHand.Get(nameof(BoneType.LeftThumbProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightThumbProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightThumbProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftThumbIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightThumbIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightThumbIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftThumbDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightThumbDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightThumbDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.LeftIndexProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightIndexProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightIndexProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftIndexIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightIndexIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightIndexIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftIndexDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightIndexDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightIndexDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.LeftMiddleDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightMiddleDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightMiddleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftMiddleIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightMiddleIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightMiddleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftMiddleProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightMiddleProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightMiddleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.LeftRingDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightRingDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightRingDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftRingIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightRingIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightRingIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftRingProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightRingProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightRingProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.Get(nameof(BoneType.LeftLittleDistal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightLittleDistal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightLittleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftLittleIntermediate)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightLittleIntermediate), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightLittleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            tempData = ScriptableHand.Get(nameof(BoneType.LeftLittleProximal)).Rot;
            ScriptableHand.Set(nameof(BoneType.RightLittleProximal), new SpatialDataInfo(ScriptableHand.Get(nameof(BoneType.RightLittleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            tempData = ScriptableHand.LeftHandPosition;
            ScriptableHand.RightHandPosition = new Vector3(-tempData.x, tempData.y, tempData.z);

            tempData = ScriptableHand.LeftHandEulerRotation;
            ScriptableHand.RightHandEulerRotation = new Vector3(tempData.x, -tempData.y, -tempData.z);

            SceneView.RepaintAll();
        }
    }
}
#endif