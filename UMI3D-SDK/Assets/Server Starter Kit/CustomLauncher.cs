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
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using UnityEngine.UI;

namespace ServerStarterKit
{
    public class CustomLauncher : UMI3DLauncher
    {
        public InputField Name;
        public Button StartStopButton;
        public Text _StartStop;
        string StartStop { get => _StartStop.text; set => _StartStop.text = value; }
        public Button IpButton;
        public Text _Ip;
        string Ip { get => _Ip.text; set => _Ip.text = value; }
        public Button PortButton;
        public Text _Port;
        string Port { get => _Port.text; set => _Port.text = value; }

        bool started = false;

        void OnClick()
        {
            if (started)
                OnStop();
            else
                OnStart();
        }

        void OnStart()
        {
            UMI3DEnvironment.Instance.environmentName = Name.text;
            UMI3DServer.Instance.Init();

            Ip = "Ip : "+ UMI3DCollaborationServer.GetHttpUrl();
            Port = "Port : " + UMI3DCollaborationServer.Instance.httpPort;
            StartStop = "Stop";
            started = true;
        }

        void OnStop()
        {
            UMI3DCollaborationServer.Stop();
            Ip = "Ip : _";
            Port = "Port : _";
            StartStop = "Start";
            started = false;
        }

        public override void LaunchServer()
        {
            OnStart();
        }

        protected override void Start()
        {
            started = LaunchServerOnStart;
            StartStop = "Start";
            StartStopButton.onClick.AddListener(OnClick);
            IpButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = UMI3DCollaborationServer.GetHttpUrl());
            PortButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = Port);
            Name.text = UMI3DEnvironment.Instance.environmentName;
            base.Start();

        }
    }
}
