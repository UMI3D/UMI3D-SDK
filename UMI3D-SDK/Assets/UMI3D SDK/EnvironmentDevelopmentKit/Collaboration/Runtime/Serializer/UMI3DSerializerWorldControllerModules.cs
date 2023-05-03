using umi3d.common;
using UnityEditor.VersionControl;


namespace umi3d.worldController
{
    public class UMI3DSerializerWorldControllerModules : UMI3DSerializerModule
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
                case WorldControllerMessage c:
                    bytable = c.ToBytableArray(parameters);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}
