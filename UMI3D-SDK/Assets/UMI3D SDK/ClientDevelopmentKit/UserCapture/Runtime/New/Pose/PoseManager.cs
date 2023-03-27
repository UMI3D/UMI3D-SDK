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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class PoseManager : SingleBehaviour<PoseManager>
    {
        List<UMI3DPose_so> clientPoses = new List<UMI3DPose_so>();

        public PoseDto defaultPose;
        public PoseDto[] localPoses;

        public Dictionary<ulong, List<PoseDto>> allPoses;

        private void Start()
        {
            localPoses = new PoseDto[clientPoses.Count];
            clientPoses.ForEach(p =>
            {
                localPoses.Append(p.ToDTO());
            });
        }

        public void SetPoses(Dictionary<ulong, List<PoseDto>> allPoses)
        {
            this.allPoses = allPoses;
        }

        public PoseDto GetPose(ulong key, int index)
        {
            List<PoseDto> poses = allPoses[key];
            return poses?[index];
        }
    }
}