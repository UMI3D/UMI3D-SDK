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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace TestUtils.UserCapture
{
    public class HierarchyDefinitionLocal : IUMI3DSkeletonHierarchyDefinition
    {
        public IList<UMI3DSkeletonHierarchyDefinition.BoneRelation> Relations => relations;

        private readonly List<UMI3DSkeletonHierarchyDefinition.BoneRelation> relations = new();
    }

    public static class HierarchyTestHelper
    {
        public static UMI3DSkeletonHierarchy CreateTestHierarchy()
        {
            HierarchyDefinitionLocal hierarchyDef = new();
            hierarchyDef.Relations.Add(new(BoneType.Hips, BoneType.None, Vector3.zero));
            hierarchyDef.Relations.Add(new(BoneType.Chest, BoneType.Hips, Vector3.zero));
            hierarchyDef.Relations.Add(new(BoneType.Spine, BoneType.Chest, Vector3.zero));
            hierarchyDef.Relations.Add(new(BoneType.LeftForearm, BoneType.Chest, Vector3.zero));
            return new UMI3DSkeletonHierarchy(hierarchyDef);
        }
    }
}