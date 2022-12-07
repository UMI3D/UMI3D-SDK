using UnityEngine;
using System.Collections;
using System;
using MumbleProto;
using UnityEngine.Events;

namespace Mumble
{
    public class MumbleAudioPlayerPlayingEvent : UnityEvent<bool> { }

    [RequireComponent(typeof(AudioSource))]
    public class MumbleAudioPlayer : MonoBehaviour
    {

        public float Gain = 1;
        public UInt32 Session { get; private set; }
        private bool _isPlaying
        {
            get => __isPlaying;
            set
            {
                if (__isPlaying != value)
                {
                    __isPlaying = value;
                    OnPlaying.Invoke(__isPlaying);
                }
            }
        }

        /// <summary>
        /// Notification that a new audio sample is available for processing
        /// It will be called on the audio thread
        /// It will contain the audio data, which you may want to process in
        /// your own code, and it contains the percent of the data left
        /// un-read
        /// </summary>
        public Action<float[], float> OnAudioSample;

        private MumbleClient _mumbleClient;
        private AudioSource _audioSource;
        private bool __isPlaying = false;
        private float _pendingAudioVolume = -1f;

        public MumbleAudioPlayerPlayingEvent OnPlaying = new MumbleAudioPlayerPlayingEvent();

        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            // In editor, double check that "auto-play" is turned off
#if UNITY_EDITOR
            if (_audioSource.playOnAwake)
                Debug.LogWarning("For best performance, please turn \"Play On Awake\" off");
#endif
            // In principle, this line shouldn't need to be here.
            // however, from profiling it seems that Unity will
            // call OnAudioFilterRead when the audioSource hits
            // Awake, even if PlayOnAwake is off
            _audioSource.Stop();

            if (_pendingAudioVolume >= 0)
                _audioSource.volume = _pendingAudioVolume;
            _pendingAudioVolume = -1f;
        }
        public string GetUsername()
        {
            if (_mumbleClient == null)
                return null;
            UserState state = _mumbleClient.GetUserFromSession(Session);
            if (state == null)
                return null;
            return state.Name;
        }
        public string GetUserComment()
        {
            if (_mumbleClient == null)
                return null;
            UserState state = _mumbleClient.GetUserFromSession(Session);
            if (state == null)
                return null;
            return state.Comment;
        }
        public byte[] GetUserTexture()
        {
            if (_mumbleClient == null)
                return null;
            UserState state = _mumbleClient.GetUserFromSession(Session);
            if (state == null)
                return null;
            return state.Texture;
        }
        public void Initialize(MumbleClient mumbleClient, UInt32 session)
        {
            //Debug.Log("Initialized " + session, this);
            Session = session;
            _mumbleClient = mumbleClient;
        }
        public void Reset()
        {
            _mumbleClient = null;
            Session = 0;
            OnAudioSample = null;
            _isPlaying = false;
            if (_audioSource != null)
                _audioSource.Stop();
            _pendingAudioVolume = -1f;
        }

        public void Setup(MumbleAudioPlayer source)
        {
            _mumbleClient = source._mumbleClient;
            Session = source.Session;
            OnAudioSample = source.OnAudioSample;
            _isPlaying = false;
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            _pendingAudioVolume = source._pendingAudioVolume;
            if (source._audioSource != null)
            {
                SetVolume(source._audioSource.volume);
            }
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (_mumbleClient == null || !_mumbleClient.ConnectionSetupFinished)
                return;

            if (_isPlaying && !_mumbleClient.HasPlayableAudio(Session))
            {
                return;
            }
            //Debug.Log("Filter read for: " + GetUsername());

            int numRead = _mumbleClient.LoadArrayWithVoiceData(Session, data, 0, data.Length);
            float percentUnderrun = 1f - numRead / data.Length;

            if (OnAudioSample != null)
                OnAudioSample(data, percentUnderrun);

            //Debug.Log("playing audio with avg: " + data.Average() + " and max " + data.Max());
            if (Gain == 1)
                return;

            for (int i = 0; i < data.Length; i++)
                data[i] = Mathf.Clamp(data[i] * Gain, -1f, 1f);
            //Debug.Log("playing audio with avg: " + data.Average() + " and max " + data.Max());
        }
        public bool GetPositionData(out byte[] positionA, out byte[] positionB, out float distanceAB)
        {
            if (!_isPlaying)
            {
                positionA = null;
                positionB = null;
                distanceAB = 0;
                return false;
            }
            double prevPosTime;
            bool ret = _mumbleClient.LoadArraysWithPositions(Session, out positionA, out positionB, out prevPosTime);

            // Get the percent from posA->posB based on the dsp time
            distanceAB = (float)((AudioSettings.dspTime - prevPosTime) / (1000.0 * MumbleConstants.FRAME_SIZE_MS));

            return ret;
        }
        public void SetVolume(float volume)
        {
            if (_audioSource == null)
                _pendingAudioVolume = volume;
            else
                _audioSource.volume = volume;
        }

        public float GetVolume()
        {
            if (_audioSource == null)
                return _pendingAudioVolume;
            else
                return _audioSource.volume;
        }

        void Update()
        {
            if (_mumbleClient == null)
            {
                if (_isPlaying)
                {
                    _audioSource.Stop();
                    _isPlaying = false;
                }
                return;
            }
            if (!_isPlaying && _mumbleClient.HasPlayableAudio(Session))
            {
                _audioSource.Play();
                _isPlaying = true;
            }
            else if (_isPlaying && !_mumbleClient.HasPlayableAudio(Session))
            {
                _audioSource.Stop();
                _isPlaying = false;
            }
        }

        public bool IsMumbleClientSet() => _mumbleClient != null;
    }
}
