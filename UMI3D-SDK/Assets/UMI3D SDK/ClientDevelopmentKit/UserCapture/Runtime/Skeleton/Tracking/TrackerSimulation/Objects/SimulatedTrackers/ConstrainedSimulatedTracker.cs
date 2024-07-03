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

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Simulated tracker responsible for the application of a bone constraint.
    /// <inheritdoc/>
    /// </summary>
    public class ConstrainedSimulatedTracker : SimulatedTracker
    {
        public IBoneConstraint Constraint => constraint;
        private IBoneConstraint constraint;

        public void Init(IBoneConstraint constraint)
        {
            this.constraint = constraint;

            base.Init(constraint.ConstrainedBone);
        }


        protected virtual void Update()
        {
            BeConstrained();
        }

        /// <summary>
        /// Force the controller to be at the place specified by the constraint.
        /// </summary>
        protected virtual void BeConstrained()
        {
            if (!constraint.IsApplied)
                return;

            var t = constraint.Resolve();
            transform.SetPositionAndRotation(t.position, t.rotation);
        }
    }
}