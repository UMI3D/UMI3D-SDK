using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.common.userCapture
{
    public class UMI3DSerializerBoneModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(ControllerDto) => true,
                true when typeof(T) == typeof(BoneDto) => true,
                _ => null,
            };
        }

        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
            }

            result = default(T);
            readable = false;
            return false;
        }

        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case ControllerDto c:
                    bytable = UMI3DSerializer.Write(c.boneType)
                            + UMI3DSerializer.Write(c.rotation ?? new Vector4Dto())
                            + UMI3DSerializer.Write(c.position ?? new Vector3Dto());
                    return true;
                case BoneDto c:
                    bytable = UMI3DSerializer.Write(c.boneType)
                            + UMI3DSerializer.Write(c.rotation ?? new Vector4Dto());
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
