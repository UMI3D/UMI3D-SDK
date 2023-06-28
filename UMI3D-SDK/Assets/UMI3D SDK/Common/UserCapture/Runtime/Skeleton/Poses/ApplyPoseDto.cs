namespace umi3d.common.userCapture.pose
{
    public class ApplyPoseDto : AbstractOperationDto
    {
        public ulong userID { get; set; }
        public ulong poseKey { get; set; }
        public int indexInList { get; set; }

        public bool stopPose { get; set; } = false;
    }
}