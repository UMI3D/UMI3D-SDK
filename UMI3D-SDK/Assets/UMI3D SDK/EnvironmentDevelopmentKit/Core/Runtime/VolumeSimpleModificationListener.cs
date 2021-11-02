using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace umi3d.edk.volume
{
    public class VolumeSimpleModificationListener : MonoBehaviour
    {
        protected virtual void Start()
        {
            List<Box> boxes = FindObjectsOfType<Box>().ToList();
            foreach (Box box in boxes)
            {
                box.bounds.Attach(b => Debug.Log(b)); //TODO : create transaction for update !
            }
        }
    }
}