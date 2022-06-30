/*
Copy 2019 - 2021 Inetum

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
    [RequireComponent(typeof(umi3d.edk.UMI3DNode))]
    public class BodyPoseSetter : MonoBehaviour
    {
        public string PoseName;

        public bool IsRelativeToNode = true;

        public bool ShowBody = false;

        public bool EditBodyPosition = false;
        public bool EditLeftLeg = false;
        public bool EditRightLeg = false;
        public bool EditLeftArm = false;
        public bool EditRightArm = false;
        public bool EditTrunk = false;

        public bool DrawLine = false;

        [HideInInspector]
        public Color BodyColor = Color.blue;
        [HideInInspector]
        public Color JointColor = new Color(1f, 0.5f, 0f);
        [HideInInspector]
        public Color LineColor = Color.green;

        [HideInInspector]
        public BodyDescription ScriptableBody;
        public UMI3DBodyPose BodyPose;

        public bool tempValueForTest = true;

        private void Reset()
        {
            ScriptableBody = ScriptableObject.CreateInstance<BodyDescription>();
            ScriptableBody.name = "Body Pose Information";
            SetBodyDictionary();
        }

        private void OnDestroy()
        {
            ScriptableObject.Destroy(ScriptableBody);
        }

        private void SetBodyDictionary()
        {
            //ScriptableBody.Add(nameof(BoneType.Hips), new SpatialDataInfo(Vector3.zero, Vector3.zero));

            ScriptableBody.Add(nameof(BoneType.Hips), new SpatialDataInfo(new Vector3(6.757011e-08f, 0.9979194f, 4.84474e-07f), Vector3.zero));

            ScriptableBody.Add(nameof(BoneType.LeftHip), new SpatialDataInfo(new Vector3(-0.09124453f, -0.06656399f, -0.0005537792f), new Vector3(-0.002f, 0, 0)));
            ScriptableBody.Add(nameof(BoneType.LeftKnee), new SpatialDataInfo(new Vector3(-0.002446802f, -0.405954f, -0.005170411f), new Vector3(0.004f, 0, 0)));
            ScriptableBody.Add(nameof(BoneType.LeftAnkle), new SpatialDataInfo(new Vector3(0.002446696f, -0.42048f, -0.02057641f), new Vector3(-0.002f, 0, 0)));
            ScriptableBody.Add(nameof(BoneType.LeftToeBase), new SpatialDataInfo(new Vector3(-0.003733598f, -0.1049219f, 0.1264066f), Vector3.zero));
            ScriptableBody.Add("LeftToeBaseEnd", new SpatialDataInfo(new Vector3(2.053664e-09f, 9.634096e-09f, 0.09992521f), Vector3.zero));

            ScriptableBody.Add(nameof(BoneType.RightHip), new SpatialDataInfo(new Vector3(0.09124453f, -0.06656399f, -0.0005537792f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.RightKnee), new SpatialDataInfo(new Vector3(0.002446802f, -0.405954f, -0.005170411f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.RightAnkle), new SpatialDataInfo(new Vector3(-0.002446696f, -0.42048f, -0.02057641f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.RightToeBase), new SpatialDataInfo(new Vector3(0.003733598f, -0.1049219f, 0.1264066f), Vector3.zero));
            ScriptableBody.Add("RightToeBaseEnd", new SpatialDataInfo(new Vector3(-1.13474e-09f, 4.587741e-09f, 0.09992521f), Vector3.zero));

            ScriptableBody.Add(nameof(BoneType.Spine), new SpatialDataInfo(new Vector3(-5.64832e-09f, 0.09923459f, -0.01227335f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.Chest), new SpatialDataInfo(new Vector3(1.388652e-06f, 0.1164544f, -0.01422341f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.UpperChest), new SpatialDataInfo(new Vector3(-9.144072e-07f, 0.133602f, -0.0162646f), Vector3.zero));

            ScriptableBody.Add(nameof(BoneType.Neck), new SpatialDataInfo(new Vector3(-2.520208e-07f, 0.1503249f, 0.007929048f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.Head), new SpatialDataInfo(new Vector3(-2.370748e-08f, 0.1032183f, 0.03142429f), Vector3.zero));
            ScriptableBody.Add("TopHeadEnd", new SpatialDataInfo(new Vector3(-1.541726e-06f, 0.1847467f, 0.06636399f), Vector3.zero));

            ScriptableBody.Add(nameof(BoneType.LeftShoulder), new SpatialDataInfo(new Vector3(-0.06105824f, 0.09110424f, 0.007055508f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.LeftUpperArm), new SpatialDataInfo(new Vector3(-0.1265504f, -0.002659345f, -0.02600922f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.LeftForearm), new SpatialDataInfo(new Vector3(-0.2740468f, 3.912668e-07f, 6.58811e-08f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.LeftHand), new SpatialDataInfo(new Vector3(-0.2761446f, 2.895181e-07f, 1.964659e-07f), new Vector3(0.003f, 0, 0)));
            ScriptableBody.Add("LeftHandEnd", new SpatialDataInfo(new Vector3(-0.1277552f, 1.334202e-07f, 2.041846e-07f), Vector3.zero));
            ScriptableBody.Add("LeftThumbEnd", new SpatialDataInfo(new Vector3(-0.03788809f, -0.02166997f, 0.03003088f), new Vector3(-0.001f, 0.001f, -0.001f)));

            ScriptableBody.Add(nameof(BoneType.RightShoulder), new SpatialDataInfo(new Vector3(0.06105824f, 0.09110424f, 0.007055508f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.RightUpperArm), new SpatialDataInfo(new Vector3(0.1265504f, -0.002659345f, -0.02600922f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.RightForearm), new SpatialDataInfo(new Vector3(0.2740468f, 3.912668e-07f, 6.58811e-08f), Vector3.zero));
            ScriptableBody.Add(nameof(BoneType.RightHand), new SpatialDataInfo(new Vector3(0.2761446f, 2.895181e-07f, 1.964659e-07f), new Vector3(0.003f, 0, 0)));
            ScriptableBody.Add("RightHandEnd", new SpatialDataInfo(new Vector3(0.1277552f, 8.945774e-08f, 1.801164e-07f), Vector3.zero));
            ScriptableBody.Add("RightThumbEnd", new SpatialDataInfo(new Vector3(0.03788809f, -0.02166998f, 0.03003087f), new Vector3(-0.001f, -0.001f, 0.001f)));
        }

        public void ResetDictionary()
        {
            ScriptableBody.JointsData.Clear();
            SetBodyDictionary();
            ScriptableBody.BodyPosition = Vector3.zero;
            ScriptableBody.BodyEulerRotation = Vector3.zero;
            SceneView.RepaintAll();
        }

        public void SavePose()
        {
            if (BodyPose != null)
            {
                BodyPose.PoseName = PoseName;

                BodyPose.isRelativeToNode = IsRelativeToNode;

                BodyPose.BodyPosition = ScriptableBody.BodyPosition;

                if (IsRelativeToNode)
                    BodyPose.BodyEulerRotation = ScriptableBody.BodyEulerRotation;
                else
                    BodyPose.BodyEulerRotation = (this.transform.rotation * Quaternion.Euler(ScriptableBody.BodyEulerRotation)).eulerAngles;

                BodyPose.JointRotations.Clear();

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.Hips, nameof(BoneType.Hips), ScriptableBody.Get(nameof(BoneType.Hips)).Rot));

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftHip, nameof(BoneType.LeftHip), ScriptableBody.Get(nameof(BoneType.LeftHip)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftKnee, nameof(BoneType.LeftKnee), ScriptableBody.Get(nameof(BoneType.LeftKnee)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftAnkle, nameof(BoneType.LeftAnkle), ScriptableBody.Get(nameof(BoneType.LeftAnkle)).Rot));

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightHip, nameof(BoneType.RightHip), ScriptableBody.Get(nameof(BoneType.RightHip)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightKnee, nameof(BoneType.RightKnee), ScriptableBody.Get(nameof(BoneType.RightKnee)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightAnkle, nameof(BoneType.RightAnkle), ScriptableBody.Get(nameof(BoneType.RightAnkle)).Rot));

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.Spine, nameof(BoneType.Spine), ScriptableBody.Get(nameof(BoneType.Spine)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.Chest, nameof(BoneType.Chest), ScriptableBody.Get(nameof(BoneType.Chest)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.UpperChest, nameof(BoneType.UpperChest), ScriptableBody.Get(nameof(BoneType.UpperChest)).Rot));

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.Neck, nameof(BoneType.Neck), ScriptableBody.Get(nameof(BoneType.Neck)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.Head, nameof(BoneType.Head), ScriptableBody.Get(nameof(BoneType.Head)).Rot));

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftShoulder, nameof(BoneType.LeftShoulder), ScriptableBody.Get(nameof(BoneType.LeftShoulder)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftUpperArm, nameof(BoneType.LeftUpperArm), ScriptableBody.Get(nameof(BoneType.LeftUpperArm)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftForearm, nameof(BoneType.LeftForearm), ScriptableBody.Get(nameof(BoneType.LeftForearm)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.LeftHand, nameof(BoneType.LeftHand), ScriptableBody.Get(nameof(BoneType.LeftHand)).Rot));

                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightShoulder, nameof(BoneType.RightShoulder), ScriptableBody.Get(nameof(BoneType.RightShoulder)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightUpperArm, nameof(BoneType.RightUpperArm), ScriptableBody.Get(nameof(BoneType.RightUpperArm)).Rot));
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightForearm, nameof(BoneType.RightForearm), ScriptableBody.Get(nameof(BoneType.RightForearm)).Rot)); ;
                BodyPose.JointRotations.Add(new UMI3DBodyPose.JointRotation(BoneType.RightHand, nameof(BoneType.RightHand), ScriptableBody.Get(nameof(BoneType.RightHand)).Rot));

                BodyPose.TargetTransforms.Clear();

                BodyPose.TargetTransforms.Add(new UMI3DBodyPose.TargetTransform(BoneType.Hips, nameof(BoneType.Hips), ScriptableBody.Get(nameof(BoneType.Hips)).Pos, ScriptableBody.Get(nameof(BoneType.Hips)).Rot));
                BodyPose.TargetTransforms.Add(new UMI3DBodyPose.TargetTransform(BoneType.LeftAnkle, nameof(BoneType.LeftAnkle), ScriptableBody.LeftAnklePosition, ScriptableBody.LeftAnkleEulerRotation));
                BodyPose.TargetTransforms.Add(new UMI3DBodyPose.TargetTransform(BoneType.RightAnkle, nameof(BoneType.RightAnkle), ScriptableBody.RightAnklePosition, ScriptableBody.RightAnkleEulerRotation));
                BodyPose.TargetTransforms.Add(new UMI3DBodyPose.TargetTransform(BoneType.LeftHand, nameof(BoneType.LeftHand), ScriptableBody.LeftHandPosition, ScriptableBody.LeftHandEulerRotation));
                BodyPose.TargetTransforms.Add(new UMI3DBodyPose.TargetTransform(BoneType.RightHand, nameof(BoneType.RightHand), ScriptableBody.RightHandPosition, ScriptableBody.RightHandEulerRotation));

                Debug.Log("Saved");
            }

            EditorUtility.SetDirty(BodyPose);
        }

        public void LoadPose()
        {
            if (BodyPose != null)
            {
                IsRelativeToNode = BodyPose.isRelativeToNode;
                PoseName = BodyPose.PoseName;

                ScriptableBody.BodyPosition = BodyPose.BodyPosition;

                if (IsRelativeToNode)
                {
                    ScriptableBody.BodyEulerRotation = BodyPose.BodyEulerRotation;
                }
                else
                {
                    ScriptableBody.BodyEulerRotation = (Quaternion.Inverse(this.transform.rotation) * Quaternion.Euler(BodyPose.BodyEulerRotation)).eulerAngles;
                }

                if (BodyPose.JointRotations.Count > 0)
                //if (BodyPose.TargetTransforms.Count > 0)
                {
                    foreach (UMI3DBodyPose.JointRotation pr in BodyPose.JointRotations)
                    //foreach (UMI3DBodyPose.TargetTransform tt in BodyPose.TargetTransforms)
                    {
                        ScriptableBody.SetRotation(pr.Joint, pr.JointEulerRotation);
                        //ScriptableBody.SetRotation(tt.Joint, tt.relativeRotation);
                    }
                }
                else
                {
                    ScriptableBody.JointsData.Clear();
                    SetBodyDictionary();
                }

                SceneView.RepaintAll();
            }
        }

        public void CreateLeftSymmetry()
        {
            Vector3 tempData;

            //tempData = ScriptableBody.Get(nameof(BoneType.ThumbProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftThumbProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftThumbProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.ThumbIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftThumbIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftThumbIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.ThumbDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftThumbDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftThumbDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.IndexProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftIndexProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftIndexProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.IndexIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftIndexIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftIndexIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.IndexDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftIndexDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftIndexDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.MiddleDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftMiddleDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftMiddleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.MiddleIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftMiddleIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftMiddleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.MiddleProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftMiddleProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftMiddleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.RingDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftRingDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftRingDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.RingIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftRingIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftRingIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.RingProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftRingProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftRingProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.LittleDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftLittleDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftLittleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LittleIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftLittleIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftLittleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LittleProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LeftLittleProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LeftLittleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            SceneView.RepaintAll();
        }

        public void CreateRightSymmetry()
        {
            Vector3 tempData;

            //tempData = ScriptableBody.Get(nameof(BoneType.LeftThumbProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.ThumbProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.ThumbProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftThumbIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.ThumbIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.ThumbIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftThumbDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.ThumbDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.ThumbDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.LeftIndexProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.IndexProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.IndexProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftIndexIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.IndexIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.IndexIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftIndexDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.IndexDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.IndexDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.LeftMiddleDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.MiddleDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.MiddleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftMiddleIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.MiddleIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.MiddleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftMiddleProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.MiddleProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.MiddleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.LeftRingDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.RingDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.RingDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftRingIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.RingIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.RingIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftRingProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.RingProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.RingProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            //tempData = ScriptableBody.Get(nameof(BoneType.LeftLittleDistal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LittleDistal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LittleDistal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftLittleIntermediate)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LittleIntermediate), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LittleIntermediate)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));
            //tempData = ScriptableBody.Get(nameof(BoneType.LeftLittleProximal)).Rot;
            //ScriptableBody.Set(nameof(BoneType.LittleProximal), new SpatialDataInfo(ScriptableBody.Get(nameof(BoneType.LittleProximal)).Pos, new Vector3(tempData.x, -tempData.y, -tempData.z)));

            SceneView.RepaintAll();
        }
    }
}
#endif