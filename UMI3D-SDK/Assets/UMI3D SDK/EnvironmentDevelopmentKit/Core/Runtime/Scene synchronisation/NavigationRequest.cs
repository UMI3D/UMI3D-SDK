using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class NavigationRequest : DispatchableRequest
    {

        public SerializableVector3 position;

        public NavigationRequest(Vector3 position, bool reliable, HashSet<UMI3DUser> users = null) : base(reliable, users)
        {
            this.position = position;
            
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

        public override byte[] ToBytes()
        {
            return ToBytable().ToBytes();
        }

        public override byte[] ToBson()
        {
            var dto = CreateDto();
            WriteProperties(dto);
            return dto.ToBson();
        }

        protected virtual NavigateDto CreateDto() { return new NavigateDto(); }
        protected virtual void WriteProperties(NavigateDto dto) { dto.position = position; }
    }
}