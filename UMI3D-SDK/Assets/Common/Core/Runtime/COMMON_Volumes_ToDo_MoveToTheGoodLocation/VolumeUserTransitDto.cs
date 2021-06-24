
namespace umi3d.common.volume
{
    public class VolumeUserTransitDto : AbstractBrowserRequestDto
    {
        public string volumeId;

        /// <summary>
        /// True if the user entered in the volume, false if the user exited the volume.
        /// </summary>
        public bool direction;
    }
}