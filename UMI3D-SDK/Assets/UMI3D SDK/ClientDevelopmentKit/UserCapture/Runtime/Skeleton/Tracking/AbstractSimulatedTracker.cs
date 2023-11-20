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

using inetum.unityUtils;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public abstract class AbstractSimulatedTracker : Tracker, ISimulatedTracker
    {
        uint ISimulatedTracker.Bonetype => Bonetype;

        public Vector3 PositionOffset => positionOffset;
        protected Vector3 positionOffset;

        public Quaternion RotationOffset => rotationOffset;
        protected Quaternion rotationOffset;

        protected void Init(uint bonetype, Vector3 posOffset, Quaternion rotOffset)
        {
            this.boneType = bonetype;
            this.positionOffset = posOffset;
            this.rotationOffset = rotOffset;

            distantController = new DistantController()
            {
                boneType = boneType,
                position = transform.position,
                rotation = transform.rotation,
                isActif = isActif,
                isOverrider = isOverrider
            };
        }

        void ISimulatedTracker.SimulatePosition() => this.SimulatePosition();
        protected abstract void SimulatePosition();
    }
}
