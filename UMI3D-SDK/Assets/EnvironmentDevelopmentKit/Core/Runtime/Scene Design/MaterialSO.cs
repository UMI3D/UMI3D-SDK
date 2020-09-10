using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.edk;
using UnityEngine;

namespace umi3d.edk
{
    //[CreateAssetMenu(fileName = "Umi3DMaterial", menuName = "UMI3D/Umi3DMaterial")]
    public abstract class MaterialSO : ScriptableObject, UMI3DEntity
    {
        /*
        [SerializeField]
        private List<string> materialsToOveride;
        //public IMaterialDto material;
        public List<string> MaterialsToOveride
        {
            get => this.materialsToOveride;
            set => this.materialsToOveride = value;
        }
        */
        //[HideInInspector]
        // public string idMaterialSO = "";

        public enum AlphaMode
        {
            OPAQUE, MASK, BLEND
        }
        public AlphaMode alphaMode = AlphaMode.BLEND;

        protected abstract void OnEnable();

        public abstract string GetId();

        /*   private void Awake()
           {
               Debug.Log("on Awake");

               idMaterialSO = System.Guid.NewGuid().ToString();
           }*/

        public abstract GlTFMaterialDto ToDto();

        public string Id()
        {
            return GetId();
        }
    }
}
