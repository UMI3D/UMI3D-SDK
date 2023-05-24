using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


namespace umi3d.edk.interaction
{
    public class UMI3DSerializerInteractionModules : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(AbstractInteraction) => true,
                true when typeof(T) == typeof(UMI3DManipulation.DofGroup) => true,
                true when typeof(T) == typeof(UMI3DManipulation.DofGroupOption) => true,
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
                case AbstractInteraction c:
                    bytable = c.ToBytableArray(parameters);
                    return true;
                case UMI3DManipulation.DofGroup c:
                    bytable = UMI3DSerializer.Write(c.name)
                    + UMI3DSerializer.Write((int)c.dofs);
                    return true;
                case UMI3DManipulation.DofGroupOption c:
                    bytable = UMI3DSerializer.Write(c.name)
                    + UMI3DSerializer.WriteCollection(c.separations);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
