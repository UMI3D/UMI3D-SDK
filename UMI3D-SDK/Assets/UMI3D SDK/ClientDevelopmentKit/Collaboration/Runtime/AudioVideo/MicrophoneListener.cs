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

using inetum.unityUtils;
using Mumble;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    [Serializable]
    public class MicrophoneEvent : UnityEngine.Events.UnityEvent<bool>
    {
    }

    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : SingleBehaviour<MicrophoneListener>, ILoggable
    {

        #region Mumble
        // Basic mumble audio player
        public GameObject MyMumbleAudioPlayerPrefab;
        // Mumble audio player that also receives position commands
        public GameObject MyMumbleAudioPlayerPositionedPrefab;

        MumbleMicrophone MyMumbleMic;
        DebugValues DebuggingVariables;

        public bool UseLocalLoopback
        {
            get => DebuggingVariables?.UseLocalLoopback ?? false;
            set { if (DebuggingVariables != null) DebuggingVariables.UseLocalLoopback = value; }
        }

        public MumbleClient mumbleClient;
        const bool connectAsyncronously = true;
        const bool sendPosition = false;

        string hostName = "1.2.3.4";
        int port = 64738;
        string username = "ExampleUser";
        string password = "1passwordHere!";
        string channelToJoin = "";

        bool useMumble;

        bool playing = false;

        static bool muted;


        public static bool IsMute
        {
            get { return Exists ? !Instance.useMumble || muted : true; }

            set { muted = value; if (Exists) Instance.ShouldStop(); }
        }

        void ShouldStop()
        {
            if (useMumble && mumbleClient != null)
                mumbleClient.SetSelfMute(IsMute);
        }

        void SetMumbleUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;
            var s = url.Split(':');

            hostName = s[0];
            if (s.Length > 1 && int.TryParse(s[1], out int port))
                this.port = port;
        }

        protected void Start()
        {
            MyMumbleMic = gameObject.GetOrAddComponent<MumbleMicrophone>();
            gameObject.GetOrAddComponent<EventProcessor>();
            DebuggingVariables = new DebugValues()
            {
                EnableEditorIOGraph = false,
                UseLocalLoopback = false,
                UseRandomUsername = false,
                UseSyntheticSource = false
            };
            QuittingManager.OnApplicationIsQuitting.AddListener(_OnApplicationQuit);

            UMI3DUser.OnUserMicrophoneIdentityUpdated.AddListener(IdentityUpdate);
            UMI3DUser.OnUserMicrophoneChannelUpdated.AddListener(ChannelUpdate);
            UMI3DUser.OnUserMicrophoneServerUpdated.AddListener(ServerUpdate);
            UMI3DUser.OnUserMicrophoneUseMumbleUpdated.AddListener(UseMumbleUpdate);
        }

        bool IdentityUpdateOnce = false;
        async void IdentityUpdate(UMI3DUser user)
        {
            if (user != null && user.isClient && !IdentityUpdateOnce)
            {
                IdentityUpdateOnce = true;
                await UMI3DAsyncManager.Yield();
                await UMI3DAsyncManager.Yield();

                username = user.audioLogin;
                password = user.audioPassword;

                if (playing)
                {
                    StopMicrophone();
                    StartMicrophone();
                }
                IdentityUpdateOnce = false;
            }
        }

        async void ChannelUpdate(UMI3DUser user)
        {
            channelToJoin = user.audioChannel;
            if (playing)
            {
                await JoinChannel();
            }
        }
        void ServerUpdate(UMI3DUser user)
        {
            SetMumbleUrl(user.audioServer);
            if (playing)
            {
                StopMicrophone();
                StartMicrophone();
            }
        }
        void UseMumbleUpdate(UMI3DUser user)
        {
            if (useMumble != user.useMumble)
            {
                useMumble = user.useMumble;

                Debug.Log(useMumble);
                if (useMumble)
                    StartMicrophone();
                else
                    StopMicrophone();
            }
        }


        public void StopMicrophone()
        {
            if (playing && mumbleClient != null)
            {
                MyMumbleMic.StopSendingAudio();
                mumbleClient.Close();
            }
            playing = false;
        }

        public async void StartMicrophone()
        {
            Debug.LogError($"Start {!useMumble} || { playing}");
            if (!useMumble || playing) return;

            if (hostName == "1.2.3.4")
            {
                Debug.LogError("Please set the mumble host name to your mumble server");
                return;
            }
            playing = true;

            if (UMI3DCollaborationEnvironmentLoader.Exists)
            {
                Debug.LogError($"Exist");
                var user = UMI3DCollaborationEnvironmentLoader.Instance.GetClientUser();
                if (user != null)
                {
                    Debug.LogError($"user {username} {password} {hostName} {port} {channelToJoin}");
                    username = user.audioLogin;
                    password = user.audioPassword;
                    SetMumbleUrl(user.audioServer);
                    channelToJoin = user.audioChannel;
                    useMumble = user.useMumble;


                    Application.runInBackground = true;
                    // If SendPosition, we'll send three floats.
                    // This is roughly the standard for Mumble, however it seems that
                    // Murmur supports more
                    int posLength = sendPosition ? 3 * sizeof(float) : 0;
                    mumbleClient = new MumbleClient(hostName, port, CreateMumbleAudioPlayerFromPrefab,
                        DestroyMumbleAudioPlayer, OnOtherUserStateChange, connectAsyncronously,
                        SpeakerCreationMode.ALL, DebuggingVariables, posLength);

                    if (connectAsyncronously)
                        while (!mumbleClient.ReadyToConnect)
                            await UMI3DAsyncManager.Yield();

                    mumbleClient.Connect(username, password);

                    if (connectAsyncronously)
                        await UMI3DAsyncManager.Yield();

                    if (MyMumbleMic != null)
                    {
                        mumbleClient.AddMumbleMic(MyMumbleMic);
                        mumbleClient.SetSelfMute(IsMute);
                        if (sendPosition)
                            MyMumbleMic.SetPositionalDataFunction(WritePositionalData);
                        MyMumbleMic.OnMicDisconnect += OnMicDisconnected;
                        await JoinChannel();
                        return;
                    }
                }
            }
            Debug.LogError($"playing false");
            playing = false;
        }

        async Task JoinChannel(int trycount = 0)
        {
            if (trycount < 3)
            {
                await UMI3DAsyncManager.Delay(5000);
                if (!mumbleClient.JoinChannel(channelToJoin))
                {
                    await JoinChannel(trycount++);
                }
            }
        }

        /// <summary>
        /// An example of how to serialize the positional data that you're interested in
        /// NOTE: this function, in the current implementation, is called regardless
        /// of if the user is speaking
        /// </summary>
        /// <param name="posData"></param>
        private void WritePositionalData(ref byte[] posData, ref int posDataLength)
        {
            // Get the XYZ position of the camera
            Vector3 pos = Camera.main.transform.position;
            //Debug.Log("Sending pos: " + pos);
            // Copy the XYZ floats into our positional array
            int dstOffset = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, posData, dstOffset, sizeof(float));
            dstOffset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, posData, dstOffset, sizeof(float));
            dstOffset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, posData, dstOffset, sizeof(float));

            posDataLength = 3 * sizeof(float);
            // The reverse method is in MumbleExamplePositionDisplay
        }

        private MumbleAudioPlayer CreateMumbleAudioPlayerFromPrefab(string username, uint session)
        {
            Debug.LogError("New player");
            // Depending on your use case, you might want to add the prefab to an existing object (like someone's head)
            // If you have users entering and leaving frequently, you might want to implement an object pool
            GameObject newObj = sendPosition
                ? GameObject.Instantiate(MyMumbleAudioPlayerPositionedPrefab)
                : GameObject.Instantiate(MyMumbleAudioPlayerPrefab);

            newObj.name = username + "_MumbleAudioPlayer";
            MumbleAudioPlayer newPlayer = newObj.GetComponent<MumbleAudioPlayer>();
            Debug.Log("Adding audio player for: " + username);
            return newPlayer;
        }
        private void OnOtherUserStateChange(uint session, MumbleProto.UserState updatedDeltaState, MumbleProto.UserState fullUserState)
        {
            print("User #" + session + " had their user state change");
            // Here we can do stuff like update a UI with users' current channel/mute etc.
        }
        private void DestroyMumbleAudioPlayer(uint session, MumbleAudioPlayer playerToDestroy)
        {
            UnityEngine.GameObject.Destroy(playerToDestroy.gameObject);
        }
        private void OnMicDisconnected()
        {
            Debug.LogError("Connected microphone has disconnected!");
            string disconnectedMicName = MyMumbleMic.GetCurrentMicName();
            // This means that the mic that we were previously receiving audio from has disconnected
            // you may want to present a notification to the user, allowing them to select
            // a new mic to use
            // here, we will start a coroutine to wait until the mic we want is connected again
            StartCoroutine(ExampleMicReconnect(disconnectedMicName));
        }
        IEnumerator ExampleMicReconnect(string micToConnect)
        {
            while (true)
            {
                string[] micNames = Microphone.devices;
                // try to see if the desired mic is connected
                for (int i = 0; i < micNames.Length; i++)
                {
                    if (micNames[i] == micToConnect)
                    {
                        Debug.Log("Desired mic reconnected");
                        MyMumbleMic.MicNumberToUse = i;
                        MyMumbleMic.StartSendingAudio(mumbleClient.EncoderSampleRate);
                        yield break;
                    }
                }
                yield return new WaitForSeconds(2f);
            }
        }

        void _OnApplicationQuit()
        {
            StopMicrophone();
        }

        #endregion

        public List<DebugInfo> GetInfos()
        {
            return new List<DebugInfo>();
        }

        public string GetLogName()
        {
            return "Microphone";
        }

    }
}