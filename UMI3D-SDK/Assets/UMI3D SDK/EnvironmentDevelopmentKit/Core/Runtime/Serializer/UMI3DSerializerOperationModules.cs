using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.edk
{
    public class UMI3DSerializerOperationModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            if( typeof(T).IsAssignableFrom(typeof(Operation)))
                return false;
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
                case Operation c:
                    bytable = c.ToBytableArray(parameters);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
