/*
Copyright 2019 - 2025 Inetum

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

using System;
using System.Runtime.InteropServices;

public static class AudioProcessingWebRTCWrapper
{
    public enum NoiseReductionLevel { Low, Moderate, High, VeryHigh };

    [StructLayout(LayoutKind.Sequential)]
    public struct AudioProcessingSettings
    {
        public int sampleRate;

        public int nbChannels;

        [MarshalAs(UnmanagedType.I1)]
        public bool useEchoCanceller;

        [MarshalAs(UnmanagedType.I1)]
        public bool useNoiseReduction;

        public NoiseReductionLevel noiseReductionLevel;
    };

    #region Private API

    private static IntPtr audioProcessor = IntPtr.Zero;

    [DllImport("AudioProcessingWebRTC.dll")]
    private static extern IntPtr InitAudioProcessing(AudioProcessingSettings settings);

    [DllImport("AudioProcessingWebRTC.dll")]
    private static extern void SetSettings(IntPtr audioProcessor, AudioProcessingSettings settings);

    [DllImport("AudioProcessingWebRTC.dll")]
    private static extern void Destroy(IntPtr audioProcessor);

    [DllImport("AudioProcessingWebRTC.dll")]
    private static extern void ProcessAudio(IntPtr audioProcessor, int nbSamples, short[] inputSamples, short[] echoSamples, short[] outputSamples);

    #endregion

    #region Public API

    /// <summary>
    /// Initializes the AudioProcessor object. Ownership is transferred to whoever calls this method. The <see cref="Destroy()"/> method must be called to free up memory.
    /// </summary>
    public static void Init(AudioProcessingSettings settings)
    {
        if (audioProcessor != IntPtr.Zero)
            throw new Exception($"{nameof(AudioProcessingWebRTCWrapper)} already init, call Destroy before");

        if (settings.sampleRate < 0)
            new Exception($"{nameof(AudioProcessingWebRTCWrapper)}.{nameof(Init)} sample rate can't be negative.");

        audioProcessor = InitAudioProcessing(settings);
    }

    /// <summary>
    /// Sets the audio processing settings of an AudioProcessor created by InitAudioProcessing.
    /// </summary>
    /// <param name="settings"></param>
    public static void SetSettings(AudioProcessingSettings settings)
    {
        if (audioProcessor == IntPtr.Zero)
            throw new Exception($"{nameof(AudioProcessingWebRTCWrapper)} not init");

        if (settings.sampleRate < 0)
            new Exception($"{nameof(AudioProcessingWebRTCWrapper)}.{nameof(SetSettings)} sample rate can't be negative.");

        SetSettings(audioProcessor, settings);
    }

    /// <summary>
    /// Releases memory allocated by <see cref="InitAudioProcessing(int, int, bool, int, bool)"/>.
    /// </summary>
    public static void Destroy()
    {
        if (audioProcessor != IntPtr.Zero)
        {
            Destroy(audioProcessor);
            audioProcessor = IntPtr.Zero;
        }
    }

    /// <summary>
    /// Processes audio. The AudioProcessor must have been initialized by the InitAudioProcessing method beforehand.
    /// Input, echo and output samples must have the same length.
    /// </summary>
    /// <param name="inputSamples">Audio to process</param>
    /// <param name="echoSamples">Echo to remove</param>
    /// <param name="outputSamples">Input samples processed</param>
    public static void ProcessAudio(short[] inputSamples, short[] echoSamples, short[] outputSamples)
    {
        if (audioProcessor == IntPtr.Zero)
            throw new Exception($"{nameof(AudioProcessingWebRTCWrapper)} not init");

        if (inputSamples == null)
            new Exception($"{nameof(AudioProcessingWebRTCWrapper)}.{nameof(ProcessAudio)} input samples must not be null");

        if (echoSamples == null)
            new Exception($"{nameof(AudioProcessingWebRTCWrapper)}.{nameof(ProcessAudio)} echo samples must not be null");

        if (outputSamples == null)
            new Exception($"{nameof(AudioProcessingWebRTCWrapper)}.{nameof(ProcessAudio)} echo samples must not be null");

        if ((inputSamples.Length != echoSamples.Length) || (inputSamples.Length != outputSamples.Length))
            throw new Exception($"{nameof(AudioProcessingWebRTCWrapper)}.{nameof(ProcessAudio)} input, echo and output samples must have the same length");

        ProcessAudio(audioProcessor, inputSamples.Length, inputSamples, echoSamples, outputSamples);
    }

    #endregion
}

