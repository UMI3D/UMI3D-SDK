using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace umi3d.common
{
    [System.Serializable]
    public class AvatarMappingDto : AbstractObject3DDto
    {
        public BonePairDictionary bonePairDictionary = null;

        public string userId = null;

        public AvatarMappingDto() : base() { }
    }
}
