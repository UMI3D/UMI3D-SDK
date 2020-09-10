using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace umi3d.common
{
    [System.Serializable]
    public class PBRMaterialDto 
    {
        public SerializableVector4 baseColorFactor;
        public float metallicFactor;
        public float roughnessFactor;

    }
}
