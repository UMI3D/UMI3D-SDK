using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class NavigationRequest
    {
        /// <summary>
        /// List of users to which this operation should be send.
        /// </summary>
        public HashSet<UMI3DUser> users = new HashSet<UMI3DUser>();
        public SerializableVector3 position;
        public bool reliable;

        public NavigationRequest(Vector3 position, bool reliable)
        {
            this.position = position;
            this.reliable = reliable;
        }

        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.NavigationRequest;
        }

        protected virtual Bytable ToBytable()
        {
            if (position == null) position = new SerializableVector3();
            return UMI3DNetworkingHelper.Write(GetOperationKey())
                + UMI3DNetworkingHelper.Write(position);
        }

        public byte[] ToBytes()
        {
            return ToBytable().ToBytes();
        }

        public byte[] ToBson()
        {
            var dto = CreateDto();
            WriteProperties(dto);
            return dto.ToBson();
        }

        protected virtual NavigateDto CreateDto() { return new NavigateDto(); }
        protected virtual void WriteProperties(NavigateDto dto) { dto.position = position; }
    }
}