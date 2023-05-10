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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Detect volume entrance and exit.
    /// </summary>
    public class VolumeTracker : MonoBehaviour
    {
        /// <summary>
        /// Tracked volumes in the environment.
        /// </summary>
        public List<AbstractVolumeCell> volumesToTrack = new List<AbstractVolumeCell>();
        /// <summary>
        /// Number of detection check per frame.
        /// </summary>
        public float detectionFrameRate = 30;

        private Coroutine trackingRoutine = null;
        /// <summary>
        /// Actions triggered when entering a tracked volume.
        /// </summary>
        private readonly List<UnityAction<ulong>> callbacksOnEnter = new List<UnityAction<ulong>>();
        /// <summary>
        /// Actions triggered when exiting a tracked volume.
        /// </summary>
        private readonly List<UnityAction<ulong>> callbacksOnExit = new List<UnityAction<ulong>>();

        /// <summary>
        /// All volumes where user is currently in.
        /// </summary>
        private HashSet<ulong> volumesUserInside = new();

        protected virtual void Awake()
        {
            StartTracking();
        }

        /// <summary>
        /// Active the detection in the <see cref="volumesToTrack"/>.
        /// </summary>
        public void StartTracking()
        {
            if (trackingRoutine == null)
            {
                trackingRoutine = StartCoroutine(Track());
            }
        }

        /// <summary>
        /// Deactive the detection in the <see cref="volumesToTrack"/>.
        /// </summary>
        public void StopTracking()
        {
            if (trackingRoutine != null)
            {
                StopCoroutine(trackingRoutine);
                trackingRoutine = null;
            }
        }

        protected virtual void OnDestroy()
        {
            StopTracking();
        }

        /// <summary>
        /// Coroutine constantly checking if the gameobject this component is attached to is inside a cell.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Track()
        {
            while (true)
            {
                var cellIds = volumesToTrack.Where(v => v.IsInside(this.transform.position, Space.World)).Select(v => v.Id());

                foreach (ulong cellId in cellIds)
                {
                    if (!volumesUserInside.Contains(cellId))
                    {
                        volumesUserInside.Add(cellId);
                        foreach (UnityAction<ulong> callback in callbacksOnEnter)
                            callback.Invoke(cellId);
                    }
                }

                var oldVolumeIds = volumesUserInside.Where(v => !cellIds.Contains(v)).ToArray();

                foreach (var cellId in oldVolumeIds)
                {
                    foreach (UnityAction<ulong> callback in callbacksOnExit)
                        callback.Invoke(cellId);

                    volumesUserInside.Remove(cellId);
                }

                yield return new WaitForSeconds(1f / detectionFrameRate);
            }
        }

        /// <summary>
        /// The ulong argument corresponds to the volume id.
        /// </summary>
        public void SubscribeToVolumeEntrance(UnityAction<ulong> callback)
        {
            callbacksOnEnter.Add(callback);
        }

        /// <summary>
        /// The ulong argument corresponds to the volume id.
        /// </summary>
        public void SubscribeToVolumeExit(UnityAction<ulong> callback)
        {
            callbacksOnExit.Add(callback);
        }

        /// <summary>
        /// The ulong argument corresponds to the volume id.
        /// </summary>
        public void UnsubscribeToVolumeEntrance(UnityAction<ulong> callback)
        {
            callbacksOnEnter.Remove(callback);
        }

        /// <summary>
        /// The ulong argument corresponds to the volume id.
        /// </summary>
        public void UnsubscribeToVolumeExit(UnityAction<ulong> callback)
        {
            callbacksOnExit.Remove(callback);
        }
    }
}