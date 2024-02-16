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

        private const string CONVERSATION_GROUP = "Master/Conversation";
        private const string ENVIRONMENT_GROUP = "Master/Environment";

        private const string AUDIO_MIXER = "AudioMixer";

        public static void SetConversationGroup(AudioSource audioSource)
        {
            if (audioMixer == null) audioMixer = GetAudioMixer();
            if (audioMixer == null) return;
            audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(CONVERSATION_GROUP)[0];
        }

        public static void SetEnvironmentGroup(AudioSource audioSource)
        {
            if (audioMixer == null) audioMixer = GetAudioMixer();
            if (audioMixer == null) return;
            audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(ENVIRONMENT_GROUP)[0];
        }

        private static AudioMixer GetAudioMixer()
        {
            AudioMixer mixer = Resources.Load<AudioMixer>(AUDIO_MIXER);
            return mixer;
        }
    }
}

