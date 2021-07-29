using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class GetLocalInfoRequest : DispatchableRequest
    {
        public string key;

        public GetLocalInfoRequest(string key, bool reliable, HashSet<UMI3DUser> users = null) : base(reliable, users)
        {
            this.key = key;
        }

        protected virtual Bytable ToBytable()
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.GetLocalInfoRequest)
                + UMI3DNetworkingHelper.Write(key);
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

        protected virtual GetLocalInfoRequestDto CreateDto() { return new GetLocalInfoRequestDto(); }
        protected virtual void WriteProperties(GetLocalInfoRequestDto dto) { dto.key = key; }
    }
}