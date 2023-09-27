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

using umi3d.common.userCapture;
using umi3d.edk.binding;
using umi3d.edk.userCapture.binding;

using UnityEngine;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Manager to handle audio spatialization in the environment.
    /// </summary>
    public class UMI3DAudioBridger : inetum.unityUtils.SingleBehaviour<UMI3DAudioBridger>
    {
        public Transform parent;

        /// <summary>
        /// Is sound spatialized?
        /// </summary>
        [SerializeField, Tooltip("Should sound be spatialized?")]
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
            UMI3DCollaborationServer.Instance.OnUserActive.AddListener(newUser);
            UMI3DCollaborationServer.Instance.OnUserLeave.AddListener(x => RemoveAudioSource(x as UMI3DCollaborationUser));
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
            while (user.CurrentTrackingFrame == null)
                yield return new WaitForFixedUpdate();

            Transaction tr = new Transaction() { reliable = true };

            // Look for audio source
            UMI3DNode audioSourceNode;
            if (user.audioPlayer == null) // create one if does not exist
            {
                GameObject go = new GameObject($"AudioSource_user_{user.Id()}");
                go.transform.SetParent(parent);
                audioSourceNode = go.AddComponent<UMI3DNode>();

                user.audioPlayer = audioSourceNode.gameObject.AddComponent<UMI3DAudioPlayer>();
                user.audioPlayer.ObjectSpacialBlend.SetValue(Spacialized ? 1 : 0);
                user.audioPlayer.ObjectNode.SetValue(audioSourceNode);

                tr.AddIfNotNull(audioSourceNode.GetLoadEntity());
                tr.AddIfNotNull(user.audioPlayer.GetLoadEntity());
            }
            else
            {
                audioSourceNode = user.audioPlayer.ObjectNode.GetValue();
                if (audioSourceNode == null)
                {
                    GameObject go = new GameObject($"AudioSource_user_{user.Id()}");
                    audioSourceNode = go.AddComponent<UMI3DNode>();
                    go.transform.SetParent(this.transform);
                    tr.AddIfNotNull(user.audioPlayer.ObjectNode.SetValue(audioSourceNode));
                }
            }

            var binding = new BoneBinding(audioSourceNode.Id(), user.Id(), BoneType.Head)
            {
                syncPosition = true,
                syncRotation = true,
                priority = 100
            };

            tr.AddIfNotNull(BindingManager.Instance.AddBinding(binding));
            UMI3DServer.Dispatch(tr);

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

        private void RemoveAudioSource(UMI3DCollaborationUser user)
        {
            if (user is null || user.audioPlayer == null)
                return;
            Transaction t = new() { reliable = true };
            var audioSource = user.audioPlayer.ObjectNode.GetValue();
            t.AddIfNotNull(BindingManager.Instance.RemoveAllBindings(audioSource.Id()));
            t.AddIfNotNull(audioSource.GetDeleteEntity());
            t.Dispatch();
            UnityEngine.Object.Destroy(audioSource.gameObject);
        }
    }
}