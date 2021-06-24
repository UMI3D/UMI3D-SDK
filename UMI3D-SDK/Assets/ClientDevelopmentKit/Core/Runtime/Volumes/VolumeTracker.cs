using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common.volume;
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
		private List<UnityAction<string>> callbacksOnEnter = new List<UnityAction<string>>();
		private List<UnityAction<string>> callbacksOnExit = new List<UnityAction<string>>();
		private bool wasInsideOneVolumeLastFrame = false;
		private string lastVolumeId;

		protected virtual void Awake()
        {
			wasInsideOneVolumeLastFrame = volumesToTrack.Exists(v => v.IsInside(this.transform.position));
			trackingRoutine = StartCoroutine(Track());
        }

		IEnumerator Track()
        {
            while (true)
            {
				AbstractVolumeCell cell = volumesToTrack.Find(v => v.IsInside(this.transform.position));
				bool inside = (cell != null);

				if (inside && !wasInsideOneVolumeLastFrame)
					foreach (var callback in callbacksOnEnter)
						callback.Invoke(cell.Id());
				else if (!inside && wasInsideOneVolumeLastFrame)
					foreach (var callback in callbacksOnExit)
						callback.Invoke(lastVolumeId);

				wasInsideOneVolumeLastFrame = inside;
				lastVolumeId = cell?.Id();

				yield return new WaitForSeconds(1f / detectionFrameRate);
			}
        }

		public void SubscribeToVolumeEntrance(UnityAction<string> callback)
		{
			callbacksOnEnter.Add(callback);
		}

		public void SubscribeToVolumeExit(UnityAction<string> callback)
		{
			callbacksOnExit.Add(callback);
		}

		public void UnsubscribeToVolumeEntrance(UnityAction<string> callback)
		{
			callbacksOnEnter.Remove(callback);
		}

		public void UnsubscribeToVolumeExit(UnityAction<string> callback)
		{
			callbacksOnExit.Remove(callback);
		}



	}
}