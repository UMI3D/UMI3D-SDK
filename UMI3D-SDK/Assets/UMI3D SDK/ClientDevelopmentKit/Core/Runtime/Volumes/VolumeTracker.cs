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
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.volumes
{
	/// <summary>
	/// Detect volume entrance and exit.
	/// </summary>
	public class VolumeTracker : MonoBehaviour
	{
		public List<AbstractVolumeCell> volumesToTrack = new List<AbstractVolumeCell>();
		public float detectionFrameRate = 30;

		private Coroutine trackingRoutine = null;
		private List<UnityAction<ulong>> callbacksOnEnter = new List<UnityAction<ulong>>();
		private List<UnityAction<ulong>> callbacksOnExit = new List<UnityAction<ulong>>();
		private List<ulong> volumesThisWasInsideOfLastFrame = new List<ulong>();

		protected virtual void Awake()
        {
			StartTracking();
        }

		public void StartTracking()
        {
			if (trackingRoutine == null)
			{
                volumesThisWasInsideOfLastFrame = volumesToTrack.FindAll(v => v.IsInside(this.transform.position, Space.World)).ConvertAll(cell => cell.Id());
				trackingRoutine = StartCoroutine(Track());
			}
		}

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

		IEnumerator Track()
        {
            while (true)
            {
				List<ulong> cells = volumesToTrack.FindAll(v => v.IsInside(this.transform.position, Space.World)).ConvertAll(cell => cell.Id());
				
                foreach(ulong cid in cells)
                {
                    if (!volumesThisWasInsideOfLastFrame.Contains(cid))
                    {
                        volumesThisWasInsideOfLastFrame.Add(cid);
                        callbacksOnEnter.ForEach(call => call.Invoke(cid));
                    }
                }

                foreach(ulong cid in volumesThisWasInsideOfLastFrame)
                {
                    if (!cells.Contains(cid))
                    {
                        volumesThisWasInsideOfLastFrame.Remove(cid);
                        callbacksOnExit.ForEach(call => call.Invoke(cid));
                    }
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