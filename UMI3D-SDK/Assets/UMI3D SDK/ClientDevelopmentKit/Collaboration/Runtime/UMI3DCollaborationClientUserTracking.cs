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
using System.Linq;
using umi3d.cdk.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class UMI3DCollaborationClientUserTracking : UMI3DClientUserTracking
    {
        public GameObject UnitSkeleton;

        private Coroutine forceSendTrackingCoroutine;

        private int lastNbUsers = 0;

        protected override void Start()
        {
            base.Start();
            avatarEvent.AddListener(UMI3DCollaborativeUserAvatar.SkeletonCreation);
            UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(() => embodimentDict.Clear());
            UMI3DCollaborationClientServer.Instance.OnReconnect.AddListener(() => embodimentDict.Clear());
        }

        private void OnEnable()
        {
            UMI3DCollaborationEnvironmentLoader.OnUpdateUserList += ForceSendTracking;
        }

        private void OnDisable()
        {
            UMI3DCollaborationEnvironmentLoader.OnUpdateUserList -= ForceSendTracking;
        }

        /// <summary>
        /// Force tracking frame sending for new users.
        /// </summary>
        private void ForceSendTracking()
        {
            int newNbOfUsers = UMI3DCollaborationEnvironmentLoader.Instance.UserList.Count;

            if (newNbOfUsers > lastNbUsers && newNbOfUsers > 1)
            {
                if (forceSendTrackingCoroutine != null)
                    StopCoroutine(forceSendTrackingCoroutine);
                StartCoroutine(ForceSendTrackingCoroutine());
            }
            lastNbUsers = newNbOfUsers;
        }

        /// <summary>
        /// Coroutine used by <see cref="ForceSendTracking"/> to only send tracking frames when everyone is active.
        /// </summary>
        /// <returns></returns>
        IEnumerator ForceSendTrackingCoroutine()
        {
            while (UMI3DCollaborationEnvironmentLoader.Instance.UserList.Any(u => u.status != common.StatusType.ACTIVE))
            {
                yield return null;
            }

            yield return null;

            //Tracking frame sent twice for Kalman filter
            for (int i = 0; i < 2; i++)
            {
                BonesIterator(true);
                UMI3DCollaborationClientServer.SendTracking(LastFrameDto);

                yield return new WaitForSeconds(.5f);
            }

            forceSendTrackingCoroutine = null;
        }

        ///<inheritdoc/>
        protected override IEnumerator DispatchTracking()
        {
            while (sendTracking)
            {
                if (targetTrackingFPS > 0)
                {
                    if (UMI3DCollaborationClientServer.Connected())
                    {
                        BonesIterator();
                        if (LastFrameDto != null)
                        {
                            UMI3DCollaborationClientServer.SendTracking(LastFrameDto);
                        }
                    }
                    yield return new WaitForSeconds(1f / targetTrackingFPS);
                }
                else
                {
                    yield return new WaitUntil(() => targetTrackingFPS > 0 || !sendTracking);
                }
            }
        }

        ///<inheritdoc/>
        protected override IEnumerator DispatchCamera()
        {
            yield return new WaitUntil(() => UMI3DCollaborationClientServer.Connected());

            base.DispatchCamera();
        }
    }
}
