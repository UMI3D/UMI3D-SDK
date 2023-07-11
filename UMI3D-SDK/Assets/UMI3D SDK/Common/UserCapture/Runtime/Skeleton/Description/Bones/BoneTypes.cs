/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License = ""; Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing = ""; software
distributed under the License is distributed on an "AS IS" BASIS = "";
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND = ""; either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Indexes all the available bones in UMI3D.
    /// </summary>
    /// A bone is a part of the skeleton associated to a user. It represents a part of the human body.
    /// See complentary documentation for the exact position of each bone.
    [System.Serializable]
    public static class BoneType
    {
        public const uint None = 0;
        public const uint LeftEye = 1;
        public const uint RightEye = 2;
        public const uint Head = 3;
        public const uint LeftAnkle = 4;
        public const uint LeftForearm = 5;
        public const uint LeftHand = 6;
        public const uint LeftHip = 7;
        public const uint LeftToeBase = 8;
        public const uint LeftKnee = 9;
        public const uint LeftShoulder = 10;
        public const uint LeftUpperArm = 11;
        public const uint Neck = 12;
        public const uint Hips = 13;
        public const uint RightAnkle = 14;
        public const uint RightForearm = 15;
        public const uint RightHand = 16;
        public const uint RightHip = 17;
        public const uint RightToeBase = 18;
        public const uint RightKnee = 19;
        public const uint RightShoulder = 20;
        public const uint RightUpperArm = 21;
        public const uint Jaw = 22;
        public const uint UpperChest = 23;
        public const uint Chest = 24;
        public const uint Spine = 25;
        public const uint LastBone = 26;
        public const uint LeftThumbProximal = 27;
        public const uint LeftThumbIntermediate = 28;
        public const uint LeftThumbDistal = 29;
        public const uint LeftIndexProximal = 30;
        public const uint LeftIndexIntermediate = 31;
        public const uint LeftIndexDistal = 32;
        public const uint LeftMiddleProximal = 33;
        public const uint LeftMiddleIntermediate = 34;
        public const uint LeftMiddleDistal = 35;
        public const uint LeftRingProximal = 36;
        public const uint LeftRingIntermediate = 37;
        public const uint LeftRingDistal = 38;
        public const uint LeftLittleProximal = 39;
        public const uint LeftLittleIntermediate = 40;
        public const uint LeftLittleDistal = 41;
        public const uint RightThumbProximal = 42;
        public const uint RightThumbIntermediate = 43;
        public const uint RightThumbDistal = 44;
        public const uint RightIndexProximal = 45;
        public const uint RightIndexIntermediate = 46;
        public const uint RightIndexDistal = 47;
        public const uint RightMiddleProximal = 48;
        public const uint RightMiddleIntermediate = 49;
        public const uint RightMiddleDistal = 50;
        public const uint RightRingProximal = 51;
        public const uint RightRingIntermediate = 52;
        public const uint RightRingDistal = 53;
        public const uint RightLittleProximal = 54;
        public const uint RightLittleIntermediate = 55;
        public const uint RightLittleDistal = 56;
        /// <summary>
        /// Position on the floor between the feet
        /// </summary>
        public const uint CenterFeet = 57;
        /// <summary>
        /// Browser's camera
        /// </summary>
        public const uint Viewpoint = 58;
    }
}