/*
Copyright 2019 - 2022 Inetum

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
using UnityEngine;
using umi3d.common.volume;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Detect volume entrance and exit in any volume.
    /// </summary>
    public class BasicAllVolumesTracker : MonoBehaviour
    {
        public float frameRate = 30;
        VolumeTracker tracker;

        protected virtual void Awake()
        {
            //ExternalVolumeDataManager.SubscribeToExternalVolumeCreation(addTracker, true); TOO LONG
            VolumePrimitiveManager.SubscribeToPrimitiveCreation(AddTracker, true);
            VolumePrimitiveManager.SubscribeToPrimitiveDelete(RemoveTracker);
        }

        void AddTracker(AbstractVolumeCell volume)
        {
            if (tracker == null)
            {
                tracker = this.gameObject.AddComponent<VolumeTracker>();
                tracker.detectionFrameRate = frameRate;
                tracker.volumesToTrack = new List<AbstractVolumeCell>() { volume };
                tracker.SubscribeToVolumeEntrance(vid => UMI3DClientServer.SendData(new VolumeUserTransitDto() { direction = true, volumeId = vid }, true));
                tracker.SubscribeToVolumeExit(vid => UMI3DClientServer.SendData(new VolumeUserTransitDto() { direction = false, volumeId = vid }, true));
                tracker.StartTracking();
            }
            else
            {
                tracker.volumesToTrack.Add(volume);
            }
        }

        void RemoveTracker(AbstractVolumeCell volume)
        {
            if (tracker != null)
            {
                tracker.volumesToTrack.Remove(volume);
                if (tracker.volumesToTrack.Count == 0)
                {
                    tracker.StopTracking();
                    Destroy(tracker);
                    tracker = null;
                }
            }
        }

    }
}