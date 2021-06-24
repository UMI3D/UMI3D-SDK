using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk.volumes
{
    public abstract class AbstractPrimitive : AbstractVolumeCell
    {
        public string id = "";
        public override string Id()
        {
            if (id.Equals(""))
                throw new System.Exception("Id should have been set on dto reception !");
            return id;
        }

        public abstract void Delete();
    }
}