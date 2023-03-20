using inetum.unityUtils;
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

        public UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> allPoses;

        public async Task InitNewUserPoses(UMI3DUser user, List<PoseDto> userPoses)
        {
            lock (joinLock)
            {
                allPoses.Add(user.Id(), userPoses);
            }

            // TODO 
            // Send all poses to this user 
            // Send new Poses to new users
        }
    }
}