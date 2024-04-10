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

using umi3d.common;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public class NodeAnchoredSimulatedTracker : AbstractSimulatedTracker
    {
        private Transform nodeReference;

        public void Init(UMI3DNodeInstance node, uint boneType, Vector3 posOffset, Quaternion rotOffset)
        {
            this.nodeReference = node.transform;
            Init(boneType, posOffset, rotOffset);
        }

        public override (Vector3 position, Quaternion rotation) SimulatePosition()
        {
            if (nodeReference == null)
            {
                UMI3DLogger.LogWarning($"Anchoring reference node destroyed without destroying simulated tracker first. Destroying tracker.", DebugScope.CDK | DebugScope.UserCapture);
                Destroy(this);
                return default;
            }
            Quaternion rotation = nodeReference.rotation * rotationOffset;
            Vector3 position = nodeReference.position + nodeReference.rotation * positionOffset;
            return (position, rotation);
        }
    }
}