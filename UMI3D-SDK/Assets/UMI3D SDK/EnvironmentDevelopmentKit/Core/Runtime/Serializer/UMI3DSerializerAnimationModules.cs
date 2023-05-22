using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.edk
{
    public class UMI3DSerializerAnimationModules : UMI3DSerializerModule
    {
        public override bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(UMI3DAnimation.AnimationChain) => true,
                true when typeof(T) == typeof(UMI3DNodeAnimation.OperationChain) => false,
                _ => null,
            };
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
                case UMI3DAnimation.AnimationChain c:
                    bytable = c.ToByte(null);
                    return true;
                case UMI3DNodeAnimation.OperationChain c:
                    bytable = c.ToBytableArray(parameters);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
