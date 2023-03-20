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
    public class PoseManager : Singleton<PoseManager>
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.UserCapture | DebugScope.User;
        /// <summary>
        /// Lock for  <see cref="JoinDtoReception(UMI3DUser, SerializableVector3, Dictionary{uint, bool})"/>.
        /// </summary>
        static object joinLock = new object();

        bool posesInitialized = false;

        private Dictionary<ulong, List<PoseDto>> allPoses = new Dictionary<ulong, List<PoseDto>>();
        public UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> _objectAllPoses;
        public UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> objectAllPoses
        {
            get
            {
                Init();
                return _objectAllPoses;
            }
        }

        public void Init()
        {
            if (posesInitialized == false)
            {
                posesInitialized = true;
                _objectAllPoses = new UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.AllPoses, allPoses, null, null);
            }
        }
        public async Task InitNewUserPoses(UMI3DUser user, List<PoseDto> userPoses)
        {
            Operation operation;
            lock (joinLock)
            {
                operation = objectAllPoses.Add(user.Id(), userPoses);
            }

            SendNewPosesToAllNewUsers(operation);
        }

        private void SendNewPosesToAllNewUsers(Operation operation)
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