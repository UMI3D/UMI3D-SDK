using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.edk.userCapture
{
    public class UMI3DSerializerAnimationModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            if (typeof(T) == typeof(UMI3DHandAnimation.PhalanxRotations))
                return true;
            return null;
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
                case UMI3DHandAnimation.PhalanxRotations c:
                    bytable = UMI3DSerializer.Write(c.Phalanx)
                            + UMI3DSerializer.Write(c.PhalanxEulerRotation);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
