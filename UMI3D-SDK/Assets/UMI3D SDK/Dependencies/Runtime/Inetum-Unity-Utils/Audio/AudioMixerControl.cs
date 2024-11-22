/*
Copyright 2019 - 2023 Inetum

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

using UnityEngine;
using UnityEngine.Audio;

namespace inetum.unityUtils.audio
{
    public class AudioMixerControl
    {
        public enum Group
        {
            Conversation,
            Environment
        }

        const string AUDIO_MIXER = "AudioMixer";
        const string ENVIRONMENT_GROUP = "Environment";
        const string CONVERSATION_GROUP = "Conversation";
        const string VOLUME_PARAMETER = "Volume";

        static AudioMixer audioMixer;

        /// <summary>
        /// Set the audio source's audio mixer group for a given group.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="audioSource"></param>
        public static void SetGroup(Group group, AudioSource audioSource)
        {
            // Check if the AudioMixer is available
            if (!TryGetAudioMixer())
            {
                return;
            }

            if (audioSource == null)
            {
                UnityEngine.Debug.LogError($"Error: audioSource is empty");
                return;
            }

            // Find the matching audio mixer group for the given group.
            AudioMixerGroup[] mixerGroup = audioMixer.FindMatchingGroups(GroupName(group));
            if (mixerGroup == null || mixerGroup.Length == 0)
            {
                UnityEngine.Debug.LogError($"Error: mixer group not found for group {group} with name {GroupName(group)}");
                return;
            }

            audioSource.outputAudioMixerGroup = mixerGroup[0];
        }

        /// <summary>
        /// Set the volume for a given audio mixer group.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="volume"></param>
        public static void SetVolume(Group group, float volume)
        {
            // Check if the AudioMixer is available
            if (!TryGetAudioMixer())
            {
                return;
            }

            float db = PercentToDB(volume);
            bool hasSucceed = audioMixer.SetFloat(ParameterName(group), db);
            if (!hasSucceed)
            {
                UnityEngine.Debug.LogError($"Error: setting the audio mixer volume failed. Group: {group}, parameter name: {ParameterName(group)}");
            }
        }

        public static float GetVolume(Group group)
        {
            // Check if the AudioMixer is available
            if (!TryGetAudioMixer())
            {
                return 0f;
            }

            // Get the volume for the given audio mixer group
            bool hasSucceed = audioMixer.GetFloat(ParameterName(group), out float volume);
            if (!hasSucceed)
            {
                UnityEngine.Debug.LogError($"Error: getting the audio mixer volume failed. Group: {group}, parameter name: {ParameterName(group)}");
                return 0f;
            }

            // Return the volume
            return volume;
        }

        /// <summary>
        /// Load the AudioMixer from resources.
        /// </summary>
        /// <returns></returns>
        static bool TryGetAudioMixer()
        {
            if (audioMixer == null)
            {
                audioMixer = Resources.Load<AudioMixer>(AUDIO_MIXER);
            }

            if (audioMixer == null)
            {
                UnityEngine.Debug.LogError($"Error: audioMixer not found");
            }

            return audioMixer;
        }

        /// <summary>
        /// Get the name of the audio mixer group for a given group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        static string GroupName(Group group)
        {
            switch (group)
            {
                case Group.Conversation:
                    return CONVERSATION_GROUP;
                case Group.Environment:
                    return ENVIRONMENT_GROUP;
                default:
                    UnityEngine.Debug.LogError($"Error: Case not handled {group}");
                    return "";
            }
        }

        /// <summary>
        /// Get the name of the volume parameter for a given group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        static string ParameterName(Group group)
        {
            switch (group)
            {
                case Group.Conversation:
                    return $"{CONVERSATION_GROUP}{VOLUME_PARAMETER}";
                case Group.Environment:
                    return $"{ENVIRONMENT_GROUP}{VOLUME_PARAMETER}";
                default:
                    UnityEngine.Debug.LogError($"Error: Case not handled {group}");
                    return "";
            }
        }

        static float PercentToDB(float volumeInPercent)
        {
            float db = volumeInPercent != 0
                ? Mathf.Log10(volumeInPercent / 100f) * 20f
                : float.MinValue;
            return db;
        }
    }
}

