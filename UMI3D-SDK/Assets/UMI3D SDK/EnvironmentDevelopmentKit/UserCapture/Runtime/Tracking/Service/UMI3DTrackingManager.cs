using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    public class UMI3DTrackingManager : Singleton<UMI3DTrackingManager>
    {
        public float FPSTrackingTarget = 15f;

        private Dictionary<ulong, float> asyncFPS = new Dictionary<ulong, float>();
        private Dictionary<ulong, Dictionary<uint, float>> asyncBonesFPS = new Dictionary<ulong, Dictionary<uint, float>>();


        public void SyncFPSTracking()
        {
            SetTrackingTargetFPS FPSRequest = new SetTrackingTargetFPS(FPSTrackingTarget);

            FPSRequest.users = new HashSet<UMI3DUser>(UMI3DServer.Instance.Users());
            FPSRequest.ToTransaction(true).Dispatch();

            foreach (UMI3DUser user in UMI3DServer.Instance.Users())
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

            FPSRequest.users = UMI3DServer.Instance.UserSet();
            FPSRequest.ToTransaction(true).Dispatch();

            if (FPSTarget != FPSTrackingTarget)
            {
                foreach (UMI3DUser user in UMI3DServer.Instance.Users())
                {
                    asyncFPS[user.Id()] = FPSTarget;
                }
            }
            else
            {
                foreach (UMI3DUser user in UMI3DServer.Instance.Users())
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

            BoneFPSRequest.users = new HashSet<UMI3DUser>(UMI3DServer.Instance.Users());
            BoneFPSRequest.ToTransaction(true).Dispatch();

            if (FPSTarget != FPSTrackingTarget)
            {
                foreach (UMI3DUser user in UMI3DServer.Instance.Users())
                {
                    if (asyncBonesFPS.TryGetValue(user.Id(), out var userBonesFPS))
                    {
                        userBonesFPS[boneType] = FPSTarget;
                        asyncBonesFPS[user.Id()] = userBonesFPS;
                    }

                    else
                        asyncBonesFPS.Add(user.Id(), new Dictionary<uint, float>() {{ boneType, FPSTarget }});
                }
            }
            else
            {
                foreach (UMI3DUser user in UMI3DServer.Instance.Users())
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

            BoneFPSRequest.users = new HashSet<UMI3DUser>(UMI3DServer.Instance.Users());
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

                    BoneFPSRequest.users = new HashSet<UMI3DUser>() { UMI3DServer.Instance.Users().First<UMI3DUser>(u => u.Id() == userBonesFPS.Key) };
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
