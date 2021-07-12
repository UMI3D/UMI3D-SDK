﻿/*
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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DAudioBridger : Singleton<UMI3DAudioBridger>
    {
        [SerializeField]
        bool _Spacialized = false;
        public bool Spacialized
        {
            get => _Spacialized; set
            {
                _Spacialized = value;
                UpdateSpacial();
            }
        }

        private void Start()
        {
            UMI3DCollaborationServer.Instance.OnUserJoin.AddListener(newUser);
        }

        void newUser(UMI3DUser _user)
        {
            if (_user is UMI3DCollaborationUser user)
            {
                if (user.audioPlayer == null)
                {
                    StartCoroutine(SetAudioSource(user));
                }
                StartCoroutine(SetSpacialBlend(user));
            }
        }


        IEnumerator SetAudioSource(UMI3DCollaborationUser user)
        {
            var wait = new WaitForFixedUpdate();
            while (user.Avatar == null)
                yield return wait;
            user.audioPlayer = user.Avatar.gameObject.GetComponent<UMI3DAudioPlayer>();
            if (user.audioPlayer == null)
            {
                user.audioPlayer = user.Avatar.gameObject.AddComponent<UMI3DAudioPlayer>();
                user.audioPlayer.ObjectSpacialBlend.SetValue(Spacialized ? 1 : 0);
                user.audioPlayer.ObjectNode.SetValue(user.Avatar);
                UMI3DServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { user.audioPlayer.GetLoadEntity() } });
            }
            if (user.audioPlayer.ObjectNode.GetValue() == null)
            {
                var op = user.audioPlayer.ObjectNode.SetValue(user.Avatar);
                if (op != null)
                    UMI3DServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { op } });
            }
            UMI3DServer.Instance.NotifyUserChanged(user);
        }

        IEnumerator SetSpacialBlend(UMI3DCollaborationUser user)
        {
            var wait = new WaitForFixedUpdate();
            while (user.audioPlayer == null)
                yield return wait;
            var op = user.audioPlayer.ObjectSpacialBlend.SetValue(Spacialized ? 1 : 0);
            if (op != null)
                UMI3DServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { op } });
        }

        void UpdateSpacial()
        {
            foreach (var user in UMI3DEnvironment.GetEntities<UMI3DCollaborationUser>())
            {
                var op = user.audioPlayer.ObjectSpacialBlend.SetValue(Spacialized ? 1 : 0);
                if (op != null)
                    UMI3DServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { op } });
            }
        }



    }
}