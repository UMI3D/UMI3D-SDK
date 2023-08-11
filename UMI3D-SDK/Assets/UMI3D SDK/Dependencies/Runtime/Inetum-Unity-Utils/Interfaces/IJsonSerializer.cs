using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Interface for serialization and deserialization of scriptable objects
    /// </summary>
    public interface IJsonSerializer
    {
        public string JsonSerialize();
        public ScriptableObject JsonDeserializeScriptableObject(string data);
    }
}