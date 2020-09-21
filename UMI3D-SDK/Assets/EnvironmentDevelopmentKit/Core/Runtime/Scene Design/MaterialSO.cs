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

        public enum AlphaMode
        {
            OPAQUE, MASK, BLEND
        }
        public AlphaMode alphaMode = AlphaMode.BLEND;

        protected abstract void OnEnable();

        protected abstract string GetId();

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
        protected abstract void InitDefinition(string id);

        protected void RegisterMaterial(AbstractEntityDto mat)
        {
            //   Debug.Log("try registered");
            if (string.IsNullOrEmpty(mat.id) || UMI3DEnvironment.GetEntity<MaterialSO>(mat.id) == null)
            {
                mat.id = UMI3DEnvironment.Register(this);
                InitDefinition(mat.id);

                //      Debug.Log("registered");
            }
        }

    }
}
