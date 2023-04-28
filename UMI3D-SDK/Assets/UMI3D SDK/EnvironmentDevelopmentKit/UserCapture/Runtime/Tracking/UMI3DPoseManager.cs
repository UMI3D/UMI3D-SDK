using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    public class UMI3DPoseManager : SingleBehaviour<UMI3DPoseManager>
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.UserCapture | DebugScope.User;
        /// <summary>
        /// Lock for  <see cref="JoinDtoReception(UMI3DUser, SerializableVector3, Dictionary{uint, bool})"/>.
        /// </summary>
        static object joinLock = new object();
        static object logoutLock = new object();

        bool posesInitialized = false;

        [SerializeField] List<UMI3DPose_so> allServerPoses = new List<UMI3DPose_so>();

        private Dictionary<ulong, List<PoseDto>> allPoses = new Dictionary<ulong, List<PoseDto>>();
        private UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> _objectAllPoses;
        public UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> objectAllPoses
        {
            get
            {
                Init();
                return _objectAllPoses;
            }
        }


        //private List<UMI3DPoseOverriderContainer> allPoseOverriderContainer = new();
        //private UMI3DAsyncListProperty<UMI3DPoseOverriderContainer> _objectAllPoseOverrider;
        //public UMI3DAsyncListProperty<UMI3DPoseOverriderContainer> objectAllPoseOverrider
        //{
        //    get
        //    {
        //        Init();
        //        return _objectAllPoseOverrider;
        //    }
        //}

        public void Init()
        {
            if (posesInitialized == false)
            {
                posesInitialized = true;
                List<PoseDto> poses = new List<PoseDto>();
                for (int i = 0; i < allServerPoses.Count; i++)
                {
                    allServerPoses[i].SendPoseIndexationEvent(i);
                    PoseDto poseDto = allServerPoses[i].ToDTO();
                    poseDto.id = i;
                    poses.Add(poseDto);
                }

                allPoses.Add(0, poses);

                _objectAllPoses = new UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.AllPoses, allPoses, null, null);
                //_objectAllPoseOverrider = new UMI3DAsyncListProperty<UMI3DPoseOverriderContainer>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.AllPoseOverriderContainers, null, null);
            }

            UMI3DServer.Instance.OnUserLeave.AddListener((u) => RemovePosesOnLeftUser(u));
        }

        public async Task InitNewUserPoses(UMI3DUser user, List<PoseDto> userPoses)
        {
            Operation operation;
            lock (joinLock)
            {
                operation = objectAllPoses.Add(user.Id(), userPoses);
            }

            SendOperation(operation);
        }

        private void RemovePosesOnLeftUser(UMI3DUser user)
        {
            Operation operation;
            lock (logoutLock)
            {
                operation = objectAllPoses.Remove(user.Id());
            }

            SendOperation(operation);
        }

        private void SendOperation(Operation operation)
        {
            Transaction transaction = new Transaction()
            {
                reliable = true
            };

            transaction.Add(operation);

            transaction.Dispatch();
        }
    }
}