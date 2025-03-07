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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace inetum.unityUtils.audio
{
    public class AudioMixerControl
    {
        private static AudioMixer audioMixer;

        public enum Group
        {
            Conversation,
            Environment
        }

        private const string AUDIO_MIXER = "AudioMixer";

        public static void SetGroup(Group group, AudioSource audioSource)
        {
            if (audioSource == null)
                return;
            if (audioMixer == null)
                audioMixer = GetAudioMixer();
            if (audioMixer == null)
                return;
            audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(GroupName(group))[0];
        }

        public static void SetVolume(Group group, float volume)
        {
            if (audioMixer == null)
                audioMixer = GetAudioMixer();
            if (audioMixer == null)
                return;

            audioMixer.SetFloat(GroupName(group), volume);
        }

        private static AudioMixer GetAudioMixer()
        {
            AudioMixer mixer = Resources.Load<AudioMixer>(AUDIO_MIXER);
            return mixer;
        }

        private static string GroupName(Group group)
        {
            switch (group)
            {
                case Group.Conversation:
                    return "Master/Conversation";
                case Group.Environment:
                    return "Master/Environment";
            }

            return "";
        }
    }
}

