using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    public interface IJsonSerializer
    {
        public string JsonSerialize();
        public ScriptableObject JsonDeserializeScriptableObject(string data);
    }
}