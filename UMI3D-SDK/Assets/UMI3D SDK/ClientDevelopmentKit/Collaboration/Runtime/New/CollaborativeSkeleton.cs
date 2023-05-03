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
using umi3d.cdk.userCapture;
using umi3d.cdk.utils.extrapolation;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class CollaborativeSkeleton : AbstractSkeleton
    {
        public UMI3DUser User;

        public TrackedSkeleton TrackedSkeleton;
        public PoseSkeleton poseSkeleton = new PoseSkeleton();

        public override void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (Skeletons != null)
                foreach (ISubWritableSkeleton skeleton in Skeletons.OfType<ISubWritableSkeleton>())
                    skeleton.UpdateFrame(frame);

            this.transform.position = frame.position;
            this.transform.rotation = frame.rotation;
        }

        public void SetSubSkeletons()
        {
            TrackedSkeleton = Instantiate((UMI3DCollaborationEnvironmentLoader.Parameters as UMI3DCollabLoadingParameters).CollabTrackedSkeleton, this.transform).GetComponent<TrackedSkeleton>();
            Skeletons.Add(TrackedSkeleton);
            Skeletons.Add(poseSkeleton);
            //skeletons.Add(new AnimatedSkeleton());
        }
    }
}