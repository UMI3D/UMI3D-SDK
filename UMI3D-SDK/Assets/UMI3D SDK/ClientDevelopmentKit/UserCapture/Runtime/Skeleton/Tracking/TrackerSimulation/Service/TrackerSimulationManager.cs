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

using inetum.unityUtils;

using System.Collections.Generic;

namespace umi3d.cdk.userCapture.tracking
{
    public class TrackerSimulationManager : Singleton<TrackerSimulationManager>, ITrackerSimulationService
    {
        public IReadOnlyDictionary<ISkeleton, ITrackerSimulator> TrackerSimulators => trackerSimulators;

        public Dictionary<ISkeleton, ITrackerSimulator> trackerSimulators = new();

        #region DI

        private readonly ILoadingManager loadingManager;

        public TrackerSimulationManager(ILoadingManager loadingManager)
        {
            this.loadingManager = loadingManager;
        }

        public TrackerSimulationManager() : this(UMI3DEnvironmentLoader.Instance)
        {
        }

        #endregion DI

        public ITrackerSimulator GetTrackerSimulator(ISkeleton skeleton)
        {
            if (TrackerSimulators.TryGetValue(skeleton, out ITrackerSimulator simulator))
                return simulator;

            simulator = new TrackerSimulator(loadingManager, skeleton);
            trackerSimulators.Add(skeleton, simulator);
            return simulator;
        }

        public void RemoveTrackerSimulator(ISkeleton skeleton)
        {
            if (TrackerSimulators.ContainsKey(skeleton))
                trackerSimulators.Remove(skeleton);
        }
    }
}