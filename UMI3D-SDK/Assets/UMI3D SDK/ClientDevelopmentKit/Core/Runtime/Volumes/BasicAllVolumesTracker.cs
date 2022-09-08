using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using umi3d.common.volume;

namespace umi3d.cdk.volumes
{
    public class BasicAllVolumesTracker : MonoBehaviour
    {
        public float frameRate = 30;

        protected virtual void Awake()
        {
            UnityAction<AbstractVolumeCell> addTracker = volume =>
            {
                VolumeTracker tracker = this.gameObject.AddComponent<VolumeTracker>();
                tracker.detectionFrameRate = frameRate;
                tracker.volumesToTrack = new List<AbstractVolumeCell>() { volume };
                tracker.SubscribeToVolumeEntrance(vid => UMI3DClientServer.SendData(new VolumeUserTransitDto() { direction = true, volumeId = vid }, true));
                tracker.SubscribeToVolumeExit(vid => UMI3DClientServer.SendData(new VolumeUserTransitDto() { direction = false, volumeId = vid }, true));
                tracker.StartTracking();
            };

            //ExternalVolumeDataManager.SubscribeToExternalVolumeCreation(addTracker, true); TOO LONG
            VolumePrimitiveManager.SubscribeToPrimitiveCreation(addTracker, true);
        }
    }
}
