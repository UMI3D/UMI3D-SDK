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
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DAudioBridger : inetum.unityUtils.SingleBehaviour<UMI3DAudioBridger>
    {
        [SerializeField]
        private bool _Spacialized = false;
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

        private void newUser(UMI3DUser _user)
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

        private IEnumerator SetAudioSource(UMI3DCollaborationUser user)
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
                var tr = new Transaction() { reliable = true };
                tr.AddIfNotNull(user.audioPlayer.GetLoadEntity());
                UMI3DServer.Dispatch(tr);
            }
            if (user.audioPlayer.ObjectNode.GetValue() == null)
            {
                SetEntityProperty op = user.audioPlayer.ObjectNode.SetValue(user.Avatar);
                if (op != null)
                {
                    var tr = new Transaction() { reliable = true };
                    tr.AddIfNotNull(op);
                    UMI3DServer.Dispatch(tr);
                }
            }
            UMI3DServer.Instance.NotifyUserChanged(user);
        }

        private IEnumerator SetSpacialBlend(UMI3DCollaborationUser user)
        {
            var wait = new WaitForFixedUpdate();
            while (user.audioPlayer == null)
                yield return wait;
            SetEntityProperty op = user.audioPlayer.ObjectSpacialBlend.SetValue(Spacialized ? 1 : 0);
            if (op != null)
            {
                var tr = new Transaction() { reliable = true };
                tr.AddIfNotNull(op);
                UMI3DServer.Dispatch(tr);
            }
        }

        private void UpdateSpacial()
        {
            foreach (UMI3DCollaborationUser user in UMI3DEnvironment.GetEntities<UMI3DCollaborationUser>())
            {
                SetEntityProperty op = user.audioPlayer.ObjectSpacialBlend.SetValue(Spacialized ? 1 : 0);
                if (op != null)
                {
                    var tr = new Transaction() { reliable = true };
                    tr.AddIfNotNull(op);
                    UMI3DServer.Dispatch(tr);
                }
            }
        }
    }
}