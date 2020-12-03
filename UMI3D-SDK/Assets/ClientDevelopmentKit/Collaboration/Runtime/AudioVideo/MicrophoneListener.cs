using System;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// MonoBehaviour use to Listen a microphone and send it via an RTC audio channel.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : MonoBehaviour
    {
        const int FREQUENCY = 44100;
        AudioClip mic;
        int lastPos, pos;
        public Action<AudioDto> AudioUpdate;
        public bool IsOn = true;

        bool ok = true;

        /// <summary>
        /// Enable or disable the microphone listener.
        /// </summary>
        /// <param name="status">Enable the microphone if true.</param>
        public void Activate(bool status)
        {
            IsOn = status;
        }

        // Use this for initialization
        void Start()
        {
            try
            {
                mic = Microphone.Start(null, true, 10, FREQUENCY);
                //Debug.Log(mic.channels);
                AudioSource audio = GetComponent<AudioSource>();
                audio.clip = AudioClip.Create("microphone", 10 * FREQUENCY, mic.channels, FREQUENCY, false);
                audio.loop = true;
                if (UMI3DCollaborationClientServer.Instance.webRTCClient != null)
                    AudioUpdate = UMI3DCollaborationClientServer.Instance.webRTCClient.sendAudio;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                ok = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!ok) return;
            if (AudioUpdate == null && UMI3DCollaborationClientServer.Instance.webRTCClient != null)
                AudioUpdate = UMI3DCollaborationClientServer.Instance.webRTCClient.sendAudio;
            if ((pos = Microphone.GetPosition(null)) > 0)
            {
                if (lastPos > pos) lastPos = 0;

                if (pos - lastPos > 0)
                {
                    // Allocate the space for the sample.
                    float[] sample = new float[(pos - lastPos) * mic.channels];
                    // Get the data from microphone.
                    mic.GetData(sample, lastPos);
                    if (IsOn)
                    {
                        AudioUpdate?.Invoke(new AudioDto() { sample = sample, pos = lastPos });
                    }

                    lastPos = pos;
                }
            }
        }

        //byte[] ToBytes(float[] sample)
        //{
        //    var byteArray = new byte[sample.Length * sizeof(float)];
        //    Buffer.BlockCopy(sample, 0, byteArray, 0, byteArray.Length);
        //    return byteArray;
        //}

        //float[] Tofloats(byte[] sample)
        //{
        //    var floatArray2 = new float[sample.Length / sizeof(float)];
        //    Buffer.BlockCopy(sample, 0, floatArray2, 0, sample.Length);
        //    return floatArray2;
        //}

        void OnDestroy()
        {
            Microphone.End(null);
        }
    }
}