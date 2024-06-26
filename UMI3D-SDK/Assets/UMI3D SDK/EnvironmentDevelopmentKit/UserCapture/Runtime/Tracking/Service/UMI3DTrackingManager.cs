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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.tracking;

namespace umi3d.edk.userCapture.tracking
{
    public class UMI3DTrackingManager : Singleton<UMI3DTrackingManager>
    {
        public float FPSTrackingTarget = 15f;

        public Dictionary<ulong, float> asyncFPS = new Dictionary<ulong, float>();
        public Dictionary<ulong, Dictionary<uint, float>> asyncBonesFPS = new Dictionary<ulong, Dictionary<uint, float>>();

        public event Action<UserTrackingFrameDto, ulong> AvatarFrameReceived;

        #region DI

        private readonly IUMI3DServer umi3dServerService;

        public UMI3DTrackingManager(IUMI3DServer umi3dServerService) : base()
        {
            this.umi3dServerService = umi3dServerService;
        }

        public UMI3DTrackingManager() : this(umi3dServerService: UMI3DServer.Instance)
        {
        }

        #endregion DI

        public void OnAvatarFrameReceived(UserTrackingFrameDto trackingFrameDto, ulong period)
        {
            AvatarFrameReceived?.Invoke(trackingFrameDto, period);
        }

        public void SyncFPSTracking()
        {
            SetTrackingTargetFPS FPSRequest = new SetTrackingTargetFPS(FPSTrackingTarget);

            FPSRequest.users = new HashSet<UMI3DUser>(umi3dServerService.Users());
            FPSRequest.ToTransaction(true).Dispatch();

            foreach (UMI3DUser user in umi3dServerService.Users())
            {
                if (asyncFPS.ContainsKey(user.Id()))
                    asyncFPS.Remove(user.Id());
            }
        }

        public void SyncFPSTracking(UMI3DUser user)
        {
            SetTrackingTargetFPS FPSRequest = new SetTrackingTargetFPS(FPSTrackingTarget);

            FPSRequest.users = new HashSet<UMI3DUser>() { user };
            FPSRequest.ToTransaction(true).Dispatch();

            if (asyncFPS.ContainsKey(user.Id()))
                asyncFPS.Remove(user.Id());
        }

        public void UpdateFPSTracking(float FPSTarget)
        {
            SetTrackingTargetFPS FPSRequest = new SetTrackingTargetFPS(FPSTarget);

            FPSRequest.users = umi3dServerService.UserSet();
            FPSRequest.ToTransaction(true).Dispatch();

            if (FPSTarget != FPSTrackingTarget)
            {
                foreach (UMI3DUser user in umi3dServerService.Users())
                {
                    asyncFPS[user.Id()] = FPSTarget;
                }
            }
            else
            {
                foreach (UMI3DUser user in umi3dServerService.Users())
                {
                    if (asyncFPS.ContainsKey(user.Id()))
                        asyncFPS.Remove(user.Id());
                }
            }
        }

        public void UpdateFPSTracking(UMI3DUser user, float FPSTarget)
        {
            SetTrackingTargetFPS FPSRequest = new SetTrackingTargetFPS(FPSTarget);

            FPSRequest.users = new HashSet<UMI3DUser>() { user };
            FPSRequest.ToTransaction(true).Dispatch();

            if (FPSTarget != FPSTrackingTarget)
                asyncFPS[user.Id()] = FPSTarget;
            else if (asyncFPS.ContainsKey(user.Id()))
                asyncFPS.Remove(user.Id());
        }

