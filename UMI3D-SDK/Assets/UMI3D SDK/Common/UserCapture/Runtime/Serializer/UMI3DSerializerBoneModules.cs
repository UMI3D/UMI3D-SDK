using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.common.userCapture
{
    public class UMI3DSerializerBoneModules : UMI3DSerializerModule
    {
        public override bool IsCountable<T>()
        {
            return true;
        }

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            switch (true)
            {
            }

            result = default(T);
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case ControllerDto c:
                    bytable = UMI3DSerializer.Write(c.boneType)
                            + UMI3DSerializer.Write(c.rotation ?? new SerializableVector4())
                            + UMI3DSerializer.Write(c.position ?? new SerializableVector3());
                    return true;
                case BoneDto c:
                    bytable = UMI3DSerializer.Write(c.boneType)
                            + UMI3DSerializer.Write(c.rotation ?? new SerializableVector4());
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
