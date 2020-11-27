using UnityEngine;

namespace umi3d.cdk
{
    public class OldMaterialContainer : MonoBehaviour
    {
        public Material[] oldMats
        {
            get {
                if (_oldMats == null)
                {
                    InitOldMats();
                }
                return _oldMats;
            }
            set { _oldMats = value; }
        }


        private Material[] _oldMats = null;



        private void InitOldMats()
        {

            int lenght = GetComponent<Renderer>().sharedMaterials.Length;
            _oldMats = new Material[lenght];

        }

    }
}