        public void UpdateBoneFPS(float FPSTarget, uint boneType)
        {
            SetTrackingBoneTargetFPS BoneFPSRequest = new SetTrackingBoneTargetFPS(FPSTarget, boneType);

            BoneFPSRequest.users = new HashSet<UMI3DUser>(umi3dServerService.Users());
            BoneFPSRequest.ToTransaction(true).Dispatch();

            if (FPSTarget != FPSTrackingTarget)
            {
                foreach (UMI3DUser user in umi3dServerService.Users())
                {
                    if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
                    {
                        userBonesFPS[boneType] = FPSTarget;
                        asyncBonesFPS[user.Id()] = userBonesFPS;
                    }
                    else
                        asyncBonesFPS.Add(user.Id(), new Dictionary<uint, float>() { { boneType, FPSTarget } });
                }
            }
            else
            {
                foreach (UMI3DUser user in umi3dServerService.Users())
                {
                    if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
                    {
                        userBonesFPS.Remove(boneType);
                        asyncBonesFPS[user.Id()] = userBonesFPS;
                    }
                }
            }
        }

        public void UpdateBoneFPS(UMI3DUser user, float FPSTarget, uint boneType)
        {
            SetTrackingBoneTargetFPS BoneFPSRequest = new SetTrackingBoneTargetFPS(FPSTarget, boneType);

            BoneFPSRequest.users = new HashSet<UMI3DUser>() { user };
            BoneFPSRequest.ToTransaction(true).Dispatch();

            if (FPSTarget != FPSTrackingTarget)
            {
                if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
                {
                    userBonesFPS[boneType] = FPSTarget;
                    asyncBonesFPS[user.Id()] = userBonesFPS;
                }
                else
                    asyncBonesFPS.Add(user.Id(), new Dictionary<uint, float>() { { boneType, FPSTarget } });
            }
            else
            {
                if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
                {
                    userBonesFPS.Remove(boneType);
                    asyncBonesFPS[user.Id()] = userBonesFPS;
                }
            }
        }

        public void SyncBoneFPS(uint boneType)
        {
            SetTrackingBoneTargetFPS BoneFPSRequest = new SetTrackingBoneTargetFPS(FPSTrackingTarget, boneType);

            BoneFPSRequest.users = new HashSet<UMI3DUser>(umi3dServerService.Users());
            BoneFPSRequest.ToTransaction(true).Dispatch();

            foreach (var userBonesFPS in asyncBonesFPS)
            {
                userBonesFPS.Value.Remove(boneType);
            }
        }

        public void SyncBoneFPS(UMI3DUser user, uint boneType)
        {
            SetTrackingBoneTargetFPS BoneFPSRequest = new SetTrackingBoneTargetFPS(FPSTrackingTarget, boneType);

            BoneFPSRequest.users = new HashSet<UMI3DUser>() { user };
            BoneFPSRequest.ToTransaction(true).Dispatch();

            if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
            {
                userBonesFPS.Remove(boneType);
                asyncBonesFPS[user.Id()] = userBonesFPS;
            }
        }

        public void SyncAllBones()
        {
            foreach (var userBonesFPS in asyncBonesFPS)
            {
                var t = new Transaction(true);

                foreach (var boneFPS in userBonesFPS.Value)
                {
                    SetTrackingBoneTargetFPS BoneFPSRequest = new SetTrackingBoneTargetFPS(FPSTrackingTarget, boneFPS.Key);

                    BoneFPSRequest.users = new HashSet<UMI3DUser>() { umi3dServerService.Users().First<UMI3DUser>(u => u.Id() == userBonesFPS.Key) };
                    t.Add(BoneFPSRequest);
                }

                t.Dispatch();
            }

            asyncBonesFPS.Clear();
        }

        public void SyncAllBones(UMI3DUser user)
        {
            if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
            {
                var t = new Transaction(true);

                foreach (var boneFPS in userBonesFPS)
                {
                    SetTrackingBoneTargetFPS BoneFPSRequest = new SetTrackingBoneTargetFPS(FPSTrackingTarget, boneFPS.Key);

                    BoneFPSRequest.users = new HashSet<UMI3DUser>() { user };
                    t.Add(BoneFPSRequest);
                }

                t.Dispatch();

                asyncBonesFPS.Remove(user.Id());
            }
        }
    }
}