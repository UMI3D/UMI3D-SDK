using System;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DClientServer : PersistentSingleton<UMI3DClientServer>
    {
        /// <summary>
        /// Environment connected to.
        /// </summary>
        protected MediaDto environment;

        /// <summary>
        /// Environment connected to.
        /// </summary>
        public static MediaDto Media
        {
            get {
                return Exists ? Instance.environment : null;
            }
            set {
                if (Exists)
                    Instance.environment = value;
            }
        }


        static public void Send(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (Exists)
                Instance._Send(dto, reliable);
        }

        protected virtual void _Send(AbstractBrowserRequestDto dto, bool reliable) { }

        static public void SendTracking(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (Exists)
                Instance._SendTracking(dto, reliable);
        }
        protected virtual void _SendTracking(AbstractBrowserRequestDto dto, bool reliable) { }


        static public void GetFile(string url, Action<byte[]> callback, Action<string> onError)
        {
            if (Exists)
                Instance._GetFile(url, callback, onError);
        }

        protected virtual void _GetFile(string url, Action<byte[]> callback, Action<string> onError) { }


        public virtual string GetId() { return null; }

    }
}