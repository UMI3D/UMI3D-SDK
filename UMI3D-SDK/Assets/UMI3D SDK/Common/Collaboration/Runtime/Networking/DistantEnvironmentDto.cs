using System;
using System.Collections.Generic;
namespace umi3d.common
{
    /// <summary>
    /// Abstract DTO to describe a UMI3D entity.
    /// </summary>
    [Serializable]
    public class DistantEnvironmentDto : AbstractEntityDto, IEntity
    {
        public GlTFEnvironmentDto environmentDto { get; set; }
        public string resourcesUrl { get; set; }
        public bool useDto { get; set; }

        public List<BinaryDto> binaries { get; set; }
    }
}