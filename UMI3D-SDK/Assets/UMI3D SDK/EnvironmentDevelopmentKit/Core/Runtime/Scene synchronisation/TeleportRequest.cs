using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{

    public class TeleportRequest : NavigationRequest
    {
        public SerializableVector4 rotation;

        public TeleportRequest(Vector3 position, Quaternion rotation, bool reliable) : base(position, reliable)
        {
            this.rotation = rotation;
        }

        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.TeleportationRequest;
        }

        protected override Bytable ToBytable()
        {
            if (rotation == null) rotation = new SerializableVector4();
            return base.ToBytable()
                + UMI3DNetworkingHelper.Write(rotation);
        }

        protected override NavigateDto CreateDto() { return new TeleportDto(); }
        protected override void WriteProperties(NavigateDto dto)
        {
            base.WriteProperties(dto);
            if (dto is TeleportDto tpDto)
                tpDto.rotation = rotation;
        }
    }
}