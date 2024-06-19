/*
Copyright 2019 - 2024 Inetum

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

using umi3d.common.userCapture.tracking.constraint;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Restrict or controls a bone movement.
    /// </summary>
    public abstract class AbstractBoneConstraint : IBoneConstraint
    {
        protected AbstractBoneConstraint(AbstractBoneConstraintDto dto)
        {
            this.Id = dto.id;
            this.ShouldBeApplied = dto.ShouldBeApplied;
            this.ConstrainedBone = dto.ConstrainedBone;
            this.PositionOffset = dto.PositionOffset.Struct();
            this.RotationOffset = dto.RotationOffset.Quaternion();
        }

        public ulong Id { get; private set; }

        public bool ShouldBeApplied { get; set; }

        public bool IsApplied { get; internal set; }

        public uint ConstrainedBone { get; internal set; }

        public Vector3 PositionOffset { get; internal set; }

        public Quaternion RotationOffset { get; internal set; }

        public ConstrainedSimulatedTracker Tracker { get; internal set; }

        public virtual string TrackerLabel { get; protected set; } = "Constrained Tracker";

        public virtual void Apply(ISkeleton skeleton)
        {
            IsApplied = true;
            Tracker = CreateSimulatedTracker();
            skeleton.TrackedSubskeleton.ReplaceController(Tracker.Controller, true);
        }

        public virtual void EndApply(ISkeleton skeleton)
        {
            skeleton.TrackedSubskeleton.RemoveController(Tracker.Controller.boneType);
            DestroySimulatedTracker();
            IsApplied = false;
        }

        protected ConstrainedSimulatedTracker CreateSimulatedTracker()
        {
            ConstrainedSimulatedTracker tracker = new GameObject(TrackerLabel).AddComponent<ConstrainedSimulatedTracker>();
            tracker.Init(this);
            tracker.isOverrider = true;
            return tracker;
        }

        protected void DestroySimulatedTracker()
        {
            Tracker.Controller.isActive = false;

            if (Tracker.GameObject != null)
                UnityEngine.Object.Destroy(Tracker.GameObject);

            Tracker = null;
        }

        public abstract (Vector3 position, Quaternion rotation) Resolve();
    }
}