using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public abstract class DispatchableRequest
    {
        /// <summary>
        /// List of users to which this operation should be send.
        /// </summary>
        public HashSet<UMI3DUser> users;
        public bool reliable;

        public DispatchableRequest(bool reliable, HashSet<UMI3DUser> users)
        {
            this.users = users ?? new HashSet<UMI3DUser>();
            this.reliable = reliable;
        }

        public abstract byte[] ToBytes();

        public abstract byte[] ToBson();

    }
}