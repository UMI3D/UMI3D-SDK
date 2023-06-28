/*
Copyright 2019 - 2023 Inetum

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

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace umi3d.common.userCapture
{
    public static class BoneTypeHelper
    {
        private static readonly Dictionary<uint, string> BoneNames = new();

        public static Dictionary<uint, string> GetBoneNames()
        {
            if (BoneNames.Count > 0)
                return BoneNames;

            var keyValues = typeof(BoneType).GetFields().Select(f => (f.Name, boneType: (uint)f.GetRawConstantValue()));
            foreach (var keyValue in keyValues)
            {
                BoneNames.Add(keyValue.boneType, keyValue.Name);
            }
            return BoneNames;
        }

        public static string GetBoneName(uint bone)
        {
            return GetBoneNames().ContainsKey(bone) ? BoneNames[bone] : "Custom bone";
        }
    }
}