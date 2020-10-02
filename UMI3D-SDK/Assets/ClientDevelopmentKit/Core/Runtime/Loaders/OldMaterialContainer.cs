using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk
{
    public class OldMaterialContainer : MonoBehaviour
    {
        public Material[] oldMats = null;

        private void Awake()
        {
            int lenght = GetComponent<Renderer>().sharedMaterials.Length;
            oldMats = new Material[lenght];
        }
    }
}
