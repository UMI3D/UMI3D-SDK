using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.edk
{
    public class UMI3DSerializerResourcesModules : UMI3DSerializerModule
    {
        public override bool? IsCountable<T>()
        {
            if (typeof(T) == typeof(UMI3DResourceFile))
                return true;
            return null;
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
                case UMI3DResourceFile c:
                    bytable = c.ToByte();
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
